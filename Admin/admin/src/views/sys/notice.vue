<template>
    <div class="l-container">
        <div style="text-align:right;">
            <el-button v-if="isAdmin" type="success" @click="open_add">发布商户通知</el-button>
        </div>
        <div style="color:#909399;font-size:12px;margin:8px 2px 10px;">
            通知发布后将展示在商户端小程序首页「公告通知」区域
        </div>
        <el-table :data="list" border height="calc(100vh - 200px)">
            <el-table-column label="标题" prop="title" show-overflow-tooltip />
            <el-table-column label="内容" prop="content" show-overflow-tooltip />
            <el-table-column label="类型" width="100">
                <template #default="s">{{ notice_type_text(s.row.type) }}</template>
            </el-table-column>
            <el-table-column label="创建时间" prop="createTime" width="170">
                <template #default="s">{{ format_date(s.row.createTime) }}</template>
            </el-table-column>
            <el-table-column v-if="isAdmin" label="操作" width="160" fixed="right">
                <template #default="s">
                    <el-button size="small" @click="open_edit(s.row)">编辑</el-button>
                    <el-button size="small" type="danger" plain @click="on_del(s.row)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>

        <el-dialog v-model="show" :title="form.id ? '编辑商户通知' : '发布商户通知'" width="520px">
            <el-form ref="formRef" :model="form" :rules="rules" label-width="80px" size="small">
                <el-form-item label="标题" prop="title"><el-input v-model="form.title" /></el-form-item>
                <el-form-item label="内容" prop="content"><el-input v-model="form.content" type="textarea" :rows="4" /></el-form-item>
                <el-form-item label="类型" prop="type">
                    <el-select v-model="form.type" style="width:100%">
                        <el-option v-for="o in type_opts" :key="o.v" :label="o.l" :value="o.v" />
                    </el-select>
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
import { get_notices, save_notice, remove_notice } from '@/api/sys'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { format_date } from '@/utils/format'
import { useNoticeStore } from '@/stores/notice'
import { useUserStore } from '@/stores/user'
import { ElMessage, ElMessageBox } from 'element-plus'
import { computed, onMounted, ref } from 'vue'

const list = ref<any[]>([])
const show = ref(false)
const form = ref<any>({ type: 1, isBan: 0 })
const formRef = ref<any>(null)
const rules = {
    title: [{ required: true, message: '请输入标题', trigger: 'blur' }],
    content: [{ required: true, message: '请输入内容', trigger: 'blur' }],
    type: [{ required: true, message: '请选择类型', trigger: 'change' }]
}

const noticeStore = useNoticeStore()
const userStore = useUserStore()
const isAdmin = computed(() => userStore.get_name === '超级管理员')

const type_opts = [{ v: 1, l: '公告' }, { v: 2, l: '通知' }, { v: 3, l: '系统' }]
const notice_type_text = (v: number) => (type_opts.find(o => o.v === v)?.l) || '未知'

const bind = async () => {
    open_loading(); const res = await get_notices(); close_loading()
    if (res.code === Api_Code.ok) list.value = res.data
}
const open_add = () => { form.value = { type: 1 }; show.value = true }
const open_edit = (row: any) => { form.value = { ...row }; show.value = true }
const save = async () => {
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) { ElMessage.warning('必填字段未填，请检查表单'); return }
    open_loading(); const res = await save_notice(form.value); close_loading(); show_notice(res)
    if (res.code === Api_Code.ok) { show.value = false; bind(); noticeStore.fetchNotices() }
}
const on_del = async (row: any) => {
    ElMessageBox.confirm('确认删除该通知？').then(async () => {
        const res = await remove_notice(row.id); show_notice(res); if (res.code === Api_Code.ok) bind()
    })
}
onMounted(async () => {
    await bind()
    await noticeStore.fetchNotices() // 确保 store.notices 已填充（不受 MyHead 加载时序影响）
    noticeStore.markAllRead()         // 用户打开通知页即视为已读 → 角标数字消失
})
</script>

<style scoped></style>
