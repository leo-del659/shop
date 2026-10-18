// 商品评价（消费者端）
import http from '../utils/request'

/** 评价可选标签（与 pages/review/review.ts 的 TAG_OPTIONS 保持一致，用于商品详情页筛选） */
export const REVIEW_TAGS = ['品质好', '品质一般', '服务好', '发货快', '态度差', '发货慢']

export interface MiniReview {
  id: string
  goodsId?: string
  goodsName?: string
  skuId?: string
  skuName?: string
  memberId?: string
  memberName?: string
  orderId?: string
  rating: number
  content?: string
  /** 后端以逗号分隔存储，前端展示需 split */
  images?: string
  tags?: string
  status?: number
  reply?: string
  createTime?: string
}

export interface GoodsReviewList {
  total: number
  list: MiniReview[]
}

/** 商品详情页评价列表（公开）。支持 tier(good/mid/bad) / tag(标签) 筛选 */
export function getGoodsReviews(
  goodsId: string,
  params?: { page?: number; size?: number; tier?: string; tag?: string }
) {
  // 过滤掉 undefined / 空字符串，避免 wx.request 把 undefined 序列化成查询参数，
  // 导致后端按 tag='undefined' 过滤而返回空列表。
  const clean: Record<string, any> = {}
  if (params) {
    if (params.page !== undefined && params.page !== null) clean.page = params.page
    if (params.size !== undefined && params.size !== null) clean.size = params.size
    if (params.tier) clean.tier = params.tier
    if (params.tag) clean.tag = params.tag
  }
  return http.get<GoodsReviewList>('/api/mini-program/reviews/goods/' + goodsId, clean)
}

/**
 * 提交评价（需登录）。
 * 后端 GoodsReviewCreateDto 字段为 snake_case（goods_id / sku_id / order_id / member_name ...），
 * 与本项目其它接口保持一致。
 */
export function submitReview(data: {
  goods_id: string
  sku_id?: string
  order_id?: string
  member_name?: string
  rating: number
  content?: string
  images?: string
  tags?: string
  /** 对商家评价（可选,独立文本） */
  merchant_review?: string
  /** 是否匿名：0 否 / 1 是 */
  is_anonymous?: number
}) {
  return http.post('/api/mini-program/reviews', data)
}

/** 查询某订单已评价的商品（用于订单详情页禁用已评价项，防止重复评价） */
export function getOrderReviewed(orderId: string) {
  return http.get<{ goodsId: string; skuId: string }[]>(
    '/api/mini-program/reviews/order/' + orderId
  )
}
