<template>
    <div class="l-container">
        <el-row :gutter="12">
            <el-col :span="10">
                <el-card shadow="never" header="部门管理">
                    <el-tree :data="dept_tree" :props="{ label: 'name', children: 'children' }" node-key="id" default-expand-all highlight-current @node-click="on_dept" style="margin-bottom:8px" />
                    <el-form ref="deptFormRef" :model="dept_form" :rules="deptRules" label-width="70px" size="small">
                        <el-form-item label="部门名" prop="deptName"><el-input v-model="dept_form.deptName" /></el-form-item>
                        <el-form-item label="上级" prop="parentId"><el-select v-model="dept_form.parentId" clearable placeholder="顶级部门" style="width:100%">
                            <el-option v-for="d in depts" :key="d.id" :label="d.deptName" :value="d.id" />
                        </el-select></el-form-item>
                        <el-form-item label="排序"><el-input-number v-model="dept_form.sort" /></el-form-item>
                        <el-form-item label="禁用"><el-switch v-model="dept_form.isBan" :active-value="1" :inactive-value="0" /></el-form-item>
                        <el-button type="primary" size="small" @click="save_dept">保存部门</el-button>
                        <el-button type="danger" size="small" @click="del_dept" :disabled="!dept_form.id">删除</el-button>
                        <el-button size="small" @click="dept_form = { sort: 1, isBan: 0 }">新增</el-button>
                    </el-form>
                </el-card>
            </el-col>
            <el-col :span="14">
                <el-card shadow="never" header="角色管理">
                    <el-button type="success" size="small" style="margin-bottom:8px" @click="open_add_role">新增角色</el-button>
                    <el-table :data="roles" border height="calc(100vh - 320px)">
                        <el-table-column label="角色名" prop="roleName" />
                        <el-table-column label="角色Key" prop="roleKey" />
                        <el-table-column label="操作" width="150">
                            <template #default="s">
                                <el-button size="small" @click="open_edit_role(s.row)">编辑</el-button>
                                <el-button size="small" type="danger" plain @click="del_role(s.row)">删除</el-button>
                            </template>
                        </el-table-column>
                    </el-table>
                    <el-dialog v-model="role_show" :title="role_form.id ? '编辑角色' : '新增角色'" width="420px">
                        <el-form ref="roleFormRef" :model="role_form" :rules="roleRules" label-width="80px" size="small">
                            <el-form-item label="角色名" prop="roleName"><el-input v-model="role_form.roleName" /></el-form-item>
                            <el-form-item label="角色Key" prop="roleKey"><el-input v-model="role_form.roleKey" /></el-form-item>
                            <el-form-item label="备注"><el-input v-model="role_form.remark" /></el-form-item>
                            <el-form-item label="禁用"><el-switch v-model="role_form.isBan" :active-value="1" :inactive-value="0" /></el-form-item>
                        </el-form>
                        <template #footer>
                            <el-button @click="role_show = false">取消</el-button>
                            <el-button type="primary" @click="save_role">保存</el-button>
                        </template>
                    </el-dialog>
                </el-card>
            </el-col>
        </el-row>
    </div>
</template>

<script setup lang="ts">
import { get_dept_tree, get_depts, get_roles, remove_dept, remove_role, save_dept as api_save_dept, save_role as api_save_role, get_dept_roles, save_dept_role } from '@/api/sys'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { ElMessage, ElMessageBox } from 'element-plus'
import { onMounted, reactive, ref } from 'vue'

const dept_tree = ref<any[]>([])
const depts = ref<any[]>([])
const roles = ref<any[]>([])
const dept_form = ref<any>({ sort: 1, isBan: 0 })
const role_form = ref<any>({ sort: 1, isBan: 0 })
const role_show = ref(false)
const deptFormRef = ref<any>(null)
const roleFormRef = ref<any>(null)
const deptRules = {
    deptName: [{ required: true, message: '请输入部门名', trigger: 'blur' }]
}
const roleRules = {
    roleName: [{ required: true, message: '请输入角色名', trigger: 'blur' }],
    roleKey: [{ required: true, message: '请输入角色Key', trigger: 'blur' }]
}

const bind = async () => {
    open_loading()
    const [t, d, r] = await Promise.all([get_dept_tree(), get_depts(), get_roles()])
    close_loading()
    if (t.code === Api_Code.ok) dept_tree.value = t.data
    if (d.code === Api_Code.ok) depts.value = d.data
    if (r.code === Api_Code.ok) roles.value = r.data
    void get_dept_roles() // 预加载部门角色（权限分配页使用）
}

const on_dept = (data: any) => {
    dept_form.value = {
        id: data.id,
        deptName: data.name,
        parentId: data.parent_id,
        sort: data.sort,
        isBan: data.isBan || 0
    }
}

const save_dept = async () => {
    const valid = await deptFormRef.value?.validate().catch(() => false)
    if (!valid) { ElMessage.warning('必填字段未填，请检查表单'); return }
    open_loading(); const res = await api_save_dept(dept_form.value); close_loading(); show_notice(res); if (res.code === Api_Code.ok) bind()
}
const del_dept = () => {
    if (!dept_form.value.id) return
    ElMessageBox.confirm('确认删除该部门？').then(async () => { const res = await remove_dept(dept_form.value.id); show_notice(res); if (res.code === Api_Code.ok) bind() })
}
const save_role = async () => {
    const valid = await roleFormRef.value?.validate().catch(() => false)
    if (!valid) { ElMessage.warning('必填字段未填，请检查表单'); return }
    open_loading(); const res = await api_save_role(role_form.value); close_loading(); show_notice(res); if (res.code === Api_Code.ok) { role_show.value = false; bind() }
}
// 打开新增角色对话框（去除排序后默认仅初始化 isBan）
const open_add_role = () => { role_form.value = { isBan: 0 }; role_show.value = true }
// 打开编辑角色对话框
const open_edit_role = (row: any) => { role_form.value = { ...row }; role_show.value = true }
const del_role = (row: any) => {
    ElMessageBox.confirm('确认删除该角色？').then(async () => { const res = await remove_role(row.id); show_notice(res); if (res.code === Api_Code.ok) bind() })
}

// 暴露保存部门角色的方法（供权限分配页复用，这里不渲染按钮）
void save_dept_role

onMounted(bind)
</script>

<style scoped></style>
