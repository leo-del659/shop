import { getClientProfile, updateClientProfile } from '../../api/auth'
import { getAppToken, clearAppToken } from '../../app'
import { uploadFile, resolveUrl } from '../../utils/request'

interface ClientProfile {
  account: string
  name: string
  avatar: string
}

Page({
  data: {
    profile: { account: '', name: '', avatar: '' } as ClientProfile,
    avatarUrl: '',
    loading: true,
    logged: false
  },

  onShow() {
    this.loadProfile()
  },

  async loadProfile() {
    const token = getAppToken()
    if (!token) {
      this.setData({ logged: false, loading: false })
      return
    }
    try {
      // 消费者用户资料：/api/Login/Client_Profile（client_login 表）
      const p = await getClientProfile()
      this.setData({
        profile: p,
        avatarUrl: p.avatar ? resolveUrl(p.avatar) : '',
        logged: true,
        loading: false
      })
    } catch (e) {
      // 拉取失败（token 失效 / 后端报错），把页面切回未登录态，
      // 避免顶部依然显示「点击登录」却被后端 message 弹成 toast。
      this.setData({ logged: false, profile: { account: '', name: '', avatar: '' }, avatarUrl: '', loading: false })
    }
  },

  /** 选择并上传头像（写入 client_login.avatar） */
  onChooseAvatar() {
    if (!this.data.logged) {
      wx.navigateTo({ url: '/pages/login/login' })
      return
    }
    wx.chooseMedia({
      count: 1,
      mediaType: ['image'],
      sourceType: ['album', 'camera'],
      sizeType: ['compressed'],
      success: async (res) => {
        const temp = res.tempFiles[0].tempFilePath
        wx.showLoading({ title: '上传中' })
        try {
          const { url } = await uploadFile(temp)
          const data = await updateClientProfile({ avatar: url })
          this.applyProfile(data)
          wx.showToast({ title: '头像已更新', icon: 'none' })
        } catch (e) {
          // 错误提示已在 uploadFile / request 中处理
        } finally {
          wx.hideLoading()
        }
      }
    })
  },

  /** 修改用户名称（弹窗输入，写入 client_login.name） */
  onEditName() {
    if (!this.data.logged) {
      wx.navigateTo({ url: '/pages/login/login' })
      return
    }
    const cur = this.data.profile.name || ''
    wx.showModal({
      title: '修改名称',
      editable: true,
      placeholderText: '请输入名称',
      content: cur,
      success: async (r) => {
        if (!r.confirm) return
        const name = (r.content || '').trim()
        if (!name || name === cur) return
        try {
          const data = await updateClientProfile({ name })
          this.applyProfile(data)
          wx.showToast({ title: '已保存', icon: 'none' })
        } catch (e) {
          // 错误提示已在 request 中处理
        }
      }
    })
  },

  /** 用接口返回的最新资料刷新页面 */
  applyProfile(data: ClientProfile) {
    this.setData({
      profile: data,
      avatarUrl: data.avatar ? resolveUrl(data.avatar) : ''
    })
  },

  goLogin() {
    wx.navigateTo({ url: '/pages/login/login' })
  },

  /** 退出登录：带二次确认 */
  onLogout() {
    wx.showModal({
      title: '提示',
      content: '确定要退出当前账号吗？',
      confirmText: '退出',
      confirmColor: '#fa3534',
      cancelText: '取消',
      success: (r) => {
        if (!r.confirm) return
        clearAppToken()
        wx.removeStorageSync('loginMobile')
        this.setData({ logged: false, profile: { account: '', name: '', avatar: '' }, avatarUrl: '' })
        wx.showToast({ title: '已退出登录', icon: 'none' })
        // 回到上一页（个人中心）
        setTimeout(() => {
          wx.navigateBack({ delta: 1, fail: () => { wx.switchTab({ url: '/pages/mine/mine' }) } })
        }, 600)
      }
    })
  }
})
