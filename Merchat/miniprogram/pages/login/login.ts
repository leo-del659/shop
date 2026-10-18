import { accountLogin } from '../../utils/auth'

// 商户默认演示账号（手机号 + 密码），便于快速登录验证
const DEFAULT_PHONE = '12500000001'
const DEFAULT_PASSWORD = '123456'

Page({
  data: {
    phone: DEFAULT_PHONE,
    password: DEFAULT_PASSWORD,
    submitting: false,
    errorMsg: '',
  },

  onLoad(options: any) {
    // 注册成功后回跳登录页并带入手机号，自动填充
    if (options && options.phone) {
      this.setData({ phone: String(options.phone) })
    }
  },

  onInput(e: any) {
    const field = e.currentTarget.dataset.field
    this.setData({ [field]: e.detail.value, errorMsg: '' })
  },

  // 跳转快速注册页
  goRegister() {
    wx.navigateTo({ url: '/pages/register/register' })
  },

  async onLogin() {
    // 防止重复提交（submitting 同时控制 disabled 锁 + 内存判断）
    if (this.data.submitting) return
    const { phone, password } = this.data
    if (!/^1\d{10}$/.test(phone || '')) {
      this.setData({ errorMsg: '请输入正确的 11 位手机号' })
      return
    }
    if (!password) {
      this.setData({ errorMsg: '请输入密码' })
      return
    }
    this.setData({ submitting: true, errorMsg: '' })

    // 用全局 loading 遮罩代替 button 的 loading 属性，避免按钮重绘闪烁
    wx.showLoading({ title: '登录中', mask: true })

    try {
      await accountLogin(phone, password)
      // 先关 loading 再跳转，避免遮罩残留在新页面
      wx.hideLoading()
      // home 是 tabBar 页面，必须用 switchTab
      wx.switchTab({ url: '/pages/home/home' })
    } catch (err: any) {
      wx.hideLoading()
      this.setData({
        submitting: false,
        errorMsg: err.message || '登录失败',
      })
    }
  },
})
