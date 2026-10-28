import {
  getOrderDetail,
  cancelOrder,
  payOrder,
  confirmReceipt,
  deleteOrder,
  ORDER_STATUS_TEXT,
  OrderSummary
} from '../../api/order'
import { applyRefund } from '../../api/refund'
import { getFreight, payFreight, FreightDetail } from '../../api/freight'
import { getOrderReviewed } from '../../api/review'
import { requireLogin } from '../../app'
import { resolveUrl } from '../../utils/request'

type DetailOrder = OrderSummary & { statusText: string }

/** 顶部单一状态文案：未付款→待付款 / 已付款→待发货 / 已发货→运送中 / 已完成→隐藏 / 退款/取消→已退款 */
function topStatusText(status: number, paid: boolean): { text: string; hide: boolean } {
  // 已取消/已退款统一显示为「已退款」（退款成功后订单状态置 9）
  if (status === 9) return { text: ORDER_STATUS_TEXT[9], hide: false }
  // 商户确认已完成（4/6）：隐藏订单状态
  if (status === 4 || status === 6) return { text: '', hide: true }
  if (status === 3) return { text: '运送中', hide: false }
  if (status === 2) return { text: '待发货', hide: false }
  // status === 1
  if (paid) return { text: '待发货', hide: false }
  return { text: '待付款', hide: false }
}

/** 手机号脱敏：138****1234 */
function maskMobile(m?: string): string {
  if (!m) return ''
  const s = String(m)
  if (s.length === 11) return s.slice(0, 3) + '****' + s.slice(7)
  if (s.length >= 7) return s.slice(0, 3) + '****' + s.slice(-2)
  return s
}

/** 时间格式化：yyyy-MM-dd HH:mm */
function fmtTime(t?: string): string {
  if (!t) return ''
  const d = new Date(t.replace(' ', 'T'))
  if (isNaN(d.getTime())) return t
  const p = (n: number) => (n < 10 ? '0' : '') + n
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())} ${p(d.getHours())}:${p(d.getMinutes())}`
}

interface InfoRow { label: string; value: string }

Page({
  data: {
    id: '',
    order: null as DetailOrder | null,
    loading: true,
    /** 已评价过的商品 key：goodsId_skuId -> true */
    reviewed: {} as Record<string, boolean>,
    /** 已评价列表是否已加载（幂等标志，避免返回时重复请求/重绘引起页面闪烁） */
    reviewedLoaded: false,
    /** 顶部状态：文案 + 是否隐藏 */
    topStatus: { text: '', hide: true } as { text: string; hide: boolean },
    /** 是否已取消（状态置灰） */
    cancelled: false,
    /** 是否已支付 */
    paid: false,
    /** 订单信息行（收货人 / 电话 / 地址 / 订单号 / 时间等） */
    infoRows: [] as InfoRow[],
    /** 是否可删除（非进行中可删：默认允许，已发货/待收货可保留删除入口由业务决定） */
    canDelete: true,
    /** 运费明细（运费金额 / 计费方式 / 生效时间 / 支付状态） */
    freight: null as FreightDetail | null,
    /** 运费生效时间（格式化） */
    freightEffectiveTime: '',
    /** 运费支付时间（格式化） */
    freightPayTime: '',
    /** 运费待支付（有运费且未支付时展示「支付运费」入口） */
    freightPending: false
  },

  onLoad(options: any) {
    this.setData({ id: options.id })
    this.loadDetail()
  },

  onShow() {
    if (!this.data.order) return
    if (this.data.order.orderStatus === 4 && !this.data.reviewedLoaded) {
      this.loadReviewed()
    }
  },

  async loadDetail() {
    if (!requireLogin()) return
    const o = await getOrderDetail(this.data.id)
    const status = o.orderStatus
    const paid = o.payStatus === 1
    const displayStatus = paid && status === 1 ? 2 : status
    const order = {
      ...o,
      statusText: ORDER_STATUS_TEXT[displayStatus] || '未知',
      items: (o.items || []).map((it: any) => ({
        ...it,
        goodsImg: resolveUrl(it.goodsImg)
      }))
    }

    // 顶部单一状态
    const topStatus = topStatusText(status, paid)

    // 订单信息行（按需展示，空值不显示）
    const infoRows: InfoRow[] = []
    if (o.receiverName || o.receiverMobile) {
      const phone = o.receiverMobile ? '  ' + maskMobile(o.receiverMobile) : ''
      infoRows.push({ label: '收货人', value: (o.receiverName || '') + phone })
    }
    if (o.address) infoRows.push({ label: '收货地址', value: o.address })
    if (o.orderNo) infoRows.push({ label: '订单编号', value: o.orderNo })
    infoRows.push({ label: '创建时间', value: fmtTime(o.createTime) })
    if (paid) infoRows.push({ label: '付款时间', value: fmtTime(o.payTime) })
    if (o.shipTime) infoRows.push({ label: '发货时间', value: fmtTime(o.shipTime) })
    if (o.completeTime) infoRows.push({ label: '成交时间', value: fmtTime(o.completeTime) })

    this.setData({
      order,
      topStatus,
      cancelled: status === 9,
      paid,
      infoRows,
      loading: false
    })
    if (order.orderStatus === 4) this.loadReviewed()
    this.loadFreight()
  },

  /** 加载运费明细：无运费单时后端会按订单当前参数实时报价 */
  async loadFreight() {
    try {
      const f = await getFreight(this.data.id)
      this.setData({
        freight: f,
        freightEffectiveTime: fmtTime(f.effectiveTime || ''),
        freightPayTime: fmtTime(f.payTime || ''),
        freightPending: f.payStatus !== 1 && Number(f.freightAmount || 0) > 0
      })
    } catch (e) {
      // 运费明细加载失败不影响订单主流程
      console.warn('[order-detail loadFreight] failed', e)
      this.setData({ freight: null, freightPending: false })
    }
  },

  /** 支付运费：提交即完成（后端幂等，已支付直接返回） */
  onPayFreight() {
    const f = this.data.freight
    if (!f) return
    wx.showModal({
      title: '支付运费',
      content: '需支付运费 ' + (f.freightAmountText || '') + '，确认支付？',
      confirmText: '支付',
      success: async (res) => {
        if (!res.confirm) return
        wx.showLoading({ title: '支付中...', mask: true })
        try {
          await payFreight({ orderId: this.data.id, payChannel: 1 })
          wx.hideLoading()
          wx.showToast({ title: '运费支付成功', icon: 'success' })
          this.loadFreight()
        } catch (e: any) {
          wx.hideLoading()
          wx.showToast({ title: e?.message || '运费支付失败', icon: 'none' })
        }
      }
    })
  },

  async loadReviewed() {
    if (this.data.reviewedLoaded) return
    try {
      const list = await getOrderReviewed(this.data.id)
      const reviewed: Record<string, boolean> = {}
      for (const r of list || []) {
        reviewed[r.goodsId + '_' + (r.skuId || '')] = true
      }
      const merged = { ...reviewed, ...(this.data.reviewed || {}) }
      this.setData({ reviewed: merged, reviewedLoaded: true })
    } catch (e) {
      // 忽略
    }
  },

  onReview(e: any) {
    const item = e.currentTarget.dataset.item
    if (!item) return
    const order = this.data.order
    const params = [
      'orderId=' + this.data.id,
      'orderNo=' + encodeURIComponent(order?.orderNo || ''),
      'goodsId=' + item.goodsId,
      'skuId=' + (item.skuId || ''),
      'goodsName=' + encodeURIComponent(item.goodsName || ''),
      'goodsImg=' + encodeURIComponent(item.goodsImg || ''),
      'skuName=' + encodeURIComponent(item.skuName || '')
    ].join('&')
    wx.navigateTo({ url: '/pages/review/review?' + params })
  },

  async onPay() {
    if (this.data.paid) {
      wx.showToast({ title: '订单已支付，等待商家接单', icon: 'none' })
      return
    }
    wx.showLoading({ title: '支付中...', mask: true })
    try {
      await payOrder(this.data.id)
      wx.hideLoading()
      wx.showToast({ title: '支付成功', icon: 'success' })
      this.loadDetail()
    } catch (e) {
      wx.hideLoading()
    }
  },

  onCancel() {
    wx.showModal({
      title: '提示',
      content: '确定取消该订单？',
      success: async (res) => {
        if (!res.confirm) return
        try {
          await cancelOrder(this.data.id, '用户取消')
          wx.showToast({ title: '已取消', icon: 'success' })
          this.loadDetail()
        } catch (e) {
        }
      }
    })
  },

  onConfirmReceipt() {
    wx.showModal({
      title: '确认收货',
      content: '确认已收到商品？确认后订单将标记为已完成。',
      success: async (res) => {
        if (!res.confirm) return
        try {
          await confirmReceipt(this.data.id)
          wx.showToast({ title: '确认收货成功', icon: 'success' })
          this.loadDetail()
        } catch (e) {
        }
      }
    })
  },

  /** 联系商家：跳转到商品所属商户的聊天界面 */
  onContactMerchant() {
    const order = this.data.order
    if (!order) return
    const merchantId = order.merchantId || order.sellerMerchantId
    if (!merchantId) {
      wx.showToast({ title: '未找到所属商家', icon: 'none' })
      return
    }
    const params = [
      'merchantId=' + merchantId,
      'name=' + encodeURIComponent(order.merchantName || '')
    ].join('&')
    wx.navigateTo({ url: '/pages/chat/chat?' + params })
  },

  /** 删除订单：二次确认后逻辑删除并退出详情页 */
  onDelete() {
    wx.showModal({
      title: '删除订单',
      content: '删除后该订单将不再显示，确定删除？',
      confirmText: '删除',
      confirmColor: '#ff4d4f',
      success: async (res) => {
        if (!res.confirm) return
        wx.showLoading({ title: '删除中...', mask: true })
        try {
          await deleteOrder(this.data.id)
          wx.hideLoading()
          wx.showToast({ title: '已删除', icon: 'success' })
          setTimeout(() => wx.navigateBack(), 600)
        } catch (e) {
          wx.hideLoading()
        }
      }
    })
  },

  /** 退款：待发货订单直接退款；已发货/已完成仍走申请页（退货退款需要物流） */
  onApplyRefund() {
    const order = this.data.order
    if (!order) return
    const status = order.orderStatus
    const paid = order.payStatus === 1
    const canDirectRefund = (status === 1 && paid) || status === 2

    if (!canDirectRefund) {
      wx.navigateTo({ url: '/pages/refund-apply/refund-apply?id=' + this.data.id })
      return
    }

    wx.showModal({
      title: '确认退款',
      content: '确定申请退款？款项将原路退回。',
      confirmText: '退款',
      confirmColor: '#ff4d4f',
      success: async (res) => {
        if (!res.confirm) return
        wx.showLoading({ title: '退款中...', mask: true })
        try {
          await applyRefund({
            orderId: this.data.id,
            refundType: 1,
            refundAmount: order.totalAmount,
            refundReason: '待发货直接退款'
          })
          wx.hideLoading()
          wx.showToast({ title: '退款成功', icon: 'success' })
          this.loadDetail()
        } catch (e: any) {
          wx.hideLoading()
          wx.showToast({ title: e?.message || '退款失败', icon: 'none' })
        }
      }
    })
  }
})
