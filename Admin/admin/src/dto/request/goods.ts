/**
 * 商品创建
 */
export interface GoodCreateDto {
    category_id?: string
    goods_name?: string
    price?: number
    sale_price?: number
    stock?: number
    cover_img?: string
    img_list?: string
    unit?: string
    merchant_id?: string
    img_urls?: string[]
    skus?: GoodSkuDto[]
}

export interface GoodSkuDto {
    spec_name?: string
    sku_name?: string
    price?: number
    stock?: number
}

/**
 * 商品修改
 */
export interface GoodModifyDto extends GoodCreateDto {
    id: string
}

/**
 * 审核处理
 */
export interface AuditHandleDto {
    id: string
    biz_type?: string
    audit_status: number
    audit_remark?: string
    auditor?: string
}
