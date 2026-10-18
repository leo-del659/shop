<template>
    <div class="l-container">
        <el-button type="primary" @click="bind">刷新待审核</el-button>
        <el-table :data="list" border height="calc(100vh - 200px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="商品名称" prop="goodsName" show-overflow-tooltip />
            <el-table-column label="分类" show-overflow-tooltip>
                <template #default="s">{{ cat_name(s.row.categoryId) }}</template>
            </el-table-column>
            <el-table-column label="价格" prop="price" />
            <el-table-column label="售价" prop="salePrice" />
            <el-table-column label="库存" prop="stock" />
            <el-table-column label="创建时间" width="170">
                <template #default="s">{{ format_date(s.row.createTime) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="220" fixed="right">
                <template #default="s">
                    <el-button size="small" type="success" @click="on_pass(s.row)">通过</el-button>
                    <el-button size="small" type="danger" @click="on_reject(s.row)">拒绝</el-button>
                </template>
            </el-table-column>
        </el-table>
        <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />
    </div>
</template>

<script setup lang="ts">
import MyPagination from '@/components/common/MyPagination.vue'
import { handle_audit, pending_list } from '@/api/goodsAudit'
import { get_category_tree } from '@/api/category'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { format_date } from '@/utils/format'
import { ElMessageBox } from 'element-plus'
import { onMounted, reactive, ref } from 'vue'

const list = ref<any[]>([])
const pages = reactive({ page: 1, size: 10, total: 0 })

// 分类 ID → 名称 映射（递归扁平化分类树，供列表渲染分类列）
const category_map = ref<Record<string, string>>({})
const cat_name = (id: any) => id ? (category_map.value[String(id)] || id) : '-'
const flatten_category = (nodes: any[], out: Record<string, string>) => {
    for (const n of nodes || []) {
        if (n && n.id != null) out[String(n.id)] = n.categoryName ?? ''
        if (n && Array.isArray(n.children) && n.children.length) flatten_category(n.children, out)
    }
}
const load_category = async () => {
    if (Object.keys(category_map.value).length) return
    const res = await get_category_tree()
    if (res.code === Api_Code.ok) {
        const map: Record<string, string> = {}
        flatten_category(res.data || [], map)
        category_map.value = map
    }
}

const bind = async () => {
    open_loading()
    const res = await pending_list({ page: pages.page, size: pages.size })
    close_loading()
    if (res.code === Api_Code.ok) {
        list.value = res.data.list
        pages.total = res.data.total
    }
}

const on_pass = async (row: any) => {
    const res = await handle_audit({ id: row.id, biz_type: 'good', audit_status: 1, auditor: 'admin' })
    show_notice(res)
    if (res.code === Api_Code.ok) bind()
}

const on_reject = (row: any) => {
    ElMessageBox.prompt('请输入拒绝原因', '拒绝审核', {
        inputValidator: (val: string) => {
            if (!val || !val.trim()) return '拒绝原因不能为空'
            return true
        },
        inputErrorMessage: '拒绝原因不能为空'
    }).then(async ({ value }) => {
        const res = await handle_audit({ id: row.id, biz_type: 'good', audit_status: 2, audit_remark: value.trim(), auditor: 'admin' })
        show_notice(res)
        if (res.code === Api_Code.ok) bind()
    })
}

const on_size = () => { pages.page = 1; bind() }

onMounted(async () => {
    // 先预加载分类映射，再拉列表——保证 cat_name 能查到名称
    await load_category()
    bind()
})
</script>

<style scoped></style>
