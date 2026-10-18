import { getProfile } from '../../utils/api'
import { isLogin, logout } from '../../utils/auth'
import { formatWan, confirmDialog, toast } from '../../utils/util'
import { resolveUrl } from '../../utils/request'

Page({
  data: {
    displayName: '',
    subLine: '',
    phone: '',
    address: '',
    avatarText: '',
    avatarUrl: '',
    statOrders: 0,
    statRevenue: '',
    statProducts: 0,
    statPending: 0,
  },
  onShow() {
    const tabBar = this.getTabBar() as any
    if (tabBar) tabBar.setData({ selected: 4 })
    this.load()
  },
  async load() {
    if (!isLogin()) {
      wx.redirectTo({ url: '/pages/login/login' })
      return
    }
    const d = await getProfile()
    const s = d.merchant
    this.setData({
      displayName: s.merchantName,
      subLine: s.merchantNo || s.id || '',
      phone: s.account ? String(s.account).replace(/^(\d{3})\d{4}(\d{4})$/, '$1****$2') : (s.phone ? String(s.phone).replace(/^(\d{3})\d{4}(\d{4})$/, '$1****$2') : ''),
      address: s.address || '',
      avatarText: (s.merchantName || '商').charAt(0),
      avatarUrl: s.avatar ? resolveUrl(s.avatar) : '',
      statOrders: d.stats.orders,
      statRevenue: formatWan(d.stats.revenue),
      statProducts: d.stats.products,
      statPending: d.stats.pending,
    })
  },
  goBack() {
    const pages = getCurrentPages()
    if (pages.length > 1) {
      wx.navigateBack()
    } else {
      wx.switchTab({ url: '/pages/home/home' })
    }
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
  goDashboard() {
    wx.switchTab({ url: '/pages/home/home' })
  },
  onEditProfile() {
    wx.navigateTo({ url: '/pages/profile/edit/edit' })
  },
  onMenu(e: any) {
    const urls: any = {
      address: '/pages/profile/address/list',
      security: '/pages/profile/security/security',
      bind: '/pages/profile/bind/bind',
      qualification: '/pages/profile/qualification/qualification',
    }
    const key = e.currentTarget.dataset.key
    const url = urls[key]
    if (url) {
      wx.navigateTo({ url })
    } else {
      toast('功能开发中')
    }
  },
  async onLogout() {
    const ok = await confirmDialog('确认退出登录？')
    if (!ok) return
    logout()
    wx.reLaunch({ url: '/pages/login/login' })
  },
})
