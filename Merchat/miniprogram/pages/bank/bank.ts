import { getBank, updateBank, BankInfo } from '../../utils/api'
import { isLogin } from '../../utils/auth'
import { toast } from '../../utils/util'

Page({
  data: {
    saving: false,
    bankName: '',
    bankAccount: '',
    accountName: '',
  },
  onShow() {
    if (!isLogin()) {
      wx.redirectTo({ url: '/pages/login/login' })
      return
    }
    this.load()
  },
  async load() {
    try {
      const bank: BankInfo | null = await getBank()
      if (bank) {
        this.setData({
          bankName: bank.bankName || '',
          bankAccount: bank.bankAccount || '',
          accountName: bank.accountName || '',
        })
      }
    } catch (e: any) {
      toast('加载失败：' + (e.message || '未知错误'))
    }
  },
  onInput(e: any) {
    const field = e.currentTarget.dataset.field as string
    this.setData({ [field]: e.detail.value })
  },
  async save() {
    if (this.data.saving) return
    const { bankName, bankAccount, accountName } = this.data
    if (!bankName.trim() || !bankAccount.trim() || !accountName.trim()) {
      toast('请填写完整的开户行、账号和开户名')
      return
    }
    this.setData({ saving: true })
    try {
      await updateBank({
        bankName: bankName.trim(),
        bankAccount: bankAccount.trim(),
        accountName: accountName.trim(),
      })
      toast('保存成功')
      setTimeout(() => wx.navigateBack(), 600)
    } catch (e: any) {
      toast(e.message || '保存失败')
    } finally {
      this.setData({ saving: false })
    }
  },
})
