import type { RecordReceiptDto } from '@/dto/request/receipt'
import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

export const get_receipt_list = (params: { page: number; size: number; keyword?: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Receipt/GetList", method: "get", params });
}
export const get_receipt = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Receipt/Get", method: "get", params: { id } });
}
export const record_receipt = (data: RecordReceiptDto): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Receipt/RecordReceipt", method: "post", data });
}
export const settle_receipt = (receiptId: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Receipt/Settle", method: "get", params: { receiptId } });
}
export const get_fee_summary = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Receipt/GetFeeSummary", method: "get" });
}
