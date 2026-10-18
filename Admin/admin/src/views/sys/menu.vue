<template>
    <div class="l-container">
        <div style="display:flex;gap:12px;">
            <el-card shadow="never" header="菜单树" style="width:45%">
                <el-tree :data="menus" :props="{ label: 'name', children: 'children' }" node-key="id" default-expand-all highlight-current @node-click="on_node" />
            </el-card>
            <el-card shadow="never" header="菜单编辑" style="flex:1">
                <el-form ref="formRef" :model="form" :rules="rules" label-width="100px" size="small">
                    <el-form-item label="菜单名称" prop="menuName"><el-input v-model="form.menuName"  /></el-form-item>
                    <!-- 页面路径对普通人隐藏：编辑时自动保留原 path；新增子菜单时根据名称自动匹配 -->
                    <el-form-item label="页面路径" v-show="false">
                        <el-input v-model="form.path" />
                    </el-form-item>
                    <el-form-item label="菜单图标">
                        <div style="display:flex;gap:12px;align-items:center;">
                            <el-upload
                                action="#"
                                :auto-upload="false"
                                :show-file-list="false"
                                :on-change="on_icon_change"
                                accept="image/png,image/jpg,image/jpeg,image/gif,image/webp"
                            >
                                <el-button size="small" type="primary">上传图标</el-button>
                            </el-upload>
                            <div v-if="form.icon" style="display:flex;align-items:center;gap:8px;">
                                <img :src="iconUrl" @error="(e: any) => { (e.target as HTMLImageElement).style.display = 'none' }" style="width:28px;height:28px;object-fit:contain;border-radius:4px;border:1px solid #dcdfe6;" />
                                <el-button link type="danger" size="small" @click="form.icon = ''">清除</el-button>
                            </div>
                            <span v-else style="color:#909399;font-size:12px;">未上传</span>
                        </div>
                    </el-form-item>
                    <el-form-item label="上级菜单">
                        <el-select v-model="form.parentId" clearable placeholder="顶级菜单" style="width:100%">
                            <el-option v-for="m in flat.filter(x => x.id)" :key="m.id" :label="m.menuName" :value="m.id" />
                        </el-select>
                    </el-form-item>
                    <el-form-item label="显示顺序">
                        <el-input-number v-model="form.sort" :min="1" />
                        <span style="margin-left:8px;color:#909399;font-size:12px;">数字越小，排在越前面</span>
                    </el-form-item>
                    <el-form-item label="是否菜单">
                        <el-switch v-model="form.isMenu" :active-value="1" :inactive-value="0" />
                        <span style="margin-left:8px;color:#909399;font-size:12px;"></span>
                    </el-form-item>
                    <el-form-item>
                        <el-button type="primary" size="small" @click="save">保存菜单</el-button>
                        <el-button type="danger" size="small" :disabled="!form.id" @click="del">删除</el-button>
                        <el-button size="small" @click="form = { isMenu: 1, sort: 1, path: '', icon: '' }">新增</el-button>
                    </el-form-item>
                </el-form>
            </el-card>
        </div>
    </div>
</template>

<script setup lang="ts">
import { get_menu_tree } from '@/api/org'
import { get_menus_all, remove_menu, save_menu } from '@/api/sys'
import { upload_file } from '@/api/file'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { get_image_url } from '@/utils/image'
import { ElMessage, ElMessageBox } from 'element-plus'
import { computed, onMounted, ref, watch } from 'vue'

// 菜单名 -> 页面路径 自动映射（新增子菜单时自动填充，避免普通人手动填路径）
const pathMapByName: Record<string, string> = {
    '商品列表': '/goods/list',
    '商品审核': '/goods/audit',
    'SKU规格': '/goods/sku',
    '资质管理': '/qualification',
    '商户资质': '/qualification/merchant',
    '商户列表': '/merchant',
    '订单列表': '/order/list',
    '退款管理': '/order/refund',
    '订单退款': '/order/refund/list',
    '退款审核': '/order/review',
    '物流管理': '/logistics',
    '库存管理': '/stock',
    '商户账单': '/settle/merchantbill',
    '组织架构': '/sys/org',
    '菜单管理': '/sys/menu',
    '系统通知': '/sys/notice',
    '权限分配': '/sys/right'
}

const menus = ref<any[]>([])
const flat = ref<any[]>([])
const form = ref<any>({ isMenu: 1, sort: 1, path: '', icon: '' })
const formRef = ref<any>(null)
const rules = {
    menuName: [{ required: true, message: '请输入菜单名称', trigger: 'blur' }]
}
const iconUrl = computed(() => get_image_url(form.value.icon))

// 新增子菜单时，根据菜单名称自动匹配页面路径
watch(() => form.value.menuName, (name) => {
    if (!form.value.id && form.value.parentId && !form.value.path && name) {
        const key = name.trim()
        if (pathMapByName[key]) {
            form.value.path = pathMapByName[key]
        }
    }
})

const bind = async () => {
    open_loading()
    const [t, all] = await Promise.all([get_menu_tree(), get_menus_all()])
    close_loading()
    if (t.code === Api_Code.ok) menus.value = t.data
    if (all.code === Api_Code.ok) flat.value = all.data
}

const on_node = (data: any) => {
    form.value = {
        id: data.id,
        menuName: data.name,
        path: data.path || '',
        icon: data.icon || '',
        parentId: data.parent_id,
        isMenu: data.isMenu ?? 1,
        sort: data.sort ?? 1
    }
}

const on_icon_change = async (file: any) => {
    const raw = file?.raw
    if (!raw) return
    open_loading()
    const res = await upload_file(raw)
    close_loading()
    if (res.code === Api_Code.ok && res.data?.url) {
        form.value.icon = res.data.url
    } else {
        show_notice(res)
    }
}

const save = async () => {
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) { ElMessage.warning('必填字段未填，请检查表单'); return }
    open_loading(); const res = await save_menu(form.value); close_loading(); show_notice(res); if (res.code === Api_Code.ok) bind()
}
const del = () => {
    if (!form.value.id) return
    ElMessageBox.confirm('确认删除该菜单？').then(async () => { const res = await remove_menu(form.value.id); show_notice(res); if (res.code === Api_Code.ok) bind() })
}

onMounted(bind)
</script>

<style scoped></style>
