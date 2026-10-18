<template>
    <div class="l-container">
        <div style="display:flex;gap:8px;">
            <div style="width:260px;border-right:1px solid #eee;">
                <el-input v-model="key" placeholder="搜索商品" style="margin-bottom:8px" clearable @keyup.enter="bind_goods" />
                <el-table :data="goods" highlight-current-row @current-change="on_goods" height="calc(100vh - 200px)" size="small">
                    <el-table-column prop="goodsName" label="商品" show-overflow-tooltip />
                </el-table>
            </div>
            <div style="flex:1">
                <div v-if="curr" style="margin-bottom:8px;">
                    <el-tag>当前商品：{{ curr.goodsName }}</el-tag>
                    <el-button type="primary" size="small" style="margin-left:8px" @click="add_sku">新增SKU</el-button>
                    <el-button type="success" size="small" @click="save">保存SKU规格</el-button>
                </div>
                <el-table :data="skus" border height="calc(100vh - 240px)">
                    <el-table-column label="规格值" width="220">
                        <template #default="s"><el-input v-model="s.row.skuName" /></template>
                    </el-table-column>
                    <el-table-column label="价格">
                        <template #default="s"><el-input-number v-model="s.row.price" :precision="2" /></template>
                    </el-table-column>
                    <el-table-column label="库存">
                        <template #default="s"><el-input-number v-model="s.row.stock" :min="0" /></template>
                    </el-table-column>
                    <el-table-column label="操作" width="80">
                        <template #default="s"><el-button size="small" type="danger" @click="del_sku(s.$index)">删除</el-button></template>
                    </el-table-column>
                </el-table>
                <el-alert v-if="!curr" title="请先在左侧选择一个商品" type="info" :closable="false" style="margin-top:40px" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { get_goods, get_goods_list, update_goods } from '@/api/goods'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { ElMessage } from 'element-plus'
import { onMounted, ref } from 'vue'

const goods = ref<any[]>([])
const key = ref('')
const curr = ref<any>(null)
const skus = ref<any[]>([])

const bind_goods = async () => {
    open_loading()
    const res = await get_goods_list({ page: 1, size: 100, keyword: key.value || undefined })
    close_loading()
    if (res.code === Api_Code.ok) goods.value = res.data.list
}

const on_goods = async (row: any) => {
    if (!row) return
    open_loading()
    const res = await get_goods(row.id)
    close_loading()
    if (res.code === Api_Code.ok) {
        curr.value = res.data.good
        // 后端响应是 camelCase（skuName/specId），UI 字段保持一致，
        // 让 to_save_payload() 通过 .skuName/.specName 正确取到值。
        skus.value = (res.data.skus || []).map((d: any) => ({
            id: d.id,
            specId: d.specId ?? null,
            specName: d.specName || '',
            skuName: d.skuName || '',
            price: d.price ?? 0,
            stock: d.stock ?? 0
        }))
    }
}

const add_sku = () => skus.value.push({ specId: null, specName: '', skuName: '', price: curr.value?.salePrice || 0, stock: 0 })

const del_sku = (i: number) => skus.value.splice(i, 1)

const save = async () => {
    if (!curr.value) return
    if (skus.value.length === 0) { ElMessage.warning('请新增至少一条SKU规格'); return }
    for (const s of skus.value) {
        if (!s.skuName?.trim()) { ElMessage.warning('规格值必填'); return }
        if (s.price == null) { ElMessage.warning('价格必填'); return }
        if (s.stock == null || s.stock < 0) { ElMessage.warning('库存必填且不能小于0'); return }
    }
    open_loading()
    // 把 curr 整对象铺底 + skus 覆盖，确保不会因为缺字段而触发后端 "操作异常"。
    // （商户 merchantId 来自已拉取的商品数据，编辑时不改；库存 stock 由规格汇总。）
    const payload = {
        ...curr.value,
        skus: skus.value
    }
    const res = await update_goods(payload)
    close_loading()
    show_notice(res)
}

onMounted(bind_goods)
</script>

<style scoped></style>
