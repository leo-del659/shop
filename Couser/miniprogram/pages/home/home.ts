import { getHome, GoodsItem } from '../../api/shop'
import { addToCart, getCartQuantity, incCartItem, decCartItem, syncCartBadge } from '../../utils/cart'
import { resolveCategoryTree, DisplayCategory, normalizeDisplayCategories } from '../../utils/tree'
import { requireLogin } from '../../app'
import { camelCaseKeys } from '../../utils/util'
import { resolveUrl } from '../../utils/request'
import { setCheckoutItems } from '../../utils/checkoutItems'

Page({
  data: {
    tabs: [] as DisplayCategory[],
    allGoods: [] as (GoodsItem & { _qty?: number })[],
    loading: true,
    // 首页公告
    notice: '集邻生活 · 新鲜直达 · 满 99 减 10 包邮',
    // SKU 弹窗状态（弹窗逻辑已抽到 components/sku-popup）
    showSku: false,
    skuGoods: null as any,
    skuAction: 'buy' as 'cart' | 'buy'
  },

  onLoad() {
    this.loadHome()
  },

  async onPullDownRefresh() {
    await this.loadHome()
    wx.stopPullDownRefresh()
  },

  /** 刷新购物车数量角标 */
  refreshQty() {
    const allQty = this.data.allGoods.map(g => ({ ...g, _qty: getCartQuantity(g.id) }))
    this.setData({ allGoods: allQty })
  },

  async loadHome() {
    try {
      const raw = await getHome()
      // 兼容后端 PascalCase / camelCase（C# 实体默认 PascalCase）
      const data = camelCaseKeys(raw) as any[]
      const categories = resolveCategoryTree(data)
      const allGoods: GoodsItem[] = []
      for (const c of categories) {
        if (c.goods && c.goods.length) allGoods.push(...c.goods)
        for (const sub of c.children || []) {
          if (sub.goods && sub.goods.length) allGoods.push(...sub.goods)
        }
      }
      // 注入购物车数量
      const withQty = allGoods.map(g => ({ ...g, _qty: getCartQuantity(g.id) }))
      // 整理首页宫格：拆分组合类目 + 注入真实图标
      const tabs = normalizeDisplayCategories(categories)
      this.setData({
        tabs,
        allGoods: withQty,
        loading: false
      })
    } catch (e) {
      this.setData({ loading: false })
    }
  },

  onShow() {
    // 每次显示时刷新购物车数量（从其他页面返回可能已变化）
    if (!this.data.loading) this.refreshQty()
    syncCartBadge()
  },

  goSearch() {
    wx.navigateTo({ url: '/pages/goods-list/goods-list' })
  },

  onTab(e: any) {
    const id = e.currentTarget.dataset.id
    wx.reLaunch({ url: '/pages/category/category?categoryId=' + id })
  },

  goDetail(e: any) {
    const id = e.detail.id
    wx.navigateTo({ url: '/pages/goods-detail/goods-detail?id=' + id })
  },

  /** 打开 SKU 选择弹窗（点击立即加购触发） */
  onOpenSku(e: any) {
    if (!requireLogin()) return
    const g = e.detail.goods as GoodsItem
    // 解析 coverImg 取第一张图作为弹窗展示图
    const coverImg = (g.coverImg || '').split(',')[0].trim()
    this.setData({
      showSku: true,
      skuGoods: { ...g, coverImg: resolveUrl(coverImg) },
      skuAction: 'buy'
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
      setCheckoutItems([item])
      this.setData({ showSku: false })
      wx.navigateTo({ url: '/pages/checkout/checkout' })
    }
  },

  toNum(val: any): number {
    if (typeof val === 'number') return val
    const s = String(val || '')
    const n = parseFloat(s.replace(/[^\d.]/g, ''))
    return isNaN(n) ? 0 : n
  },

  onIncCart(e: any) {
    if (!requireLogin()) return
    const goodsId = e.detail.goodsId
    incCartItem(goodsId)
    this.refreshQty()
  },

  onDecCart(e: any) {
    const goodsId = e.detail.goodsId
    decCartItem(goodsId)
    this.refreshQty()
  }
})
