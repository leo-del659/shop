import type { MerchantOrder } from '@/dto/request/order'
import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

export const get_order_list = (params: { page: number; size: number; keyword?: string; status?: number; merchantId?: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/GetList", method: "get", params });
}
export const get_order = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/Get", method: "get", params: { id } });
}
export const add_order = (data: MerchantOrder): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/Add", method: "post", data });
}
export const update_order = (data: MerchantOrder): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/Update", method: "post", data });
}
export const remove_order = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/Remove", method: "get", params: { id } });
}
export const pay_order = (data: { orderId: string; payNo: string; payAmount: number; payType?: number }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/Pay", method: "post", data });
}
export const delivery_order = (data: { orderId: string; logisticsId: string; trackingNo: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/Delivery", method: "post", data });
}
export const refund_order = (data: { orderId: string; refundNo: string; refundAmount: number; reason: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/Refund", method: "post", data });
}
export const return_order = (data: { orderId: string; returnNo: string; returnNum: number; reason: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/Return", method: "post", data });
}
/** 后台退款列表（status: 0待审核 1已退款 2驳回） */
export const get_refund_list = (params: { page: number; size: number; status?: number }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/RefundList", method: "get", params });
}
/** 后台退款审核：approve=true 同意(订单→已退款6)；false 驳回(订单→已驳回7) */
export const review_refund = (data: { id: string; approve: boolean; remark?: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/RefundReview", method: "post", data });
}
/** 后台退款详情：管理员可看任意退款单（含订单/退货/审核记录） */
export const get_refund_detail = (params: { id: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/RefundDetail", method: "get", params });
}
/**
 * 自动生成订单号：{yyMMdd}{4位当日序号}，例如 2608120001
 */
export const generate_order_no = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/GenerateOrderNo", method: "get" });
}
/** 管理首页多维销售/财务分析看板（销售周趋势/分类占比/退款趋势/商户排行/财务概览） */
export const get_dashboard = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Order/Dashboard", method: "get" });
}
