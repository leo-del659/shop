<template>
    <div class="l-container">
        <!-- 筛选 -->
        <el-input v-model="orderNo" placeholder="订单号" style="max-width:220px" clearable @keyup.enter="bind" />
        <el-select v-model="merchantKey" placeholder="请选择商户" style="max-width:220px" clearable filterable @change="do_search">
            <el-option v-for="m in merchant_options" :key="m.id" :label="m.merchantName" :value="String(m.id)" />
        </el-select>
        <el-select v-model="status" placeholder="订单状态" clearable style="max-width:160px" @change="do_search">
            <el-option v-for="o in status_opts" :key="o.v" :label="o.l" :value="o.v" />
        </el-select>
        <el-button type="primary" @click="bind">查询</el-button>
        <el-button @click="do_refresh">刷新</el-button>

        <!-- 表格 -->
        <el-table :data="list" border v-loading="loading" height="calc(100vh - 260px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="订单号" prop="orderNo" width="180" show-overflow-tooltip />
            <el-table-column label="商户" min-width="150" show-overflow-tooltip>
                <template #default="s">{{ s.row.merchantName || s.row.merchantId || '-' }}</template>
            </el-table-column>
            <el-table-column label="商品" min-width="220" show-overflow-tooltip>
                <template #default="s">{{ goods_summary(s.row) }}</template>
            </el-table-column>
            <el-table-column label="金额" width="110">
                <template #default="s">
                    <span style="color:#C0821B;font-weight:600;">¥ {{ Number(s.row.totalAmount || 0).toFixed(2) }}</span>
                </template>
            </el-table-column>
            <el-table-column label="下单时间" width="160">
                <template #default="s">{{ format_date(s.row.createTime) }}</template>
            </el-table-column>
            <el-table-column label="订单状态" width="100">
                <template #default="s">
                    <el-tag :type="order_status_type(s.row.orderStatus)">{{ order_status(s.row.orderStatus) }}</el-tag>
                </template>
            </el-table-column>
            <el-table-column label="支付状态" width="100">
                <template #default="s">
                    <el-tag :type="s.row.payStatus === 1 ? 'success' : 'info'">
                        {{ s.row.payStatus === 1 ? '已支付' : '未支付' }}
                    </el-tag>
                </template>
            </el-table-column>
        </el-table>

        <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />
    </div>
</template>

<script setup lang="ts">
/**
 * 商户账单页 —— 与订单列表共用同一份 Pinia store 数据
 * 数据源：/api/Order/GetList（与「订单管理 → 订单列表」完全一致）
 * 同步策略：无轮询、无 BroadcastChannel；订单列表在订单上做修改时通过 store.patch/remove
 *           更新同一份 store，本页通过 computed 直接读 store.list，自动随之更新。
 */
import MyPagination from '@/components/common/MyPagination.vue'
import { Api_Code } from '@/dto/response/base'
import { show_notice } from '@/utils/message'
import { format_date } from '@/utils/format'
import { computed, onMounted, reactive, ref } from 'vue'
import { useDebounceBind } from '@/utils/useDebounceBind'
import { useOrderStore, type OrderRow } from '@/stores/order'

// 列表与翻页都来自共享 store。订单列表写接口成功后通过 store.patch / store.remove
// 更新同一份数据，本页因 computed 自动跟随刷新，无需任何同步机制。
const orderStore = useOrderStore()
const list = computed<OrderRow[]>(() => orderStore.list)
const merchantKey = ref('')
const orderNo = ref('')
const status = ref<number | undefined>(undefined)
const pages = reactive({ page: 1, size: 10, total: 0 })
const loading = ref(false)

const merchant_options = computed<{ id: string; merchantName: string }[]>(() => {
    const seen = new Map<string, { id: string; merchantName: string }>()
    for (const o of orderStore.list) {
        if (o.merchantId && o.merchantName && !seen.has(o.merchantId)) {
            seen.set(o.merchantId, { id: o.merchantId, merchantName: o.merchantName })
        }
    }
    return Array.from(seen.values())
})

const status_opts = [
    { v: 1, l: '待付款' },
    { v: 2, l: '待发货' },
    { v: 3, l: '已发货' },
    { v: 4, l: '已完成' },
    { v: 5, l: '退款中' },
    { v: 6, l: '已退款' },
    { v: 7, l: '已驳回' },
    { v: 9, l: '已取消' },
]
const order_status = (v: number | undefined) => v == null ? '-' : (status_opts.find(o => o.v === v)?.l || '未知')
const order_status_type = (v: number): any => {
    if (v === 4) return 'success'
    if (v === 7) return 'danger'
    if (v === 6) return 'warning'
    return 'primary'
}

const goods_summary = (row: OrderRow) => {
    if (!row) return '-'
    if (row.goodsInfo) return row.goodsInfo
    if (!row.items || row.items.length === 0) return '-'
    return row.items.map(i => `${i.goodsName || '未知商品'}${i.buyNum ? '×' + i.buyNum : ''}`).join('，')
}

// 全量缓存（用于商户下拉，单次拉够）
const bind = async () => {
    loading.value = true
    try {
        const res = await orderStore.fetch({
            page: pages.page,
            size: pages.size,
            keyword: orderNo.value || undefined,
            merchantId: merchantKey.value || undefined,
            status: status.value,
        })
        if (res.code === Api_Code.ok) {
            pages.total = orderStore.total
        } else {
            show_notice(res, '')
        }
    } finally {
        loading.value = false
    }
}

const do_refresh = () => { bind() }
const on_size = () => { pages.page = 1; bind() }
const do_search = () => { pages.page = 1; bind() }
useDebounceBind(orderNo, do_search)

// 进入页面时拉一次最新数据；之后任何「订单列表」上的写操作都会通过 store 自动反映到本页。
onMounted(() => { bind() })
</script>

<style scoped></style>
