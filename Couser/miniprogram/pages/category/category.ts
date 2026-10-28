import { getHome, GoodsItem } from '../../api/shop'
import { getCartQuantity, incCartItem, decCartItem, syncCartBadge } from '../../utils/cart'
import { resolveCategoryTree, DisplayCategory, normalizeDisplayCategories } from '../../utils/tree'
import { requireLogin } from '../../app'
import { camelCaseKeys } from '../../utils/util'
import { resolveUrl } from '../../utils/request'
import { setCheckoutItems } from '../../utils/checkoutItems'

Page({
  data: {
    categories: [] as DisplayCategory[],
    activeId: '',
    activeCategoryName: '',
    subCats: [] as { id: string; name: string }[],
    activeSubId: 'all',
    goods: [] as (GoodsItem & { _qty?: number })[],
    loading: true,
    // SKU 弹窗（复用首页/详情页组件）
    showSku: false,
    skuGoods: null as any,
    skuAction: 'buy' as 'cart' | 'buy'
  },

  onLoad(options: any) {
    this.loadCats(options.categoryId || '')
  },

  async loadCats(preselectId?: string) {
    try {
      const raw = await getHome()
      // 兼容后端 PascalCase / camelCase（C# 实体默认 PascalCase）
      const categories = normalizeDisplayCategories(resolveCategoryTree(camelCaseKeys(raw)))
      let { activeId, activeSubId } = this.resolveActive(categories, preselectId)
      this.setData({
        categories,
        activeId,
        activeSubId,
        loading: false
      })
      this.switchCategory(activeId, activeSubId)
    } catch (e) {
      this.setData({ loading: false })
    }
  },

  /**
   * 解析应该激活哪个展示分类。
   * 首页宫格传入的是展示名（如「水果」）；旧入口可能传入后端子分类 id（如 cat00101），
   * 这里统一找到对应的 DisplayCategory 并预选该子类。
   */
  resolveActive(categories: DisplayCategory[], preselectId?: string) {
    let activeId = categories[0] ? categories[0].id : ''
    let activeSubId = 'all'
    if (!preselectId || !categories.length) return { activeId, activeSubId }

    // 1) 优先按展示名匹配
    const byName = categories.find(c => c.id === preselectId || c.categoryName === preselectId)
    if (byName) {
      activeId = byName.id
      return { activeId, activeSubId }
    }

    // 2) 按后端子分类 id 匹配：找到包含该子分类的展示分类并预选它
    for (const c of categories) {
      if (c.sourceIds.includes(preselectId)) {
        activeId = c.id
        activeSubId = preselectId
        break
      }
      const sub = (c.children || []).find(s => s.id === preselectId)
      if (sub) {
        activeId = c.id
        activeSubId = preselectId
        break
      }
    }
    return { activeId, activeSubId }
  },

  onSelect(e: any) {
    const id = e.currentTarget.dataset.id
    if (id === this.data.activeId) return
    this.setData({ activeId: id, activeSubId: 'all' })
    this.switchCategory(id, 'all')
  },

  switchCategory(catId: string, preSubId: string = 'all') {
    const cat = this.data.categories.find(c => c.id === catId)
    if (!cat) return

    const subs: { id: string; name: string }[] = [{ id: 'all', name: '全部' }]
    if (cat.children && cat.children.length) {
      for (const s of cat.children) {
        subs.push({ id: s.id, name: s.categoryName })
      }
    }

    let goods: GoodsItem[]
    if (preSubId === 'all') {
      goods = cat.goods || []
    } else {
      const sub = (cat.children || []).find(s => s.id === preSubId)
      goods = (sub && sub.goods) ? sub.goods : []
    }
    this.setData({
      subCats: subs,
      goods: goods.map(g => ({ ...g, _qty: getCartQuantity(g.id) })),
      activeCategoryName: cat.categoryName,
      activeSubId: preSubId
    })
  },

  onSubSelect(e: any) {
    const subId = e.currentTarget.dataset.id
    this.setData({ activeSubId: subId })

    const cat = this.data.categories.find(c => c.id === this.data.activeId)
    if (!cat) return

    if (subId === 'all') {
      this.setData({ goods: (cat.goods || []).map(g => ({ ...g, _qty: getCartQuantity(g.id) })) })
    } else {
      const sub = (cat.children || []).find(c => c.id === subId)
      this.setData({ goods: (sub && sub.goods ? sub.goods : []).map(g => ({ ...g, _qty: getCartQuantity(g.id) })) })
    }
  },

  onShow() {
    if (!this.data.loading && this.data.goods.length) this.refreshQty()
    syncCartBadge()
  },

  refreshQty() {
    this.setData({ goods: this.data.goods.map(g => ({ ...g, _qty: getCartQuantity(g.id) })) })
  },

  goDetail(e: any) {
    const id = e.detail.id
    wx.navigateTo({ url: '/pages/goods-detail/goods-detail?id=' + id })
  },

  /** 打开 SKU 选择弹窗 */
  onOpenSku(e: any) {
    if (!requireLogin()) return
    const g = e.detail.goods as GoodsItem
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

  /** 弹窗确定回调：立即购买 */
  onSkuConfirm(e: any) {
    const { goods, skuId, skuName, price, qty, coverImg } = e.detail
    if (!requireLogin()) return
    const item = {
      goodsId: goods.id,
      goodsName: goods.goodsName,
      coverImg,
      price: this.toNum(price),
      buyNum: qty,
      skuId,
      skuName,
      specName: ''
    }
    setCheckoutItems([item])
    this.setData({ showSku: false })
    wx.navigateTo({ url: '/pages/checkout/checkout' })
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
