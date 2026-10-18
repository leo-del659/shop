<template>
    <div class="l-container">
        <el-input v-model="key" placeholder="物流公司名称" style="max-width:240px" clearable @keyup.enter="bind" />
        <el-button type="primary" @click="bind">查询</el-button>
        <el-button type="success" @click="open_add">新增物流公司</el-button>
        <el-table :data="list" border height="calc(100vh - 220px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="编码" prop="code" />
            <el-table-column label="公司名称" prop="companyName" />
            <el-table-column label="电话" prop="phone" />
            <el-table-column label="创建时间" prop="createTime" width="170">
                <template #default="s">{{ format_date(s.row.createTime) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="160" fixed="right">
                <template #default="s">
                    <el-button size="small" @click="open_edit(s.row)">编辑</el-button>
                    <el-button size="small" type="danger" plain @click="on_del(s.row)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>
        <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />

        <el-dialog v-model="show" :title="form.id ? '编辑物流公司' : '新增物流公司'" width="460px">
            <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
                <el-form-item label="公司名称" prop="companyName"><el-input v-model="form.companyName" /></el-form-item>
                <el-form-item label="编码" prop="code">
                    <el-input v-model="form.code" placeholder="系统自动生成，可手动修改" />
                </el-form-item>
                <el-form-item label="电话" prop="phone"><el-input v-model="form.phone" /></el-form-item>
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
import { add_logistics, get_logistics_list, preview_next_logistics_code, remove_logistics, update_logistics } from '@/api/logistics'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { format_date } from '@/utils/format'
import { ElMessage, ElMessageBox } from 'element-plus'
import { onMounted, reactive, ref } from 'vue'
import { useDebounceBind } from '@/utils/useDebounceBind'

const list = ref<any[]>([])
const key = ref('')
const pages = reactive({ page: 1, size: 10, total: 0 })
const show = ref(false)
const form = ref<any>({})
const formRef = ref<any>(null)
const rules = {
    companyName: [{ required: true, message: '请输入公司名称', trigger: 'blur' }],
    code: [{ required: true, message: '请输入编码', trigger: 'blur' }],
    phone: [{ required: true, message: '请输入电话', trigger: 'blur' }]
}

const bind = async () => {
    open_loading()
    const res = await get_logistics_list({ page: pages.page, size: pages.size, keyword: key.value || undefined })
    close_loading()
    if (res.code === Api_Code.ok) { list.value = res.data.list; pages.total = res.data.total }
}

const open_add = async () => {
    form.value = { code: '' }
    show.value = true
    // 打开对话框时自动获取下一个可用编码
    const res = await preview_next_logistics_code()
    if (res.code === Api_Code.ok) form.value.code = res.data.code
}
const open_edit = (row: any) => { form.value = { ...row }; show.value = true }

// 启用 / 禁用 切换 — 已去除禁用功能
const toggle_ban = async (_row: any) => { /* noop: 禁用功能已下线 */ }

const save = async () => {
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) { ElMessage.warning('必填字段未填，请检查表单'); return }
    open_loading()
    const res = form.value.id ? await update_logistics(form.value) : await add_logistics(form.value)
    close_loading(); show_notice(res)
    if (res.code === Api_Code.ok) { show.value = false; bind() }
}

const on_del = async (row: any) => {
    ElMessageBox.confirm('确认删除该物流公司？').then(async () => { const res = await remove_logistics(row.id); show_notice(res); if (res.code === Api_Code.ok) bind() })
}

const on_size = () => { pages.page = 1; bind() }
const do_search = () => { pages.page = 1; bind() }
useDebounceBind(key, do_search)

onMounted(bind)
</script>

<style scoped></style>
