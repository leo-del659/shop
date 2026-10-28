import { sendSmsCode, smsLogin } from '../../api/auth'
import { getAppToken, setAppToken } from '../../app'
import { markCacheOwner, migrateLegacyCaches, adoptGuestCaches } from '../../utils/userScope'

/** tabBar 页面不能用 reLaunch/navigateTo 直达，需走 switchTab */
const TAB_PAGES = [
  '/pages/home/home',
  '/pages/category/category',
  '/pages/cart/cart',
  '/pages/mine/mine'
]

/** 验证码倒计时总时长（秒） */
const COUNTDOWN_TOTAL = 60

Page({
  data: {
    /** 登录手机号 */
    mobile: '',
    /** 短信验证码 */
    smsCode: '',
    /** 是否正在发送验证码（防止重复点击） */
    sending: false,
    /** 验证码倒计时剩余秒数，> 0 表示冷却中 */
    countdown: 0,
    /** 登录成功后回跳的页面路径（由 requireLogin / 请求层 401 传入） */
    redirect: ''
  },

  onLoad(options: any) {
    // 已登录（token 持久化有效）时无需再次登录：直接进首页。
    // 覆盖分享链接、旧入口等途径落到登录页的场景。
    if (getAppToken()) {
      wx.reLaunch({ url: '/pages/home/home' })
      return
    }
    if (options && options.redirect) {
      this.setData({ redirect: decodeURIComponent(options.redirect) })
    }
  },

  onUnload() {
    this.clearTimer()
  },

  onInputMobile(e: any) {
    this.setData({ mobile: e.detail.value })
  },

  onInputCode(e: any) {
    this.setData({ smsCode: e.detail.value })
  },

  clearTimer() {
    if ((this as any)._timer) {
      clearInterval((this as any)._timer)
      ;(this as any)._timer = null
    }
  },

  startCountdown() {
    this.clearTimer()
    this.setData({ countdown: COUNTDOWN_TOTAL })
    const timer = setInterval(() => {
      const left = this.data.countdown - 1
      if (left <= 0) {
        this.clearTimer()
        this.setData({ countdown: 0 })
      } else {
        this.setData({ countdown: left })
      }
    }, 1000)
    ;(this as any)._timer = timer
  },

  /**
   * 获取验证码（模拟接收：开发环境后端未接短信网关，
   * 会把验证码直接返回，这里用弹窗展示给用户输入）
   */
  async onSendCode() {
    if (this.data.sending || this.data.countdown > 0) return
    const mobile = (this.data.mobile || '').trim()
    if (!/^1\d{10}$/.test(mobile)) {
      wx.showToast({ title: '请输入正确的手机号', icon: 'none' })
      return
    }
    this.setData({ sending: true })
    try {
      const res = await sendSmsCode(mobile)
      this.startCountdown()
      if (res && res.code) {
        // 模拟接收：开发环境直接展示验证码
        wx.showModal({
          title: '模拟短信',
          content: `验证码：${res.code}\n（开发环境模拟接收，请输入）`,
          showCancel: false,
          confirmText: '知道了'
        })
      } else {
        wx.showToast({ title: '验证码已发送', icon: 'none' })
      }
    } catch (e) {
      // 错误已在 request 层 toast
    } finally {
      this.setData({ sending: false })
    }
  },

  async onSubmit() {
    const mobile = (this.data.mobile || '').trim()
    const code = (this.data.smsCode || '').trim()
    if (!/^1\d{10}$/.test(mobile)) {
      wx.showToast({ title: '请输入正确的手机号', icon: 'none' })
      return
    }
    if (!code) {
      wx.showToast({ title: '请输入验证码', icon: 'none' })
      return
    }
    try {
      // 免注册登录：手机号+验证码直接登录，首次登录后端自动创建账号
      const res = await smsLogin(mobile, code)
      setAppToken(res.token)
      // 缓存登录手机号，便于个人中心展示
      wx.setStorageSync('loginMobile', mobile)
      // 切换缓存作用域：把游客期间的数据并入该账号 + 迁移旧版全局缓存，
      // 保证「换号后看到别人的足迹/订单」这类串数据问题不再出现
      markCacheOwner(mobile)
      migrateLegacyCaches()
      adoptGuestCaches(mobile)
      // 清理旧商户端缓存
      wx.removeStorageSync('merchantAccount')
      wx.removeStorageSync('merchant')

      wx.showToast({ title: '登录成功', icon: 'success' })
      setTimeout(() => {
        const redirect = this.data.redirect
        if (redirect) {
          // 登录后回到触发登录的页面：tabBar 页走 switchTab，其余走 reLaunch（重置栈、避免堆叠）
          if (TAB_PAGES.some(p => redirect === p || redirect.startsWith(p + '?'))) {
            wx.switchTab({ url: redirect.split('?')[0] })
          } else {
            wx.reLaunch({ url: redirect })
          }
        } else if (getCurrentPages().length > 1) {
          wx.navigateBack()
        } else {
          wx.switchTab({ url: '/pages/home/home' })
        }
      }, 800)
    } catch (e) {
      // 错误已在 request 层 toast
    }
  }
})
