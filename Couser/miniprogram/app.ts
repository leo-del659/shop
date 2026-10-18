// app.ts
import { syncCartBadge } from './utils/cart'
import { getClientProfile } from './api/auth'

App({
  globalData: {},
  onLaunch() {
    // 登录态持久化：不再每次冷启动清 token——首次登录成功后 token 落盘，
    // 下次启动直接进入首页，无需再次登录（entryPagePath 已指向首页）。
    // 商户端账号缓存与消费者端隔离，仍照常清理。
    wx.removeStorageSync('merchantAccount')
    wx.removeStorageSync('merchant')
    // 启动时同步购物车 tabBar 角标（延迟确保 tabBar 已初始化）
    setTimeout(() => syncCartBadge(), 300)
    // 后台静默校验本地 token 是否仍有效：失效则清理登录态（不主动跳登录页，
    // 避免启动过程被打断；后续请求若再遇 401，由 request 层统一跳转登录）
    this.silentCheckToken()
  },
  onShow() {
    // 从后台切回前台时再次同步，防止外部变更 storage 后角标不一致
    syncCartBadge()
  },

  /**
   * 有本地 token 时静默校验有效性。
   * 仅「登录态失效」（request 层 reject 的 AUTH_EXPIRED，此时已跳转登录页）才兜底清理，
   * 网络错误不误伤——不能因为一次请求失败就把用户登掉。
   */
  silentCheckToken() {
    if (!getAppToken()) return
    getClientProfile().catch((e: any) => {
      if (e && e.message === 'AUTH_EXPIRED') {
        clearAppToken()
        wx.removeStorageSync('loginMobile')
      }
    })
  }
})

/** 读取当前登录 token（本地存储） */
export function getAppToken(): string {
  return wx.getStorageSync('token') || ''
}

/** 保存 token 到本地存储并同步到 globalData */
export function setAppToken(token: string) {
  wx.setStorageSync('token', token)
  const app = getApp()
  if (app) app.globalData.token = token
}

/** 清空登录态 */
export function clearAppToken() {
  wx.removeStorageSync('token')
  const app = getApp()
  if (app) app.globalData.token = ''
}

/** 取当前页面完整路径（含 query），用于登录后回跳原页面 */
export function currentPageUrl(): string {
  const pages = getCurrentPages()
  const cur = pages[pages.length - 1]
  if (!cur) return ''
  const route = '/' + cur.route
  const opts = (cur as any).options || {}
  const query = Object.keys(opts)
    .map(k => `${k}=${opts[k]}`)
    .join('&')
  return query ? `${route}?${query}` : route
}

/**
 * 未登录时跳转登录页，返回是否已登录。
 * @param redirect 登录成功后回跳的页面路径；不传则登录页自行 navigateBack。
 */
export function requireLogin(redirect?: string): boolean {
  if (!getAppToken()) {
    const url =
      '/pages/login/login' + (redirect ? `?redirect=${encodeURIComponent(redirect)}` : '')
    wx.navigateTo({ url })
    return false
  }
  return true
}
