<template>
    <div class="l-container">
        <el-select v-model="status" placeholder="审核状态" clearable style="width:140px" @change="on_status_change">
            <el-option v-for="o in status_opts" :key="o.v" :label="o.l" :value="o.v" />
        </el-select>
        <el-button type="primary" @click="bind">查询</el-button>
        <el-table :data="list" border height="calc(100vh - 220px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="退款单号" prop="refundNo" width="150" />
            <el-table-column label="订单号" prop="orderNo"  width="150"/>
            <el-table-column label="退款金额" prop="refundAmount" />
            <el-table-column label="类型" width="90">
                <template #default="s">{{ s.row.refundTypeText }}</template>
            </el-table-column>
            <el-table-column label="退款原因" prop="refundReason" show-overflow-tooltip />
            <el-table-column label="状态" width="100">
                <template #default="s">
                    <el-tag :type="status_tag(s.row.refundStatus)">{{ s.row.refundStatusText }}</el-tag>
                </template>
            </el-table-column>
            <el-table-column label="创建时间" prop="createTime" width="170">
                <template #default="s">{{ format_datetime(s.row.createTime) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="240" fixed="right">
                <template #default="s">
                    <el-button size="small" @click="open_detail(s.row)">详情</el-button>
                    <template v-if="s.row.refundStatus === 0">
                        <el-button size="small" type="success" @click="open_review(s.row, true)">同意</el-button>
                        <el-button size="small" type="danger" @click="open_review(s.row, false)">驳回</el-button>
                    </template>
                    <span v-else>-</span>
                </template>
            </el-table-column>
        </el-table>
        <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />

        <!-- 审核弹窗 -->
        <el-dialog v-model="show" :title="approve ? '同意退款' : '驳回退款'" width="460px">
            <el-form label-width="90px">
                <el-form-item label="退款单号"><el-input :model-value="curr?.refundNo" disabled /></el-form-item>
                <el-form-item label="审核备注">
                    <el-input v-model="remark" type="textarea" :placeholder="approve ? '可选，留空则记录默认备注' : '驳回必须填写原因'" />
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="show = false">取消</el-button>
                <el-button :type="approve ? 'success' : 'danger'" @click="do_review">确认</el-button>
            </template>
        </el-dialog>

        <!-- 详情弹窗 -->
        <el-dialog v-model="detail_show" title="退款详情" width="640px">
            <el-descriptions v-if="detail" :column="2" border size="small">
                <el-descriptions-item label="退款单号">{{ detail.refundNo }}</el-descriptions-item>
                <el-descriptions-item label="退款金额">{{ detail.refundAmount }}</el-descriptions-item>
                <el-descriptions-item label="类型">{{ detail.refundTypeText }}</el-descriptions-item>
                <el-descriptions-item label="状态">
                    <el-tag :type="status_tag(detail.refundStatus)" size="small">{{ detail.refundStatusText }}</el-tag>
                </el-descriptions-item>
                <el-descriptions-item label="退款原因" :span="2">{{ detail.refundReason || '-' }}</el-descriptions-item>
                <el-descriptions-item label="商品">{{ detail.goodsName || '-' }}</el-descriptions-item>
                <el-descriptions-item label="退货数量">{{ detail.refundNum || '-' }}</el-descriptions-item>
                <el-descriptions-item label="订单号">{{ detail.orderNo || '-' }}</el-descriptions-item>
                <el-descriptions-item label="订单状态">{{ detail.orderStatusText }}</el-descriptions-item>
                <el-descriptions-item label="订单总额">{{ detail.orderTotal }}</el-descriptions-item>
                <el-descriptions-item label="申请时间">{{ format_datetime(detail.createTime) }}</el-descriptions-item>
            </el-descriptions>

            <el-divider v-if="detail.items && detail.items.length">商品明细</el-divider>
            <el-table v-if="detail.items && detail.items.length" :data="detail.items" border size="small">
                <el-table-column label="商品" prop="goodsName" />
                <el-table-column label="数量" prop="buyNum" width="70" />
                <el-table-column label="单价" prop="buyPrice" width="100" />
                <el-table-column label="小计" prop="subAmount" width="100" />
            </el-table>

            <el-divider v-if="detail && detail.refundType === 2">退货信息</el-divider>
            <el-descriptions v-if="detail && detail.refundType === 2" :column="2" border size="small">
                <el-descriptions-item label="退货单号">{{ detail.refundNo }}</el-descriptions-item>
                <el-descriptions-item label="退货状态">{{ return_status_text(detail) }}</el-descriptions-item>
                <el-descriptions-item label="退货数量">{{ detail.refundNum || '-' }}</el-descriptions-item>
                <el-descriptions-item label="退货原因">{{ detail.refundReason || '-' }}</el-descriptions-item>
            </el-descriptions>

            <el-divider>审核记录</el-divider>
            <el-descriptions :column="1" border size="small">
                <el-descriptions-item label="处理人">{{ handleByName || '-' }}</el-descriptions-item>
                <el-descriptions-item label="审核备注">{{ detail.handleRemark || '-' }}</el-descriptions-item>
                <el-descriptions-item label="审核时间">{{ format_datetime(detail.handleTime) }}</el-descriptions-item>
            </el-descriptions>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import MyPagination from '@/components/common/MyPagination.vue'
import { get_refund_detail, get_refund_list, review_refund } from '@/api/order'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { format_datetime } from '@/utils/format'
import { ElMessage } from 'element-plus'
import { onMounted, reactive, ref } from 'vue'

const list = ref<any[]>([])
const status = ref<number | undefined>(0)          // 默认看待审核
// 全部 + 三种状态
const status_opts = [{ v: undefined as any, l: '全部' }, { v: 0, l: '待审核' }, { v: 1, l: '已退款' }, { v: 2, l: '已驳回' }]
const pages = reactive({ page: 1, size: 10, total: 0 })
const show = ref(false)
const curr = ref<any>(null)
const approve = ref(true)
const remark = ref('')

const detail_show = ref(false)
const detail = ref<any>(null)
const handleByName = ref<string | null>(null)

const status_tag = (v: number) => v === 0 ? 'warning' : (v === 1 ? 'success' : 'info')

// 合并 order_return 后：退货信息来自主退款单。仅退货退款(refundType=2)展示，状态文案与退款状态同源
const return_status_text = (d: any) => {
    if (!d || d.refundType !== 2) return ''
    return d.refundStatus === 0 ? '待退货' : d.refundStatus === 1 ? '已退货' : d.refundStatus === 2 ? '已驳回' : '未知'
}

const bind = async () => {
    open_loading()
    const res = await get_refund_list({ page: pages.page, size: pages.size, status: status.value })
    close_loading()
    if (res.code === Api_Code.ok) { list.value = res.data.list; pages.total = res.data.total }
}

const on_status_change = () => { pages.page = 1; bind() }

const open_review = (row: any, ok: boolean) => { curr.value = row; approve.value = ok; remark.value = ''; show.value = true }
const do_review = async () => {
    if (!approve.value && !remark.value.trim()) { ElMessage.warning('必填字段未填：驳回必须填写审核备注'); return }
    const res = await review_refund({ id: curr.value.id, approve: approve.value, remark: remark.value })
    show_notice(res); if (res.code === Api_Code.ok) { show.value = false; bind() }
}

const open_detail = async (row: any) => {
    open_loading()
    const res = await get_refund_detail({ id: row.id })
    close_loading()
    if (res.code === Api_Code.ok) { detail.value = res.data.detail; handleByName.value = res.data.handleByName; detail_show.value = true }
}

const on_size = () => { pages.page = 1; bind() }

onMounted(bind)
</script>

<style scoped></style>
