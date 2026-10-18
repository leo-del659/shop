import { getGoodsDetail } from '../../api/shop'
import { camelCaseKeys } from '../../utils/util'
import { resolveUrl } from '../../utils/request'

function firstCover(url?: string): string {
  if (!url) return ''
  return resolveUrl(url.split(',')[0].trim())
}

Component({
  options: {
    styleIsolation: 'shared'
  },

  properties: {
    /** 是否显示弹窗 */
    visible: {
      type: Boolean,
      value: false
    },
    /** 商品基本信息（必须包含 id、goodsName、coverImg、spec、salePrice、price） */
    goods: {
      type: Object,
      value: null as any
    },
    /** 操作类型：cart=加入购物车，buy=立即购买 */
    action: {
      type: String,
      value: 'buy'
    },
    /** 初始数量 */
    initialQty: {
      type: Number,
      value: 1
    }
  },

  data: {
    loading: false,
    coverImg: '',
    skus: [] as any[],
    specs: [] as any[],
    specGroups: [] as any[],
    selectedSkuId: '',
    selectedSkuName: '',
    /** 当前选中 SKU 的库存（用于头部展示） */
    selectedStock: 0,
    qty: 1,
    displayPrice: '0.0',
    /** 当前选中 SKU 的库存上限（步进器用）；缺省 0 表示未加载/无库存 */
    maxStock: 0,
    /** 整件商品是否缺货（所有 SKU 库存均为 0，或无 SKU 且商品库存为 0） */
    soldOut: false
  },

  observers: {
    'visible,goods': function (this: WechatMiniprogram.Component.TrivialInstance, visible: boolean, goods: any) {
      if (visible && goods && goods.id) {
        this.setData({
          qty: this.properties.initialQty || 1,
          coverImg: firstCover(goods.coverImg)
        })
        this.loadSku(goods.id)
      }
    }
  },

  methods: {
    /** 加载商品 SKU/规格数据 */
    async loadSku(this: WechatMiniprogram.Component.TrivialInstance, goodsId: string) {
      this.setData({ loading: true })
      try {
        const raw = await getGoodsDetail(goodsId)
        const res: any = camelCaseKeys(raw)
        const good = res.good || {}
        const skus = (res.skus || []).map((k: any) => ({
          id: this.toId(k.id),
          specId: this.toId(k.specId),
          skuName: k.skuName,
          price: this.toNum(k.price),
          stock: this.toNum(k.stock)
        }))
        const specs = res.specs || []

        // 缺省单价：SKU 无价时用商品级价格兜底
        const fallbackUnitPrice = this.toNum(good.salePrice || good.price || 0)
        const withPriceText = (k: any) => ({
          ...k,
          priceText: (k.price > 0 ? k.price : fallbackUnitPrice).toFixed(2)
        })

        const specGroups = specs
          .map((s: any) => ({
            spec: { ...s, id: this.toId(s.id) },
            options: skus.filter((k: any) => k.specId === this.toId(s.id)).map(withPriceText)
          }))
          .filter((g: any) => g.options.length > 0)

        // 如果没有规格组，直接把全部 SKU 作为默认组
        const finalGroups = specGroups.length
          ? specGroups
          : skus.length
            ? [{ spec: { id: '', specName: '规格' }, options: skus.map(withPriceText) }]
            : []

        const firstSku = skus.length ? skus[0] : null
        const unitPrice = firstSku && firstSku.price > 0
          ? firstSku.price
          : this.toNum(good.salePrice || good.price || 0)

        // 库存：有 SKU 用首个 SKU 的库存；无 SKU 用商品级库存
        const totalSkuStock = skus.reduce((s: number, k: any) => s + (this.toNum(k.stock) || 0), 0)
        const goodsStock = this.toNum(good.stock)
        const maxStock = skus.length ? (firstSku ? this.toNum(firstSku.stock) : 0) : goodsStock
        const soldOut = skus.length ? totalSkuStock <= 0 : goodsStock <= 0

        const displayPrice = (unitPrice * this.data.qty).toFixed(1)
        this.setData({
          skus,
          specs,
          specGroups: finalGroups,
          selectedSkuId: firstSku ? firstSku.id : '',
          selectedSkuName: firstSku?.skuName || good.spec || '',
          selectedStock: firstSku ? this.toNum(firstSku.stock) : goodsStock,
          displayPrice,
          maxStock,
          soldOut,
          loading: false
        })
        this.emitChange(firstSku ? firstSku.id : '', firstSku?.skuName || good.spec || '', displayPrice)
      } catch (err) {
        console.error('[sku-popup] 加载规格失败', err)
        this.setData({ loading: false })
      }
    },

    /** 选中规格 */
    onPickSpec(this: WechatMiniprogram.Component.TrivialInstance, e: any) {
      const id = this.toId(e.currentTarget.dataset.id)
      const sku = this.data.skus.find((k: any) => k.id === id)
      if (!sku) return
      if (sku.stock <= 0) {
        wx.showToast({ title: '该规格暂时缺货', icon: 'none' })
        return
      }
      const unitPrice = sku.price > 0 ? sku.price : this.toNum(this.properties.goods?.salePrice || this.properties.goods?.price || 0)
      // 切换规格时，把上限同步为该规格库存，并把已选数量收敛到新上限内
      let qty = this.data.qty
      if (sku.stock > 0 && qty > sku.stock) qty = sku.stock
      const displayPrice = (unitPrice * qty).toFixed(1)
      this.setData({
        selectedSkuId: id,
        selectedSkuName: sku.skuName,
        selectedStock: sku.stock,
        maxStock: sku.stock,
        qty,
        displayPrice
      })
      this.emitChange(id, sku.skuName, displayPrice)
    },

    /** 数量变化 */
    onStep(this: WechatMiniprogram.Component.TrivialInstance, e: any) {
      let qty = e.detail
      if (qty < 1) qty = 1
      // 不能超过当前选中 SKU 的最大库存
      const max = this.data.maxStock
      if (max > 0 && qty > max) qty = max
      const sku = this.data.skus.find((k: any) => k.id === this.data.selectedSkuId)
      let unitPrice = sku && sku.price > 0 ? sku.price : this.toNum(this.properties.goods?.salePrice || this.properties.goods?.price || 0)
      const displayPrice = (unitPrice * qty).toFixed(1)
      this.setData({
        qty,
        displayPrice
      })
      this.emitChange(this.data.selectedSkuId, this.data.selectedSkuName, displayPrice)
    },

    /** 选择变化时通知父页面（用于详情页顶部同步） */
    emitChange(this: WechatMiniprogram.Component.TrivialInstance, skuId: string, skuName: string, displayPrice: string) {
      this.triggerEvent('change', {
        skuId,
        skuName,
        displayPrice,
        qty: this.data.qty
      })
    },

    /** 关闭弹窗 */
    close(this: WechatMiniprogram.Component.TrivialInstance) {
      this.triggerEvent('close')
    },

    /** 阻止冒泡 */
    stopProp() {},

    /** 确定 */
    confirm(this: WechatMiniprogram.Component.TrivialInstance) {
      const goods = this.properties.goods
      // 售罄拦截：整件商品无库存时禁止下单
      if (this.data.soldOut) {
        wx.showToast({ title: '该商品已售罄', icon: 'none' })
        return
      }
      let qty = this.data.qty
      if (qty < 1) qty = 1
      // 兜底：确认下单时再次收敛到最大库存，防止越界
      if (this.data.maxStock > 0 && qty > this.data.maxStock) qty = this.data.maxStock
      const sku = this.data.skus.find((k: any) => k.id === this.data.selectedSkuId) || null
      const group = this.data.specGroups.find((g: any) => g.options.some((o: any) => o.id === this.data.selectedSkuId))
      // price 必须传「单价」：checkout 页会再乘以 buyNum 计算总价，传总价会导致金额二次翻倍
      const unitPrice = sku && sku.price > 0 ? sku.price : this.toNum(goods?.salePrice || goods?.price || 0)
      this.triggerEvent('confirm', {
        action: this.properties.action,
        goods,
        skuId: this.data.selectedSkuId,
        skuName: this.data.selectedSkuName,
        specName: group ? group.spec.specName : '',
        price: unitPrice,
        unitPrice,
        qty,
        stock: this.data.maxStock,
        coverImg: goods?.coverImg || ''
      })
    },

    toNum(val: any): number {
      if (typeof val === 'number') return val
      const s = String(val || '')
      const n = parseFloat(s.replace(/[^\d.]/g, ''))
      return isNaN(n) ? 0 : n
    },

    toId(val: any): string {
      return val == null ? '' : String(val)
    }
  }
})
