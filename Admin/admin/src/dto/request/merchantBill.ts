export interface MerchantBill {
    id?: string
    billNo?: string
    merchantId?: string
    billType?: number
    periodStart?: string
    periodEnd?: string
    amount?: number
    paidAmount?: number
    payStatus?: number
    payType?: number
    payNo?: string
    payTime?: string
    remark?: string
}

export interface MerchantBillPayDto {
    billId: string
    payNo: string
    payAmount: number
    payType?: number
}
