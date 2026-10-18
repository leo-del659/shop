import {
  getBalance, getReconcile, getWithdrawals, submitWithdraw, getBillList, getBank,
} from '../../utils/api'
import { isLogin } from '../../utils/auth'
import { WITHDRAW_STATUS, toast } from '../../utils/util'

Page({
  data: {
    canSettle: true,
    canWithdraw: true,
    available: 0,
    totalPaid: 0,
    withdrawn: 0,
    paidOrderCount: 0,
    monthKey: '',
    monthLabel: '',
    monthOptions: [] as string[],
    monthLabels: [] as string[],
    monthIndex: 0,
    summaryRevenue: 0,
    summaryOrders: 0,
    summaryPaid: 0,
    tab: 'orders',
    tabOrders: true,
    tabPayments: false,
    tabWithdrawals: false,
    orders: [] as any[],
    payments: [] as any[],
    withdrawals: [] as any[],
    showWithdraw: false,
    wdAmount: '',
    wdAccount: '',
    wdRemark: '',
    bankName: '',
    bankAccount: '',
    accountName: '',
  },
  onShow() {
    const tabBar = this.getTabBar() as any
    if (tabBar) tabBar.setData({ selected: 3 })
    this.load()
  },
  async load() {
    if (!isLogin()) {
      wx.redirectTo({ url: '/pages/login/login' })
      return
    }
    this.setData({ canSettle: true, canWithdraw: true })

    const now = new Date()
    const y = now.getFullYear()
    const m = now.getMonth() + 1
    const options: string[] = []
    const labels: string[] = []
    for (let i = 0; i < 6; i++) {
      const d = new Date(y, m - 1 - i, 1)
      options.push(`${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`)
      labels.push(`${d.getFullYear()}年${d.getMonth() + 1}月`)
    }
    this.setData({
      monthOptions: options,
      monthLabels: labels,
      monthIndex: 0,
      monthLabel: labels[0],
    })

    try {
      const [balance, bank] = await Promise.all([getBalance(), getBank()])
      const available = Number(balance.available ?? (balance.totalPaid - balance.withdrawn))
      this.setData({
        available: Number.isFinite(available) ? available : 0,
        totalPaid: balance.totalPaid,
        withdrawn: balance.withdrawn,
        paidOrderCount: balance.paidOrderCount,
        bankName: bank?.bankName || '',
        bankAccount: bank?.bankAccount || '',
        accountName: bank?.accountName || '',
      })
    } catch (e: any) {
      toast('余额加载失败：' + (e.message || '未知错误'))
    }

    await Promise.all([
      this.loadMonth(options[0]).catch((e: any) => toast('对账加载失败：' + e.message)),
      this.loadCompletedOrders().catch((e: any) => toast('订单加载失败：' + e.message)),
    ])
    await this.loadWithdrawals().catch((e: any) => toast('提现记录加载失败：' + e.message))
  },
  async loadMonth(monthKey: string) {
    const d = await getReconcile(monthKey)
    this.setData({
      monthKey: d.monthKey,
      monthLabel: d.monthLabel,
      summaryRevenue: d.summary.revenue,
      summaryOrders: d.summary.orders,
      summaryPaid: d.summary.paid,
      payments: d.payments.map((p: any) => ({
        ...p,
        orderId: p.orderId || p.billNo || '-',
        amount: p.amount,
        time: p.time,
        method: p.method || p.payTypeLabel || '-',
      })),
    })
  },
  async loadCompletedOrders() {
    const rows = await getBillList('all')
    const orders = (rows || []).map((b: any) => {
      const os = Number(b.orderStatus ?? 0)
      let label = '未知', cls = ''
      if (os === 0 || os === 1 || os === 8) { label = '待处理'; cls = 'pending' }
      else if (os === 2 || os === 3) { label = '配送中'; cls = 'shipping' }
      else if (os === 4 || os === 6) { label = '已完成'; cls = 'completed' }
      else if (os === 9) { label = '已取消'; cls = 'cancelled' }
      // 商户端对账明细直接显示「商户·商品」
      const displayName = [(b.merchantName || '-'), (b.goodsName || '-')].join(' · ')
      return {
        id: String(b.id || ''),
        billNo: b.billNo || String(b.id || ''),
        displayName,
        merchantName: b.merchantName || '-',
        goodsName: b.goodsName || '-',
        buyNum: Number(b.buyNum ?? 0),
        total: Number(b.amount ?? 0),
        amount: Number(b.amount ?? 0),
        oLabel: label,
        oCls: cls,
        orderStatus: os,
        time: b.createTime ? String(b.createTime).slice(0, 16).replace('T', ' ') : '',
      }
    })
    this.setData({ orders })
  },
  async loadWithdrawals() {
    const rows = await getWithdrawals()
    const withdrawals = rows.map((w) => {
      const s = WITHDRAW_STATUS[w.status] || { label: w.status, cls: '' }
      return { ...w, wdLabel: s.label, wdCls: s.cls }
    })
    this.setData({ withdrawals })
  },
  onMonthChange(e: any) {
    const idx = e.detail.value as number
    this.setData({ monthIndex: idx })
    this.loadMonth(this.data.monthOptions[idx])
  },
  setTab(e: any) {
    const tab = e.currentTarget.dataset.tab as string
    this.setData({
      tab,
      tabOrders: tab === 'orders',
      tabPayments: tab === 'payments',
      tabWithdrawals: tab === 'withdrawals',
    })
  },
  goOrder(e: any) {
    wx.navigateTo({ url: '/pages/order/detail?id=' + e.currentTarget.dataset.id })
  },
  openWithdraw() {
    const { bankName, bankAccount } = this.data
    const accountText = bankName && bankAccount
      ? `${bankName} 尾号${bankAccount.slice(-4)}`
      : '请先绑定银行卡'
    this.setData({ showWithdraw: true, wdAmount: '', wdAccount: accountText, wdRemark: '' })
  },
  closeWithdraw() {
    this.setData({ showWithdraw: false })
  },
  noop() {},
  onWdInput(e: any) {
    const field = e.currentTarget.dataset.field as string
    const value = e.detail.value
    if (field === 'wdAmount') this.setData({ wdAmount: value })
    else if (field === 'wdRemark') this.setData({ wdRemark: value })
  },
  async confirmWithdraw() {
    const amount = parseFloat(this.data.wdAmount)
    if (!amount || amount <= 0) {
      toast('请输入有效金额')
      return
    }
    if (!this.data.bankName || !this.data.bankAccount) {
      toast('请先绑定银行卡')
      return
    }
    if (amount > this.data.available) {
      toast(`余额不足，当前可结算余额 ¥${this.data.available}`)
      return
    }
    try {
      await submitWithdraw(amount, this.data.wdRemark)
      toast('提现成功')
      this.setData({ showWithdraw: false })
      this.load()
    } catch (err: any) {
      toast(err.message || '提现失败')
    }
  },
})
