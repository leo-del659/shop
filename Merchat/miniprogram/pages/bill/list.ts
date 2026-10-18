import { getBillList, MerchantBillItem, OrderFilter } from '../../utils/api'
import { isLogin } from '../../utils/auth'
import { orderStatusDetail, toast } from '../../utils/util'

type FilterKey = 'all' | 'pending' | 'shipping' | 'completed' | 'cancelled'

Page({
  data: {
    filter: 'all' as FilterKey,
    filters: [
      { key: 'all', label: '全部' },
      { key: 'pending', label: '待处理' },
      { key: 'shipping', label: '配送中' },
      { key: 'completed', label: '已完成' },
      { key: 'cancelled', label: '已取消' },
    ] as { key: FilterKey; label: string }[],
    list: [] as any[],
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
      const rows: MerchantBillItem[] = await getBillList(this.data.filter as OrderFilter)
      const list = (rows || []).map((b) => {
        const os = Number(b.orderStatus ?? 0)
        const st = b.statusLabel && b.statusCls
          ? { label: b.statusLabel, cls: b.statusCls }
          : orderStatusDetail(os)
        return {
          id: String(b.id || ''),
          billNo: b.billNo || String(b.id || ''),
          merchantName: b.merchantName || '-',
          goodsName: b.goodsName || '-',
          buyNum: Number(b.buyNum ?? 0),
          amount: Number(b.amount ?? 0),
          oLabel: st.label,
          oCls: st.cls,
          time: b.createTime ? String(b.createTime).slice(0, 16).replace('T', ' ') : '',
        }
      })
      this.setData({ list })
    } catch (e: any) {
      toast('加载失败：' + (e.message || '未知错误'))
    }
  },
  setFilter(e: any) {
    const key = e.currentTarget.dataset.key as FilterKey
    if (key === this.data.filter) return
    this.setData({ filter: key })
    this.load()
  },
})
