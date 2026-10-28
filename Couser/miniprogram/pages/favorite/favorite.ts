import { getFavorites, removeFavorite, removeFavorites, FavoriteItem, favoriteCount } from '../../utils/favorite'
import { resolveUrl } from '../../utils/request'
import { createSwipeHandlers, SWIPE_OPEN_WIDTH } from '../../utils/swipe'

Page({
  data: {
    // 列表 + 计数
    rawList: [] as FavoriteItem[], // 全量
    list: [] as FavoriteItem[],    // 当前展示（搜索过滤后）
    count: 0,
    // 搜索（顶部常驻搜索框）
    keyword: '',
    // 管理
    manageMode: false,
    _selectedMap: {} as Record<string, boolean>,
    selectedCount: 0,
    // 右滑删除状态
    _swipeOffsets: {} as Record<string, number>,
    _isDragging: false,
    swipeOpenWidth: SWIPE_OPEN_WIDTH,
    // 自定义导航栏
    statusBarHeight: 0,
    navBarHeight: 44,
    navGap: 96
  },

  // 内部运行时状态（不在 data 里，避免每次 setData 触发渲染）
  _swipe: null as any,
  _suppressClick: false,

  onLoad() {
    this._swipe = createSwipeHandlers(this)
    // 计算导航栏尺寸：状态栏高度 + 胶囊对齐的导航行高度，右侧避开胶囊按钮
    try {
      const sys = wx.getSystemInfoSync()
      const menu = wx.getMenuButtonBoundingClientRect()
      const statusBarHeight = sys.statusBarHeight || 0
      const navBarHeight = (menu.top - statusBarHeight) * 2 + menu.height
      const navGap = sys.windowWidth - menu.left + 8
      this.setData({ statusBarHeight, navBarHeight, navGap })
    } catch (e) {
      // 取不到胶囊信息时用兜底值
    }
  },

  goBack() {
    const pages = getCurrentPages()
    if (pages.length > 1) {
      wx.navigateBack()
    } else {
      wx.switchTab({ url: '/pages/mine/mine' })
    }
  },

  onShow() {
    this.load()
  },

  load() {
    const list = getFavorites().map(item => ({
      ...item,
      coverImg: resolveUrl(item.coverImg)
    }))
    this.setData({
      rawList: list,
      list,
      count: favoriteCount(),
      _swipeOffsets: {},
      // 每次进入页面恢复非管理态、清空选择
      manageMode: false,
      _selectedMap: {},
      selectedCount: 0
    })
  },

  /** 用 keyword 重新计算显示列表 */
  applyFilter() {
    const kw = (this.data.keyword || '').trim().toLowerCase()
    const list = kw
      ? this.data.rawList.filter(item => (item.goodsName || '').toLowerCase().includes(kw))
      : this.data.rawList
    this.setData({ list })
  },

  // ===== 触摸 & 点击 =====
  onTouchStart(e: any) {
    if (this.data.manageMode) return
    this._swipe.start(e)
  },
  onTouchMove(e: any) {
    if (this.data.manageMode) return
    this._swipe.move(e)
  },
  onTouchEnd() {
    if (this._suppressClick) return
    if (this._swipe.end()) {
      this._suppressClick = true
      setTimeout(() => { this._suppressClick = false }, 250)
    }
  },

  goDetail(e: any) {
    if (this._suppressClick) return
    if (this.data.manageMode) {
      this.toggleSelect(e)
      return
    }
    const id = e.currentTarget.dataset.id
    wx.navigateTo({ url: '/pages/goods-detail/goods-detail?id=' + id })
  },

  toggleSelect(e: any) {
    const id = e.currentTarget.dataset.id
    const map = { ...this.data._selectedMap }
    if (map[id]) delete map[id]
    else map[id] = true
    let n = 0
    for (const k in map) if (map[k]) n++
    this.setData({ _selectedMap: map, selectedCount: n })
  },

  onRemove(e: any) {
    if (this.data.manageMode) return
    const id = e.currentTarget.dataset.id
    removeFavorite(id)
    this.load()
    wx.showToast({ title: '已取消收藏', icon: 'none' })
  },

  // ===== 搜索 =====
  onSearch() {
    this.applyFilter()
    wx.hideKeyboard()
  },

  onSearchInput(e: any) {
    this.setData({ keyword: e.detail.value || '' })
    this.applyFilter()
  },

  onClearKeyword() {
    this.setData({ keyword: '' })
    this.applyFilter()
  },

  // ===== 管理 =====
  onManage() {
    const newManageMode = !this.data.manageMode
    // 进入管理模式时清空已展开的滑动并清空选择
    this.setData({
      manageMode: newManageMode,
      _swipeOffsets: {},
      _selectedMap: {},
      selectedCount: 0
    })
  },

  onBulkDelete() {
    const ids = Object.keys(this.data._selectedMap).filter(id => this.data._selectedMap[id])
    if (!ids.length) {
      wx.showToast({ title: '请先勾选商品', icon: 'none' })
      return
    }
    wx.showModal({
      title: '提示',
      content: `确定删除已选的 ${ids.length} 项？`,
      success: (res) => {
        if (res.confirm) {
          removeFavorites(ids)
          this.load()
          wx.showToast({ title: '已删除', icon: 'none' })
        }
      }
    })
  },

  // 保留原 onClear 函数（清空按钮已移除，但函数不删，符合最小改动原则）
  onClear() {
    wx.showModal({
      title: '提示',
      content: '确定清空全部收藏？',
      success: (res) => {
        if (res.confirm) {
          // 原行为仅保留死代码，wxml 不再触发
          // 清空全部收藏（收藏已按账号隔离，见 utils/favorite.ts）
          // wx.removeStorageSync(scopedKey('favorites'))
          this.load()
          wx.showToast({ title: '已清空', icon: 'none' })
        }
      }
    })
  },

  goHome() {
    wx.switchTab({ url: '/pages/home/home' })
  }
})
