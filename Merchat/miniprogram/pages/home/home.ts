import { getHome, getOrderList, getUnreadCount } from '../../utils/api'
import { isLogin } from '../../utils/auth'
import { resolveUrl } from '../../utils/request'
import type { HomeNotice, Order } from '../../utils/types'

interface RecentOrderItem {
  id: string
  orderNo: string
  time: string
  total: string
  statusLabel: string
  statusCls: string
}

// 公告类型文案（与管理端 notice.vue 类型选项一致）
const NOTICE_TYPE_TEXT: Record<number, string> = { 1: '公告', 2: '通知', 3: '系统' }

// 通知滚动计时器句柄（模块级，单页场景安全）
let noticeTimer: any = null

/**
 * 通知条展示文本：优先显示通知内容（content），为空时回退标题。
 * 内容过长时条内做横向滚动（noticeScroll）。
 */
function noticeTextOf(n: HomeNotice | undefined): string {
  if (!n) return ''
  const content = (n.content || '').trim()
  if (content) return content
  return (n.title || '').trim()
}

/**
 * 是否需要条内横向滚动：仅「只有一条通知」且内容明显超出可视宽度时启用。
 * 多条通知时改为按间隔轮播切换（此时横向动画会因内容重绘而重置，体验反而更差）。
 * 粗略估算：中文按 24rpx 字号折算，容器可视宽度约 20 个字。
 */
function needScroll(text: string, total: number): boolean {
  if (total > 1) return false
  return text.length > 18
}

/** "2026-10-18T14:30:00" / "2026-10-18 14:30:00" → "10-18 14:30" */
function shortTime(t: string): string {
  if (!t) return ''
  const s = t.replace('T', ' ')
  return s.length >= 16 ? s.slice(5, 16) : s
}

Page({
  data: {
    // 顶部商户信息（仅首页展示）
    merchantName: '',
    merchantAvatar: '',
    avatarChar: '',
    merchantLevel: '',
    shopStatus: '营业中',
    /** 右上角操作区位置：距顶部(px)，与胶囊右边缘对齐(px) */
    actionsTop: 48,
    actionsRight: 15,
    // 今日数据（含昨日对比）
    todayOrders: 0,
    todayRevenue: '0.00',
    yesterdayOrders: 0,
    yesterdayRevenue: '0.00',
    kpiPending: 0,
    /** 消息按钮红点：聊天未读 + 通知未读 */
    unreadTotal: 0,
    // 公告通知
    notices: [] as HomeNotice[],
    // 动态通知滚动状态
    noticeIndex: 0,
    noticeAnim: '',
    /** 当前展示的通知内容 */
    noticeText: '',
    /** 内容过长时条内横向滚动 */
    noticeScroll: false,
    // 最新订单（取前 3 条）
    recentOrders: [] as RecentOrderItem[],
    // 在售商品
    goodsSectionTitle: '在售商品',
    products: [] as any[],
    // 页面状态：加载中 / 加载失败
    loading: true,
    loadError: '',
  },

  onLoad() {
    this.setupNav()
  },

  onShow() {
    const tabBar = this.getTabBar() as any
    if (tabBar) tabBar.setData({ selected: 0 })
    this.load()
    this.loadUnread()
  },

  /**
   * 顶部布局适配（自定义导航栏 navigationStyle: custom）：
   * 用胶囊按钮的真实位置，把「消息」放到胶囊正下方并右对齐。
   * 按钮是绝对定位，不占布局空间，页面其他元素的位置保持不变。
   */
  setupNav() {
    try {
      const wxAny = wx as any
      const win = wxAny.getWindowInfo ? wxAny.getWindowInfo() : wx.getSystemInfoSync()
      const menu = wx.getMenuButtonBoundingClientRect()
      const winWidth = win.windowWidth || 375
      // 操作区顶部 = 胶囊底部 + 2px（在 -3 基础上再下移 5px，仅按钮自身位移）
      const actionsTop = menu.bottom + 2
      // 右边缘与胶囊右边缘对齐
      const actionsRight = Math.max(winWidth - menu.right, 10)
      this.setData({ actionsTop, actionsRight })
    } catch (e) {
      // 取不到胶囊信息时用兜底值
      this.setData({ actionsTop: 48, actionsRight: 15 })
    }
  },

  /** 拉取未读数（聊天 + 库存预警通知），用于消息按钮红点 */
  async loadUnread() {
    try {
      const res = await getUnreadCount()
      this.setData({ unreadTotal: (res && res.total) || 0 })
    } catch (e) {
      // 拉取失败不显示红点
    }
  },

  onHide() {
    this.stopMarquee()
  },

  onUnload() {
    this.stopMarquee()
  },

  async load() {
    if (!isLogin()) {
      wx.redirectTo({ url: '/pages/login/login' })
      return
    }
    this.setData({ loading: true, loadError: '' })
    try {
      // 首页数据源并行；订单失败时降级为空，不阻塞主内容
      const [home, orders] = await Promise.all([
        getHome(),
        getOrderList('all').catch(() => [] as Order[]),
      ])

      const m = home.merchant || ({} as any)
      const t = home.today || ({} as any)
      const name = m.merchantName || '商户'

      const notices: HomeNotice[] = (home.notices || []).map((n: any) => ({
        id: n.id,
        title: n.title || '',
        content: n.content || '',
        type: n.type || 1,
        typeText: n.typeText || NOTICE_TYPE_TEXT[n.type || 1] || '公告',
        createTime: n.createTime || '',
      }))

      const products = (home.products || []).map((p: any) => ({
        ...p,
        hasImage: !!(p.coverImg || '').trim(),
        defaultEmoji: '\u{1F4EA}',
      }))

      const recentOrders: RecentOrderItem[] = orders.slice(0, 3).map((o: Order) => ({
        id: o.id,
        orderNo: o.orderNo || o.id,
        time: shortTime(o.time),
        total: Number(o.total || 0).toFixed(2),
        statusLabel: o.detailStatusLabel || '',
        statusCls: o.detailStatusCls || '',
      }))

      this.setData({
        loading: false,
        merchantName: name,
        merchantAvatar: m.avatar ? resolveUrl(m.avatar) : '',
        avatarChar: name.charAt(0),
        merchantLevel: m.level || '',
        shopStatus: m.shopStatus || '营业中',
        todayOrders: Number(t.orders || 0),
        todayRevenue: Number(t.revenue || 0).toFixed(2),
        yesterdayOrders: Number(t.yesterdayOrders || 0),
        yesterdayRevenue: Number(t.yesterdayRevenue || 0).toFixed(2),
        kpiPending: Number((home.kpi && home.kpi.pending) || 0),
        notices,
        noticeIndex: 0,
        noticeAnim: '',
        noticeText: noticeTextOf(notices[0]),
        noticeScroll: needScroll(noticeTextOf(notices[0]), notices.length),
        recentOrders,
        products,
        goodsSectionTitle: '在售商品',
      })
      this.startMarquee()
    } catch (e: any) {
      this.setData({
        loading: false,
        loadError: (e && e.message) || '加载失败，请稍后重试',
      })
    }
  },

  // 顶部通知条点击：查看当前这条全文
  onMarqueeTap() {
    const n = this.data.notices[this.data.noticeIndex]
    if (!n) return
    wx.showModal({
      title: n.title || n.typeText,
      content: n.content || '',
      showCancel: false,
      confirmText: '知道了',
    })
  },

  startMarquee() {
    this.stopMarquee()
    if (this.data.notices.length <= 1) return
    noticeTimer = setInterval(() => this.rollNotice(), 3200)
  },

  stopMarquee() {
    if (noticeTimer) {
      clearInterval(noticeTimer)
      noticeTimer = null
    }
  },

  // 滚动到下一条：先淡出，换内容后淡入（横向滚动与淡出动画互不冲突）
  rollNotice() {
    if (this.data.noticeAnim) return
    const total = this.data.notices.length
    this.setData({ noticeAnim: 'out' })
    setTimeout(() => {
      const next = (this.data.noticeIndex + 1) % total
      const text = noticeTextOf(this.data.notices[next])
      this.setData({
        noticeIndex: next,
        noticeAnim: 'in',
        noticeText: text,
        noticeScroll: needScroll(text, this.data.notices.length),
      })
      setTimeout(() => this.setData({ noticeAnim: '' }), 30)
    }, 360)
  },

  goOrders() {
    wx.switchTab({ url: '/pages/order/list' })
  },
  goGoods() {
    wx.switchTab({ url: '/pages/goods/list' })
  },
  goReconcile() {
    wx.switchTab({ url: '/pages/reconcile/reconcile' })
  },
  goGoodsAdd() {
    wx.navigateTo({ url: '/pages/goods/add/add' })
  },
  goBill() {
    wx.navigateTo({ url: '/pages/bill/list' })
  },
  goAddress() {
    wx.navigateTo({ url: '/pages/profile/address/list' })
  },
  goQualification() {
    wx.navigateTo({ url: '/pages/profile/qualification/qualification' })
  },
  /** 消息中心：与买家的聊天会话列表 */
  goChatList() {
    wx.navigateTo({ url: '/pages/chat/list' })
  },
  goGoodsDetail(e: any) {
    wx.navigateTo({ url: '/pages/goods/detail?id=' + e.currentTarget.dataset.id })
  },
  goOrderDetail(e: any) {
    wx.navigateTo({ url: '/pages/order/detail?id=' + e.currentTarget.dataset.id })
  },
  onRetry() {
    this.load()
  },
  onImageError(e: any) {
    const idx = e.currentTarget.dataset.index
    const key = `products[${idx}].hasImage`
    this.setData({ [key]: false })
  },
})
