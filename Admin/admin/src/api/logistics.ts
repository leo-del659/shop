import type { LogisticsCompany } from '@/dto/request/logistics'
import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

export const get_logistics_list = (params: { page: number; size: number; keyword?: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Logistics/GetList", method: "get", params });
}
export const get_logistics = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Logistics/Get", method: "get", params: { id } });
}
export const add_logistics = (data: LogisticsCompany): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Logistics/Add", method: "post", data });
}
export const update_logistics = (data: LogisticsCompany): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Logistics/Update", method: "post", data });
}
export const remove_logistics = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Logistics/Remove", method: "get", params: { id } });
}
// 预览下一个物流编码（WL + 4 位数字），不写入数据库
export const preview_next_logistics_code = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Logistics/PreviewNextLogisticsCode", method: "get" });
}
