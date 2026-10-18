import type { SettleRecord } from '@/dto/request/settle'
import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

export const get_settle_list = (params: { page: number; size: number; keyword?: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Settle/GetList", method: "get", params });
}
export const get_settle = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Settle/Get", method: "get", params: { id } });
}
export const add_settle = (data: SettleRecord): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Settle/Add", method: "post", data });
}
export const update_settle = (data: SettleRecord): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Settle/Update", method: "post", data });
}
export const remove_settle = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Settle/Remove", method: "get", params: { id } });
}
export const pay_settle = (data: { settleId: string; payNo: string; payAmount: number }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Settle/Pay", method: "post", data });
}
export const refund_settle = (data: { settleId: string; refundNo: string; refundAmount: number }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Settle/Refund", method: "post", data });
}
/**
 * 自动生成结算单号：JS{yyyyMMdd}-{2位序号}
 * 与 DB 现存数据格式一致
 */
export const generate_settle_no = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Settle/GenerateSettleNo", method: "get" });
}
