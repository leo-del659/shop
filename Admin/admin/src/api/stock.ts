import type { Stock, StockInOut } from '@/dto/request/logistics'
import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

export const get_stock_list = (params: { page: number; size: number; keyword?: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Stock/GetList", method: "get", params });
}
export const get_stock = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Stock/Get", method: "get", params: { id } });
}
export const add_stock = (data: Stock): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Stock/Add", method: "post", data });
}
export const update_stock = (data: Stock): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Stock/Update", method: "post", data });
}
export const remove_stock = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Stock/Remove", method: "get", params: { id } });
}
export const stock_in = (data: StockInOut): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Stock/StockIn", method: "post", data });
}
export const stock_out = (data: StockInOut): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Stock/StockOut", method: "post", data });
}
export const get_warehouse_list = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Stock/GetWarehouseList", method: "get" });
}
