import type { MerchantFeeConfig } from '@/dto/request/feeConfig'
import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

export const get_fee_config_list = (params: { page: number; size: number; keyword?: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/FeeConfig/GetList", method: "get", params });
}
export const get_fee_config = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/FeeConfig/Get", method: "get", params: { id } });
}
export const add_fee_config = (data: MerchantFeeConfig): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/FeeConfig/Add", method: "post", data });
}
export const update_fee_config = (data: MerchantFeeConfig): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/FeeConfig/Update", method: "post", data });
}
export const remove_fee_config = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/FeeConfig/Remove", method: "get", params: { id } });
}
export const get_fee_config_by_merchant = (merchantId: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/FeeConfig/GetByMerchant", method: "get", params: { merchantId } });
}
