export interface MerchantReceipt {
    id?: string
    receiptNo?: string
    merchantId?: string
    sourceType?: number
    sourceId?: string
    grossAmount?: number
    feeType?: number
    feeRate?: number
    feeAmount?: number
    netAmount?: number
    settleStatus?: number
    receiptTime?: string
    remark?: string
}

export interface RecordReceiptDto {
    merchantId: string
    grossAmount: number
    sourceType?: number
    sourceId?: string
    defaultRate?: number
    receiptTime?: string
    remark?: string
}
