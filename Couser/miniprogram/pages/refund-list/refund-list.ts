import { getMyRefunds, RefundItem, REFUND_STATUS_TEXT, REFUND_TYPE_TEXT } from '../../api/refund'
import { resolveUrl } from '../../utils/request'
import { getAppToken } from '../../app'

Page({
  data: {
    list: [] as (RefundItem & { cover: string; statusText: string; typeText: string })[],
    loading: true,
    finished: false,
    page: 1
  },

  onShow() {
    this.setData({ page: 1, list: [], finished: false, loading: true })
    this.loadList()
  },

  async loadList() {
    // 未登录：不跳登录页，直接展示空态（无数据）
    if (!getAppToken()) {
      this.setData({ page: 1, list: [], finished: true, loading: false })
      return
    }
    try {
      const res = await getMyRefunds({ page: this.data.page, pageSize: 20 })
      const decorated = (res.list || []).map((r: RefundItem) => ({
        ...r,
        cover: resolveUrl(r.goodsImg),
        statusText: REFUND_STATUS_TEXT[r.refundStatus] || '未知',
        typeText: REFUND_TYPE_TEXT[r.refundType] || '退货退款'
      }))
      const list = this.data.page === 1 ? decorated : this.data.list.concat(decorated)
      this.setData({
        list,
        finished: (res.list || []).length < 20,
        loading: false
      })
    } catch (e) {
      this.setData({ loading: false })
    }
  },

  onReachBottom() {
    if (this.data.finished || this.data.loading) return
    this.setData({ page: this.data.page + 1 })
    this.loadList()
  },

  goDetail(e: any) {
    wx.navigateTo({ url: '/pages/refund-detail/refund-detail?id=' + e.currentTarget.dataset.id })
  }
})
