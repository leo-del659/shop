<template>
    <div class="l-container">
        <el-input v-model="key" placeholder="商品名" style="max-width:240px" clearable @keyup.enter="bind" />
        <el-button type="primary" @click="bind">查询</el-button>
        <el-button type="success" @click="open_add">新增库存记录</el-button>
        <el-table :data="list" border height="calc(100vh - 220px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="商品" prop="goodsName" show-overflow-tooltip />
            <el-table-column label="仓库" prop="warehouse" />
            <el-table-column label="总库存" prop="totalStock" />
            <el-table-column label="锁定库存" prop="lockStock" />
            <el-table-column label="创建时间" prop="createTime" width="170">
                <template #default="s">{{ format_date(s.row.createTime) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="240" fixed="right">
                <template #default="s">
                    <el-button size="small" type="success" @click="open_in(s.row)">入库</el-button>
                    <el-button size="small" type="warning" @click="open_out(s.row)">出库</el-button>
                    <el-button size="small" type="danger" plain @click="on_del(s.row)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>
        <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />

        <el-dialog v-model="show" :title="form.id ? '编辑库存' : '新增库存记录'" width="460px">
            <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
                <el-form-item label="商品" prop="goodsId">
                    <el-select v-model="form.goodsId" filterable clearable placeholder="选择商品" style="width:100%">
                        <el-option v-for="g in goods_options" :key="g.id" :label="g.goodsName" :value="g.id" />
                    </el-select>
                </el-form-item>
                <el-form-item label="仓库" prop="warehouse">
                    <el-select v-model="form.warehouse" filterable allow-create default-first-option clearable placeholder="选择或输入仓库名" style="width:100%">
                        <el-option v-for="w in warehouse_options" :key="w" :label="w" :value="w" />
                    </el-select>
                </el-form-item>
                <el-form-item label="总库存" prop="totalStock"><el-input-number v-model="form.totalStock" :min="0" /></el-form-item>
                <el-form-item label="锁定库存"><el-input-number v-model="form.lockStock" :min="0" /></el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="show = false">取消</el-button>
                <el-button type="primary" @click="save">保存</el-button>
            </template>
        </el-dialog>

        <el-dialog v-model="io_show" :title="io_type === 'in' ? '库存入库' : '库存出库'" width="460px">
            <el-form label-width="90px">
                <el-form-item label="商品">
                    <span>{{ goods_name(curr?.goodsId) }}</span>
                </el-form-item>
                <el-form-item label="类型">
                    <el-select v-model="io_sub_type" style="width:100%" placeholder="请选择出入库类型">
                        <el-option v-for="o in io_type_opts" :key="o.v" :label="o.l" :value="o.v" />
                    </el-select>
                </el-form-item>
                <el-form-item label="状态">
                    <el-select v-model="io_status" style="width:100%" placeholder="根据类型联动">
                        <el-option v-for="o in io_status_opts" :key="o.v" :label="o.l" :value="o.v" />
                    </el-select>
                </el-form-item>
                <el-form-item label="仓库">
                    <el-select v-model="warehouse" filterable placeholder="选择仓库" style="width:100%">
                        <el-option v-for="w in warehouse_options" :key="w" :label="w" :value="w" />
                    </el-select>
                </el-form-item>
                <el-form-item label="数量"><el-input-number v-model="num" :min="1" /></el-form-item>
                <el-form-item label="备注"><el-input v-model="remark" type="textarea" /></el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="io_show = false">取消</el-button>
                <el-button :type="io_type === 'in' ? 'success' : 'warning'" @click="do_io">确认</el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import MyPagination from '@/components/common/MyPagination.vue'
import { get_goods_list } from '@/api/goods'
import { add_stock, get_stock_list, get_warehouse_list, remove_stock, stock_in, stock_out, update_stock } from '@/api/stock'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { format_date } from '@/utils/format'
import { ElMessage, ElMessageBox } from 'element-plus'
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useDebounceBind } from '@/utils/useDebounceBind'

const list = ref<any[]>([])
const key = ref('')
const pages = reactive({ page: 1, size: 10, total: 0 })
const show = ref(false)
const form = ref<any>({})
const formRef = ref<any>(null)
const rules = {
    goodsId: [{ required: true, message: '请选择商品', trigger: 'change' }],
    warehouse: [{ required: true, message: '请选择或输入仓库', trigger: 'change' }],
    totalStock: [
        { required: true, message: '请输入总库存', trigger: 'blur' },
        { type: 'number', min: 0, message: '总库存不能小于0', trigger: 'blur' }
    ]
}

// 商品 名称映射与下拉数据源
const goods_options = ref<any[]>([])
const goods_map = ref<Record<string, string>>({})
const goods_name = (id: any) => id ? (goods_map.value[String(id)] || id) : '-'
const load_goods = async () => {
    if (goods_options.value.length) return
    const r = await get_goods_list({ page: 1, size: 1000, keyword: '' })
    if (r.code === Api_Code.ok) {
        goods_options.value = r.data.list || []
        const map: Record<string, string> = {}
        for (const g of goods_options.value) if (g && g.id != null) map[String(g.id)] = g.goodsName ?? ''
        goods_map.value = map
    }
}

// 仓库下拉数据源：来自后端 stock 表 distinct，取一次即可缓存
const warehouse_options = ref<string[]>([])
const load_warehouse = async () => {
    if (warehouse_options.value.length) return
    const r = await get_warehouse_list()
    if (r.code === Api_Code.ok) warehouse_options.value = (r.data || []) as string[]
}

const io_show = ref(false)
const io_type = ref<'in' | 'out'>('in')
const io_sub_type = ref<string>('')   // 弹窗内二级联动：出入库的子类型（绑定文字 label）
const io_status = ref<string>('')     // 弹窗内二级联动：状态（按子类型联动）
const curr = ref<any>(null)
const warehouse = ref('')
const num = ref(1)
const remark = ref('')

// 类型选项：按 入库/出库 给出不同的子类型。label 为中文文字（"类型绑定文字"）
const io_type_opts = computed(() => io_type.value === 'in'
    ? [
        { v: 'purchase', l: '采购入库' },
        { v: 'return', l: '退货入库' },
        { v: 'transfer', l: '调拨入库' }
    ]
    : [
        { v: 'sale', l: '销售出库' },
        { v: 'damage', l: '报损出库' },
        { v: 'transfer', l: '调拨出库' }
    ]
)

// 状态选项：按 io_sub_type 二级联动
const io_status_opts = computed(() => {
    if (io_type.value === 'in') {
        // 入库侧：采购/退货/调拨都共用 待入库 → 已入库 两个阶段
        return [
            { v: 'pending', l: '待入库' },
            { v: 'done', l: '已入库' }
        ]
    }
    // 出库侧：销售/报损/调拨都共用 待出库 → 已出库 两个阶段
    return [
        { v: 'pending', l: '待出库' },
        { v: 'done', l: '已出库' }
    ]
})

// 子类型变化时，重置状态为该类型下的第一项，避免状态残留非法值
watch(io_sub_type, () => {
    io_status.value = io_status_opts.value[0]?.v || ''
})

const bind = async () => {
    open_loading()
    const res = await get_stock_list({ page: pages.page, size: pages.size, keyword: key.value || undefined })
    close_loading()
    if (res.code === Api_Code.ok) { list.value = res.data.list; pages.total = res.data.total }
}

const open_add = () => { form.value = { totalStock: 0, lockStock: 0 }; show.value = true }
const save = async () => {
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) { ElMessage.warning('必填字段未填，请检查表单'); return }
    open_loading()
    const res = form.value.id ? await update_stock(form.value) : await add_stock(form.value)
    close_loading(); show_notice(res)
    if (res.code === Api_Code.ok) { show.value = false; bind() }
}

const open_in = (row: any) => {
    curr.value = row
    io_type.value = 'in'
    io_sub_type.value = 'purchase'   // 默认选中「采购入库」
    io_status.value = 'pending'      // 子类型联动后自动定位到「待入库」
    warehouse.value = row.warehouse || ''
    num.value = 1
    remark.value = ''
    io_show.value = true
}
const open_out = (row: any) => {
    curr.value = row
    io_type.value = 'out'
    io_sub_type.value = 'sale'       // 默认选中「销售出库」
    io_status.value = 'pending'      // 子类型联动后自动定位到「待出库」
    warehouse.value = row.warehouse || ''
    num.value = 1
    remark.value = ''
    io_show.value = true
}
const do_io = async () => {
    if (!io_sub_type.value) { ElMessage.warning('请选择类型'); return }
    if (!io_status.value) { ElMessage.warning('请选择状态'); return }
    if (!warehouse.value) { ElMessage.warning('请选择仓库'); return }
    if (!num.value || num.value <= 0) { ElMessage.warning('请输入大于0的数量'); return }
    // 入库/出库"不指定"项对应 value=''，转回 null 让后端按 goodsId 任意匹配逻辑生效
    const wh = warehouse.value && warehouse.value.length ? warehouse.value : undefined
    const data = {
        goodsId: curr.value.goodsId,
        warehouse: wh,
        num: num.value,
        remark: remark.value,
        type: io_sub_type.value,
        status: io_status.value
    }
    const res = io_type.value === 'in' ? await stock_in(data) : await stock_out(data)
    show_notice(res); if (res.code === Api_Code.ok) { io_show.value = false; bind() }
}

const on_del = async (row: any) => {
    ElMessageBox.confirm('确认删除该库存记录？').then(async () => { const res = await remove_stock(row.id); show_notice(res); if (res.code === Api_Code.ok) bind() })
}

const on_size = () => { pages.page = 1; bind() }
const do_search = () => { pages.page = 1; bind() }
useDebounceBind(key, do_search)

onMounted(async () => {
    // 预加载商品映射、商品下拉项、仓库下拉项，再拉库存列表
    await Promise.all([load_goods(), load_warehouse()])
    bind()
})
</script>

<style scoped></style>
