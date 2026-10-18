import { getAddress, saveAddress } from '../../../utils/api'
import { toast } from '../../../utils/util'
import { isLogin } from '../../../utils/auth'

Page({
  data: {
    id: '',
    name: '',
    mobile: '',
    province: '北京市',
    city: '北京市',
    district: '朝阳区',
    detailAddr: '',
    isDefault: 0,
    saving: false,
    /** 微信内置区域选择器列出的省市区字符串 */
    region: ['北京市', '北京市', '朝阳区'],
  },
  onLoad(options: any) {
    if (!isLogin()) {
      wx.redirectTo({ url: '/pages/login/login' })
      return
    }
    const id = (options.id || '').toString()
    if (id) {
      this.setData({ id })
      this.loadOne(id)
    } else {
      wx.setNavigationBarTitle({ title: '新增地址' })
    }
  },
  async loadOne(id: string) {
    wx.showLoading({ title: '加载中' })
    try {
      const a = await getAddress(id)
      if (!a) {
        toast('地址不存在')
        return
      }
      const region: string[] = [
        a.province || '北京市',
        a.city || '北京市',
        a.district || '朝阳区',
      ]
      this.setData({
        id: a.id || id,
        name: a.name || '',
        mobile: a.mobile || '',
        province: a.province || region[0],
        city: a.city || region[1],
        district: a.district || region[2],
        detailAddr: a.detailAddr || '',
        isDefault: a.isDefault || 0,
        region,
      })
    } catch (err: any) {
      toast(err.message || '加载失败')
    } finally {
      wx.hideLoading()
    }
  },
  goBack() {
    const pages = getCurrentPages()
    if (pages.length > 1) {
      wx.navigateBack()
    } else {
      wx.redirectTo({ url: '/pages/profile/address/list' })
    }
  },
  onInputName(e: any) { this.setData({ name: e.detail.value }) },
  onInputMobile(e: any) { this.setData({ mobile: e.detail.value }) },
  onInputAddr(e: any) { this.setData({ detailAddr: e.detail.value }) },
  onPickRegion(e: any) {
    const v = e.detail.value || []
    this.setData({
      region: v,
      province: v[0] || '',
      city: v[1] || '',
      district: v[2] || '',
    })
  },
  onToggleDefault(_e: any) {
    this.setData({ isDefault: this.data.isDefault === 1 ? 0 : 1 })
  },
  validate(): string | null {
    if (!this.data.name.trim()) return '请输入收件人'
    const m = this.data.mobile.trim()
    if (!m) return '请输入手机号'
    if (!/^1\d{10}$/.test(m)) return '手机号格式不正确（11 位数字，1 开头）'
    if (!this.data.detailAddr.trim()) return '请输入详细地址'
    if (!this.data.province) return '请选择省/市/区'
    return null
  },
  async onSave() {
    // 防双击：保存进行中再次点击直接忽略，避免重复提交产生重复地址
    if (this.data.saving) return
    const err = this.validate()
    if (err) {
      toast(err)
      return
    }
    this.setData({ saving: true })
    try {
      await saveAddress({
        id: this.data.id || undefined,
        name: this.data.name.trim(),
        mobile: this.data.mobile.trim(),
        province: this.data.province,
        city: this.data.city,
        district: this.data.district,
        detailAddr: this.data.detailAddr.trim(),
        isDefault: this.data.isDefault,
      })
      toast(this.data.id ? '已保存' : '已添加')
      setTimeout(() => wx.navigateBack(), 400)
    } catch (e: any) {
      toast(e.message || '保存失败')
    } finally {
      this.setData({ saving: false })
    }
  },
})
