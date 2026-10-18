import { getRefundDetail, RefundDetail, REFUND_STATUS_TEXT, REFUND_TYPE_TEXT } from '../../api/refund'
import { resolveUrl } from '../../utils/request'
import { requireLogin } from '../../app'

type Detail = RefundDetail & { cover: string; statusText: string; typeText: string; itemsDecorated: any[] }

Page({
  data: {
    id: '',
    detail: null as Detail | null,
    loading: true
  },

  onLoad(options: any) {
    this.setData({ id: options.id || '' })
    this.loadDetail()
  },

  async loadDetail() {
    if (!requireLogin()) return
    try {
      const d = (await getRefundDetail(this.data.id)) as RefundDetail
      const detail: Detail = {
        ...d,
        cover: resolveUrl(d.goodsImg),
        statusText: REFUND_STATUS_TEXT[d.refundStatus] || '未知',
        typeText: REFUND_TYPE_TEXT[d.refundType] || '退货退款',
        itemsDecorated: (d.items || []).map((i) => ({ ...i, cover: resolveUrl(i.goodsImg) }))
      }
      this.setData({ detail, loading: false })
    } catch (e) {
      this.setData({ loading: false })
    }
  }
})
