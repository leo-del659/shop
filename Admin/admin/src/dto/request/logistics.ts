export interface LogisticsCompany {
    id?: string
    companyName?: string
    code?: string
    phone?: string
    isBan?: number
}

export interface Stock {
    id?: string
    goodsId?: string
    warehouse?: string
    totalStock?: number
    lockStock?: number
}

export interface StockInOut {
    goodsId?: string
    warehouse?: string
    num?: number
    remark?: string
    /** 入出库子类型（绑定文字，如"采购入库"），由前端二级联动传给后端 */
    type?: string
    /** 状态（按类型联动），由前端二级联动传给后端 */
    status?: string
}
