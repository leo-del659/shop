import { getQualifications, addQualification } from '../../../utils/api'
import { uploadFile, resolveUrl } from '../../../utils/request'
import { toast } from '../../../utils/util'

Page({
  data: {
    saving: false,
    uploading: false,
    saveBtnText: '添加资质',
    uploadText: '点击上传',
    list: [] as any[],
    form: {
      qualType: '',
      qualNo: '',
      expireDate: '',
    },
    fileUrl: '',
    previewUrl: '',
  },
  onLoad() {
    this.load()
  },
  async load() {
    try {
      const rows = await getQualifications()
      this.setData({ list: rows.map((r: any) => ({ ...r, qualStatusText: r.isBan ? '已失效' : '有效' })) })
    } catch (err: any) {
      toast(err.message || '加载失败')
    }
  },
  goBack() {
    wx.navigateBack()
  },
  onInput(e: any) {
    const field = e.currentTarget.dataset.field
    this.setData({ [`form.${field}`]: e.detail.value })
  },
  onDateChange(e: any) {
    this.setData({ 'form.expireDate': e.detail.value })
  },
  chooseImage() {
    if (this.data.uploading) return
    wx.chooseMedia({
      count: 1,
      mediaType: ['image'],
      sourceType: ['album', 'camera'],
      success: (res) => {
        const tempFilePath = res.tempFiles[0]?.tempFilePath
        if (!tempFilePath) return
        this.upload(tempFilePath)
      },
      fail: () => toast('选择图片失败'),
    })
  },
  async upload(tempFilePath: string) {
    this.setData({ uploading: true, uploadText: '上传中...' })
    try {
      const result = await uploadFile(tempFilePath)
      // fileUrl 保留后端相对地址（提交时用），previewUrl 经 resolveUrl 拼成完整展示地址（与商户端一致）
      this.setData({ fileUrl: result.url, previewUrl: resolveUrl(result.url) })
      toast('上传成功')
    } catch (err: any) {
      toast(err.message || '上传失败')
    } finally {
      this.setData({ uploading: false, uploadText: '点击上传' })
    }
  },
  previewImage() {
    if (!this.data.previewUrl) return
    wx.previewImage({ urls: [this.data.previewUrl], current: this.data.previewUrl })
  },
  previewListImage(e: any) {
    const url = e.currentTarget.dataset.url
    if (!url) return
    wx.previewImage({ urls: [url], current: url })
  },
  removeImage() {
    this.setData({ fileUrl: '', previewUrl: '', uploadText: '点击上传' })
  },
  onImgError() {
    // 列表图片加载失败时标记，WXML 会显示降级占位
  },
  onPreviewImgError() {
    // 上传预览图片加载失败时清除 URL，回退到上传占位
    this.setData({ fileUrl: '', previewUrl: '' })
    toast('图片加载失败，请重新上传')
  },
  async save() {
    if (this.data.saving) return
    const { qualType, qualNo, expireDate } = this.data.form
    if (!qualType.trim() || !qualNo.trim()) {
      toast('请填写资质类型和证书编号')
      return
    }
    if (!this.data.fileUrl) {
      toast('请上传资质证书图片')
      return
    }
    this.setData({ saving: true, saveBtnText: '添加中...' })
    try {
      await addQualification({
        qualType: qualType.trim(),
        qualNo: qualNo.trim(),
        expireDate,
        fileUrl: this.data.fileUrl,
      })
      toast('添加成功')
      this.setData({
        'form.qualType': '',
        'form.qualNo': '',
        'form.expireDate': '',
        fileUrl: '',
        previewUrl: '',
      })
      this.load()
    } catch (err: any) {
      toast(err.message || '添加失败')
    } finally {
      this.setData({ saving: false, saveBtnText: '添加资质' })
    }
  },
})
