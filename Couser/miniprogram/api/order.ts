// 订单接口（消费者）
import http from '../utils/request'

export interface OrderItem {
  goodsId: string
  goodsName?: string
  goodsImg?: string
  /** 下单时选择的规格（后台 OrderItemDto 已返回） */
  skuId?: string
  skuName?: string
  buyNum: number
  buyPrice: number
  subAmount: number
  /** 该商品所属售卖商户ID（后端冗余自 good.merchant_id） */
  sellerMerchantId?: string
}

export interface OrderSummary {
  id: string
  orderNo: string
  totalAmount: number
  orderStatus: number
  /** 支付状态：0 未支付 / 1 已支付（与 orderStatus 分离，支付成功只置 payStatus） */
  payStatus?: number
  remark?: string
  createTime: string
  receiverName?: string
  receiverMobile?: string
  address?: string
  /** 商品所属商户ID（后端创建订单时从 good.merchant_id 回填） */
  sellerMerchantId?: string
  /** 商品所属商户名称（后端联表 merchant.merchant_name 回填） */
  sellerMerchantName?: string
  /** 所属商户名称（后端联表 merchant.merchant_name 回填） */
  merchantName?: string
  /** 付款时间 */
  payTime?: string
  /** 发货时间 */
  shipTime?: string
  /** 成交/完成时间 */
  completeTime?: string
  items: OrderItem[]
}

export interface OrderCreateResult {
  orderId: string
  orderNo: string
  totalAmount: number
  orderStatus: number
  remark?: string
  sellerMerchantId?: string
  items: OrderItem[]
}

export interface CreateOrderDto {
  remark?: string
  deliveryDate?: string
  receiverName?: string
  receiverMobile?: string
  address?: string
  items: { goodsId: string; buyNum: number; skuId?: string }[]
}

/** 订单状态：1 待付款 / 2 待发货(已支付) / 3 已发货(待收货) / 4 已完成 / 9 已取消 */
export const ORDER_STATUS = {
  ALL: -1,
  WAIT_PAY: 1,
  WAIT_SHIP: 2,    // 待发货（已支付）
  SHIPPED: 3,      // 已发货（待收货）
  DONE: 4,         // 已完成
  CANCELLED: 9     // 已取消
}

export const ORDER_STATUS_TEXT: Record<number, string> = {
  1: '待付款',
  2: '待发货',
  3: '已发货',
  4: '已完成',
  5: '退款中',
  6: '已退款',
  7: '已驳回',
  9: '已退款'
}

export function createOrder(data: CreateOrderDto) {
  return http.post<OrderCreateResult>('/api/mini-program/orders', data)
}

export function getOrders(params: { status?: number; page?: number; pageSize?: number }) {
  return http.get<OrderSummary[]>('/api/mini-program/orders', params)
}

export function getOrderDetail(orderId: string) {
  return http.get<OrderSummary>('/api/mini-program/orders/' + orderId)
}

export function cancelOrder(orderId: string, reason?: string) {
  return http.post('/api/mini-program/orders/' + orderId + '/cancel', { reason })
}

/** 模拟支付回调（演示用，后端 AllowAnonymous） */
export function payOrder(orderId: string) {
  return http.post('/api/mini-program/orders/' + orderId + '/pay-callback', {}, false)
}

/** 确认收货：将已发货订单标记为已完成 */
export function confirmReceipt(orderId: string) {
  return http.post('/api/mini-program/orders/' + orderId + '/receive')
}

/** 删除订单（买家逻辑删除） */
export function deleteOrder(orderId: string) {
  return http.delete('/api/mini-program/orders/' + orderId)
}
