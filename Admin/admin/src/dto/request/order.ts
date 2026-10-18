export interface MerchantOrder {
    id?: string
    orderNo?: string
    merchantId?: string
    totalAmount?: number
    orderStatus?: number
    rejectReason?: string
    payStatus?: number
    settleType?: number
    settleBatchId?: string
    deliveryDate?: string
    remark?: string
}
