import { getCart, updateCart, CartItem, syncCartBadge } from '../../utils/cart'
import { getGoodsList, GoodsItem } from '../../api/shop'
import { resolveUrl } from '../../utils/request'
import { requireLogin } from '../../app'

Page({
  data: {
    items: [] as CartItem[],
    total: 0,
    count: 0,
    recommend: [] as GoodsItem[]
  },

  onShow() {
    this.refresh()
    this.loadRecommend()
    syncCartBadge()
  },

  refresh() {
    const items = getCart()
    const total = items.reduce((s, c) => s + c.price * c.buyNum, 0)
    const count = items.reduce((s, c) => s + c.buyNum, 0)
    this.setData({ items, total, count })
  },

  onStep(e: any) {
    const id = e.currentTarget.dataset.id
    const sku = e.currentTarget.dataset.sku || ''
    let num = e.detail
    // 不能超过该商品/规格的最大库存
    const item = getCart().find(c => c.goodsId === id && (c.skuName || '') === sku)
    const stock = item?.stock
    if (stock !== undefined && stock > 0 && num > stock) {
      num = stock
      wx.showToast({ title: '已达最大库存', icon: 'none' })
    }
    if (num <= 0) {
      // 减到 0 直接删除
      const items = getCart().filter(
        c => !(c.goodsId === id && (c.skuName || '') === sku)
      )
      updateCart(items)
    } else {
      const items = getCart().map(c =>
        c.goodsId === id && (c.skuName || '') === sku ? { ...c, buyNum: num } : c
      )
      updateCart(items)
    }
    this.refresh()
  },

  onDelete(e: any) {
    const id = e.currentTarget.dataset.id
    const sku = e.currentTarget.dataset.sku || ''
    const items = getCart().filter(
      c => !(c.goodsId === id && (c.skuName || '') === sku)
    )
    updateCart(items)
    this.refresh()
  },

  goCheckout() {
    if (!this.data.items.length) {
      wx.showToast({ title: '购物车为空', icon: 'none' })
      return
    }
    if (!requireLogin()) return
    wx.setStorageSync('checkout_items', this.data.items)
    wx.navigateTo({ url: '/pages/checkout/checkout' })
  },

  goShop() {
    wx.switchTab({ url: '/pages/home/home' })
  },

  /** 加载为你推荐 */
  async loadRecommend() {
    try {
      const res = await getGoodsList({ page: 1, size: 6 })
      const list = (res.list || []).slice(0, 6).map(g => ({
        ...g,
        coverImg: resolveUrl(g.coverImg)
      }))
      this.setData({ recommend: list })
    } catch (e) {
      // 推荐失败不影响购物车主功能
    }
  },

  /** 推荐商品跳转详情 */
  goDetail(e: any) {
    const id = e.currentTarget.dataset.id
    wx.navigateTo({ url: '/pages/goods-detail/goods-detail?id=' + id })
  }
})
