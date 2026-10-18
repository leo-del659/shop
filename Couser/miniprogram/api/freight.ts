// 运费接口（消费者）
// 字段与后端 FreightDetailDto 一一对应（camelCase）
import http from '../utils/request'

export interface FreightDetail {
  /** 运费标识（下单前预览计算时为 null） */
  id?: string | null
  /** 运费单号 */
  freightNo?: string | null
  /** 关联订单ID */
  orderId?: string | null
  /** 关联订单编号 */
  orderNo?: string | null
  /** 商品金额（运费计费基数） */
  goodsAmount: number
  /** 运费金额 */
  freightAmount: number
  /** 计费方式：1 按件计费 / 2 满额包邮 */
  chargeType: number
  /** 计费方式文案 */
  chargeTypeText: string
  /** 计费规则说明 */
  chargeRule: string
  /** 是否包邮 */
  freeShipping: boolean
  /** 应付合计 = 商品金额 + 运费金额 */
  totalAmount: number
  /** 运费支付状态：0 未支付 / 1 已支付 */
  payStatus: number
  /** 运费支付状态文案 */
  payStatusText: string
  /** 运费支付时间 */
  payTime?: string | null
  /** 生效时间 */
  effectiveTime?: string | null
  /** 运费展示文案：包邮 / ¥8.00 */
  freightAmountText: string
  /** 合计展示文案 */
  totalAmountText: string
  /** 计费件数 */
  itemCount?: number
  remark?: string | null
}

/** 计费方式文案兜底映射（后端已下发 chargeTypeText，此处仅作容错） */
export const CHARGE_TYPE_TEXT: Record<number, string> = {
  1: '按件计费',
  2: '满额包邮'
}

/**
 * 计算运费
 * - 传 orderId：按已有订单计算并落库
 * - 传 items：下单前预览计算（不落库）
 */
export function calcFreight(data: {
  orderId?: string
  address?: string
  items?: { goodsId: string; skuId?: string; buyNum: number }[]
}) {
  return http.post<FreightDetail>('/api/mini-program/freight/calc', data)
}

/** 运费支付：提交即完成 */
export function payFreight(data: {
  orderId: string
  freightId?: string
  payChannel?: number
  remark?: string
}) {
  return http.post<FreightDetail>('/api/mini-program/freight/pay', data)
}

/** 查询指定订单的运费明细 */
export function getFreight(orderId: string) {
  return http.get<FreightDetail>('/api/mini-program/freight/' + orderId)
}
