/**
 * 商户订单 vs 商户账单 - 模拟数据
 *
 * 业务含义：
 *   - 商户订单：是商户面向终端消费者售出商品产生的订单流，对应后台"订单列表"。
 *     字段与后端 Order 实体的子集保持一致，便于替换为真实 API。
 *   - 商户账单：是商户将商品供货给商户时产生的内部结算账单，不进入"订单列表"。
 *     字段与 MerchantBill 实体保持一致，含供货关系与购入数量 / 单价。
 *
 * 数据约定：
 *   - 两套数据互相隔离，订单列表只会消费 merchantOrders。
 *   - 为演示筛选与统计，特意覆盖不同商户与商品品类，并保留若干种订单状态。
 */

export type OrderStatus =
    | 1  // 待付款
    | 2  // 待发货
    | 3  // 已发货
    | 4  // 已完成
    | 5  // 退款中
    | 6  // 已退款
    | 7  // 已驳回
    | 9  // 已取消

export interface MerchantOrder {
    id: string
    orderNo: string
    sellMerchantId: string
    sellMerchantName: string
    goodsId: string
    goodsName: string
    items: Array<{ goodsName: string; buyNum: number; unitPrice?: number }>
    goodsInfo?: string
    totalAmount: number
    orderStatus: OrderStatus
    payStatus: 0 | 1
    payType?: 1 | 2
    remark?: string
    createTime: string
}

export interface MerchantBillOrder {
    id: string
    billNo: string
    /** 供货商户ID（merchant 表） */
    merchantId: string
    merchantName: string
    /** 购买商户ID（发起补货/采购的商户） */
    buyMerchantId: string
    buyMerchantName: string
    goodsId: string
    goodsName: string
    buyNum: number
    unitPrice: number
    amount: number
    orderStatus:
        | 0  // 待付款
        | 1  // 已付款
        | 2  // 待发货
        | 3  // 已发货
        | 4  // 已完成
        | 8  // 已接单备货中
        | 9  // 已取消
    createTime: string
}

// =============================================================
// 商户订单：消费者在商户下单产生的数据（与订单列表绑定）
// =============================================================
export const merchantOrders: MerchantOrder[] = [
    {
        id: 'mb-001',
        orderNo: '2608180001',
        sellMerchantId: 'M001',
        sellMerchantName: '青禾杂货铺',
        goodsId: 'G001',
        goodsName: '日式和风陶瓷餐具六件套',
        items: [{ goodsName: '日式和风陶瓷餐具六件套', buyNum: 1, unitPrice: 298 }],
        goodsInfo: '日式和风陶瓷餐具六件套×1',
        totalAmount: 298.0,
        orderStatus: 4,
        payStatus: 1,
        payType: 1,
        remark: '工作日 18:00 前送达，放门卫处即可',
        createTime: '2026-08-18 09:23:11',
    },
    {
        id: 'mb-002',
        orderNo: '2608170002',
        sellMerchantId: 'M002',
        sellMerchantName: '山下日料',
        goodsId: 'G014',
        goodsName: '北海道刺身拼盘(家庭装)',
        items: [
            { goodsName: '北海道刺身拼盘(家庭装)', buyNum: 1, unitPrice: 388 },
            { goodsName: '寿司礼盒 A 款', buyNum: 1, unitPrice: 168 },
        ],
        goodsInfo: '北海道刺身拼盘(家庭装)×1，寿司礼盒 A 款×1',
        totalAmount: 556.0,
        orderStatus: 4,
        payStatus: 1,
        payType: 1,
        remark: '配芥末和酱油各两份',
        createTime: '2026-08-17 19:42:55',
    },
    {
        id: 'mb-003',
        orderNo: '2608160003',
        sellMerchantId: 'M001',
        sellMerchantName: '青禾杂货铺',
        goodsId: 'G021',
        goodsName: '新疆灰枣 500g 罐装',
        items: [{ goodsName: '新疆灰枣 500g 罐装', buyNum: 2, unitPrice: 68 }],
        goodsInfo: '新疆灰枣 500g 罐装×2',
        totalAmount: 136.0,
        orderStatus: 4,
        payStatus: 1,
        payType: 1,
        remark: '送礼用，需要礼盒包装',
        createTime: '2026-08-16 16:08:30',
    },
    {
        id: 'mb-004',
        orderNo: '2608150004',
        sellMerchantId: 'M003',
        sellMerchantName: '南山茶舍',
        goodsId: 'G030',
        goodsName: '明前龙井·特级 50g',
        items: [{ goodsName: '明前龙井·特级 50g', buyNum: 1, unitPrice: 480 }],
        goodsInfo: '明前龙井·特级 50g×1',
        totalAmount: 480.0,
        orderStatus: 4,
        payStatus: 1,
        payType: 1,
        remark: '用户对湿度有要求，附干燥剂',
        createTime: '2026-08-15 11:15:08',
    },
    {
        id: 'mb-005',
        orderNo: '2608140005',
        sellMerchantId: 'M002',
        sellMerchantName: '山下日料',
        goodsId: 'G015',
        goodsName: '手工稻荷寿司 6 件套',
        items: [{ goodsName: '手工稻荷寿司 6 件套', buyNum: 3, unitPrice: 58 }],
        goodsInfo: '手工稻荷寿司 6 件套×3',
        totalAmount: 174.0,
        orderStatus: 4,
        payStatus: 1,
        payType: 1,
        remark: '门店自提',
        createTime: '2026-08-14 12:34:47',
    },
    {
        id: 'mb-006',
        orderNo: '2608130006',
        sellMerchantId: 'M001',
        sellMerchantName: '青禾杂货铺',
        goodsId: 'G005',
        goodsName: '手工麻薯礼盒(抹茶/原味)',
        items: [{ goodsName: '手工麻薯礼盒(抹茶/原味)', buyNum: 1, unitPrice: 88 }],
        goodsInfo: '手工麻薯礼盒(抹茶/原味)×1',
        totalAmount: 88.0,
        orderStatus: 4,
        payStatus: 1,
        payType: 1,
        remark: '勿压',
        createTime: '2026-08-13 17:55:21',
    },
]

// =============================================================
// 商户账单：商户向商户供货的内部账单（不进入订单列表）
// =============================================================
export const merchantBillOrders: MerchantBillOrder[] = [
    {
        id: 'sb-001',
        billNo: 'GYS260815-01',
        merchantId: 'S001',
        merchantName: '景德镇陶艺工坊',
        buyMerchantId: 'M001',
        buyMerchantName: '青禾杂货铺',
        goodsId: 'G001',
        goodsName: '日式和风陶瓷餐具六件套',
        buyNum: 30,
        unitPrice: 78.0,
        amount: 2340.0,
        orderStatus: 4, // 已完成
        createTime: '2026-08-15 09:18:42',
    },
    {
        id: 'sb-002',
        billNo: 'GYS260815-02',
        merchantId: 'S002',
        merchantName: '江南味噌研究所',
        buyMerchantId: 'M002',
        buyMerchantName: '山下日料',
        goodsId: 'G014',
        goodsName: '北海道刺身拼盘(家庭装)',
        buyNum: 50,
        unitPrice: 196.0,
        amount: 9800.0,
        orderStatus: 4,
        createTime: '2026-08-14 14:30:08',
    },
    {
        id: 'sb-003',
        billNo: 'GYS260814-03',
        merchantId: 'S003',
        merchantName: '阿克苏果源农场',
        buyMerchantId: 'M001',
        buyMerchantName: '青禾杂货铺',
        goodsId: 'G021',
        goodsName: '新疆灰枣 500g 罐装',
        buyNum: 200,
        unitPrice: 32.5,
        amount: 6500.0,
        orderStatus: 4,
        createTime: '2026-08-12 10:05:50',
    },
    {
        id: 'sb-004',
        billNo: 'GYS260813-04',
        merchantId: 'S004',
        merchantName: '西湖龙井茶厂',
        buyMerchantId: 'M003',
        buyMerchantName: '南山茶舍',
        goodsId: 'G030',
        goodsName: '明前龙井·特级 50g',
        buyNum: 80,
        unitPrice: 220.0,
        amount: 17600.0,
        orderStatus: 4,
        createTime: '2026-08-13 08:42:15',
    },
    {
        id: 'sb-005',
        billNo: 'GYS260813-05',
        merchantId: 'S002',
        merchantName: '江南味噌研究所',
        buyMerchantId: 'M002',
        buyMerchantName: '山下日料',
        goodsId: 'G015',
        goodsName: '手工稻荷寿司 6 件套',
        buyNum: 120,
        unitPrice: 24.5,
        amount: 2940.0,
        orderStatus: 4,
        createTime: '2026-08-12 16:22:00',
    },
    {
        id: 'sb-006',
        billNo: 'GYS260812-06',
        merchantId: 'S005',
        merchantName: '京都和果子工坊',
        buyMerchantId: 'M001',
        buyMerchantName: '青禾杂货铺',
        goodsId: 'G005',
        goodsName: '手工麻薯礼盒(抹茶/原味)',
        buyNum: 60,
        unitPrice: 38.0,
        amount: 2280.0,
        orderStatus: 4,
        createTime: '2026-08-11 13:48:33',
    },
]

/**
 * 模拟接口返回的「商户订单列表」。
 * - 入参：page/size 与简易筛选
 * - 出参结构保持与后端 Order/GetList 一致，便于切换为真实 API
 */
export const fetchMerchantOrderPage = (
    params: { page?: number; size?: number; keyword?: string; merchantId?: string; status?: number }
): Promise<{ code: number; message: string; data: { list: MerchantOrder[]; total: number; page: number; size: number } }> => {
    return new Promise(resolve => {
        const page = params.page ?? 1
        const size = params.size ?? 10
        const kw = (params.keyword || '').trim().toLowerCase()
        const mer = (params.merchantId || '').trim()
        const status = params.status
        setTimeout(() => {
            const filtered = merchantOrders.filter(o => {
                if (kw && !(o.orderNo.toLowerCase().includes(kw) || (o.goodsInfo || '').toLowerCase().includes(kw))) return false
                if (mer && String(o.sellMerchantId) !== mer) return false
                if (status !== undefined && o.orderStatus !== status) return false
                return true
            })
            const start = (page - 1) * size
            resolve({
                code: 200,
                message: 'ok',
                data: {
                    list: filtered.slice(start, start + size),
                    total: filtered.length,
                    page,
                    size,
                },
            })
        }, 220)
    })
}

/**
 * 模拟接口返回的「商户账单列表」。
 * - 入参：page/size 与简易筛选
 * - 与订单列表数据源完全隔离。
 */
export const fetchMerchantBillPage = (
    params: { page?: number; size?: number; keyword?: string; merchantId?: string; status?: number }
): Promise<{ code: number; message: string; data: { list: MerchantBillOrder[]; total: number; page: number; size: number } }> => {
    return new Promise(resolve => {
        const page = params.page ?? 1
        const size = params.size ?? 10
        const kw = (params.keyword || '').trim().toLowerCase()
        const mer = (params.merchantId || '').trim()
        const status = params.status
        setTimeout(() => {
            const filtered = merchantBillOrders.filter(o => {
                if (kw && !(o.billNo.toLowerCase().includes(kw) || o.goodsName.toLowerCase().includes(kw))) return false
                if (mer && String(o.merchantId) !== mer) return false
                if (status !== undefined && o.orderStatus !== status) return false
                return true
            })
            const start = (page - 1) * size
            resolve({
                code: 200,
                message: 'ok',
                data: {
                    list: filtered.slice(start, start + size),
                    total: filtered.length,
                    page,
                    size,
                },
            })
        }, 220)
    })
}
