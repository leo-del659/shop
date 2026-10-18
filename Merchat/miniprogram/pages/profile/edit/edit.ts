import { getProfile, updateProfile } from '../../../utils/api'
import { uploadFile, resolveUrl } from '../../../utils/request'
import { toast } from '../../../utils/util'

Page({
  data: {
    saving: false,
    saveBtnText: '保存',
    // avatarUrl 为完整展示地址，avatarFile 为后端相对地址（提交用）
    avatarUrl: '',
    avatarFile: '',
    avatarText: '商',
    merchantName: '',
    uploading: false,
  },
  onLoad() {
    this.load()
  },
  async load() {
    const d = await getProfile()
    const s = d.merchant
    this.setData({
      merchantName: s.merchantName || '',
      avatarText: (s.merchantName || '商').charAt(0),
      avatarUrl: s.avatar ? resolveUrl(s.avatar) : '',
      avatarFile: s.avatar || '',
    })
  },
  goBack() {
    wx.navigateBack()
  },
  // 选择并上传头像
  chooseAvatar() {
    if (this.data.uploading || this.data.saving) return
    wx.chooseMedia({
      count: 1,
      mediaType: ['image'],
      sourceType: ['album', 'camera'],
      success: (res) => {
        const tempFilePath = res.tempFiles[0] && res.tempFiles[0].tempFilePath
        if (!tempFilePath) return
        this.uploadAvatar(tempFilePath)
      },
      fail: () => toast('选择图片失败'),
    })
  },
  async uploadAvatar(tempFilePath: string) {
    this.setData({ uploading: true })
    wx.showLoading({ title: '上传中', mask: true })
    try {
      const result = await uploadFile(tempFilePath)
      this.setData({
        avatarFile: result.url,
        avatarUrl: resolveUrl(result.url),
      })
      toast('头像上传成功')
    } catch (err: any) {
      toast(err.message || '头像上传失败')
    } finally {
      wx.hideLoading()
      this.setData({ uploading: false })
    }
  },
  onInput(e: any) {
    const field = e.currentTarget.dataset.field
    this.setData({ [field]: e.detail.value })
  },
  async save() {
    if (this.data.saving) return
    const merchantName = (this.data.merchantName || '').trim()
    if (!merchantName) {
      toast('请输入商户名称')
      return
    }
    this.setData({ saving: true, saveBtnText: '保存中...' })
    try {
      await updateProfile({ merchantName, avatar: this.data.avatarFile || '' })
      toast('保存成功')
      setTimeout(() => wx.navigateBack(), 600)
    } catch (err: any) {
      toast(err.message || '保存失败')
    } finally {
      this.setData({ saving: false, saveBtnText: '保存' })
    }
  },
})
