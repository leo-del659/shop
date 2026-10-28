import { getHome, GoodsItem } from '../../api/shop'
import { addToCart, buildCartItem, getCartQuantity, incCartItem, decCartItem, syncCartBadge } from '../../utils/cart'
import { resolveUrl } from '../../utils/request'
import { requireLogin } from '../../app'
import { camelCaseKeys } from '../../utils/util'
import { scopedKey } from '../../utils/userScope'
import { setCheckoutItems } from '../../utils/checkoutItems'

const HISTORY_KEY_BASE = 'search_history'
const MAX_HISTORY = 10

/** 搜索历史按登录账号隔离（key = search_history__<手机号>） */
function historyKey(): string {
  return scopedKey(HISTORY_KEY_BASE)
}

Page({
  data: {
    keyword: '',
    searching: false,
    searchResults: [] as any[],
    hotList: [] as any[],
    newList: [] as any[],
    historyList: [] as string[],
    allGoods: [] as any[],
    loading: true,
    /** 搜索匹配到的子分类标签 */
    searchSubCats: [] as { id: string; name: string }[],
    activeSearchSubId: 'all',
    // SKU 弹窗（复用首页/详情页组件）
    showSku: false,
    skuGoods: null as any,
    skuAction: 'buy' as 'cart' | 'buy'
  },

  onLoad() {
    this.loadRanks()
    this.loadHistory()
  },

  onShow() {
    this.loadHistory()
    // 进入搜索页时，把搜索栏预填为最近一次搜索词
    const hist: string[] = this.data.historyList || wx.getStorageSync(historyKey()) || []
    if (hist.length && !this.data.searching) {
      this.setData({ keyword: hist[0] })
    }
    if (!this.data.loading) this.refreshQty()
    syncCartBadge()
  },

  /* ========== 搜索历史 ========== */

  loadHistory() {
    const list: string[] = wx.getStorageSync(historyKey()) || []
    this.setData({ historyList: list })
  },

  saveHistory(kw: string) {
    const clean = kw.trim()
    if (!clean) return
    let list: string[] = wx.getStorageSync(historyKey()) || []
    list = list.filter((item: string) => item !== clean)
    list.unshift(clean)
    if (list.length > MAX_HISTORY) list = list.slice(0, MAX_HISTORY)
    wx.setStorageSync(historyKey(), list)
    this.setData({ historyList: list })
  },

  clearHistory() {
    wx.removeStorageSync(historyKey())
    this.setData({ historyList: [] })
    wx.showToast({ title: '已清空', icon: 'none' })
  },

  onHistoryTap(e: any) {
    const kw = e.currentTarget.dataset.kw
    this.setData({ keyword: kw })
    this.doSearch(kw)
    this.saveHistory(kw)
  },

  /* ========== 榜单数据 ========== */

  async loadRanks() {
    this.setData({ loading: true })
    try {
      const raw = await getHome()
      // 兼容后端 PascalCase / camelCase（C# 实体默认 PascalCase）
      const tree = camelCaseKeys(raw) as any[]
      const items: any[] = []
      for (const c of tree) {
        const pName = c.categoryName
        if (c.goods && c.goods.length) {
          for (const g of c.goods) {
            items.push({ ...g, categoryName: pName, categoryPath: pName })
          }
        }
        for (const sub of c.children || []) {
          const sName = sub.categoryName
          if (sub.goods && sub.goods.length) {
            for (const g of sub.goods) {
              items.push({ ...g, categoryName: sName, categoryPath: pName + '/' + sName })
            }
          }
        }
      }

      const resolved = items.map(g => ({
        ...g,
        coverImg: resolveUrl(g.coverImg),
        _qty: getCartQuantity(g.id)
      }))

      const hot = resolved
        .slice()
        .sort((a: any, b: any) => (b.salePrice || b.price) - (a.salePrice || a.price))
        .slice(0, 10)

      const newest = resolved
        .slice()
        .sort((a: any, b: any) => {
          const ta = a.createTime ? new Date(a.createTime).getTime() : 0
          const tb = b.createTime ? new Date(b.createTime).getTime() : 0
          return tb - ta
        })
        .slice(0, 6)

      this.setData({ allGoods: resolved, hotList: hot, newList: newest, loading: false })
    } catch (e) {
      this.setData({ loading: false })
    }
  },

  refreshQty() {
    const inject = (list: any[]) => list.map(g => ({ ...g, _qty: getCartQuantity(g.id) }))
    this.setData({
      searchResults: inject(this.data.searchResults),
      hotList: inject(this.data.hotList),
      newList: inject(this.data.newList)
    })
  },

  /* ========== 搜索 ========== */

  onSearchInput(e: any) {
    this.setData({ keyword: e.detail.value })
  },

  onClear() {
    this.setData({ keyword: '', searching: false, searchResults: [] })
  },

  onSearch() {
    const kw = (this.data.keyword || '').trim()
    if (!kw) {
      this.setData({ searching: false, searchResults: [] })
      return
    }
    this.saveHistory(kw)
    this.doSearch(kw)
  },

  async doSearch(kw: string) {
    const k = kw.trim().toLowerCase()
    if (!k) {
      this.setData({ searching: false, searchResults: [], searchSubCats: [] })
      return
    }
    const matched = (this.data.allGoods as any[])
      .filter((g: any) => {
        const name = (g.goodsName || '').toLowerCase()
        const cat = (g.categoryName || '').toLowerCase()
        const path = (g.categoryPath || '').toLowerCase()
        return name.indexOf(k) >= 0 || cat.indexOf(k) >= 0 || path.indexOf(k) >= 0
      })

    // 提取匹配到的子分类（去重）
    const catMap: Record<string, string> = {}
    for (const g of matched) {
      const cn = g.categoryName
      if (cn && !catMap[cn]) catMap[cn] = cn
    }
    const subs: { id: string; name: string }[] = [{ id: 'all', name: '全部' }]
    for (const name of Object.values(catMap)) {
      if (name !== k) subs.push({ id: name, name })
    }

    const results = matched.map((g: any) => ({ ...g, _qty: getCartQuantity(g.id) }))
    this.setData({
      searching: true,
      searchResults: results,
      searchSubCats: subs,
      activeSearchSubId: 'all'
    })
  },

  /** 搜索结果中按子分类筛选 */
  onSearchSubTap(e: any) {
    const subId = e.currentTarget.dataset.id
    this.setData({ activeSearchSubId: subId })
    let filtered = this.data.searchResults as any[]
    if (subId !== 'all') {
      filtered = filtered.filter((g: any) => (g.categoryName || '') === subId)
    }
    this.setData({ searchResults: filtered.map((g: any) => ({ ...g, _qty: getCartQuantity(g.id) })) })
  },

  /* ========== 跳转 / 加购 / 加减 ========== */

  goDetail(e: any) {
    const id = e.detail?.id || e.currentTarget.dataset.id
    if (id) {
      wx.navigateTo({ url: '/pages/goods-detail/goods-detail?id=' + id })
    }
  },

  onAddToCart(e: any) {
    if (!requireLogin()) return
    const g = e.currentTarget.dataset?.goods || e.detail?.goods as GoodsItem
    if (!g) return
    addToCart(buildCartItem(g))
    this.refreshQty()
    wx.showToast({ title: '已加入购物车', icon: 'none' })
  },

  /** 榜单条目 +/- 数量 */
  onRankInc(e: any) {
    if (!requireLogin()) return
    const goodsId = e.currentTarget.dataset.goodsId
    incCartItem(goodsId)
    this.refreshQty()
  },

  onRankDec(e: any) {
    const goodsId = e.currentTarget.dataset.goodsId
    decCartItem(goodsId)
    this.refreshQty()
  },

  noop() {},

  /** 打开 SKU 选择弹窗（搜索结果页的 goods-card 使用，默认加购） */
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

  /** 弹窗确定回调：加购 / 购买 */
  onSkuConfirm(e: any) {
    const { action, goods, skuId, skuName, price, qty, coverImg } = e.detail
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
    incCartItem(e.detail.goodsId)
    this.refreshQty()
  },

  onDecCart(e: any) {
    decCartItem(e.detail.goodsId)
    this.refreshQty()
  }
})
