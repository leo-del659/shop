import {
  getGoodsDetail,
  toggleGoodsStatus,
} from '../../utils/api'
import { Goods } from '../../utils/types'
import { getStockClass, getStockLabel, toast } from '../../utils/util'
import { isLogin } from '../../utils/auth'

interface GoodsView extends Goods {
  stockClass: string
  stockLabel: string
  statusLabel: string
  defaultEmoji: string
  launchTimeText: string
  /** 详情卡片展示的成本价文本（如 "20.93"），无采购价时为空串 */
  costPriceText: string
}

function formatLaunchTime(raw?: string): string {
  if (!raw) return '-'
  const t = String(raw).replace('T', ' ')
  return t.length > 16 ? t.slice(0, 16) : t
}

function fmtMoney(n: number): string {
  return Number(n || 0).toFixed(2)
}

Page({
  data: {
    id: '',
    idx: 0,
    goods: null as GoodsView | null,
    defaultEmoji: '\u{1F4E6}',

    submitting: false,
  },
  onLoad(options: any) {
    const id = (options.id || '').toString()
    const idx = parseInt(options.idx || '0', 10) || 0
    if (!id) {
      wx.showToast({ title: '缺少商品ID', icon: 'none' })
      return
    }
    this.setData({ id, idx })
    this.load()
  },
  onShow() {},
  async load() {
    if (!isLogin()) {
      wx.redirectTo({ url: '/pages/login/login' })
      return
    }
    try {
      const g = await getGoodsDetail(this.data.id, this.data.idx)
      if (!g) {
        wx.showToast({ title: '商品不存在', icon: 'none' })
        return
      }
      const costPrice = Number(g.costPrice || 0)
      const view: GoodsView = {
        ...g,
        stockClass: getStockClass(g.stock, g.safeStock || 50),
        stockLabel: getStockLabel(g.stock, g.safeStock || 50),
        statusLabel: g.status === 'published' ? '已上架' : g.status === 'draft' ? '草稿' : '已下架',
        defaultEmoji: '\u{1F4E6}',
        launchTimeText: formatLaunchTime(g.launchTime),
        costPriceText: costPrice > 0 ? fmtMoney(costPrice) : '',
      }
      this.setData({ goods: view, showRestock: false })
    } catch (err: any) {
      wx.showModal({
        title: '加载失败',
        content: err.message || '商品详情加载失败，请稍后重试',
        showCancel: false,
        success: () => {
          const pages = getCurrentPages()
          if (pages.length > 1) {
            wx.navigateBack()
          } else {
            wx.switchTab({ url: '/pages/goods/list' })
          }
        },
      })
    }
  },
  stopPropagation() {},
  goEdit() {
    wx.navigateTo({ url: '/pages/goods/add/add?id=' + this.data.id })
  },
  async onToggleOff() {
    if (this.data.submitting) return
    const g = this.data.goods
    if (!g) return
    const isPublished = g.status === 'published'
    const ok = await new Promise<boolean>((resolve) => {
      wx.showModal({
        title: isPublished ? '确认下架' : '确认上架',
        content: isPublished ? '下架后该商品不再展示，确认？' : '上架后该商品将对买家可见，确认？',
        success: (res) => resolve(res.confirm),
      })
    })
    if (!ok) return
    this.setData({ submitting: true })
    wx.showLoading({ title: '处理中' })
    try {
      await toggleGoodsStatus(this.data.id)
      toast(isPublished ? '已下架' : '已上架')
      this.load()
    } catch (err: any) {
      toast(err.message || '操作失败')
    } finally {
      wx.hideLoading()
      this.setData({ submitting: false })
    }
  },
})
