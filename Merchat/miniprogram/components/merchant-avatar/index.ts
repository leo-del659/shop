// 统一商户头像组件 merchant-avatar
// 设计目标：首页 / 我的 / 编辑资料 三处头像视觉完全一致（同一套圆角、描边、底色、字重）。
// 规则：
//   1. 有图片 → 圆形图片（aspectFill 居中裁切）
//   2. 无图片 → 品牌蓝渐变底 + 白色粗体名称首字
//   3. 尺寸由外部传入（rpx），字号按尺寸比例联动，保证任意尺寸都协调
Component({
  options: { addGlobalClass: false },
  properties: {
    /** 图片地址（后端相对地址 /api/file/{guid} 或完整地址），为空时显示文字头像 */
    src: { type: String, value: '' },
    /** 文字兜底内容（通常是商户名首字） */
    text: { type: String, value: '' },
    /** 头像直径（rpx） */
    size: { type: Number, value: 120 },
    /** 是否显示白色内描边（叠在渐变底上，浅色背景上更立体） */
    bordered: { type: Boolean, value: true },
  },
  data: {
    /** 图片加载失败标记：降级为文字头像，避免出现破图 */
    imgErr: false,
  },
  observers: {
    /** 地址变化时重置失败标记 */
    src() {
      if (this.data.imgErr) this.setData({ imgErr: false })
    },
  },
  methods: {
    onImgError() {
      this.setData({ imgErr: true })
    },
  },
})
