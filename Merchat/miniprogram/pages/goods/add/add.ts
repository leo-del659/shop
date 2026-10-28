import { addGoods, updateGoods, getGoodsDetail, getGoodsSpecs, GoodsSpecPayload } from '../../../utils/api'
import { uploadProductImage, resolveUrl } from '../../../utils/request'
import { getLocalMerchant } from '../../../utils/auth'
import { toast } from '../../../utils/util'
import { CATEGORIES } from '../../../utils/categories'

/** 规格表行（编辑态，值均为字符串便于 input 双向绑定） */
interface SpecSkuRow {
  name: string
  price: string
  stock: string
  code: string
}

Page({
  data: {
    saving: false,
    uploading: false,
    isEdit: false,
    goodsId: '',
    categories: CATEGORIES,
    categoryIndex: 0,
    form: {
      name: '',
      price: '',
      marketPrice: '',
      stock: '',
      unit: '',
      spec: '',
      description: '',
    },
    coverImg: '',
    coverImgFull: '',
    // ---- 多规格 ----
    /** 是否开启多规格 */
    specEnabled: false,
    /** 规格组名称，如「规格」「重量」 */
    specGroupName: '规格',
    /** 规格值列表 */
    specValues: [] as string[],
    /** 新规格值输入框 */
    specInput: '',
    /** 规格表（与 specValues 一一对应） */
    specSkus: [] as SpecSkuRow[],
    /** 批量设置面板 */
    batchOpen: false,
    batchPrice: '',
    batchStock: '',
    // computed display texts (avoid single-quote literals in WXML)
    uploadIco: '\u{1F4F7}',
    uploadText: '点击上传图片',
    tipsText: '提交审核后将自动关联到当前商户',
    saveBtnText: '提交审核',
  },
  async onLoad(options: any) {
    const id = options.id
    if (id) {
      this.setData({ isEdit: true, goodsId: id })
      this._updateTexts(true, false)
      wx.setNavigationBarTitle({ title: '编辑商品' })
      await this.loadGoods(id)
    } else {
      this._updateTexts(false, false)
    }
  },
  async loadGoods(id: string) {
    try {
      const g = await getGoodsDetail(id)
      if (!g) {
        toast('商品不存在')
        return
      }
      const catIdx = CATEGORIES.findIndex((c) => c.id === g.category)
      this.setData({
        categoryIndex: catIdx >= 0 ? catIdx : 0,
        form: {
          name: g.name || '',
          price: g.price ? String(g.price) : '',
          marketPrice: g.marketPrice ? String(g.marketPrice) : '',
          stock: g.stock != null ? String(g.stock) : '',
          unit: g.unit || '',
          spec: g.spec || '',
          description: g.description || '',
        },
        coverImg: g.coverImg || '',
        coverImgFull: resolveUrl(g.coverImg),
      })
      // 回填已有多规格
      await this.loadSpecs(id)
    } catch (err: any) {
      toast(err.message || '加载商品失败')
    }
  },

  /** 读取商品已有规格并回填到规格编辑器 */
  async loadSpecs(id: string) {
    try {
      const res = await getGoodsSpecs(id)
      const skus = (res && res.skus) || []
      if (!skus.length) return
      const groupName = ((res.specs && res.specs[0] && res.specs[0].specName) || '规格') as string
      this.setData({
        specEnabled: true,
        specGroupName: groupName,
        specValues: skus.map((s: any) => String(s.skuName || '')),
        specSkus: skus.map((s: any) => ({
          name: String(s.skuName || ''),
          price: String(s.price ?? ''),
          stock: String(s.stock ?? ''),
          code: String(s.skuCode || ''),
        })),
      })
    } catch (err) {
      // 规格读取失败不影响商品其他信息
    }
  },
  goBack() {
    wx.navigateBack()
  },
  onInput(e: any) {
    const field = e.currentTarget.dataset.field
    this.setData({ [`form.${field}`]: e.detail.value })
  },
  onCategoryChange(e: any) {
    this.setData({ categoryIndex: e.detail.value })
  },

  // ---------- 多规格 ----------

  /** 开启/关闭多规格：关闭时把规格汇总回填到商品售价与库存 */
  onToggleSpec() {
    const enabled = !this.data.specEnabled
    if (!enabled) {
      // 关闭前先汇总一次，避免商品售价/库存变空
      const agg = this.aggregateSkus()
      this.setData({
        specEnabled: false,
        specValues: [],
        specSkus: [],
        specInput: '',
        batchOpen: false,
        'form.price': agg.price > 0 ? String(agg.price) : this.data.form.price,
        'form.stock': String(agg.stock),
      })
      return
    }
    this.setData({ specEnabled: true, specGroupName: this.data.specGroupName || '规格' })
  },

  /** 规格组名称输入 */
  onSpecGroupInput(e: any) {
    this.setData({ specGroupName: e.detail.value })
  },

  /** 规格值输入框 */
  onSpecInput(e: any) {
    this.setData({ specInput: e.detail.value })
  },

  /** 添加规格值（同时生成一行规格表） */
  onAddSpecValue() {
    const name = (this.data.specInput || '').trim()
    if (!name) {
      toast('请输入规格值')
      return
    }
    if (this.data.specValues.indexOf(name) >= 0) {
      toast('该规格值已存在')
      return
    }
    const rows = this.data.specSkus.slice()
    rows.push({
      name,
      price: this.data.form.price || '',
      stock: this.data.form.stock || '',
      code: '',
    })
    this.setData({
      specValues: this.data.specValues.concat(name),
      specSkus: rows,
      specInput: '',
    })
  },

  /** 删除规格值（同步删除对应规格行） */
  onRemoveSpecValue(e: any) {
    const name = e.currentTarget.dataset.name
    this.setData({
      specValues: this.data.specValues.filter((v) => v !== name),
      specSkus: this.data.specSkus.filter((s) => s.name !== name),
    })
  },

  /** 规格表单元格编辑（售价/库存/编码） */
  onSkuInput(e: any) {
    const index = Number(e.currentTarget.dataset.index)
    const field = e.currentTarget.dataset.field as keyof SpecSkuRow
    const rows = this.data.specSkus.slice()
    const row = Object.assign({}, rows[index])
    row[field] = e.detail.value
    rows[index] = row
    this.setData({ specSkus: rows })
  },

  /** 打开/关闭批量设置面板 */
  onToggleBatch() {
    this.setData({ batchOpen: !this.data.batchOpen })
  },

  /** 批量设置输入 */
  onBatchInput(e: any) {
    const field = e.currentTarget.dataset.field
    this.setData({ [field === 'price' ? 'batchPrice' : 'batchStock']: e.detail.value })
  },

  /** 应用批量设置到全部规格行 */
  onApplyBatch() {
    const { batchPrice, batchStock } = this.data
    if (!batchPrice && !batchStock) {
      toast('请填写统一售价或库存')
      return
    }
    const rows = this.data.specSkus.map((s) => ({
      ...s,
      price: batchPrice !== '' ? batchPrice : s.price,
      stock: batchStock !== '' ? batchStock : s.stock,
    }))
    this.setData({ specSkus: rows, batchOpen: false, batchPrice: '', batchStock: '' })
    toast('已应用')
  },

  /** 清空全部规格 */
  onClearSpecs() {
    this.setData({ specValues: [], specSkus: [], specInput: '', batchOpen: false })
  },

  /** 汇总规格：最低价 / 库存之和 */
  aggregateSkus(): { price: number; stock: number } {
    const prices: number[] = []
    let stock = 0
    for (const s of this.data.specSkus) {
      const p = parseFloat(s.price)
      if (!isNaN(p) && p > 0) prices.push(p)
      const st = parseInt(s.stock, 10)
      if (!isNaN(st) && st > 0) stock += st
    }
    return { price: prices.length ? Math.min.apply(null, prices) : 0, stock }
  },
  async chooseImage() {
    if (this.data.uploading) return
    try {
      const res = await wx.chooseMedia({
        count: 1,
        mediaType: ['image'],
        sourceType: ['album', 'camera'],
      })
      const tempFile = res.tempFiles[0]
      if (!tempFile?.tempFilePath) {
        toast('未选择图片')
        return
      }
      this.setData({ uploading: true })
      this._updateTexts(this.data.isEdit, true)
      const uploaded = await uploadProductImage(tempFile.tempFilePath)
      // coverImg 保留后端相对地址（提交时用），coverImgFull 经 resolveUrl 拼成完整展示地址（与商户端一致）
      this.setData({ coverImg: uploaded.url, coverImgFull: resolveUrl(uploaded.url), uploading: false })
      this._updateTexts(this.data.isEdit, false)
      toast('上传成功')
    } catch (err: any) {
      this.setData({ uploading: false })
      this._updateTexts(this.data.isEdit, false)
      toast(err.message || '上传失败')
    }
  },
  previewImage() {
    const url = this.data.coverImgFull
    if (!url) return
    wx.previewImage({ urls: [url], current: url })
  },
  async save() {
    if (this.data.saving) return
    // 立即置位，防止按钮连点导致并发提交
    this.setData({ saving: true })
    this._updateTexts(this.data.isEdit, false)
    try {
      const { form, categoryIndex, coverImg, isEdit, goodsId } = this.data
      const name = (form.name || '').trim()
      if (!name) {
        toast('请输入商品名称')
        return
      }
      // 多规格：售价/库存取规格汇总值，并逐行校验
      let price = parseFloat(form.price)
      let stock = parseInt(form.stock, 10)
      const specs: GoodsSpecPayload[] = []
      if (this.data.specEnabled) {
        if (!this.data.specSkus.length) {
          toast('请至少添加一个规格值')
          return
        }
        for (const s of this.data.specSkus) {
          const p = parseFloat(s.price)
          if (isNaN(p) || p <= 0) {
            toast('请填写规格「' + s.name + '」的售价')
            return
          }
          const st = parseInt(s.stock, 10)
          if (isNaN(st) || st < 0) {
            toast('请填写规格「' + s.name + '」的库存')
            return
          }
          specs.push({
            groupName: (this.data.specGroupName || '规格').trim(),
            specName: s.name,
            price: p,
            stock: st,
            code: (s.code || '').trim(),
          })
        }
        const agg = this.aggregateSkus()
        price = agg.price
        stock = agg.stock
      } else {
        if (isNaN(price) || price <= 0) {
          toast('请输入正确的售价')
          return
        }
        if (isNaN(stock) || stock < 0) {
          toast('请输入正确的库存')
          return
        }
      }
      const marketPrice = parseFloat(form.marketPrice || '0') || price
      if (!coverImg) {
        toast('请上传商品图片')
        return
      }
      const merchant = getLocalMerchant()
      const merchantId = merchant ? String(merchant.id) : ''
      const payload = {
        name,
        categoryId: CATEGORIES[categoryIndex].id,
        price,
        marketPrice,
        stock,
        unit: (form.unit || '份').trim(),
        spec: (form.spec || '').trim(),
        description: (form.description || '').trim(),
        coverImg,
        // 新增商品统一提交审核，默认不上架；后端会强制 audit_status=0
        isOnSale: 0,
        merchantId: merchantId,
        // 多规格：空数组表示「不使用多规格」（编辑时后端会清空旧规格）
        specs: this.data.specEnabled ? specs : [],
      }
      if (isEdit) {
        await updateGoods(goodsId, payload)
        toast('保存成功')
      } else {
        await addGoods(payload)
        toast('提交审核成功')
      }
      // 不再手动 prev.load()：返回列表页时 onShow 会自动 load 一次，避免双重刷新
      setTimeout(() => wx.navigateBack(), 600)
    } catch (err: any) {
      toast(err.message || (this.data.isEdit ? '保存失败' : '新增失败'))
    } finally {
      this.setData({ saving: false })
      this._updateTexts(this.data.isEdit, false)
    }
  },
  /** compute all display texts for WXML (avoid single-quote literals) */
  _updateTexts(isEdit: boolean, uploading: boolean) {
    this.setData({
      uploadIco: uploading ? '\u23F3' : '\u{1F4F7}',
      uploadText: uploading ? '上传中...' : '点击上传图片',
      tipsText: isEdit ? '保存后将更新商品信息' : '提交审核后将自动关联到当前商户',
      saveBtnText: uploading
        ? (isEdit ? '保存中...' : '提交中...')
        : (isEdit ? '保存商品' : '提交审核'),
    })
  },
})
