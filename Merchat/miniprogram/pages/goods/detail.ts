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
  /** 规格明细行（多规格商品） */
  skuRows: SkuRowView[]
  /** 规格组名称，如「重量」 */
  specGroupName: string
  /** 多规格价格区间文本 */
  specPriceText: string
}

/** 规格明细行（供 WXML 展示） */
interface SkuRowView {
  name: string
  code: string
  priceText: string
  stockText: string
  stockCls: string
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
  /** 是否已展示过（区分首次进入与从编辑页返回） */
  _shown: false,
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
  onShow() {
    // 从「修改」页返回时重新拉取，保证库存 / 规格 / 价格等数据与后台一致
    // （首次进入 onLoad 已加载过，这里跳过避免重复请求）
    if (this._shown) {
      this.load()
    } else {
      this._shown = true
    }
  },
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
      // 多规格：规格明细行 + 价格区间
      const skus = g.skus || []
      const skuRows: SkuRowView[] = skus.map((s) => ({
        name: s.skuName || '-',
        code: s.skuCode || '',
        priceText: fmtMoney(s.price),
        stockText: String(s.stock),
        stockCls: s.stock <= 0 ? 'danger' : s.stock < 20 ? 'warn' : 'normal',
      }))
      const priceMin = Number(g.priceMin ?? g.price ?? 0)
      const priceMax = Number(g.priceMax ?? g.price ?? 0)
      const view: GoodsView = {
        ...g,
        stockClass: getStockClass(g.stock, g.safeStock || 50),
        stockLabel: getStockLabel(g.stock, g.safeStock || 50),
        statusLabel: g.status === 'published' ? '已上架' : g.status === 'draft' ? '草稿' : '已下架',
        defaultEmoji: '\u{1F4E6}',
        launchTimeText: formatLaunchTime(g.launchTime),
        costPriceText: costPrice > 0 ? fmtMoney(costPrice) : '',
        skuRows,
        specGroupName: (g.specs && g.specs[0] && g.specs[0].specName) || '规格',
        specPriceText: priceMax > priceMin ? priceMin.toFixed(2) + ' - ' + priceMax.toFixed(2) : priceMin.toFixed(2),
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
