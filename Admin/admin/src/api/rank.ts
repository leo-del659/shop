import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base'

/** 商户销售排行榜（按采购订单金额 Top N） */
export const get_merchant_rank = (params: { top?: number }): Promise<Api_Reseponse_Dto> => {
    return request({ url: '/api/Rank/MerchantRank', method: 'get', params })
}
