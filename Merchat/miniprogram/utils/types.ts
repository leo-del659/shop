// 业务数据模型（命名与后端 camelCase 响应、以及原型页字段保持一致）

export interface Merchant {
  id: string
  merchantName: string
  merchantNo: string
  /** 商户头像图片地址（后端相对地址 /api/file/{guid}，为空回退名称首字头像） */
  avatar?: string
  /** 登录手机号（合并后 account 即手机号） */
  account?: string
  contact?: string
  phone?: string
  levelId?: number
  address?: string
  isBan?: number
  createTime?: string
  // 展示扩展字段（原型页用）
  email?: string
  joinDate?: string
  avatarText?: string
  // 集邻生活多商户扩展字段
  mobile?: string          // 登录手机号（也是租户 key）
  level?: string           // 商户等级，如 金牌商户
  settlementAccount?: string // 结算账户
  shopStatus?: string      // 营业状态，如 营业中 / 待审核
  qualificationCount?: number // 资质证书数量
}

export type GoodsStatus = 'published' | 'draft' | 'disabled'

export interface GoodsImage {
  id: number
  url?: string   // 真实图片完整地址；优先显示，为空时回退 emoji
  emoji?: string // 无图时的占位表情
  label: string
}

export interface Goods {
  id: number
  name: string
  category: string
  price: number
  stock: number
  unit: string
  spec?: string
  status: GoodsStatus
  emoji: string
  coverImg?: string  // 后端 cover_img 转 camelCase 后的本地图片地址
  sales: number
  marketPrice?: number
  costPrice?: number
  merchant?: string
  safeStock?: number
  alertStock?: number
  launchTime?: string
  description?: string
  images?: GoodsImage[]
  /** 真实规格 SKU 列表（数据库 goods_sku 表） */
  skus?: GoodsSku[]
}

/** 商品规格 SKU（对应数据库 goods_sku 表） */
export interface GoodsSku {
  id: string
  specId?: string
  skuName?: string
  price: number
  stock: number
}

export type OrderStatus = 'pending' | 'shipping' | 'completed' | 'cancelled'
export type PayStatus = 'paid' | 'unpaid' | 'partial' | 'overdue'

export interface OrderItem {
  name: string
  spec?: string
  qty: number
  price: number
  /** 商品图片（来自 merchant_order.items_json.goodsImg），列表缩略图使用 */
  img?: string
}

export interface LogisticsNode {
  text: string
  time: string
  active: boolean
}

export interface Order {
  id: string
  orderNo?: string
  merchant: string
  merchantName?: string
  merchantContact?: string
  merchantPhone?: string
  merchantAddress?: string
  merchantId?: string     // 订单所属商户 id（跨商户列表用于判断操作权限）
  status: OrderStatus
  rawStatus?: number     // 后端原始 order_status 数值（区分 shipping 子状态 2/3）
  payStatus: PayStatus
  detailStatusLabel?: string  // 订单状态明细文案（待接单/接单备货中/已发货·配送中/已完成/已取消）
  detailStatusCls?: string    // 订单状态明细样式类（pending/shipping/completed/cancelled）
  time: string
  payTime?: string
  receiver?: string
  remark?: string
  /** 拒单理由（商户端 reject 时填写） */
  rejectReason?: string
  items: OrderItem[]
  total: number
  goodsSummary?: string    // 商品汇总名称（如"五常大米 稻花香 5kg"）
  totalQty?: number        // 购入总数量（所有商品 qty 之和）
  logistics?: LogisticsNode[]
}

export interface Payment {
  id: number
  orderId: string
  amount: number
  time: string
  method: string
}

export type WithdrawStatus = 'success' | 'pending' | 'failed'

export interface Withdraw {
  id: number
  amount: number
  status: WithdrawStatus
  time: string
  account: string
  remark: string
}

export interface HomeNotice {
  id: string
  title: string
  content: string
  type: number
  typeText: string
  createTime: string
}

export interface HomeData {
  merchant: Merchant
  kpi: { orders: number; revenue: number; products: number; pending: number }
  /** 今日经营数据（含昨日对比），后端 /api/merchant/home 返回 */
  today?: { orders: number; revenue: number; yesterdayOrders: number; yesterdayRevenue: number }
  /** 公告通知列表 */
  notices?: HomeNotice[]
  products: Goods[]
  todos: { type: 'orange' | 'red' | 'blue' | 'green'; text: string; time: string }[]
}

export interface BalanceInfo {
  totalPaid: number
  withdrawn: number
  available: number
  paidOrderCount: number
}

export interface ReconcileData {
  monthLabel: string
  monthKey: string
  summary: { revenue: number; orders: number; paid: number }
  orderSummary?: { revenue: number; count: number; paid: number }
  orders: Order[]
  payments: Payment[]
  withdrawals: Withdraw[]
}

/** 收货地址（与后端 MemberAddress_Response_Dto 一致） */
export interface Address {
  id: string
  name?: string
  mobile?: string
  province?: string
  city?: string
  district?: string
  detailAddr?: string
  /** 全地址拼字符串：省/市/区+详细地址 */
  fullAddress?: string
  isDefault?: number
}

/** 新增/编辑地址的请求体 */
export interface AddressSavePayload {
  id?: string
  name?: string
  mobile?: string
  province?: string
  city?: string
  district?: string
  detailAddr?: string
  /** 1=设默认 0/不传=保留原状 */
  isDefault?: number
}
