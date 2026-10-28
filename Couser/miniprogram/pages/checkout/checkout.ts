import { createOrder, payOrder } from '../../api/order'
import { calcFreight, payFreight, FreightDetail } from '../../api/freight'
import { getCart, updateCart, CartItem } from '../../utils/cart'
import { requireLogin } from '../../app'
import { getAddressList } from '../../api/address'
import { resolveUrl } from '../../utils/request'
import { getCheckoutItems, clearCheckoutItems } from '../../utils/checkoutItems'

/** 页面实例附加字段：延迟跳转定时器句柄，卸载时清理，避免对已销毁页面 navigateBack/redirectTo 告警 */
type CheckoutPageInstance = { backTimer?: ReturnType<typeof setTimeout> }

interface SelectedAddress {
  id: string
  name?: string
  mobile?: string
  fullAddress: string
}

Page({
  data: {
    items: [] as CartItem[],
    /** 商品金额 */
    total: 0,
    remark: '',
    address: null as SelectedAddress | null,
    submitting: false,
    /** 运费明细（后端 /freight/calc 预览计算，未落库） */
    freight: null as FreightDetail | null,
    /** 运费金额（数值，用于合计与运费是否待支付的判断） */
    freightAmount: 0,
    /** 运费展示文案：包邮 / ¥8.00 */
    freightText: '',
    /** 应付合计展示文案 = 商品金额 + 运费金额 */
    payableTotalText: ''
  },

  onLoad() {
    const rawItems = getCheckoutItems<CartItem>()
    if (!rawItems.length) {
      wx.showToast({ title: '没有结算商品', icon: 'none' })
      const self = this as CheckoutPageInstance
      if (self.backTimer) clearTimeout(self.backTimer)
      self.backTimer = setTimeout(() => wx.navigateBack(), 600)
      return
    }
    // 后端 coverImg 为相对地址（/api/file/xxx.png），<image> 必须拼上 BASE_URL 才能显示
    const items = rawItems.map(c => ({ ...c, coverImg: resolveUrl(c.coverImg) }))
    const total = items.reduce((s, c) => s + c.price * c.buyNum, 0)
    this.setData({ items, total, payableTotalText: '¥' + total.toFixed(2) })
    this.loadAddress()
    this.loadFreight()
  },

  /**
   * 加载运费明细：把结算商品交给后端按同一计费规则计算（预览，不落库）。
   * 失败时静默降级为「按商品金额计」，不阻塞下单。
   */
  async loadFreight() {
    if (!requireLogin()) return
    try {
      const freight = await calcFreight({
        items: this.data.items.map(c => ({
          goodsId: c.goodsId,
          skuId: c.skuId || undefined,
          buyNum: c.buyNum
        }))
      })
      const freightAmount = Number(freight.freightAmount || 0)
      this.setData({
        freight,
        freightAmount,
        freightText: freight.freightAmountText || ('¥' + freightAmount.toFixed(2)),
        payableTotalText: '¥' + (this.data.total + freightAmount).toFixed(2)
      })
    } catch (e) {
      // 运费计算失败不阻断下单，合计退回商品金额
      console.warn('[checkout loadFreight] failed', e)
      this.setData({
        freight: null,
        freightAmount: 0,
        freightText: '',
        payableTotalText: '¥' + this.data.total.toFixed(2)
      })
    }
  },

  async loadAddress() {
    if (!requireLogin()) return
    try {
      const list = await getAddressList()
      if (list && list.length) {
        const def = list.find(a => a.isDefault === 1) || list[0]
        const fullAddress = [def.province, def.city, def.district, def.detailAddr]
          .filter(Boolean)
          .join(' ')
        this.setData({
          address: {
            id: def.id,
            name: def.name,
            mobile: def.mobile,
            fullAddress
          }
        })
      }
    } catch (e) {
      // 错误已在 request 层 toast
    }
  },

  onRemark(e: any) {
    this.setData({ remark: e.detail })
  },

  goAddress() {
    wx.navigateTo({ url: '/pages/address/address?select=1' })
  },

  async onSubmit() {
    console.log('[checkout onSubmit] start', { submitting: this.data.submitting, hasToken: !!wx.getStorageSync('token'), address: this.data.address })
    if (this.data.submitting) return
    if (!requireLogin()) {
      console.log('[checkout onSubmit] blocked by requireLogin')
      return
    }
    if (!this.data.address) {
      console.log('[checkout onSubmit] blocked by empty address')
      wx.showToast({ title: '请先选择收货地址', icon: 'none' })
      return
    }
    // 下单前库存校验：缺货或超库存时禁止下单
    for (const c of this.data.items) {
      if (typeof c.stock === 'number' && c.stock <= 0) {
        wx.showToast({ title: '「' + c.goodsName + '」已售罄', icon: 'none' })
        return
      }
      if (typeof c.stock === 'number' && c.stock > 0 && c.buyNum > c.stock) {
        wx.showToast({ title: '「' + c.goodsName + '」库存不足', icon: 'none' })
        return
      }
    }
    this.setData({ submitting: true })
    console.log('[checkout onSubmit] ready to createOrder')
    try {
      const addr = this.data.address as SelectedAddress
      const res = await createOrder({
        remark: this.data.remark,
        receiverName: addr.name,
        receiverMobile: addr.mobile,
        address: addr.fullAddress,
        items: this.data.items.map(c => ({
          goodsId: c.goodsId,
          buyNum: c.buyNum,
          skuId: c.skuId || undefined
        }))
      })

      // 先创建订单，再尝试支付；若用户未支付或支付失败，订单保留为 status=1（待付款）
      let paid = false
      try {
        await payOrder(res.orderId)
        paid = true
      } catch (payErr) {
        console.warn('[checkout onSubmit] payOrder failed, order kept as status=1', payErr)
      }

      // 运费支付：订单创建成功后按订单口径落库并结算运费（幂等，已支付直接返回）。
      // 包邮订单也会生成运费单（freightAmount=0 / freeShipping=true），便于订单详情展示运费明细。
      let freightPaid = false
      if (this.data.freight) {
        try {
          const f = await payFreight({ orderId: res.orderId, payChannel: 1 })
          freightPaid = f.payStatus === 1
        } catch (freightErr) {
          console.warn('[checkout onSubmit] payFreight failed', freightErr)
        }
      }

      // 运费待支付：商品已支付但运费未支付（详情页会提供「支付运费」入口）
      const freightPending = this.data.freightAmount > 0 && !freightPaid

      // 订单已创建，移除购物车中已下单的商品并清空结算缓存
      const remained = getCart().filter(
        c => !this.data.items.some(it => it.goodsId === c.goodsId && (it.skuName || '') === (c.skuName || ''))
      )
      updateCart(remained)
      clearCheckoutItems()

      const self = this as CheckoutPageInstance
      if (self.backTimer) clearTimeout(self.backTimer)
      self.backTimer = setTimeout(() => {
        wx.redirectTo({ url: '/pages/order-detail/order-detail?id=' + res.orderId })
      }, 700)

      if (paid && freightPending) {
        wx.showToast({ title: '已支付，运费待支付', icon: 'none' })
      } else if (paid) {
        wx.showToast({ title: '支付成功', icon: 'success' })
      } else {
        wx.showToast({ title: '订单已保存，请前往待付款支付', icon: 'none' })
      }
    } catch (e) {
      console.log('[checkout onSubmit] createOrder error', e)
      this.setData({ submitting: false })
      // 失败提示已在 request 层兜底为"请求失败，请稍后重试"
      // 如后端返回更具体的业务错误，可在 Network 面板查看原始响应
    }
  },

  /** 取消订单：不调任何接口，直接返回上一页（购物车商品不会被清空） */
  onCancelCheckout() {
    const self = this as CheckoutPageInstance
    if (self.backTimer) clearTimeout(self.backTimer)
    self.backTimer = setTimeout(() => wx.navigateBack(), 200)
  },

  onUnload() {
    // 页面销毁时清理挂起的延迟跳转定时器，避免对已销毁页面调用 navigateBack/redirectTo 告警
    const self = this as CheckoutPageInstance
    if (self.backTimer) clearTimeout(self.backTimer)
  }
})
