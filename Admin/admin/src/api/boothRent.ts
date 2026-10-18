import type { MerchantBoothRent } from '@/dto/request/boothRent'
import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

export const get_booth_rent_list = (params: { page: number; size: number; keyword?: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/BoothRent/GetList", method: "get", params });
}
export const get_booth_rent = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/BoothRent/Get", method: "get", params: { id } });
}
export const add_booth_rent = (data: MerchantBoothRent): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/BoothRent/Add", method: "post", data });
}
export const update_booth_rent = (data: MerchantBoothRent): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/BoothRent/Update", method: "post", data });
}
export const remove_booth_rent = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/BoothRent/Remove", method: "get", params: { id } });
}
export const pay_booth_rent = (data: { rentId: string; payNo: string; payAmount: number; payType: number }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/BoothRent/Pay", method: "post", data });
}
export const generate_rent_no = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/BoothRent/GenerateRentNo", method: "get" });
}
