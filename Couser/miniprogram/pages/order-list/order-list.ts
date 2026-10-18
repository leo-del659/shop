import { getOrders, deleteOrder, ORDER_STATUS, ORDER_STATUS_TEXT, OrderSummary } from '../../api/order'
import { getCachedOrders, cacheOrdersPage, appendCachedOrders } from '../../utils/orderCache'
import { getAppToken } from '../../app'
import { resolveUrl } from '../../utils/request'

Page({
  data: {
    active: 0,
    statusMap: [
      ORDER_STATUS.ALL,
      ORDER_STATUS.WAIT_PAY,
      ORDER_STATUS.WAIT_SHIP,
      ORDER_STATUS.SHIPPED,
      ORDER_STATUS.DONE,
      ORDER_STATUS.CANCELLED
    ],
    orders: [] as (OrderSummary & { statusText: string })[],   // 当前展示（搜索过滤后）
    rawOrders: [] as (OrderSummary & { statusText: string })[], // 全量已加载（含缓存恢复）
    keyword: '',
    page: 1,
    finished: false,
    loading: true,
    // 自定义导航栏
    statusBarHeight: 0,
    navBarHeight: 44,
    navGap: 96
  },

  onLoad(options: any) {
    const status = parseInt(options?.status || '0', 10)
    if (!isNaN(status) && status >= 0 && status < this.data.statusMap.length) {
      this.setData({ active: status })
    }
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
    // 未登录：不跳登录页，直接展示空态（无数据）
    if (!getAppToken()) {
      this.setData({ page: 1, rawOrders: [], orders: [], loading: false, finished: true })
      return
    }
    this.reload()
  },

  /** 把服务端订单数据装饰成页面展示结构（图片拼完整地址、补状态文案） */
  decorate(o: OrderSummary) {
    // 已支付但订单仍处待接单(1)时，按「待发货」展示，避免卡在「待付款」
    const displayStatus = o.payStatus === 1 && o.orderStatus === 1 ? 2 : o.orderStatus
    return {
      ...o,
      statusText: ORDER_STATUS_TEXT[displayStatus] || '未知',
      items: (o.items || []).map((it: any) => ({
        ...it,
        goodsImg: resolveUrl(it.goodsImg)
      }))
    }
  },

  async reload() {
    this.setData({ page: 1, finished: false, loading: true })
    // 未登录：不请求接口（避免空 token 命中 401 再跳登录），直接展示空态
    if (!getAppToken()) {
      this.setData({ rawOrders: [], orders: [], loading: false, finished: true })
      return
    }
    try {
      // 先展示本地缓存（登出后保留的订单，含历史翻页），全量即时恢复
      const status = this.data.statusMap[this.data.active]
      const cached = getCachedOrders(status)
      if (cached.length) {
        const PAGE_SIZE = 20
        // 恢复后以「下一页」为起点继续上拉，避免重复拉取已缓存的页
        const nextPage = Math.floor(cached.length / PAGE_SIZE) + 1
        this.setData({
          rawOrders: cached.map(o => this.decorate(o)),
          page: nextPage,
          loading: false
        })
        this.applyFilter()
      }
      await this.loadOrders()
    } catch (e) {
      this.setData({ loading: false })
      // 错误已在 request 层兜底 toast；如仍有问题请查看 Network 面板原始响应
    }
  },

  async loadOrders() {
    const status = this.data.statusMap[this.data.active]
    const params: any = { page: this.data.page, pageSize: 20 }
    if (status !== ORDER_STATUS.ALL) params.status = status
    const list = await getOrders(params)
    const decorated = list.map((o: OrderSummary) => this.decorate(o))
    const raw = this.data.page === 1 ? decorated : this.data.rawOrders.concat(decorated)
    this.setData({ rawOrders: raw, finished: list.length < 20, loading: false })
    this.applyFilter()
    // 写本地缓存：仅当服务端返回非空时才覆盖缓存
    // 避免「退出登录再登录后服务端短暂返回空」「网络异常 → 后端兜底为空」等场景，
    // 被误判为「该用户没订单」而清掉本地缓存，订单就此消失。
    // 下拉刷新时由 onPullDownRefresh 显式清缓存，保证强一致。
    if (!list || list.length === 0) return
    if (this.data.page === 1) cacheOrdersPage(status, list)
    else appendCachedOrders(status, list)
  },

  // ===== 搜索（本地过滤已加载订单） =====
  /** 用 keyword 过滤 rawOrders → orders（匹配订单号 / 商户名 / 商品名） */
  applyFilter() {
    const kw = (this.data.keyword || '').trim().toLowerCase()
    if (!kw) {
      this.setData({ orders: this.data.rawOrders })
      return
    }
    const match = (o: any) =>
      (o.orderNo || '').toLowerCase().includes(kw) ||
      (o.sellerMerchantName || '').toLowerCase().includes(kw) ||
      (o.merchantName || '').toLowerCase().includes(kw) ||
      (o.items || []).some((it: any) => (it.goodsName || '').toLowerCase().includes(kw))
    this.setData({ orders: this.data.rawOrders.filter(match) })
  },

  onSearchInput(e: any) {
    this.setData({ keyword: e.detail.value || '' })
    this.applyFilter()
  },

  onClearKeyword() {
    this.setData({ keyword: '' })
    this.applyFilter()
  },

  onTabChange(e: any) {
    this.setData({ active: e.detail.index })
    this.reload()
  },

  onReachBottom() {
    if (!this.data.finished) {
      this.data.page += 1
      this.loadOrders()
    }
  },

  /** 下拉刷新：主动失效本地缓存，从服务端重新拉取第 1 页并整体替换缓存 */
  async onPullDownRefresh() {
    if (!getAppToken()) {
      // 未登录：不跳登录页，保持空态
      this.setData({ page: 1, rawOrders: [], orders: [], loading: false, finished: true })
      wx.stopPullDownRefresh()
      return
    }
    const status = this.data.statusMap[this.data.active]
    // 显式清掉当前状态的本地缓存，避免 loadOrders 中「空响应保留旧缓存」的逻辑干扰强刷新
    cacheOrdersPage(status, [])
    this.setData({ page: 1, finished: false })
    try {
      await this.loadOrders()
    } catch (e) {
      // 错误已在 request 层 toast
    } finally {
      wx.stopPullDownRefresh()
    }
  },

  goDetail(e: any) {
    wx.navigateTo({ url: '/pages/order-detail/order-detail?id=' + e.currentTarget.dataset.id })
  },

  /** 删除订单（逻辑删除），从当前列表移除并刷新服务端 */
  async onDelete(e: any) {
    const id = e.currentTarget.dataset.id as string
    if (!id) return
    // 阻止卡片点击跳转详情
    e.stopPropagation && e.stopPropagation()
    const res = await wx.showModal({
      title: '确认删除',
      content: '删除后订单将不再显示，确定删除？',
      confirmText: '删除',
      confirmColor: '#ff4d4f'
    })
    if (!res.confirm) return
    wx.showLoading({ title: '删除中...', mask: true })
    try {
      await deleteOrder(id)
      wx.hideLoading()
      wx.showToast({ title: '删除成功', icon: 'success' })
      // 本地即时移除，避免等接口刷新才消失
      const rawOrders = this.data.rawOrders.filter((o: any) => o.id !== id)
      const orders = this.data.orders.filter((o: any) => o.id !== id)
      this.setData({ rawOrders, orders })
      // 同步清掉当前状态缓存，下次 onShow/pullDownRefresh 从服务端重拉
      cacheOrdersPage(this.data.statusMap[this.data.active], [])
    } catch (err: any) {
      wx.hideLoading()
      wx.showToast({ title: err?.message || '删除失败', icon: 'none' })
    }
  },

  goBuy() {
    wx.switchTab({ url: '/pages/home/home' })
  }
})
