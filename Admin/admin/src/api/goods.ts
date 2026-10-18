import type { GoodCreateDto, GoodModifyDto, GoodSkuDto, AuditHandleDto } from '@/dto/request/goods'
import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

// 把前端表单（camelCase）转成后端 DTO 要求的 snake_case
// 后端 ASP.NET Core 默认按 PascalCase 绑定；这里后端定义显式 snake_case 属性名
// （category_id/goods_name/...），所以前端必须发对应小写下划线字段，否则
// 关键字段全变 null、必填校验失败、报"操作异常"。
const to_save_payload = (form: any) => {
    // 不过滤：用户每次点"新增/添加规格"都是有效操作，原样保留；后端每条都生成 SKU，
    // 空白行用户可再点删除清掉。这样"新增"按钮立即可见效果（不算"白点"）。
    const skus: GoodSkuDto[] = (form.skus || []).map((s: any) => ({
        // 页面已移除规格名输入框，规格名随规格值保持一致；sku.vue 仍可传 specName。
        spec_name: s.specName || form.specName || s.skuName || '',
        sku_name: s.skuName || '',
        price: Number(s.price) || 0,
        stock: Number(s.stock) || 0
    }))
    return {
        id: form.id,
        category_id: form.categoryId,
        goods_name: form.goodsName,
        price: Number(form.price) || 0,
        sale_price: Number(form.salePrice) || 0,
        stock: Number(form.stock) || 0,
        // 多封面图：cover_img 取第一张，img_list 逗号全量（good 表 img_list 字段），img_urls 数组供后端统一合并写入 good.img_list
        cover_img: Array.isArray(form.coverImgs) ? (form.coverImgs[0] || '') : (form.coverImg || ''),
        img_list: Array.isArray(form.coverImgs) ? form.coverImgs.join(',') : (form.coverImg || ''),
        img_urls: Array.isArray(form.coverImgs) ? form.coverImgs : (form.coverImg ? String(form.coverImg).split(',').map((s: string) => s.trim()).filter(Boolean) : []),
        unit: form.unit || '',
        merchant_id: form.merchantId,  // 后端 GoodCreateDto 字段名为 merchant_id（商品归属商户），form.merchantId 即选中的商户 id
        skus: skus.length ? skus : undefined
    }
}

export const get_goods_list = (params: { page: number; size: number; keyword?: string; categoryId?: string; merchantId?: string }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Goods/GetList", method: "get", params });
}
export const get_goods = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Goods/Get", method: "get", params: { id } });
}
export const add_goods = (form: any): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Goods/Add", method: "post", data: to_save_payload(form) });
}
export const update_goods = (form: any): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Goods/Update", method: "post", data: to_save_payload(form) });
}
export const remove_goods = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Goods/Remove", method: "get", params: { id } });
}
export const submit_audit = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Goods/SubmitAudit", method: "get", params: { id } });
}
export const audit_goods = (data: AuditHandleDto): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/GoodsAudit/Handle", method: "post", data });
}
export const set_goods_sale = (id: string, isSale: number): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Goods/SetSale", method: "get", params: { id, isSale } });
}
