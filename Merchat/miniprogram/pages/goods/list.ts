import { getGoodsList, GoodsFilter, toggleGoodsStatus } from '../../utils/api'
import { Goods } from '../../utils/types'
import { getStockClass, getStockLabel, toast } from '../../utils/util'
import { isLogin } from '../../utils/auth'
import { CATEGORIES } from '../../utils/categories'

interface GoodsView extends Goods {
  coverImgFull?: string
  imgError?: boolean
  stockClass: string
  stockLabel: string
  isLowStock: boolean
  // computed for WXML
  statusText: string
  stockCls: string
  defaultEmoji: string
}

Page({
  data: {
    canManage: true,
    filter: 'all' as GoodsFilter,
    keyword: '',
    categories: CATEGORIES,
    // picker 选项（纯字符串数组，onLoad 中构建）
    categoryNames: [] as string[],
    categoryIndex: 0,
    category: '' as string,
    list: [] as GoodsView[],
    statTotal: 0,
    statPublished: 0,
    statLow: 0,
    statDraft: 0,
    // computed display texts
    fAll: 'active', fPublished: '', fDraft: '', fDisabled: '', fLowStock: '',
    defaultEmoji: '\u{1F4E6}',
  },
  _searchTimer: 0 as number,
  onLoad() {
    this.setData({
      canManage: true,
      categoryNames: ['全部', ...CATEGORIES.map(c => c.name)],
    })
    this.load()
  },
  onShow() {
    const tabBar = this.getTabBar() as any
    if (tabBar) tabBar.setData({ selected: 2 })
  },
  async load() {
    if (!isLogin()) {
      wx.redirectTo({ url: '/pages/login/login' })
      return
    }
    const { keyword, category, filter } = this.data
    const opts = { keyword: keyword || undefined, category: category || undefined }

    const api = getGoodsList

    const [raw, all, drafts] = await Promise.all([
      api(filter, opts),
      api('all'),
      api('draft'),
    ])
    const list: GoodsView[] = raw.map((g) => {
      const isLow = g.stock < 50
      const isPub = g.status === 'published'
      return {
        ...g,
        coverImgFull: g.coverImg || '',
        stockClass: getStockClass(g.stock, g.safeStock || 50),
        stockLabel: getStockLabel(g.stock, g.safeStock || 50),
        isLowStock: isLow,
        statusText: isPub ? '已上架' : g.status === 'draft' ? '待审核' : '已下架',
        stockCls: isLow ? 'low' : 'normal',
        defaultEmoji: '\u{1F4E6}',
      }
    })
    this.setData({
      list,
      statTotal: all.length,
      statPublished: all.filter((g) => g.status === 'published').length,
      statLow: all.filter((g) => g.stock < 50).length,
      statDraft: drafts.length,
    })
    this._updateFilterCls()
  },
  onSearchInput(e: any) {
    const keyword = (e.detail.value || '').trim()
    this.setData({ keyword })
    if (this._searchTimer) clearTimeout(this._searchTimer)
    this._searchTimer = setTimeout(() => this.load(), 350) as unknown as number
  },
  onSearchConfirm() {
    if (this._searchTimer) clearTimeout(this._searchTimer)
    this.load()
  },
  clearSearch() {
    if (this._searchTimer) clearTimeout(this._searchTimer)
    this.setData({ keyword: '' }, () => this.load())
  },
  onCategoryChange(e: any) {
    const idx = Number(e.detail.value)
    const catId = idx === 0 ? '' : (this.data.categories[idx - 1]?.id || '')
    this.setData({ categoryIndex: idx, category: catId }, () => this.load())
  },
  setFilter(e: any) {
    const f = e.currentTarget.dataset.filter as GoodsFilter
    if (f === this.data.filter) return
    this.setData({ filter: f }, () => this.load())
  },
  _updateFilterCls() {
    const f = this.data.filter
    this.setData({
      fAll: f === 'all' ? 'active' : '',
      fPublished: f === 'published' ? 'active' : '',
      fDraft: f === 'draft' ? 'active' : '',
      fDisabled: f === 'disabled' ? 'active' : '',
      fLowStock: f === 'low-stock' ? 'active' : '',
    })
  },
  onImageError(e: any) {
    const id = e.currentTarget.dataset.id
    const list = this.data.list.map((item) =>
      item.id === id ? { ...item, imgError: true } : item
    )
    this.setData({ list })
  },
  goDetail(e: any) {
    const id = e.currentTarget.dataset.id
    const idx = this.data.list.findIndex((g) => String(g.id) === String(id))
    wx.navigateTo({ url: '/pages/goods/detail?id=' + id + '&idx=' + (idx >= 0 ? idx : 0) })
  },
  goAdd() {
    wx.navigateTo({ url: '/pages/goods/add/add' })
  },
  stopPropagation() {},
  async onToggleOff(e: any) {
    const id = e.currentTarget.dataset.id
    const ok = await new Promise<boolean>((resolve) => {
      wx.showModal({
        title: '确认下架',
        content: '下架后该商品将不再对商户可见，确认？',
        success: (res) => resolve(res.confirm),
      })
    })
    if (!ok) return
    wx.showLoading({ title: '处理中' })
    try {
      await toggleGoodsStatus(id)
      toast('已下架')
      this.load()
    } catch (err: any) {
      toast(err.message || '操作失败')
    } finally {
      wx.hideLoading()
    }
  },
  async onToggleOn(e: any) {
    const id = e.currentTarget.dataset.id
    wx.showLoading({ title: '处理中' })
    try {
      await toggleGoodsStatus(id)
      toast('已上架')
      this.load()
    } catch (err: any) {
      toast(err.message || '操作失败')
    } finally {
      wx.hideLoading()
    }
  },
})
