import { submitReview } from '../../api/review'
import { getProfile } from '../../api/member'
import { requireLogin } from '../../app'
import { resolveUrl, uploadFile } from '../../utils/request'

/** 评价标签（与服务端 tags 字段值保持一致，用于按标签筛选评价列表） */
const TAG_OPTIONS = ['品质好', '品质一般', '服务好', '发货快', '态度差', '发货慢']

/** 单次最多上传图片数 */
const MAX_IMAGES = 3

Page({
  data: {
    login: false,
    orderId: '',
    orderNo: '',
    goodsId: '',
    skuId: '',
    goodsName: '',
    goodsImg: '',
    skuName: '',
    memberName: '',
    rating: 5,
    merchantReview: '',
    anonymous: false,
    tagOptions: TAG_OPTIONS.map(name => ({ name, selected: false })),
    /** 本地预览用的临时地址 */
    tempImages: [] as string[],
    submitting: false
  },

  onLoad(options: any) {
    this.setData({
      orderId: options.orderId || '',
      orderNo: options.orderNo || '',
      goodsId: options.goodsId || '',
      skuId: options.skuId || '',
      goodsName: options.goodsName || '',
      goodsImg: resolveUrl(options.goodsImg),
      skuName: options.skuName || ''
    })
    if (!requireLogin()) return
    this.setData({ login: true })
    this.loadProfile()
  },

  /** 拉取会员昵称，作为评价作者名（匿名时不会使用） */
  async loadProfile() {
    try {
      const p = await getProfile()
      if (p && p.name) this.setData({ memberName: p.name })
    } catch (e) {
      // 忽略
    }
  },

  onStar(e: any) {
    this.setData({ rating: Number(e.currentTarget.dataset.v) || 5 })
  },

  onMerchantReview(e: any) {
    this.setData({ merchantReview: e.detail.value })
  },

  toggleAnonymous() {
    this.setData({ anonymous: !this.data.anonymous })
  },

  toggleTag(e: any) {
    const tag = e.currentTarget.dataset.tag
    console.log('[toggleTag] 点击标签:', tag)
    if (!tag) return
    const tagOptions = this.data.tagOptions.slice()
    const item = tagOptions.find((t: any) => t.name === tag)
    if (item) {
      item.selected = !item.selected
      console.log('[toggleTag] 当前已选:', tagOptions.filter((t: any) => t.selected).map((t: any) => t.name))
      this.setData({ tagOptions })
    }
  },

  async chooseImage() {
    try {
      const res = await wx.chooseMedia({
        count: MAX_IMAGES - this.data.tempImages.length,
        mediaType: ['image'],
        sourceType: ['album', 'camera'],
        sizeType: ['compressed']
      })
      const tempImages = this.data.tempImages.slice()
      for (const f of res.tempFiles) tempImages.push(f.tempFilePath)
      this.setData({ tempImages })
    } catch (e) {
      // 用户取消选择
    }
  },

  removeImage(e: any) {
    const idx = Number(e.currentTarget.dataset.idx)
    const tempImages = this.data.tempImages.slice()
    tempImages.splice(idx, 1)
    this.setData({ tempImages })
  },

  async onSubmit() {
    if (this.data.submitting) return
    if (!requireLogin()) return
    if (this.data.rating < 1 || this.data.rating > 5) {
      wx.showToast({ title: '请先评分', icon: 'none' })
      return
    }
    this.setData({ submitting: true })
    try {
      // 先上传图片（如有）
      let images: string[] = []
      if (this.data.tempImages.length) {
        wx.showLoading({ title: '上传图片...' })
        try {
          images = await Promise.all(
            this.data.tempImages.map(t => uploadFile(t).then(r => r.url))
          )
        } finally {
          wx.hideLoading()
        }
      }
      await submitReview({
        goods_id: this.data.goodsId,
        sku_id: this.data.skuId || undefined,
        order_id: this.data.orderId || undefined,
        // 匿名时 member_name 不提交,由后端写入"匿名用户"
        member_name: this.data.anonymous ? undefined : (this.data.memberName || undefined),
        rating: this.data.rating,
        // 截图里没有"商品内容"字段,把对商家评价作为主评价文本；如未填则不传
        content: this.data.merchantReview || undefined,
        images: images.length ? images.join(',') : undefined,
        tags: this.data.tagOptions.filter((t: any) => t.selected).map((t: any) => t.name).join(',') || undefined,
        merchant_review: this.data.merchantReview || undefined,
        is_anonymous: this.data.anonymous ? 1 : 0
      })
      wx.showToast({ title: '评价成功，等待审核', icon: 'success' })
      this.setData({ submitting: false })
      // 直接更新上一页（订单详情）的已评价标记，避免返回后 onShow 重新请求导致页面闪烁
      try {
        const pages = getCurrentPages()
        const prev = pages[pages.length - 2]
        if (prev && (prev as any).route && (prev as any).route.indexOf('order-detail') >= 0) {
          const reviewed = { ...((prev as any).data.reviewed || {}) }
          reviewed[this.data.goodsId + '_' + (this.data.skuId || '')] = true
          ;(prev as any).setData({ reviewed })
        }
      } catch (e) {
        // 忽略
      }
      // 评价成功后立即返回，去掉原先 600ms 延迟，避免「停留一下再跳」的突兀闪烁感
      wx.navigateBack()
    } catch (e) {
      // 错误已在 request 层 toast
      this.setData({ submitting: false })
    }
  }
})