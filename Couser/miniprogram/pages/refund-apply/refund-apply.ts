import { getOrderDetail, OrderSummary } from '../../api/order'
import { applyRefund } from '../../api/refund'
import { requireLogin } from '../../app'
import { resolveUrl } from '../../utils/request'

type ApplyOrder = OrderSummary & { statusText: string, items?: any[] }

const REASONS = ['商品质量问题', '不喜欢/不想要了', '商品与描述不符', '发货太慢', '其它']

Page({
  data: {
    orderId: '',
    order: null as ApplyOrder | null,
    loading: true,
    // 表单
    refundType: 2,
    refundAmount: '' as string,
    reason: '',
    submitting: false,
    reasons: REASONS,
    /** 是否已发货：待发货(2)显示仅退款，已发货/已完成(3)只显示退货退款 */
    shipped: false,
    /** 加载失败信息（用于渲染错误占位，避免 catch 后整页空白） */
    loadError: ''
  },

  onLoad(options: any) {
    this.setData({ orderId: options.id || '' })
    this.loadOrder()
  },

  async loadOrder() {
    if (!requireLogin()) return
    // 进入加载态前先清空错误，避免上一次失败信息残留
    this.setData({ loading: true, loadError: '' })
    try {
      const o = (await getOrderDetail(this.data.orderId)) as any
      const total = o.totalAmount || 0
      const status = o.orderStatus || 0
      // status=2 待发货（未发货），status=3 已完成（已发货）
      const shipped = status >= 3
      // 未发货默认仅退款，已发货默认退货退款
      this.setData({
        order: {
          ...o,
          statusText: status === 2 ? '待发货' : status === 3 ? '已完成' : '已支付',
          items: (o.items || []).map((item: any) => ({
            ...item,
            goodsImg: resolveUrl(item.goodsImg || item.coverImg || '')
          }))
        },
        refundAmount: total ? String(total) : '',
        shipped,
        refundType: shipped ? 2 : 1,
        loading: false,
        loadError: ''
      })
    } catch (e: any) {
      // 关键修复：失败时给出明确错误信息 + 允许重试，避免整页空白
      this.setData({
        loading: false,
        order: null,
        loadError: e?.message || '订单信息加载失败，请重试'
      })
    }
  },

  onChooseType(e: any) {
    this.setData({ refundType: Number(e.currentTarget.dataset.type) })
  },

  onAmountInput(e: any) {
    this.setData({ refundAmount: e.detail.value })
  },

  onChooseReason(e: any) {
    this.setData({ reason: e.currentTarget.dataset.reason })
  },

  onReasonInput(e: any) {
    this.setData({ reason: e.detail.value })
  },

  /** 重新加载订单信息（错误占位按钮回调） */
  onRetry() {
    this.loadOrder()
  },

  async onSubmit() {
    if (this.data.submitting) return
    const amount = parseFloat(this.data.refundAmount)
    if (!amount || amount <= 0) {
      wx.showToast({ title: '请输入退款金额', icon: 'none' })
      return
    }
    const max = this.data.order?.totalAmount || 0
    if (amount > max) {
      wx.showToast({ title: '不能超过订单金额', icon: 'none' })
      return
    }
    if (!this.data.reason) {
      wx.showToast({ title: '请选择退款原因', icon: 'none' })
      return
    }
    this.setData({ submitting: true })
    try {
      await applyRefund({
        orderId: this.data.orderId,
        refundType: this.data.refundType,
        refundAmount: amount,
        refundReason: this.data.reason
      })
      wx.showToast({ title: '退款成功', icon: 'success' })
      setTimeout(() => {
        wx.navigateTo({ url: '/pages/refund-list/refund-list' })
      }, 600)
    } catch (e) {
      // 错误已在 request 层 toast
    } finally {
      this.setData({ submitting: false })
    }
  }
})
