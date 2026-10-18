<template>
    <div class="l-container">
        <el-input v-model="key" placeholder="缴费单号" style="max-width:240px" clearable @keyup.enter="bind" />
        <el-button type="primary" @click="bind">查询</el-button>
        <el-button type="success" @click="open_add">生成缴费单</el-button>
        <el-table :data="list" border height="calc(100vh - 220px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="缴费单号" prop="rentNo" width="150" />
            <el-table-column label="租位" show-overflow-tooltip>
                <template #default="s">{{ booth_name(s.row.boothId) }}</template>
            </el-table-column>
            <el-table-column label="租户" show-overflow-tooltip>
                <template #default="s">{{ mer_name(s.row.merchantId) }}</template>
            </el-table-column>
            <el-table-column label="计费周期" width="200">
                <template #default="s">{{ format_date(s.row.periodStart) }} ~ {{ format_date(s.row.periodEnd) }}</template>
            </el-table-column>
            <el-table-column label="应缴租金" prop="rentAmount" width="110" />
            <el-table-column label="状态" width="90">
                <template #default="s"><el-tag :type="s.row.payStatus === 1 ? 'success' : 'warning'">{{ s.row.payStatus === 1 ? '已缴' : '待缴' }}</el-tag></template>
            </el-table-column>
            <el-table-column label="支付方式" width="90">
                <template #default="s">{{ pay_type_text(s.row.payType) }}</template>
            </el-table-column>
            <el-table-column label="流水号" prop="payNo" width="150" show-overflow-tooltip />
            <el-table-column label="缴费时间" width="170">
                <template #default="s">{{ format_date(s.row.payTime) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="200" fixed="right">
                <template #default="s">
                    <el-button size="small" type="primary" :disabled="s.row.payStatus === 1" @click="open_pay(s.row)">缴费</el-button>
                    <el-button size="small" @click="open_edit(s.row)">编辑</el-button>
                    <el-button size="small" type="danger" plain @click="on_del(s.row)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>
        <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />

        <el-dialog v-model="show" :title="form.id ? '编辑缴费单' : '生成缴费单'" width="520px">
            <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
                <el-form-item label="缴费单号" prop="rentNo">
                    <el-input v-if="form.id" v-model="form.rentNo" />
                    <el-input v-else v-model="form.rentNo" placeholder="提交前自动生成" />
                </el-form-item>
                <el-form-item label="租位" prop="boothId">
                    <el-select v-model="form.boothId" filterable clearable placeholder="选择租位" style="width:100%" :disabled="!!form.id" @change="on_booth_change">
                        <el-option v-for="b in booth_options" :key="b.id" :label="`${b.boothNo} ${b.boothName || ''}`" :value="b.id" />
                    </el-select>
                </el-form-item>
                <el-form-item label="租户" prop="merchantId">
                    <el-select v-model="form.merchantId" filterable clearable placeholder="选择商户" style="width:100%">
                        <el-option v-for="m in merchant_options" :key="m.id" :label="m.merchantName" :value="m.id" />
                    </el-select>
                </el-form-item>
                <el-form-item label="计费起期" prop="periodStart">
                    <el-date-picker v-model="form.periodStart" type="date" value-format="YYYY-MM-DD" style="width:100%" />
                </el-form-item>
                <el-form-item label="计费止期" prop="periodEnd">
                    <el-date-picker v-model="form.periodEnd" type="date" value-format="YYYY-MM-DD" style="width:100%" />
                </el-form-item>
                <el-form-item label="应缴租金" prop="rentAmount"><el-input-number v-model="form.rentAmount" :precision="2" :min="0" /></el-form-item>
                <el-form-item label="备注"><el-input v-model="form.remark" type="textarea" :rows="2" /></el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="show = false">取消</el-button>
                <el-button type="primary" @click="save">保存</el-button>
            </template>
        </el-dialog>

        <el-dialog v-model="pay_show" title="线上缴费" width="460px">
            <el-form label-width="90px">
                <el-form-item label="缴费单"><el-input :model-value="curr?.rentNo" disabled /></el-form-item>
                <el-form-item label="支付流水号"><el-input v-model="pay_no" placeholder="如微信/支付宝流水号" /></el-form-item>
                <el-form-item label="支付方式">
                    <el-select v-model="pay_type" style="width:100%">
                        <el-option v-for="o in pay_opts" :key="o.v" :label="o.l" :value="o.v" />
                    </el-select>
                </el-form-item>
                <el-form-item label="缴费金额"><el-input-number v-model="pay_amount" :precision="2" :min="0" /></el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="pay_show = false">取消</el-button>
                <el-button type="primary" @click="do_pay">确认缴费</el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import MyPagination from '@/components/common/MyPagination.vue'
import { get_merchant_list } from '@/api/merchant'
import { get_booth_list } from '@/api/booth'
import { add_booth_rent, generate_rent_no, get_booth_rent_list, pay_booth_rent, remove_booth_rent, update_booth_rent } from '@/api/boothRent'
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
const formRef = ref<any>(null)
const rules = {
    rentNo: [{ required: true, message: '请输入缴费单号', trigger: 'blur' }],
    boothId: [{ required: true, message: '请选择租位', trigger: 'change' }],
    merchantId: [{ required: true, message: '请选择租户', trigger: 'change' }],
    periodStart: [{ required: true, message: '请选择计费起期', trigger: 'change' }],
    periodEnd: [{ required: true, message: '请选择计费止期', trigger: 'change' }],
    rentAmount: [
        { required: true, message: '请输入应缴租金', trigger: 'blur' },
        { type: 'number', min: 0, message: '应缴租金不能小于0', trigger: 'blur' }
    ]
}

const pay_opts = [{ v: 1, l: '微信' }, { v: 2, l: '支付宝' }, { v: 3, l: '银行卡' }, { v: 4, l: '线下' }]
const pay_type_text = (v: number) => (pay_opts.find(o => o.v === v)?.l) || '-'

const merchant_options = ref<any[]>([])
const merchant_map = ref<Record<string, string>>({})
const mer_name = (id: any) => id ? (merchant_map.value[String(id)] || id) : '-'
const booth_options = ref<any[]>([])
const booth_map = ref<Record<string, any>>({})
const booth_name = (id: any) => id ? (booth_map.value[String(id)]?.boothName || booth_map.value[String(id)]?.boothNo || id) : '-'

const load_basic = async () => {
    if (!merchant_options.value.length) {
        const r = await get_merchant_list({ page: 1, size: 1000, keyword: '' })
        if (r.code === Api_Code.ok) {
            merchant_options.value = r.data.list || []
            const map: Record<string, string> = {}
            for (const m of merchant_options.value) if (m && m.id != null) map[String(m.id)] = m.merchantName ?? ''
            merchant_map.value = map
        }
    }
    if (!booth_options.value.length) {
        const r = await get_booth_list({ page: 1, size: 1000, keyword: '' })
        if (r.code === Api_Code.ok) {
            booth_options.value = r.data.list || []
            const map: Record<string, any> = {}
            for (const b of booth_options.value) if (b && b.id != null) map[String(b.id)] = b
            booth_map.value = map
        }
    }
}

const on_booth_change = (boothId: any) => {
    const b = booth_map.value[String(boothId)]
    if (b) {
        form.value.merchantId = b.merchantId || ''
        form.value.rentAmount = b.monthlyRent ?? 0
    }
}

const bind = async () => {
    open_loading()
    const res = await get_booth_rent_list({ page: pages.page, size: pages.size, keyword: key.value || undefined })
    close_loading()
    if (res.code === Api_Code.ok) { list.value = res.data.list; pages.total = res.data.total }
}

const open_add = async () => {
    form.value = { rentNo: '', payStatus: 0, rentAmount: 0, periodStart: '', periodEnd: '' }
    show.value = true
    try {
        const r = await generate_rent_no()
        if (r.code === Api_Code.ok && r.data?.rentNo) form.value.rentNo = r.data.rentNo
    } catch { /* 静默 */ }
}
const open_edit = (row: any) => { form.value = { ...row }; show.value = true }

const save = async () => {
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) { ElMessage.warning('必填字段未填，请检查表单'); return }
    open_loading()
    const res = form.value.id ? await update_booth_rent(form.value) : await add_booth_rent(form.value)
    close_loading()
    show_notice(res)
    if (res.code === Api_Code.ok) { show.value = false; bind() }
}

const pay_show = ref(false)
const curr = ref<any>(null)
const pay_no = ref('')
const pay_amount = ref(0)
const pay_type = ref(1)
const open_pay = (row: any) => { curr.value = row; pay_no.value = 'P' + Date.now(); pay_amount.value = row.rentAmount || 0; pay_type.value = 1; pay_show.value = true }
const do_pay = async () => {
    if (!pay_no.value?.trim()) { ElMessage.warning('请输入支付流水号'); return }
    if (!pay_amount.value || pay_amount.value <= 0) { ElMessage.warning('请输入大于0的缴费金额'); return }
    const res = await pay_booth_rent({ rentId: curr.value.id, payNo: pay_no.value, payAmount: pay_amount.value, payType: pay_type.value })
    show_notice(res); if (res.code === Api_Code.ok) { pay_show.value = false; bind() }
}

const on_del = async (row: any) => {
    ElMessageBox.confirm('确认删除该缴费单？').then(async () => {
        const res = await remove_booth_rent(row.id)
        show_notice(res); if (res.code === Api_Code.ok) bind()
    })
}

const on_size = () => { pages.page = 1; bind() }

onMounted(async () => {
    await load_basic()
    bind()
})
</script>

<style scoped></style>
