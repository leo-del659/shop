<template>
    <div class="l-container">
        <el-input v-model="key" placeholder="商户名称" style="max-width:240px" clearable @keyup.enter="do_search" @clear="do_search" />
        <el-button type="primary" @click="do_search">查询</el-button>
        <el-button type="success" @click="open_add">新增商户</el-button>
        <el-table :data="list" border height="calc(100vh - 220px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="商户编号" prop="merchantNo" width="110" />
            <el-table-column label="商户名称" prop="merchantName" min-width="150" show-overflow-tooltip />
            <el-table-column label="联系人" prop="contact" width="100" />
            <el-table-column label="电话" prop="phone" width="130" />
            <el-table-column label="等级" width="110">
                <template #default="s">{{ level_name(s.row.levelId) }}</template>
            </el-table-column>
            <el-table-column label="银行账户" min-width="180" show-overflow-tooltip>
                <template #default="s">{{ bank_name(s.row.bankId) }}</template>
            </el-table-column>
            <el-table-column label="地址" min-width="140" show-overflow-tooltip>
                <template #default="s">{{ s.row.address || '—' }}</template>
            </el-table-column>
            <el-table-column label="禁用" width="80">
                <template #default="s">
                    <el-switch
                        :model-value="s.row.isBan === 1"
                        @change="(v: any) => on_toggle_ban(s.row, v)"
                    />
                </template>
            </el-table-column>
            <el-table-column label="创建时间" width="170">
                <template #default="s">{{ format_date(s.row.createTime) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="220" fixed="right">
                <template #default="s">
                    <el-button size="small" @click="open_edit(s.row)">编辑</el-button>
                    <el-button size="small" type="warning" @click="open_qual(s.row)">资质</el-button>
                    <el-button size="small" type="danger" plain @click="on_del(s.row)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>
        <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />

        <el-dialog v-model="show" :title="form.id ? '编辑商户' : '新增商户'" width="560px">
            <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
                <el-form-item label="商户名称" prop="merchantName">
                    <el-input v-model="form.merchantName" placeholder="请输入商户名称" />
                </el-form-item>
                <el-form-item label="商户编号" prop="merchantNo">
                    <el-input v-model="form.merchantNo" :disabled="is_edit" placeholder="新增时自动生成，可修改" />
                </el-form-item>
                <el-form-item label="联系人" prop="contact">
                    <el-input v-model="form.contact" placeholder="请输入联系人" />
                </el-form-item>
                <el-form-item label="电话" prop="phone">
                    <el-input v-model="form.phone" placeholder="请输入联系电话" />
                </el-form-item>
                <el-form-item label="等级" prop="levelId">
                    <el-select v-model="form.levelId" filterable clearable placeholder="选择等级" style="width:100%">
                        <el-option v-for="l in level_options.filter(x => x.id)" :key="l.id" :label="l.levelName || l.id" :value="l.id" />
                    </el-select>
                </el-form-item>
                <el-form-item label="银行账户" prop="bankId">
                    <el-select v-model="form.bankId" filterable clearable placeholder="选择银行账户" style="width:100%">
                        <el-option v-for="b in bank_options.filter(x => x.id)" :key="b.id" :label="bank_label(b)" :value="b.id" />
                    </el-select>
                </el-form-item>
                <el-form-item label="地址" prop="address">
                    <el-input v-model="form.address" type="textarea" :rows="2" placeholder="请输入地址" />
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
import {
    add_merchant,
    generate_merchant_no,
    get_merchant_bank_list,
    get_merchant_level_list,
    get_merchant_list,
    remove_merchant,
    update_merchant
} from '@/api/merchant'
import { Api_Code } from '@/dto/response/base'
import { format_date } from '@/utils/format'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { ElMessage, ElMessageBox } from 'element-plus'
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useDebounceBind } from '@/utils/useDebounceBind'

const router = useRouter()

const list = ref<any[]>([])
const key = ref('')
const pages = reactive({ page: 1, size: 10, total: 0 })
const show = ref(false)
const form = ref<any>({})
const formRef = ref<any>(null)
// 编辑态（有 id）时编号只读，避免误改主业务标识
const is_edit = computed(() => !!form.value.id)
const rules = {
    merchantName: [{ required: true, message: '请输入商户名称', trigger: 'blur' }],
    merchantNo: [{ required: true, message: '请输入商户编号', trigger: 'blur' }],
    contact: [{ required: true, message: '请输入联系人', trigger: 'blur' }],
    phone: [{ required: true, message: '请输入电话', trigger: 'blur' }]
}

// 等级 / 银行下拉选项 + 列表里 ID → 名称 映射
const level_options = ref<any[]>([])
const level_map = ref<Record<string, string>>({})
const bank_options = ref<any[]>([])
const bank_map = ref<Record<string, string>>({})

const level_name = (id?: string) => id ? (level_map.value[id] || id) : '—'
const bank_name = (id?: string) => id ? (bank_map.value[id] || id) : '—'
const bank_label = (b: any) => {
    const left = b.bankName || ''
    const right = b.bankAccount || ''
    if (left && right) return `${left} / ${right}`
    return (left || right || b.id || '').trim()
}

const bind = async () => {
    open_loading()
    const res = await get_merchant_list({ page: pages.page, size: pages.size, keyword: key.value || undefined })
    close_loading()
    if (res.code === Api_Code.ok) { list.value = res.data.list; pages.total = res.data.total }
}

const open_add = async () => {
    form.value = { isBan: 0, merchantNo: '' }
    show.value = true
    // 后端自动生成 GYS + 4 位顺序号；失败也允许空提交（用户可手输）
    try {
        const r = await generate_merchant_no()
        if (r.code === Api_Code.ok && r.data?.merchantNo) {
            form.value.merchantNo = r.data.merchantNo
        }
    } catch { /* 静默：空也能保存 */ }
}
const open_edit = (row: any) => { form.value = { ...row }; show.value = true }

const save = async () => {
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) { ElMessage.warning('必填字段未填，请检查表单'); return }
    open_loading()
    const res = form.value.id ? await update_merchant(form.value) : await add_merchant(form.value)
    close_loading()
    show_notice(res)
    if (res.code === Api_Code.ok) { show.value = false; bind() }
}

const on_del = async (row: any) => {
    ElMessageBox.confirm('确认删除该商户？').then(async () => {
        const res = await remove_merchant(row.id)
        show_notice(res)
        if (res.code === Api_Code.ok) bind()
    })
}

// 直接切换 isBan：乐观更新 + 失败回滚，避免用户反复进编辑弹窗
const on_toggle_ban = async (row: any, v: any) => {
    const old = row.isBan
    row.isBan = v ? 1 : 0
    const res = await update_merchant({ ...row })
    if (res.code !== Api_Code.ok) row.isBan = old  // 回滚
    show_notice(res)
}

// 资质按钮跳到独立的「商户资质」页（该页有完整 CRUD + 上传 + 缩略图）
const open_qual = (row: any) => {
    router.push({ path: '/qualification/merchant' })
}

const on_size = () => { pages.page = 1; bind() }
const do_search = () => { pages.page = 1; bind() }
useDebounceBind(key, do_search)

const load_options = async () => {
    const [lv, bk] = await Promise.all([get_merchant_level_list(), get_merchant_bank_list()])
    if (lv.code === Api_Code.ok) {
        level_options.value = lv.data || []
        level_map.value = Object.fromEntries((lv.data || []).map((x: any) => [x.id, x.levelName || x.id]))
    }
    if (bk.code === Api_Code.ok) {
        bank_options.value = bk.data || []
        bank_map.value = Object.fromEntries((bk.data || []).map((x: any) => [x.id, bank_label(x)]))
    }
}

onMounted(async () => {
    await load_options()
    bind()
})
</script>

<style scoped></style>
