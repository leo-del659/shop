import { getBank, updateBank } from '../../../utils/api'
import { toast } from '../../../utils/util'

Page({
  data: {
    saving: false,
    saveBtnText: '保存',
    form: {
      bankName: '',
      bankAccount: '',
      accountName: '',
    },
  },
  onLoad() {
    this.load()
  },
  async load() {
    try {
      const bank = await getBank()
      if (bank) {
        this.setData({
          'form.bankName': bank.bankName || '',
          'form.bankAccount': bank.bankAccount || '',
          'form.accountName': bank.accountName || '',
        })
      }
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
  async save() {
    if (this.data.saving) return
    const { bankName, bankAccount, accountName } = this.data.form
    if (!bankName.trim() || !bankAccount.trim() || !accountName.trim()) {
      toast('请填写完整的银行卡信息')
      return
    }
    this.setData({ saving: true, saveBtnText: '保存中...' })
    try {
      await updateBank({
        bankName: bankName.trim(),
        bankAccount: bankAccount.trim(),
        accountName: accountName.trim(),
      })
      toast('保存成功')
      setTimeout(() => wx.navigateBack(), 600)
    } catch (err: any) {
      toast(err.message || '保存失败')
    } finally {
      this.setData({ saving: false, saveBtnText: '保存' })
    }
  },
})
