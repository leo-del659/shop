// 退款/退货退款接口（消费者）
import http from '../utils/request'

export const REFUND_STATUS_TEXT: Record<number, string> = {
  0: '待审核',
  1: '已退款',
  2: '已驳回'
}
export const REFUND_TYPE_TEXT: Record<number, string> = {
  1: '仅退款',
  2: '退货退款'
}

export interface RefundItem {
  id: string
  refundNo?: string
  orderId?: string
  orderNo?: string
  refundAmount: number
  refundType: number
  refundTypeText?: string
  refundStatus: number
  refundStatusText?: string
  refundReason?: string
  goodsName?: string
  goodsImg?: string
  createTime?: string
}

export interface RefundDetail {
  id: string
  refundNo?: string
  orderId?: string
  orderNo?: string
  refundAmount: number
  refundType: number
  refundTypeText?: string
  refundStatus: number
  refundStatusText?: string
  refundReason?: string
  refundNum?: number
  goodsName?: string
  goodsImg?: string
  handleRemark?: string
  handleTime?: string
  createTime?: string
  orderTotal: number
  orderStatus: number
  orderStatusText?: string
  items: {
    goodsId?: string
    goodsName?: string
    goodsImg?: string
    buyNum: number
    buyPrice: number
    subAmount: number
  }[]
  return?: {
    returnNo?: string
    returnNum?: number
    returnStatus?: number
    returnStatusText?: string
    returnReason?: string
  }
}

export function applyRefund(data: {
  orderId: string
  refundType: number
  refundAmount: number
  refundReason?: string
  refundNum?: number
  goodsId?: string
}) {
  return http.post<{ id: string; refundNo: string; refundType: number; refundStatus: number }>(
    '/api/mini-program/refunds/apply',
    data
  )
}

export function getMyRefunds(params: { page?: number; pageSize?: number }) {
  return http.get<{ total: number; list: RefundItem[] }>('/api/mini-program/refunds/my', params)
}

export function getRefundDetail(id: string) {
  return http.get<RefundDetail>('/api/mini-program/refunds/' + id)
}

