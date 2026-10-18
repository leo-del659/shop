<template>
    <div class="l-container">
        <el-input v-model="key" placeholder="订单号" style="max-width:220px" clearable @keyup.enter="bind" />
        <el-button type="primary" @click="bind">查询</el-button>
        <el-button type="warning" @click="go_review">退款审核</el-button>
        <el-table :data="list" border height="calc(100vh - 240px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="订单号" prop="orderNo" width="180" show-overflow-tooltip />
            <el-table-column label="商品" min-width="180" show-overflow-tooltip>
                <template #default="s">{{ s.row.goodsInfo || goods_summary(s.row.items) }}</template>
            </el-table-column>
            <el-table-column label="金额" prop="totalAmount" width="100" />
            <el-table-column label="退款原因" min-width="140" show-overflow-tooltip>
                <template #default="s">{{ s.row.refundReason || '-' }}</template>
            </el-table-column>
            <el-table-column label="订单状态" width="100">
                <template #default="s"><el-tag :type="order_status_type(s.row.orderStatus)">{{ order_status(s.row.orderStatus) }}</el-tag></template>
            </el-table-column>
            <el-table-column label="支付状态" width="100">
                <template #default="s"><el-tag :type="s.row.payStatus === 1 ? 'success' : 'info'">{{ s.row.payStatus === 1 ? '已支付' : '未支付' }}</el-tag></template>
            </el-table-column>
            <el-table-column label="操作" width="120" fixed="right">
                <template #default="s">
                    <el-button size="small" type="danger" plain @click="on_del(s.row)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>
        <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />

    </div>
</template>

<script setup lang="ts">
import MyPagination from '@/components/common/MyPagination.vue'
import { get_order_list, remove_order } from '@/api/order'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessageBox } from 'element-plus'
import { useDebounceBind } from '@/utils/useDebounceBind'

const list = ref<any[]>([])
const key = ref('')
const pages = reactive({ page: 1, size: 10, total: 0 })
// 订单状态（与订单列表统一）
const status_opts = [{ v: 1, l: '待付款' }, { v: 2, l: '待发货' }, { v: 3, l: '已发货' }, { v: 4, l: '已完成' }, { v: 5, l: '退款中' }, { v: 6, l: '已退款' }, { v: 7, l: '已驳回' }, { v: 9, l: '已取消' }]
const order_status = (v: number) => (status_opts.find(o => o.v === v)?.l) || '未知'
// 订单状态颜色：已驳回(7) 用红色警示，其余维持默认蓝色
const order_status_type = (v: number): any => v === 7 ? 'danger' : 'primary'
// 商品列：聚合订单明细，多商品用「名称×数量」逗号拼接
const goods_summary = (items: any[]) => {
    if (!items || items.length === 0) return '-'
    return items.map(i => `${i.goodsName || '未知商品'}${i.buyNum ? '×' + i.buyNum : ''}`).join('，')
}

// 退款管理：仅展示 orderStatus=6（已退款）的订单，并提供删除操作
const bind = async () => {
    open_loading()
    const res = await get_order_list({ page: pages.page, size: pages.size, keyword: key.value || undefined, status: 6 })
    close_loading()
    if (res.code === Api_Code.ok) {
        list.value = res.data.list || []
        pages.total = res.data.total
    }
}

const on_del = async (row: any) => {
    ElMessageBox.confirm('确认删除该退款订单？').then(async () => {
        const res = await remove_order(row.id)
        show_notice(res)
        if (res.code === Api_Code.ok) bind()
    })
}

const on_size = () => { pages.page = 1; bind() }
const do_search = () => { pages.page = 1; bind() }
useDebounceBind(key, do_search)

const router = useRouter()
const go_review = () => router.push('/order/review')

onMounted(bind)
</script>

<style scoped></style>
