import {
  getBalance,
  getBank,
  submitWithdraw,
} from '../../utils/api'
import { isLogin } from '../../utils/auth'
import { toast } from '../../utils/util'

Page({
  data: {
    totalEarned: 0,
    withdrawn: 0,
    available: 0,
    completedOrderCount: 0,
    hasBank: false,
    bankName: '',
    bankAccount: '',
    accountName: '',
    showWithdraw: false,
    wdAmount: '',
    wdAccount: '',
    wdRemark: '',
    submitting: false,
  },
  onShow() {
    if (!isLogin()) {
      wx.redirectTo({ url: '/pages/login/login' })
      return
    }
    this.load()
  },
  async load() {
    try {
      const [balance, bank] = await Promise.all([getBalance(), getBank()])
      const hasBank = !!(bank && bank.bankAccount)
      this.setData({
        totalEarned: balance.totalPaid,
        withdrawn: balance.withdrawn,
        available: balance.available,
        completedOrderCount: balance.paidOrderCount,
        hasBank,
        bankName: bank?.bankName || '',
        bankAccount: bank?.bankAccount || '',
        accountName: bank?.accountName || '',
      })
    } catch (e: any) {
      toast('加载失败：' + (e.message || '未知错误'))
    }
  },
  goRecords() {
    wx.navigateTo({ url: '/pages/withdraw/records' })
  },
  goBank() {
    wx.navigateTo({ url: '/pages/bank/bank' })
  },
  openWithdraw() {
    if (!this.data.hasBank) {
      wx.showModal({
        title: '提示',
        content: '请先绑定收款银行账户后再提现',
        confirmText: '去绑定',
        success: (r) => {
          if (r.confirm) wx.navigateTo({ url: '/pages/bank/bank' })
        },
      })
      return
    }
    const accountText = this.data.bankName
      ? `${this.data.bankName} ${this.data.bankAccount}`
      : '已绑定账户'
    this.setData({ showWithdraw: true, wdAmount: '', wdAccount: accountText, wdRemark: '' })
  },
  closeWithdraw() {
    this.setData({ showWithdraw: false })
  },
  noop() {},
  onWdInput(e: any) {
    const field = e.currentTarget.dataset.field as string
    if (field === 'wdAmount') this.setData({ wdAmount: e.detail.value })
    else if (field === 'wdRemark') this.setData({ wdRemark: e.detail.value })
  },
  async confirmWithdraw() {
    if (this.data.submitting) return
    const amount = parseFloat(this.data.wdAmount)
    if (!amount || amount <= 0) {
      toast('请输入有效金额')
      return
    }
    if (amount > this.data.available) {
      toast(`余额不足，当前可提现 ¥${this.data.available}`)
      return
    }
    this.setData({ submitting: true })
    try {
      await submitWithdraw(amount, this.data.wdRemark)
      toast('提现成功')
      this.setData({ showWithdraw: false })
      this.load()
    } catch (err: any) {
      toast(err.message || '提现失败')
    } finally {
      this.setData({ submitting: false })
    }
  },
})
