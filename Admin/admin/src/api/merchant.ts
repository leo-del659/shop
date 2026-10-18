import type { Merchant } from '@/dto/request/merchant'
import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

/**
 * 商户（卖家）接口。后端控制器为 AdminMerchantController，路由前缀 /api/Merchant/[action]。
 * 实体为 ECommerce.Infrastructure.EFCore.Merchant（表 merchant，物理列 merchant_name/merchant_no），
 * 因此交互字段一律用 merchantName / merchantNo。
 */

export const get_merchant_list = (params: { page: number; size: number; keyword?: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Merchant/GetList", method: "get", params });
}
export const get_merchant = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Merchant/Get", method: "get", params: { id } });
}
export const add_merchant = (data: Merchant): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Merchant/Add", method: "post", data });
}
export const update_merchant = (data: Merchant): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Merchant/Update", method: "post", data });
}
export const remove_merchant = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Merchant/Remove", method: "get", params: { id } });
}

/**
 * 自动生成下一个商户编号（不写入数据库）。
 * 后端 action 为 GenerateMerchantNo，规则：GYS + 4 位顺序号；返回 { merchantNo }。
 * 历史上这里调的是 /api/Merchant/PreviewNextShopNo（买家侧店铺编号），该 action 不存在，会 404。
 */
export const generate_merchant_no = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Merchant/GenerateMerchantNo", method: "get" });
}

/**
 * 商户资质相关。这些 action 实际定义在 AdminMerchantController 上（/api/Merchant/[action]），
 * 后端没有独立的 AdminMerchantQualificationController，旧 URL /api/MerchantQualification/* 会 404。
 */
export const get_merchant_qualifications = (merchantId: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Merchant/GetQualifications", method: "get", params: { merchantId } });
}
/** 新增商户资质 */
export const add_merchant_qualification = (data: any): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Merchant/AddQualification", method: "post", data });
}
/** 修改商户资质 */
export const update_merchant_qualification = (data: any): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Merchant/UpdateQualification", method: "post", data });
}
/** 删除商户资质 */
export const remove_merchant_qualification = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Merchant/RemoveQualification", method: "get", params: { id } });
}

/** 商户等级列表（编辑表单下拉选项） */
export const get_merchant_level_list = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Merchant/GetLevels", method: "get" });
}
/** 商户银行账户列表（编辑表单下拉选项） */
export const get_merchant_bank_list = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Merchant/GetBanks", method: "get" });
}

/**
 * 自动生成资质证书编号
 * @param merchantId 商户 id（后端参数名 merchantId）
 * @param qualType 资质类型
 * @returns { certNo: string }
 */
export const generate_cert_no = (merchantId: string, qualType: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Merchant/GenerateCertNo", method: "get", params: { merchantId, qualType } });
}
