<template>
    <div class="l-container">
        <div class="rb-toolbar">
            <el-radio-group v-model="top" @change="bind">
                <el-radio-button :value="10">Top 10</el-radio-button>
                <el-radio-button :value="20">Top 20</el-radio-button>
                <el-radio-button :value="50">Top 50</el-radio-button>
            </el-radio-group>
            <el-button text type="primary" :loading="loading" @click="bind">刷新</el-button>
        </div>

        <RankBoard
            title="商户销售排行榜"
            subtitle="按已支付订单销售额汇总（已支付 + 未取消 + 未作废），单位：元"
            icon="🏪"
            no-label="商户编号"
            :rows="rows"
        />
    </div>
</template>

<script setup lang="ts">
import RankBoard from '@/components/rank/RankBoard.vue'
import { get_merchant_rank } from '@/api/rank'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { onMounted, ref } from 'vue'

const top = ref(10)
const loading = ref(false)
const rows = ref<any[]>([])

const is_unknown_merchant = (name?: string) => {
    return !name || name.replace(/[()（）\s]/g, '') === '未知商户'
}

const bind = async () => {
    loading.value = true
    open_loading()
    const res = await get_merchant_rank({ top: top.value })
    close_loading()
    loading.value = false
    if (res.code === Api_Code.ok) {
        const list = (res.data.list || [])
            .filter((r: any) => !is_unknown_merchant(r.merchantName))
            .sort((a: any, b: any) => (b.totalAmount || 0) - (a.totalAmount || 0))
            .map((r: any, idx: number) => ({
                rank: idx + 1,
                name: r.merchantName,
                // 商户(卖家)表无 shop_no 列，编号字段是 merchant_no
                no: r.merchantNo,
                orderCount: r.orderCount || 0,
                totalAmount: r.totalAmount || 0,
                goodsCount: r.goodsCount || 0
            }))
        rows.value = list
    } else {
        show_notice(res)
    }
}

onMounted(bind)
</script>

<style scoped>
.rb-toolbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 6px;
}
</style>
