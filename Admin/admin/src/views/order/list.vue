<template>
    <div class="l-container">
        <el-input v-model="key" placeholder="订单号" style="max-width:220px" clearable @keyup.enter="bind" />
        <el-select v-model="status" placeholder="订单状态" clearable style="width:140px">
            <el-option v-for="o in status_opts" :key="o.v" :label="o.l" :value="o.v" />
        </el-select>
        <el-button type="primary" @click="bind">查询</el-button>
        <el-table :data="list" border height="calc(100vh - 220px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="订单号" prop="orderNo" width="180" show-overflow-tooltip />
            <el-table-column label="商品" min-width="180" show-overflow-tooltip>
                <template #default="s">{{ s.row.goodsInfo || goods_summary(s.row.items) }}</template>
            </el-table-column>
            <el-table-column label="金额" prop="totalAmount" width="100" />
            <el-table-column label="备注" min-width="140" show-overflow-tooltip>
                <template #default="s">{{ s.row.remark || '-' }}</template>
            </el-table-column>
            <el-table-column label="订单状态" width="100">
                <template #default="s"><el-tag :type="order_status_type(s.row.orderStatus)">{{ order_status(s.row.orderStatus) }}</el-tag></template>
            </el-table-column>
            <el-table-column label="支付状态" width="100">
                <template #default="s"><el-tag :type="s.row.payStatus === 1 ? 'success' : 'info'">{{ s.row.payStatus === 1 ? '已支付' : '未支付' }}</el-tag></template>
            </el-table-column>
            <el-table-column label="创建时间" prop="createTime" width="160">
                <template #default="s">{{ format_date(s.row.createTime) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="90" fixed="right">
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
import { remove_order } from '@/api/order'
import { Api_Code } from '@/dto/response/base'
import { format_date } from '@/utils/format'
import { ElMessage, ElMessageBox } from 'element-plus'
import { computed, onMounted, reactive, ref } from 'vue'
import { useDebounceBind } from '@/utils/useDebounceBind'
import { useOrderStore } from '@/stores/order'

const orderStore = useOrderStore()
const list = computed(() => orderStore.list)
const key = ref('')
const status = ref<number | undefined>(undefined)
const status_opts = [{ v: 1, l: '待付款' }, { v: 2, l: '待发货' }, { v: 3, l: '已发货' }, { v: 4, l: '已完成' }, { v: 5, l: '退款中' }, { v: 6, l: '已退款' }, { v: 7, l: '已驳回' }, { v: 9, l: '已取消' }]
const pages = reactive({ page: 1, size: 10, total: 0 })

const order_status = (v: number) => (status_opts.find(o => o.v === v)?.l) || '未知'
// 订单状态颜色：已驳回(7) 用红色警示，其余维持默认蓝色
const order_status_type = (v: number): any => v === 7 ? 'danger' : 'primary'
// 商品列：聚合订单明细，多商品用「名称×数量」逗号拼接
const goods_summary = (items: any[]) => {
    if (!items || items.length === 0) return '-'
    return items.map(i => `${i.goodsName || '未知商品'}${i.buyNum ? '×' + i.buyNum : ''}`).join('，')
}
const bind = async () => {
    // 订单列表只从「商户订单表 /api/Order/GetList」读取，
    // 商户账单数据（/api/MerchantBill/GetList）完全不出现在这里。
    // 数据写入共享 store，「商户账单」页面读同一份 store，无需任何同步机制。
    await orderStore.fetch({ page: pages.page, size: pages.size, keyword: key.value || undefined, status: status.value })
    pages.total = orderStore.total
}

const on_del = async (row: any) => {
    ElMessageBox.confirm('确认删除该订单？').then(async () => {
        const res = await remove_order(row.id)
        if (res.code === Api_Code.ok) {
            orderStore.remove(row.id)
            ElMessage.success('删除成功')
        } else {
            ElMessage.error(res.message || '删除失败')
        }
    }).catch(() => {})
}

const on_size = () => { pages.page = 1; bind() }
const do_search = () => { pages.page = 1; bind() }
useDebounceBind([key, status], do_search)

onMounted(() => { bind() })
</script>

<style scoped></style>
