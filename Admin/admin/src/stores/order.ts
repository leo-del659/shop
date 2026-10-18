/**
 * 订单共享数据源（Pinia store）
 *
 * 设计目标：
 * - 「订单列表」与「商户账单」共用同一份数据；任一页修改了订单，store 立即更新，
 *   另一页因为读了 store 自动同步，无需轮询、无需 BroadcastChannel、无需手动刷新。
 * - 数据源仍是后端 /api/Order/GetList，store 只是前端共享状态的"单一可信源"。
 *
 * 行为约定：
 * - fetch(params) 由页面调用（页面挂载 / 翻页 / 筛选 / 手动刷新），store 写入 list 与 total。
 * - patch(id, p) / remove(id) 由订单列表页在写接口成功后调用，做本地乐观更新；
 *   另一页（商户账单）若已挂在同一 Tab，会因为 reactive 自动重渲。
 * - 跨 Tab 不同步——这是有意的，跨 Tab 同步需要额外机制，与"取消实时同步"目标不符。
 */
import { defineStore } from 'pinia'
import { get_order_list } from '@/api/order'
import { Api_Code } from '@/dto/response/base'

export interface OrderRow {
    id: string
    orderNo?: string
    totalAmount?: number
    orderStatus?: number
    payStatus?: number
    createTime?: string
    merchantId?: string
    merchantName?: string
    items?: any[]
    goodsInfo?: string
    [k: string]: any
}

export const useOrderStore = defineStore('order', {
    state: () => ({
        list: [] as OrderRow[],
        total: 0,
        page: 1,
        size: 10,
        loading: false,
    }),
    actions: {
        /**
         * 拉取订单列表。传入的 page/size 不传时使用 store 当前值；keyword/merchantId/status 不传时按当前 store 值调用，
         * 但通常由调用方显式传入以便过滤。
         */
        async fetch(params: { page?: number; size?: number; keyword?: string; merchantId?: string; status?: number | undefined } = {}) {
            this.loading = true
            try {
                const res = await get_order_list({
                    page: params.page ?? this.page,
                    size: params.size ?? this.size,
                    keyword: params.keyword,
                    merchantId: params.merchantId,
                    status: params.status,
                })
                if (res.code === Api_Code.ok) {
                    this.list = res.data?.list || []
                    this.total = res.data?.total || 0
                    if (params.page != null) this.page = params.page
                    if (params.size != null) this.size = params.size
                }
                return res
            } finally {
                this.loading = false
            }
        },
        /** 局部更新订单字段（订单列表页在写接口成功后调用，做乐观更新） */
        patch(orderId: string, patch: Partial<Omit<OrderRow, 'id'>>) {
            const idx = this.list.findIndex((d: OrderRow) => d.id === orderId)
            if (idx >= 0) this.list[idx] = { ...(this.list[idx] as OrderRow), ...patch }
        },
        /** 删除订单（订单列表页删除成功后调用） */
        remove(orderId: string) {
            const idx = this.list.findIndex((d: OrderRow) => d.id === orderId)
            if (idx >= 0) this.list.splice(idx, 1)
        },
    },
})