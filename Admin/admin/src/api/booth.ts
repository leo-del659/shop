import type { MerchantBooth } from '@/dto/request/booth'
import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

export const get_booth_list = (params: { page: number; size: number; keyword?: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Booth/GetList", method: "get", params });
}
export const get_booth = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Booth/Get", method: "get", params: { id } });
}
export const add_booth = (data: MerchantBooth): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Booth/Add", method: "post", data });
}
export const update_booth = (data: MerchantBooth): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Booth/Update", method: "post", data });
}
export const remove_booth = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Booth/Remove", method: "get", params: { id } });
}
export const generate_booth_no = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Booth/GenerateBoothNo", method: "get" });
}
