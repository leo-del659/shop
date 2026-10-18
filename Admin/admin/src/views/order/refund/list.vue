<template>
    <div class="l-container">
        <el-input v-model="key" placeholder="退款单号/订单号" style="max-width:260px" clearable @keyup.enter="do_search" />
        <el-select v-model="status" placeholder="退款状态" clearable style="width:160px">
            <el-option label="待审核" :value="0" />
            <el-option label="已退款" :value="1" />
            <el-option label="已驳回" :value="2" />
        </el-select>
        <el-button type="primary" @click="do_search">查询</el-button>
        <el-button type="warning" @click="go_review">退款审核</el-button>
        <el-table :data="list" border height="calc(100vh - 240px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="退款单号" prop="refundNo" width="200" show-overflow-tooltip />
            <el-table-column label="商品" prop="goodsName" min-width="180" show-overflow-tooltip />
            <el-table-column label="退款金额" width="110">
                <template #default="s">{{ s.row.refundAmount ? '¥' + Number(s.row.refundAmount).toFixed(2) : '-' }}</template>
            </el-table-column>
            <el-table-column label="退款原因" prop="refundReason" min-width="160" show-overflow-tooltip />
            <el-table-column label="状态" width="100">
                <template #default="s"><el-tag :type="status_type(s.row.refundStatus)">{{ status_text(s.row.refundStatus) }}</el-tag></template>
            </el-table-column>
            <el-table-column label="申请时间" prop="createTime" width="170" />
        </el-table>
        <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />
    </div>
</template>

<script setup lang="ts">
import MyPagination from '@/components/common/MyPagination.vue'
import { get_refund_list } from '@/api/order'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useDebounceBind } from '@/utils/useDebounceBind'

const list = ref<any[]>([])
const key = ref('')
const status = ref<number | undefined>(undefined)
const pages = reactive({ page: 1, size: 10, total: 0 })

const status_opts = [
    { v: 0, l: '待审核', t: 'warning' },
    { v: 1, l: '已退款', t: 'success' },
    { v: 2, l: '已驳回', t: 'danger' },
]
const status_text = (v: number) => (status_opts.find(o => o.v === v)?.l) || '未知'
const status_type = (v: number): any => status_opts.find(o => o.v === v)?.t || 'info'

const bind = async () => {
    open_loading()
    // 后台退款列表接口：status 0待审核 1已退款 2驳回（后端按 refund_status 过滤，不支持关键字，前端做单号兜底过滤）
    const res = await get_refund_list({
        page: pages.page,
        size: pages.size,
        status: status.value
    })
    close_loading()
    if (res.code === Api_Code.ok) {
        const all = res.data.list || []
        list.value = key.value
            ? all.filter((r: any) => (r.refundNo || '').includes(key.value) || (r.orderNo || '').includes(key.value))
            : all
        pages.total = res.data.total
    }
}

const on_size = () => { pages.page = 1; bind() }
const do_search = () => { pages.page = 1; bind() }
useDebounceBind(key, do_search)

const router = useRouter()
const go_review = () => router.push('/order/review')

onMounted(bind)
</script>

<style scoped></style>
