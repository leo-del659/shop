import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';
import type { ReviewReplyDto } from '@/dto/request/review'

export const get_review_list = (params: {
  page: number
  size: number
  keyword?: string
  tier?: string
  tag?: string
  status?: number
  goodsId?: string
}): Promise<Api_Reseponse_Dto> =>
  request({ url: '/api/GoodsReview/GetList', method: 'get', params })

export const audit_review = (data: { id: string; status: number; reject_reason?: string }): Promise<Api_Reseponse_Dto> =>
  request({ url: '/api/GoodsReview/Audit', method: 'post', data })

export const reply_review = (data: ReviewReplyDto): Promise<Api_Reseponse_Dto> =>
  request({ url: '/api/GoodsReview/Reply', method: 'post', data })

export const remove_review = (id: string): Promise<Api_Reseponse_Dto> =>
  request({ url: '/api/GoodsReview/Remove', method: 'get', params: { id } })
