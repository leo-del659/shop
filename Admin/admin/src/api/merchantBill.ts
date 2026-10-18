import type { MerchantBill, MerchantBillPayDto } from '@/dto/request/merchantBill'
import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

export const get_merchant_bill_list = (params: { page: number; size: number; keyword?: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/MerchantBill/GetList", method: "get", params });
}
export const get_merchant_bill = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/MerchantBill/Get", method: "get", params: { id } });
}
export const add_merchant_bill = (data: MerchantBill): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/MerchantBill/Add", method: "post", data });
}
export const update_merchant_bill = (data: MerchantBill): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/MerchantBill/Update", method: "post", data });
}
export const remove_merchant_bill = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/MerchantBill/Remove", method: "get", params: { id } });
}
export const pay_merchant_bill = (data: MerchantBillPayDto): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/MerchantBill/Pay", method: "post", data });
}
/**
 * 自动生成账单编号：GYS{yyyyMMdd}-{2位序号}
 */
export const generate_merchant_bill_no = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/MerchantBill/GenerateBillNo", method: "get" });
}
