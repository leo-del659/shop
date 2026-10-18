/**
 * 商户（卖家）DTO。
 * 实体为 ECommerce.Infrastructure.EFCore.Merchant，
 * 数据库表 merchant（物理列 merchant_name / merchant_no）。
 * 因此接口字段一律用 merchantName / merchantNo，历史上的 shopNo / shopAddress /
 * shopImg / openid / merchantName 均已不存在于该实体。
 */
export interface Merchant {
    id?: string
    /** 商户名称（merchant.merchant_name） */
    merchantName?: string
    /** 商户编号（merchant.merchant_no，如 GYS0001） */
    merchantNo?: string
    /** 联系人 */
    contact?: string
    /** 联系电话 */
    phone?: string
    /** 等级ID（merchant_level.id） */
    levelId?: string
    /** 默认银行账户ID（merchant_bank.id） */
    bankId?: string
    /** 地址 */
    address?: string
    /** 是否禁用 0正常 1禁用 */
    isBan?: number
}
