import { getAddressList, saveAddress, removeAddress, setDefaultAddress, AddressItem, AddressSaveDto } from '../../api/address'
import { getCachedAddresses, cacheAddresses } from '../../utils/addressCache'
import { requireLogin } from '../../app'

/** 页面实例附加字段：延迟返回定时器句柄，卸载时清理，避免对已销毁页面 navigateBack 告警 */
type AddressPageInstance = { backTimer?: ReturnType<typeof setTimeout> }

interface AddressForm {
  id: string
  name: string
  mobile: string
  province: string
  city: string
  district: string
  detailAddr: string
  isDefault: number
}

function buildFullAddress(a: AddressItem): string {
  return [a.province, a.city, a.district, a.detailAddr].filter(Boolean).join(' ')
}

Page({
  data: {
    selectMode: false,
    list: [] as (AddressItem & { fullAddress: string })[],
    showForm: false,
    form: {
      id: '',
      name: '',
      mobile: '',
      province: '',
      city: '',
      district: '',
      detailAddr: '',
      isDefault: 0
    } as AddressForm,
    submitting: false
  },

  onLoad(options: any) {
    this.setData({ selectMode: options && options.select === '1' })
  },

  onShow() {
    this.loadList()
  },

  async loadList() {
    if (!requireLogin()) return
    // 1) 先展示本地缓存（登出后保留的地址），实现即时恢复、无需等待网络
    const cached = getCachedAddresses()
    if (cached.length) {
      this.setData({
        list: cached.map(a => ({ ...a, fullAddress: buildFullAddress(a) }))
      })
    }
    // 2) 再向服务端拉取最新数据，覆盖本地缓存
    try {
      const list = await getAddressList()
      const decorated = (list || []).map(a => ({ ...a, fullAddress: buildFullAddress(a) }))
      this.setData({ list: decorated })
      cacheAddresses(list || [])
    } catch (e) {
      // 错误已在 request 层 toast；失败时保留已展示的缓存数据
    }
  },

  openAdd() {
    this.setData({
      showForm: true,
      form: { id: '', name: '张三', mobile: '13800000001', province: '广东省', city: '深圳市', district: '南山区', detailAddr: '科技园南路1号', isDefault: 1 }
    })
  },

  openEdit(e: any) {
    const item = this.data.list[e.currentTarget.dataset.index]
    if (!item) return
    this.setData({
      showForm: true,
      form: {
        id: item.id,
        name: item.name || '',
        mobile: item.mobile || '',
        province: item.province || '',
        city: item.city || '',
        district: item.district || '',
        detailAddr: item.detailAddr || '',
        isDefault: item.isDefault || 0
      }
    })
  },

  closeForm() {
    this.setData({ showForm: false })
  },

  onField(e: any) {
    const field = e.currentTarget.dataset.field
    this.setData({ [`form.${field}`]: e.detail })
  },

  onDefault(e: any) {
    this.setData({ 'form.isDefault': e.detail ? 1 : 0 })
  },

  async onSubmit() {
    if (this.data.submitting) return
    const f = this.data.form
    if (!f.name || !f.mobile || !f.detailAddr) {
      wx.showToast({ title: '请填写收件人、手机号与详细地址', icon: 'none' })
      return
    }
    this.setData({ submitting: true })
    try {
      const dto: AddressSaveDto = {
        id: f.id || undefined,
        name: f.name,
        mobile: f.mobile,
        province: f.province,
        city: f.city,
        district: f.district,
        detailAddr: f.detailAddr,
        isDefault: f.isDefault
      }
      await saveAddress(dto)
      wx.showToast({ title: '保存成功', icon: 'success' })
      this.setData({ showForm: false })
      this.loadList()
    } catch (e) {
      // 错误已在 request 层 toast
    } finally {
      this.setData({ submitting: false })
    }
  },

  async onSetDefault(e: any) {
    const id = e.currentTarget.dataset.id
    try {
      await setDefaultAddress(id)
      wx.showToast({ title: '已设为默认', icon: 'success' })
      this.loadList()
    } catch (err) {
      // 错误已在 request 层 toast
    }
  },

  onDelete(e: any) {
    const id = e.currentTarget.dataset.id
    wx.showModal({
      title: '提示',
      content: '确定删除该地址？',
      success: async (res) => {
        if (res.confirm) {
          try {
            await removeAddress(id)
            wx.showToast({ title: '已删除', icon: 'success' })
            this.loadList()
          } catch (err) {
            // 错误已在 request 层 toast
          }
        }
      }
    })
  },

  onPick(e: any) {
    if (!this.data.selectMode) return
    const item = this.data.list[e.currentTarget.dataset.index]
    if (!item) return
    const pages = getCurrentPages()
    const prev = pages[pages.length - 2]
    if (prev && prev.route && prev.route.indexOf('checkout') >= 0) {
      prev.setData({ address: item })
    }
    wx.navigateBack()
  },

  onConfirm(e: any) {
    const item = this.data.list[e.currentTarget.dataset.index]
    if (!item) return
    wx.showToast({ title: '已确认使用此地址', icon: 'success' })
    // 如果是从结算页选地址，回传并返回
    const pages = getCurrentPages()
    const prev = pages[pages.length - 2]
    if (prev && prev.route && prev.route.indexOf('checkout') >= 0) {
      prev.setData({ address: item })
      const self = this as AddressPageInstance
      if (self.backTimer) clearTimeout(self.backTimer)
      self.backTimer = setTimeout(() => wx.navigateBack(), 500)
    }
  },

  goCheckout() {
    wx.navigateTo({ url: '/pages/checkout/checkout?select=1' })
  },

  noop() {
    // 空操作：阻止卡片点击事件冒泡（用于底部操作栏）
  },

  onUnload() {
    // 页面销毁时清理挂起的延迟返回定时器，避免对已销毁页面调用 navigateBack 告警
    const self = this as AddressPageInstance
    if (self.backTimer) clearTimeout(self.backTimer)
  }
})
