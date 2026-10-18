import { getWithdrawals } from '../../utils/api'
import { Withdraw } from '../../utils/types'
import { isLogin } from '../../utils/auth'
import { WITHDRAW_STATUS, toast } from '../../utils/util'

Page({
  data: {
    list: [] as any[],
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
      const rows: Withdraw[] = await getWithdrawals()
      const list = (rows || []).map((w) => {
        const s = WITHDRAW_STATUS[w.status] || { label: w.status, cls: '' }
        return {
          ...w,
          wdLabel: s.label,
          wdCls: s.cls,
          amountText: '-¥' + Number(w.amount || 0).toFixed(2),
          timeText: w.time ? String(w.time).slice(0, 16).replace('T', ' ') : '',
        }
      })
      this.setData({ list })
    } catch (e: any) {
      toast('加载失败：' + (e.message || '未知错误'))
    }
  },
})
