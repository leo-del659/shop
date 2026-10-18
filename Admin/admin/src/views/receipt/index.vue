<template>
    <div class="l-container">
        <!-- 手续费汇总卡片 -->
        <div class="summary-cards" v-if="summary">
            <div class="sc-card">
                <div class="sc-label">到账总额</div>
                <div class="sc-value">{{ money(summary.total?.gross) }}</div>
            </div>
            <div class="sc-card">
                <div class="sc-label">手续费合计</div>
                <div class="sc-value warn">{{ money(summary.total?.fee) }}</div>
            </div>
            <div class="sc-card">
                <div class="sc-label">实际到账合计</div>
                <div class="sc-value success">{{ money(summary.total?.net) }}</div>
            </div>
        </div>

        <el-input v-model="key" placeholder="到账单号/商户ID" style="max-width:240px" clearable @keyup.enter="bind" />
        <el-button type="primary" @click="bind">查询</el-button>
        <el-button type="success" @click="open_add">登记到账</el-button>
        <el-table :data="list" border height="calc(100vh - 320px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="到账单号" prop="receiptNo" width="150" />
            <el-table-column label="商户" show-overflow-tooltip>
                <template #default="s">{{ mer_name(s.row.merchantId) }}</template>
            </el-table-column>
            <el-table-column label="来源" width="100">
                <template #default="s">{{ source_text(s.row.sourceType) }}</template>
            </el-table-column>
            <el-table-column label="到账金额" prop="grossAmount" width="110" />
            <el-table-column label="费率" width="90">
                <template #default="s">{{ rate_text(s.row.feeRate) }}</template>
            </el-table-column>
            <el-table-column label="手续费" prop="feeAmount" width="100" />
            <el-table-column label="实际到账" prop="netAmount" width="110" />
            <el-table-column label="结算状态" width="100">
                <template #default="s"><el-tag :type="s.row.settleStatus === 1 ? 'success' : 'warning'">{{ s.row.settleStatus === 1 ? '已结算' : '未结算' }}</el-tag></template>
            </el-table-column>
            <el-table-column label="到账时间" width="170">
                <template #default="s">{{ format_date(s.row.receiptTime) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="120" fixed="right">
                <template #default="s">
                    <el-button size="small" type="primary" :disabled="s.row.settleStatus === 1" @click="on_settle(s.row)">结算</el-button>
                </template>
            </el-table-column>
        </el-table>
        <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />

        <el-dialog v-model="show" title="登记商户到账" width="480px">
            <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
                <el-form-item label="商户" prop="merchantId">
                    <el-select v-model="form.merchantId" filterable clearable placeholder="选择商户" style="width:100%">
                        <el-option v-for="m in merchant_options" :key="m.id" :label="m.merchantName" :value="m.id" />
                    </el-select>
                </el-form-item>
                <el-form-item label="来源类型" prop="sourceType">
                    <el-select v-model="form.sourceType" style="width:100%">
                        <el-option v-for="o in source_opts" :key="o.v" :label="o.l" :value="o.v" />
                    </el-select>
                </el-form-item>
                <el-form-item label="到账金额" prop="grossAmount"><el-input-number v-model="form.grossAmount" :precision="2" :min="0" style="width:100%" /></el-form-item>
                <el-form-item label="兜底费率" prop="defaultRate">
                    <el-input-number v-model="form.defaultRate" :precision="4" :min="0" :max="1" :step="0.01" style="width:100%" />
                    <div style="color:#999;font-size:12px;margin-top:4px;">商户未单独配置费率时生效，0.06=6%</div>
                </el-form-item>
                <el-form-item label="备注"><el-input v-model="form.remark" type="textarea" :rows="2" /></el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="show = false">取消</el-button>
                <el-button type="primary" @click="save">保存</el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import MyPagination from '@/components/common/MyPagination.vue'
import { get_merchant_list } from '@/api/merchant'
import { get_fee_summary, get_receipt_list, record_receipt, settle_receipt } from '@/api/receipt'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { format_date } from '@/utils/format'
import { ElMessage, ElMessageBox } from 'element-plus'
import { onMounted, reactive, ref } from 'vue'

const list = ref<any[]>([])
const key = ref('')
const pages = reactive({ page: 1, size: 10, total: 0 })
const show = ref(false)
const form = ref<any>({})
const summary = ref<any>(null)
const formRef = ref<any>(null)
const rules = {
    merchantId: [{ required: true, message: '请选择商户', trigger: 'change' }],
    sourceType: [{ required: true, message: '请选择来源类型', trigger: 'change' }],
    grossAmount: [
        { required: true, message: '请输入到账金额', trigger: 'blur' },
        { type: 'number', min: 0, message: '到账金额不能小于0', trigger: 'blur' }
    ],
    defaultRate: [
        { required: true, message: '请输入兜底费率', trigger: 'blur' },
        { type: 'number', min: 0, max: 1, message: '兜底费率必须在0-1之间', trigger: 'blur' }
    ]
}

const source_opts = [{ v: 1, l: '订单收款' }, { v: 2, l: '其他' }]
const source_text = (v: number) => (source_opts.find(o => o.v === v)?.l) || '-'
const rate_text = (v: any) => (v == null ? '-' : (Number(v) * 100).toFixed(2) + '%')
const money = (v: any) => (v == null ? '0.00' : Number(v).toFixed(2))

const merchant_options = ref<any[]>([])
const merchant_map = ref<Record<string, string>>({})
const mer_name = (id: any) => id ? (merchant_map.value[String(id)] || id) : '-'
const load_merchant = async () => {
    if (merchant_options.value.length) return
    const r = await get_merchant_list({ page: 1, size: 1000, keyword: '' })
    if (r.code === Api_Code.ok) {
        merchant_options.value = r.data.list || []
        const map: Record<string, string> = {}
        for (const m of merchant_options.value) if (m && m.id != null) map[String(m.id)] = m.merchantName ?? ''
        merchant_map.value = map
    }
}

const load_summary = async () => {
    const r = await get_fee_summary()
    if (r.code === Api_Code.ok) summary.value = r.data
}

const bind = async () => {
    open_loading()
    const res = await get_receipt_list({ page: pages.page, size: pages.size, keyword: key.value || undefined })
    close_loading()
    if (res.code === Api_Code.ok) { list.value = res.data.list; pages.total = res.data.total }
    load_summary()
}

const open_add = () => { form.value = { sourceType: 1, grossAmount: 0, defaultRate: 0.06 }; show.value = true }
const save = async () => {
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) { ElMessage.warning('必填字段未填，请检查表单'); return }
    open_loading()
    const res = await record_receipt({
        merchantId: form.value.merchantId,
        grossAmount: form.value.grossAmount,
        sourceType: form.value.sourceType,
        defaultRate: form.value.defaultRate,
        remark: form.value.remark
    })
    close_loading()
    show_notice(res)
    if (res.code === Api_Code.ok) { show.value = false; bind() }
}

const on_settle = (row: any) => {
    ElMessageBox.confirm('确认将该笔到账标记为已结算？').then(async () => {
        const res = await settle_receipt(row.id)
        show_notice(res); if (res.code === Api_Code.ok) bind()
    })
}

const on_size = () => { pages.page = 1; bind() }

onMounted(async () => {
    await load_merchant()
    bind()
})
</script>

<style scoped>
.summary-cards {
    display: flex;
    gap: 12px;
    margin-bottom: 8px;
}
.sc-card {
    flex: 1;
    background: #f7f9fc;
    border: 1px solid #eef1f6;
    border-radius: 10px;
    padding: 14px 16px;
}
.sc-label {
    color: #8a94a6;
    font-size: 13px;
    margin-bottom: 6px;
}
.sc-value {
    font-size: 22px;
    font-weight: 700;
    color: #1f2d3d;
}
.sc-value.warn { color: #e6a23c; }
.sc-value.success { color: #67c23a; }
</style>
