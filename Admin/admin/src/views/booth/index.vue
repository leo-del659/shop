<template>
    <div class="l-container">
        <el-input v-model="key" placeholder="租位编号/名称" style="max-width:240px" clearable @keyup.enter="bind" />
        <el-button type="primary" @click="bind">查询</el-button>
        <el-button type="success" @click="open_add">新增租位</el-button>
        <el-table :data="list" border height="calc(100vh - 220px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="租位编号" prop="boothNo" width="110" />
            <el-table-column label="名称" prop="boothName" />
            <el-table-column label="位置" prop="location" show-overflow-tooltip />
            <el-table-column label="面积(㎡)" prop="area" width="100" />
            <el-table-column label="月租金" prop="monthlyRent" width="110" />
            <el-table-column label="状态" width="100">
                <template #default="s"><el-tag :type="s.row.status === 1 ? 'success' : (s.row.status === 2 ? 'info' : 'warning')">{{ booth_status_text(s.row.status) }}</el-tag></template>
            </el-table-column>
            <el-table-column label="租户" show-overflow-tooltip>
                <template #default="s">{{ mer_name(s.row.merchantId) }}</template>
            </el-table-column>
            <el-table-column label="备注" prop="remark" show-overflow-tooltip />
            <el-table-column label="操作" width="160" fixed="right">
                <template #default="s">
                    <el-button size="small" @click="open_edit(s.row)">编辑</el-button>
                    <el-button size="small" type="danger" plain @click="on_del(s.row)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>
        <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />

        <el-dialog v-model="show" :title="form.id ? '编辑租位' : '新增租位'" width="520px">
            <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
                <el-form-item label="租位编号" prop="boothNo">
                    <el-input v-if="form.id" v-model="form.boothNo" />
                    <el-input v-else v-model="form.boothNo" placeholder="提交前自动生成" />
                </el-form-item>
                <el-form-item label="名称" prop="boothName"><el-input v-model="form.boothName" placeholder="如 A区1号铺" /></el-form-item>
                <el-form-item label="位置" prop="location"><el-input v-model="form.location" placeholder="如 美食街A区" /></el-form-item>
                <el-form-item label="面积(㎡)" prop="area"><el-input-number v-model="form.area" :precision="2" :min="0" /></el-form-item>
                <el-form-item label="月租金" prop="monthlyRent"><el-input-number v-model="form.monthlyRent" :precision="2" :min="0" /></el-form-item>
                <el-form-item label="状态" prop="status">
                    <el-select v-model="form.status" style="width:100%">
                        <el-option v-for="o in status_opts" :key="o.v" :label="o.l" :value="o.v" />
                    </el-select>
                </el-form-item>
                <el-form-item label="租户">
                    <el-select v-model="form.merchantId" filterable clearable placeholder="选择商户" style="width:100%">
                        <el-option v-for="m in merchant_options" :key="m.id" :label="m.merchantName" :value="m.id" />
                    </el-select>
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
import { add_booth, generate_booth_no, get_booth_list, remove_booth, update_booth } from '@/api/booth'
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
    boothNo: [{ required: true, message: '请输入租位编号', trigger: 'blur' }],
    boothName: [{ required: true, message: '请输入名称', trigger: 'blur' }],
    location: [{ required: true, message: '请输入位置', trigger: 'blur' }],
    area: [
        { required: true, message: '请输入面积', trigger: 'blur' },
        { type: 'number', min: 0, message: '面积不能小于0', trigger: 'blur' }
    ],
    monthlyRent: [
        { required: true, message: '请输入月租金', trigger: 'blur' },
        { type: 'number', min: 0, message: '月租金不能小于0', trigger: 'blur' }
    ],
    status: [{ required: true, message: '请选择状态', trigger: 'change' }]
}

const status_opts = [{ v: 0, l: '空闲' }, { v: 1, l: '已租' }, { v: 2, l: '停用' }]
const booth_status_text = (v: number) => (status_opts.find(o => o.v === v)?.l) || '空闲'

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
    const res = await get_booth_list({ page: pages.page, size: pages.size, keyword: key.value || undefined })
    close_loading()
    if (res.code === Api_Code.ok) { list.value = res.data.list; pages.total = res.data.total }
}

const open_add = async () => {
    form.value = { status: 0, boothNo: '' }
    show.value = true
    try {
        const r = await generate_booth_no()
        if (r.code === Api_Code.ok && r.data?.boothNo) form.value.boothNo = r.data.boothNo
    } catch { /* 静默 */ }
}
const open_edit = (row: any) => { form.value = { ...row }; show.value = true }

const save = async () => {
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) { ElMessage.warning('必填字段未填，请检查表单'); return }
    open_loading()
    const res = form.value.id ? await update_booth(form.value) : await add_booth(form.value)
    close_loading()
    show_notice(res)
    if (res.code === Api_Code.ok) { show.value = false; bind() }
}

const on_del = async (row: any) => {
    ElMessageBox.confirm('确认删除该租位？').then(async () => {
        const res = await remove_booth(row.id)
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
