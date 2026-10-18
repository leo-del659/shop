import { changePassword } from '../../../utils/api'
import { toast, confirmDialog } from '../../../utils/util'

Page({
  data: {
    saving: false,
    saveBtnText: '确认修改',
    errorMsg: '',
    form: {
      oldPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
  },
  goBack() {
    wx.navigateBack()
  },
  onInput(e: any) {
    const field = e.currentTarget.dataset.field
    this.setData({ [`form.${field}`]: e.detail.value, errorMsg: '' })
  },
  async save() {
    if (this.data.saving) return
    const { oldPassword, newPassword, confirmPassword } = this.data.form
    if (!oldPassword || !newPassword) {
      this.setData({ errorMsg: '请输入原密码和新密码' })
      toast('请输入原密码和新密码')
      return
    }
    if (newPassword.length < 6) {
      this.setData({ errorMsg: '新密码长度不能少于6位' })
      toast('新密码长度不能少于6位')
      return
    }
    if (newPassword !== confirmPassword) {
      this.setData({ errorMsg: '两次输入的新密码不一致' })
      toast('两次输入的新密码不一致')
      return
    }
    const ok = await confirmDialog('修改密码后需要重新登录，是否继续？')
    if (!ok) return
    this.setData({ saving: true, saveBtnText: '保存中...', errorMsg: '' })
    try {
      await changePassword(oldPassword, newPassword)
      toast('密码修改成功，请重新登录')
      setTimeout(() => {
        wx.reLaunch({ url: '/pages/login/login' })
      }, 800)
    } catch (err: any) {
      const msg = err.message || '修改失败'
      this.setData({ errorMsg: msg })
      toast(msg)
    } finally {
      this.setData({ saving: false, saveBtnText: '确认修改' })
    }
  },
})
