<template>
    <div class="l-container">
        <el-input v-model="key" placeholder="商户ID/名称" style="max-width:240px" clearable @keyup.enter="bind" />
        <el-button type="primary" @click="bind">查询</el-button>
        <el-button type="success" @click="open_add">新增费率配置</el-button>
        <el-table :data="list" border height="calc(100vh - 220px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="商户" show-overflow-tooltip>
                <template #default="s">{{ mer_name(s.row.merchantId) }}</template>
            </el-table-column>
            <el-table-column label="费用类型" width="120">
                <template #default="s">{{ fee_type_text(s.row.feeType) }}</template>
            </el-table-column>
            <el-table-column label="费率" width="120">
                <template #default="s">{{ rate_text(s.row.rate) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="160" fixed="right">
                <template #default="s">
                    <el-button size="small" @click="open_edit(s.row)">编辑</el-button>
                    <el-button size="small" type="danger" plain @click="on_del(s.row)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>
        <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />

        <el-dialog v-model="show" :title="form.id ? '编辑费率配置' : '新增费率配置'" width="480px">
            <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
                <el-form-item label="商户" prop="merchantId">
                    <el-select v-model="form.merchantId" filterable clearable placeholder="选择商户" style="width:100%">
                        <el-option v-for="m in merchant_options" :key="m.id" :label="m.merchantName" :value="m.id" />
                    </el-select>
                </el-form-item>
                <el-form-item label="费用类型" prop="feeType">
                    <el-select v-model="form.feeType" style="width:100%">
                        <el-option v-for="o in fee_opts" :key="o.v" :label="o.l" :value="o.v" />
                    </el-select>
                </el-form-item>
                <el-form-item label="费率" prop="rate">
                    <el-input-number v-model="form.rate" :precision="4" :min="0" :max="1" :step="0.01" style="width:100%" />
                    <div style="color:#999;font-size:12px;margin-top:4px;">小数表示，0.06 = 6%</div>
                </el-form-item>
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
import { add_fee_config, get_fee_config_list, remove_fee_config, update_fee_config } from '@/api/feeConfig'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { ElMessage, ElMessageBox } from 'element-plus'
import { onMounted, reactive, ref } from 'vue'

const list = ref<any[]>([])
const key = ref('')
const pages = reactive({ page: 1, size: 10, total: 0 })
const show = ref(false)
const form = ref<any>({})
const formRef = ref<any>(null)
const rules = {
    merchantId: [{ required: true, message: '请选择商户', trigger: 'change' }],
    feeType: [{ required: true, message: '请选择费用类型', trigger: 'change' }],
    rate: [
        { required: true, message: '请输入费率', trigger: 'blur' },
        { type: 'number', min: 0, max: 1, message: '费率必须在0-1之间', trigger: 'blur' }
    ]
}

const fee_opts = [{ v: 1, l: '平台佣金' }, { v: 2, l: '配送费' }]
const fee_type_text = (v: number) => (fee_opts.find(o => o.v === v)?.l) || '-'
const rate_text = (v: any) => (v == null ? '-' : (Number(v) * 100).toFixed(2) + '%')

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

const bind = async () => {
    open_loading()
    const res = await get_fee_config_list({ page: pages.page, size: pages.size, keyword: key.value || undefined })
    close_loading()
    if (res.code === Api_Code.ok) { list.value = res.data.list; pages.total = res.data.total }
}

const open_add = () => { form.value = { feeType: 1, rate: 0.06 }; show.value = true }
const open_edit = (row: any) => { form.value = { ...row }; show.value = true }

const save = async () => {
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) { ElMessage.warning('必填字段未填，请检查表单'); return }
    open_loading()
    const res = form.value.id ? await update_fee_config(form.value) : await add_fee_config(form.value)
    close_loading()
    show_notice(res)
    if (res.code === Api_Code.ok) { show.value = false; bind() }
}

const on_del = async (row: any) => {
    ElMessageBox.confirm('确认删除该费率配置？').then(async () => {
        const res = await remove_fee_config(row.id)
        show_notice(res); if (res.code === Api_Code.ok) bind()
    })
}

const on_size = () => { pages.page = 1; bind() }

onMounted(async () => {
    await load_merchant()
    bind()
})
</script>

<style scoped></style>
