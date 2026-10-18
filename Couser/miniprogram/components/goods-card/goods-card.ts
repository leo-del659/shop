import { formatSoldCount } from '../../utils/util'

Component({
  properties: {
    goods: {
      type: Object,
      value: {}
    },
    /** 当前购物车中该商品的数量，由父页面传入 */
    quantity: {
      type: Number,
      value: 0
    },
    /** 展示模式：grid（首页网格）| list（列表：左图右文） */
    mode: {
      type: String,
      value: 'grid'
    }
  },
  data: {
    imgErr: false,
    soldText: '0'
  },
  observers: {
    'goods': function (this: WechatMiniprogram.Component.TrivialInstance, g: any) {
      this.setData({ soldText: formatSoldCount(g && g.soldCount) })
    }
  },
  methods: {
    onTap(this: WechatMiniprogram.Component.TrivialInstance) {
      this.triggerEvent('tap', { id: this.data.goods.id })
    },
    onImageError(this: WechatMiniprogram.Component.TrivialInstance) {
      this.setData({ imgErr: true })
    },
    noop() {
      // 阻止冒泡
    },
    /** 是否缺货（stock<=0 视为缺货；undefined 视为不限，兼容无库存字段的旧数据） */
    isSoldOut(this: WechatMiniprogram.Component.TrivialInstance) {
      const s = this.data.goods.stock
      return typeof s === 'number' && s <= 0
    },
    /** 是否已达库存上限（购物车数量 >= 库存） */
    isMaxed(this: WechatMiniprogram.Component.TrivialInstance) {
      const s = this.data.goods.stock
      return typeof s === 'number' && s > 0 && this.data.quantity >= s
    },
    onAdd(this: WechatMiniprogram.Component.TrivialInstance, e: any) {
      if (this.isSoldOut()) {
        wx.showToast({ title: '商品缺货', icon: 'none' })
        return
      }
      e && e.stopPropagation && e.stopPropagation()
      this.triggerEvent('openSku', { goods: this.data.goods })
    },
    onInc(this: WechatMiniprogram.Component.TrivialInstance, e: any) {
      if (this.isSoldOut()) {
        wx.showToast({ title: '商品缺货', icon: 'none' })
        return
      }
      if (this.isMaxed()) {
        wx.showToast({ title: '库存不足', icon: 'none' })
        return
      }
      e && e.stopPropagation && e.stopPropagation()
      this.triggerEvent('incCart', { goodsId: this.data.goods.id })
    },
    onDec(this: WechatMiniprogram.Component.TrivialInstance, e: any) {
      e && e.stopPropagation && e.stopPropagation()
      this.triggerEvent('decCart', { goodsId: this.data.goods.id })
    }
  }
})
