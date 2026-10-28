import { getGoodsDetail } from '../../api/shop'
import { getGoodsReviews } from '../../api/review'
import { addToCart, CartItem, cartCount } from '../../utils/cart'
import { requireLogin } from '../../app'
import { resolveUrl } from '../../utils/request'
import { camelCaseKeys, formatSoldCount } from '../../utils/util'
import { toggleFavorite, isFavorite } from '../../utils/favorite'
import { addFootprint } from '../../utils/footprint'
import { setCheckoutItems } from '../../utils/checkoutItems'

/** 评价筛选：档位(good/mid/bad) 或 标签(tag) 二选一
 * 标签集合必须与 pages/review/review.ts 的 TAG_OPTIONS 保持一致，
 * 否则顾客筛不到自己刚提的评价。
 */
const REVIEW_FILTERS = [
  { label: '全部', tier: '', tag: '' },
  { label: '好评', tier: 'good', tag: '' },
  { label: '中评', tier: 'mid', tag: '' },
  { label: '差评', tier: 'bad', tag: '' },
  { label: '品质好', tier: '', tag: '品质好' },
  { label: '品质一般', tier: '', tag: '品质一般' },
  { label: '服务好', tier: '', tag: '服务好' },
  { label: '发货快', tier: '', tag: '发货快' },
  { label: '态度差', tier: '', tag: '态度差' },
  { label: '发货慢', tier: '', tag: '发货慢' }
] as { label: string; tier: string; tag: string }[]

Page({
  data: {
    id: '',
    good: null as any,
    swiperImages: [] as string[],
    // SKU 弹窗逻辑已抽到 components/sku-popup
    showSku: false,
    action: 'buy' as 'cart' | 'buy',
    selectedSkuName: '',
    displayPrice: '0.0',
    soldText: '0',
    loading: true,
    cartCount: 0,
    favorite: false,
    reviews: [] as any[],
    reviewTotal: 0,
    reviewFilters: REVIEW_FILTERS,
    activeReviewFilter: 0,
    /** 评价加载失败时的错误信息；非空时 UI 显示「重试」按钮 */
    reviewError: '' as string,
    /** 评价加载状态：可观察 UI 加载/重试/已完成 */
    reviewLoading: false
  },

  onLoad(options: any) {
    // 合并到一次 setData：避免 onLoad 阶段连续两次 setData 与 first render 抢线程
    // （曾触发 [渲染层错误] Expected updated data but get first rendering data）
    this.setData({
      id: options.id,
      favorite: isFavorite(options.id)
    })
    this.loadDetail()
  },

  onShow() {
    // 推迟 1 帧再 setData，让 page first render 先就绪
    // 解决 onShow 内 setData 与路由切换时序冲突导致的 routeDone 报错
    setTimeout(() => this.setCartCount(), 0)
  },

  setCartCount() {
    this.setData({ cartCount: cartCount() })
  },

  /** 强制提取纯数字价格 */
  toNum(val: any): number {
    if (typeof val === 'number') return val
    const s = String(val || '')
    const n = parseFloat(s.replace(/[^\d.]/g, ''))
    return isNaN(n) ? 0 : n
  },

  async loadDetail() {
    try {
      const raw = await getGoodsDetail(this.data.id)
      // 兼容后端 PascalCase / camelCase 两种命名（C# 实体默认 PascalCase）
      const res: any = camelCaseKeys(raw)
      const good = res.good

      // 优先用 coverImg 作为首图（与列表页保持一致），再追加详情轮播图
      const detailImages = res.images || res.imgList || good?.imgList || good?.images || []
      const coverUrls = (good?.coverImg || '')
        .split(',')
        .map((u: string) => resolveUrl(u.trim()))
        .filter(Boolean)
      const extraUrls = (detailImages as any[])
        .map((i: any) => resolveUrl(i.imgUrl || i.imageUrl || i.url || i))
        .filter(Boolean)
      // 去重：追加的图片如果已在 coverImg 中出现则跳过
      const coverSet = new Set(coverUrls)
      const swiperImages = [
        ...coverUrls,
        ...extraUrls.filter((u: string) => !coverSet.has(u))
      ]

      this.setData({
        good,
        swiperImages,
        selectedSkuName: good.spec || '',
        displayPrice: this.toNum(good.salePrice || good.price || 0).toFixed(1),
        soldText: formatSoldCount(good?.soldCount),
        loading: false
      })
      // 进入详情即记录足迹
      addFootprint({
        id: good.id,
        goodsName: good.goodsName,
        coverImg: (good.coverImg || '').split(',')[0] || '',
        salePrice: this.toNum(good.salePrice || good.price || 0)
      })
      this.setCartCount()
      // 评价独立加载，避免阻塞首屏渲染；失败时只打日志，不重置空数组（减少渲染层 warning）
      this.loadReviews().catch(() => {})
    } catch (err) {
      console.error('[loadDetail] 加载商品详情失败', err)
      wx.showToast({ title: '加载失败', icon: 'none' })
      this.setData({ loading: false })
    }
  },

  openSku(e: any) {
    if (!requireLogin()) return
    this.setData({ showSku: true, action: e.currentTarget.dataset.action || 'buy' })
  },

  closeSku() {
    this.setData({ showSku: false })
  },

  /** 弹窗选择变化时同步更新详情页顶部价格/规格 */
  onSkuChange(e: any) {
    const { skuName, displayPrice } = e.detail
    this.setData({
      selectedSkuName: skuName,
      displayPrice
    })
  },

  /** 弹窗确定回调（加购/购买统一处理） */
  onSkuConfirm(e: any) {
    const { action, goods, skuId, skuName, price, qty, stock, specName, coverImg } = e.detail
    const item: CartItem = {
      goodsId: goods.id,
      goodsName: goods.goodsName,
      coverImg,
      price: this.toNum(price),
      buyNum: qty,
      skuId,
      skuName,
      specName,
      stock
    }

    if (action === 'cart') {
      addToCart(item)
      this.setCartCount()
      wx.showToast({ title: '已加入购物车', icon: 'success' })
      this.setData({ showSku: false })
    } else {
      setCheckoutItems([item])
      this.setData({ showSku: false })
      wx.navigateTo({ url: '/pages/checkout/checkout' })
    }
  },

  /** 图片加载失败时输出日志并替换为占位图，避免详情页轮播出现空白/裂图 */
  onImageError(e: any) {
    const idx = e.currentTarget.dataset.idx
    const url = e.currentTarget.dataset.src
    console.error('[onImageError] 图片加载失败, idx:', idx, 'url:', url, 'err:', e.detail)
    const fallback = 'https://placehold.co/400x400?text=No+Image'
    const imgs = [...this.data.swiperImages]
    if (idx >= 0 && idx < imgs.length && imgs[idx] === url) {
      imgs[idx] = fallback
      this.setData({ swiperImages: imgs })
    }
  },

  /** 评价头像加载失败时清空 avatar，让 wxml 用昵称首字母兜底（不再去拉默认头像图，避免 404） */
  onAvatarError(e: any) {
    const idx = e.currentTarget.dataset.index
    const reviews = this.data.reviews.slice()
    if (idx >= 0 && idx < reviews.length) {
      reviews[idx] = { ...reviews[idx], avatar: '' }
      this.setData({ reviews })
    }
  },

  /**
   * 加载商品评价（真实接口）。
   * 支持按档位(good/mid/bad) 或 标签(tag) 筛选，由 activeReviewFilter 决定。
   * 失败时不再 silent，而是把错误信息写到 data.reviewError，让 wxml 显示「重试」按钮。
   */
  async loadReviews() {
    this.setData({ reviewLoading: true, reviewError: '' })
    try {
      const f = REVIEW_FILTERS[this.data.activeReviewFilter] || REVIEW_FILTERS[0]
      const raw = await getGoodsReviews(this.data.id, {
        tier: f.tier || undefined,
        tag: f.tag || undefined
      })
      // 后端返回 { total, list }（已在 request 中解包 data），这里统一转 camelCase 并做兜底
      const res: any = camelCaseKeys(raw)
      console.log('[loadReviews] raw:', raw, 'camel:', res)
      // 兼容 data 嵌套结构（防止 request 解包异常导致 raw 仍是包装体）
      const total = res.total ?? res.data?.total ?? 0
      const sourceList = res.list ?? res.data?.list ?? []
      const list = sourceList.map((r: any) => ({
        id: r.id,
        nickName: r.memberName || '匿名用户',
        avatar: resolveUrl(r.avatar),
        rating: r.rating,
        content: r.content || '',
        images: (r.images || '')
          .split(',')
          .filter(Boolean)
          .map((u: string) => resolveUrl(u)),
        tags: (r.tags || '').split(',').filter(Boolean),
        createTime: (r.createTime || '').substring(0, 10)
      }))
      this.setData({ reviews: list, reviewTotal: total, reviewLoading: false })
    } catch (err: any) {
      const msg = err?.message || String(err) || '网络异常'
      console.error('[loadReviews] 加载评价失败', err)
      // 不再 silent：让 UI 显示具体错误 + 重试入口
      this.setData({ reviews: [], reviewTotal: 0, reviewError: msg, reviewLoading: false })
    }
  },

  /** 评价重试（点击 reviewError 区块的「重试」按钮调用） */
  retryReviews() {
    this.loadReviews()
  },

  /** 评价筛选切换 */
  onReviewFilter(e: any) {
    const idx = Number(e.currentTarget.dataset.idx)
    if (idx === this.data.activeReviewFilter) return
    this.setData({ activeReviewFilter: idx })
    this.loadReviews()
  },

  goCart() {
    wx.switchTab({ url: '/pages/cart/cart' })
  },

  goHome() {
    wx.switchTab({ url: '/pages/home/home' })
  },

  /** 收藏 / 取消收藏 */
  toggleFav() {
    const good = this.data.good
    if (!good) return
    const fav = !this.data.favorite
    toggleFavorite({
      id: good.id,
      goodsName: good.goodsName,
      coverImg: (good.coverImg || '').split(',')[0] || '',
      salePrice: this.toNum(good.salePrice || good.price || 0)
    })
    this.setData({ favorite: fav })
    wx.showToast({ title: fav ? '已收藏' : '已取消收藏', icon: 'none' })
  },

  goFavorites() {
    wx.navigateTo({ url: '/pages/favorite/favorite' })
  }
})
