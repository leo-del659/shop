import { getAppToken, requireLogin, clearAppToken } from '../../app'
import { getClientProfile } from '../../api/auth'
import { getHome, GoodsItem } from '../../api/shop'
import { resolveUrl } from '../../utils/request'
import { resolveCategoryTree } from '../../utils/tree'
import { camelCaseKeys } from '../../utils/util'
import { syncCartBadge, getCartQuantity, addToCart, incCartItem, decCartItem } from '../../utils/cart'
import { favoriteCount } from '../../utils/favorite'
import { footprintCount } from '../../utils/footprint'
import { getOrders, ORDER_STATUS } from '../../api/order'

Page({
  data: {
    logged: false,
    mobile: '',
    name: '',
    avatarUrl: '',
    hasAddress: false,
    // 资质图片：raw 为后端原始串（逗号分隔相对URL），qualifications 为展示用完整URL
    qualificationRaw: '',
    qualifications: [] as string[],
    favCount: 0,
    footCount: 0,
    waitPayCount: 0,
    recommend: [] as (GoodsItem & { _qty?: number })[],
    // SKU 弹窗状态（加购复用）
    showSku: false,
    skuGoods: null as any,
    skuAction: 'cart' as 'cart' | 'buy'
  },

  onShow() {
    syncCartBadge()
    this.setData({ favCount: favoriteCount(), footCount: footprintCount() })
    const token = getAppToken()
    // 消费者登录：登录手机号在登录时缓存到 loginMobile
    const account = wx.getStorageSync('loginMobile') || ''
    if (token) {
      getClientProfile()
        .then((p) => {
          this.setData({
            logged: true,
            // 顶部「用户」一栏靠显示名：优先用用户名称，没有就用登录手机号
            mobile: p.account || account,
            name: p.name || p.account || account,
            avatarUrl: p.avatar ? resolveUrl(p.avatar) : ''
          })
          this.applyQualification('')
        })
        .catch(() => {
          // 接口失败（网络/401 等）不强行影响主流程：至少把已登录态展示出来
          this.setData({ logged: true, mobile: account })
        })
    } else {
      this.setData({
        logged: false,
        mobile: '',
        name: '',
        avatarUrl: '',
        qualificationRaw: '',
        qualifications: [],
        waitPayCount: 0
      })
    }
    this.loadRecommend()
    this.loadOrderCount()
  },

  /** 加载待支付订单数量，用于「我的订单-待支付」角标 */
  async loadOrderCount() {
    if (!getAppToken()) return
    try {
      const list = await getOrders({ status: ORDER_STATUS.WAIT_PAY, page: 1, pageSize: 999 })
      this.setData({ waitPayCount: (list || []).length })
    } catch (e) {
      // 失败不阻塞主流程
    }
  },

  /** 把后端原始资质串（逗号分隔相对URL）拆成展示用完整URL数组 */
  applyQualification(raw?: string) {
    const list = (raw || '').split(',').map(s => s.trim()).filter(Boolean)
    this.setData({
      qualificationRaw: list.join(','),
      qualifications: list.map(u => resolveUrl(u))
    })
  },

  /** 加载「猜你喜欢」：取商城分类树下的全部商品，随机取前 6 个 */
  async loadRecommend() {
    try {
      const raw = await getHome()
      const categories = resolveCategoryTree(camelCaseKeys(raw) as any[])
      const list: (GoodsItem & { _qty?: number })[] = []
      for (const c of categories) {
        if (c.goods && c.goods.length) list.push(...c.goods)
        for (const sub of c.children || []) {
          if (sub.goods && sub.goods.length) list.push(...sub.goods)
        }
      }
      this.setData({ recommend: list.slice(0, 6).map(g => ({ ...g, _qty: getCartQuantity(g.id) })) })
    } catch (e) {
      // 失败不影响主流程，留空即可
    }
  },

  /** 上传资质图片（消费者端不提供资质能力，门控提示；原商户逻辑保留备查） */
  async onUploadQualification() {
    wx.showToast({ title: '消费者账号无需上传资质', icon: 'none' })
  },

  /** 点击缩略图预览大图 */
  onPreviewQualification(e: any) {
    const src = e.currentTarget.dataset.src
    wx.previewImage({ urls: this.data.qualifications, current: src })
  },

  /** 删除某张资质（消费者端不提供资质能力，门控提示；原商户逻辑保留备查） */
  async onDeleteQualification() {
    wx.showToast({ title: '消费者账号无需上传资质', icon: 'none' })
  },

  goLogin() {
    wx.navigateTo({ url: '/pages/login/login' })
  },

  /** 退出登录（头部右侧按钮，带二次确认）；profile 编辑页已下线，退出入口移到这里 */
  onLogout() {
    wx.showModal({
      title: '提示',
      content: '确定要退出当前账号吗？',
      confirmText: '退出',
      confirmColor: '#fa3534',
      cancelText: '取消',
      success: (r) => {
        if (!r.confirm) return
        clearAppToken()
        wx.removeStorageSync('loginMobile')
        this.setData({ logged: false, mobile: '', name: '', avatarUrl: '' })
        wx.showToast({ title: '已退出登录', icon: 'none' })
      }
    })
  },

  goProfile() {
    wx.navigateTo({ url: '/pages/profile/profile' })
  },

  goOrders(e?: any) {
    const status = e?.currentTarget?.dataset?.status ?? 0
    wx.navigateTo({ url: '/pages/order-list/order-list?status=' + status })
  },

  goRefunds() {
    wx.navigateTo({ url: '/pages/refund-list/refund-list' })
  },

  goAddress() {
    wx.navigateTo({ url: '/pages/address/address' })
  },

  goPoints() {
    wx.showToast({ title: '积分功能即将上线', icon: 'none' })
  },

  goCoupons() {
    wx.showToast({ title: '优惠券功能即将上线', icon: 'none' })
  },

  goFavorites() {
    wx.navigateTo({ url: '/pages/favorite/favorite' })
  },

  goFootprint() {
    wx.navigateTo({ url: '/pages/footprint/footprint' })
  },

  goDetail(e: any) {
    const id = e.detail.id
    wx.navigateTo({ url: '/pages/goods-detail/goods-detail?id=' + id })
  },

  /** 打开 SKU 选择弹窗（点击加购触发） */
  onOpenSku(e: any) {
    if (!requireLogin()) return
    const g = e.detail.goods as GoodsItem
    const coverImg = (g.coverImg || '').split(',')[0].trim()
    this.setData({
      showSku: true,
      skuGoods: { ...g, coverImg: resolveUrl(coverImg) },
      skuAction: 'cart'
    })
  },

  closeSku() {
    this.setData({ showSku: false })
  },

  /** 弹窗确定回调 */
  onSkuConfirm(e: any) {
    const { action, goods, skuId, skuName, price, qty, stock, coverImg } = e.detail
    if (!requireLogin()) return
    const item = {
      goodsId: goods.id,
      goodsName: goods.goodsName,
      coverImg,
      price: this.toNum(price),
      buyNum: qty,
      skuId,
      skuName,
      specName: '',
      stock
    }
    if (action === 'cart') {
      addToCart(item)
      this.setData({ showSku: false })
      this.refreshQty()
      wx.showToast({ title: '已加入购物车', icon: 'success' })
    } else {
      wx.setStorageSync('checkout_items', [item])
      this.setData({ showSku: false })
      wx.navigateTo({ url: '/pages/checkout/checkout' })
    }
  },

  refreshQty() {
    this.setData({ recommend: this.data.recommend.map(g => ({ ...g, _qty: getCartQuantity(g.id) })) })
  },

  toNum(val: any): number {
    if (typeof val === 'number') return val
    const s = String(val || '')
    const n = parseFloat(s.replace(/[^\d.]/g, ''))
    return isNaN(n) ? 0 : n
  },

  onIncCart(e: any) {
    if (!requireLogin()) return
    incCartItem(e.detail.goodsId)
    this.refreshQty()
  },

  onDecCart(e: any) {
    decCartItem(e.detail.goodsId)
    this.refreshQty()
  }
})
