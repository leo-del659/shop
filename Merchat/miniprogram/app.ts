import { clearAuth, getLocalMerchant, getToken, isLogin } from './utils/auth'
import { request } from './utils/request'

App<IAppOption>({
  globalData: {
    loggedIn: false,
    userType: 'merchant',
  } as any,
  onLaunch() {
    // 未登录时直接留在登录页（login 是首页）
    if (!isLogin()) {
      this.globalData.loggedIn = false
      return
    }
    const token = getToken()
    if (!token) {
      clearAuth()
      this.globalData.loggedIn = false
      return
    }

    // 以本地存储的商户信息判断角色（仅商户端）
    const merchant = getLocalMerchant()
    const role: 'merchant' | null = merchant && (merchant as any).id ? 'merchant' : null
    this.globalData.userType = role || 'merchant'

    // 本地已有身份信息 → 直接视为已登录，不额外请求后端（避免网络抖动把已登录用户踢出）
    if (role === 'merchant' && merchant && (merchant as any).id) {
      this.globalData.loggedIn = true
      return
    }

    // 无本地缓存时才拉取；失败时不粗暴 clearAuth，保留 token（下次进入页面自然触发 401 再跳登录）
    request<any>({ path: '/api/merchant/me', method: 'GET' })
      .then(() => {
        this.globalData.loggedIn = true
      })
      .catch(() => {
        this.globalData.loggedIn = false
      })
  },
})
