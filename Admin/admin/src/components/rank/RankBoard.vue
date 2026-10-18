<template>
    <div class="rank-board">
        <!-- 头部 -->
        <div class="rb-header">
            <div class="rb-title">
                <span class="rb-icon">{{ icon }}</span>
                <div>
                    <h2>{{ title }}</h2>
                    <p>{{ subtitle }}</p>
                </div>
            </div>
            <div class="rb-meta">
                共 <b>{{ rows.length }}</b> 名 · 合计 <b>¥{{ format_money(total_amount) }}</b>
            </div>
        </div>

        <!-- 领奖台 Top 3 -->
        <div class="rb-podium" v-if="top3.length">
            <div
                v-for="(item, idx) in top3"
                :key="item.rank"
                class="rb-podium-item"
                :class="'rank-' + item.rank"
                :style="{ order: podium_order[idx] }"
            >
                <div class="rb-medal">{{ ['🥇', '🥈', '🥉'][idx] }}</div>
                <div class="rb-name" :title="item.name">{{ item.name }}</div>
                <div class="rb-no" v-if="item.no">No.{{ item.no }}</div>
                <div class="rb-amount">¥{{ format_money(item.totalAmount) }}</div>
                <div class="rb-count">{{ item.orderCount }} 单</div>
            </div>
        </div>

        <!-- 完整榜单 -->
        <div class="rb-table-card">
            <el-table :data="rows" border style="width: 100%" empty-text="暂无数据">
                <el-table-column label="排名" width="90" align="center">
                    <template #default="s">
                        <span class="rb-rank-badge" :class="'rk-' + s.row.rank">{{ s.row.rank }}</span>
                    </template>
                </el-table-column>
                <el-table-column label="名称" min-width="180">
                    <template #default="s">
                        <div class="rb-cell-name">{{ s.row.name }}</div>
                        <div class="rb-cell-no" v-if="s.row.no">{{ noLabel }}：{{ s.row.no }}</div>
                    </template>
                </el-table-column>
                <el-table-column label="订单数" width="110" align="center" prop="orderCount" sortable />
                <el-table-column label="在售商品" width="110" align="center" prop="goodsCount" sortable />
                <el-table-column label="总金额" width="160" align="right">
                    <template #default="s">
                        <span class="rb-amount-text">¥{{ format_money(s.row.totalAmount) }}</span>
                    </template>
                </el-table-column>
                <el-table-column label="金额占比" min-width="220">
                    <template #default="s">
                        <div class="rb-bar-wrap">
                            <div
                                class="rb-bar"
                                :class="'rk-bar-' + s.row.rank"
                                :style="{ width: bar_width(s.row.totalAmount) }"
                            ></div>
                            <span class="rb-bar-pct">{{ pct(s.row.totalAmount) }}%</span>
                        </div>
                    </template>
                </el-table-column>
            </el-table>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface RankRow {
    rank: number
    name: string
    no?: string
    orderCount: number
    totalAmount: number
    /** 在售商品数（可选，商户榜有值） */
    goodsCount?: number
}

const props = defineProps<{
    title: string
    subtitle: string
    icon: string
    rows: RankRow[]
    noLabel: string
}>()

// 领奖台只取前三；通过 order 控制银牌(2)在左、金牌(1)居中、铜牌(3)在右（更经典的领奖台视觉）
const top3 = computed(() => props.rows.filter(r => r.rank <= 3))
const podium_order = [2, 1, 3]

const total_amount = computed(() => props.rows.reduce((sum, r) => sum + (r.totalAmount || 0), 0))
const max_amount = computed(() => {
    const m = Math.max(0, ...props.rows.map(r => r.totalAmount || 0))
    return m || 1
})

const format_money = (v: number) => {
    const n = Number(v || 0)
    return n.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
const bar_width = (v: number) => ((v || 0) / max_amount.value * 100).toFixed(1) + '%'
const pct = (v: number) => {
    if (!total_amount.value) return '0.0'
    return ((v || 0) / total_amount.value * 100).toFixed(1)
}
</script>

<style scoped>
.rank-board {
    padding: 4px;
}

/* 头部 */
.rb-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 12px;
    margin-bottom: 18px;
    padding-bottom: 14px;
    border-bottom: 1px solid #eef0f4;
}
.rb-title {
    display: flex;
    align-items: center;
    gap: 12px;
}
.rb-icon {
    font-size: 30px;
    width: 54px;
    height: 54px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    border-radius: 14px;
    background: linear-gradient(135deg, #e8f1ff, #f4e9ff);
    box-shadow: 0 6px 16px rgba(24, 144, 255, 0.12);
}
.rb-title h2 {
    margin: 0;
    font-size: 20px;
    font-weight: 700;
    color: #1f2329;
}
.rb-title p {
    margin: 2px 0 0;
    font-size: 13px;
    color: #8a909c;
}
.rb-meta {
    font-size: 13px;
    color: #6b7280;
    background: #f6f8fa;
    padding: 8px 14px;
    border-radius: 999px;
}
.rb-meta b {
    color: #1890ff;
}

/* 领奖台 */
.rb-podium {
    display: flex;
    align-items: flex-end;
    justify-content: center;
    gap: 14px;
    margin-bottom: 22px;
}
.rb-podium-item {
    flex: 1 1 0;
    max-width: 220px;
    text-align: center;
    padding: 18px 14px 16px;
    border-radius: 16px;
    background: #fff;
    box-shadow: 0 10px 30px rgba(17, 24, 39, 0.08);
    transition: transform 0.25s cubic-bezier(0.16, 1, 0.3, 1), box-shadow 0.25s;
    border: 1px solid #eef0f4;
}
.rb-podium-item:hover {
    transform: translateY(-4px);
    box-shadow: 0 18px 40px rgba(17, 24, 39, 0.14);
}
.rb-medal {
    font-size: 34px;
    line-height: 1;
    margin-bottom: 8px;
}
.rb-name {
    font-size: 15px;
    font-weight: 700;
    color: #1f2329;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}
.rb-no {
    font-size: 12px;
    color: #9aa3af;
    margin-top: 2px;
}
.rb-amount {
    margin-top: 10px;
    font-size: 19px;
    font-weight: 800;
    background: linear-gradient(135deg, #1890ff, #7c3aed);
    -webkit-background-clip: text;
    background-clip: text;
    -webkit-text-fill-color: transparent;
}
.rb-count {
    font-size: 12px;
    color: #8a909c;
    margin-top: 2px;
}
/* 金银铜的顶部色条 */
.rb-podium-item.rank-1 {
    border-top: 4px solid #f5b623;
    transform: translateY(-8px);
}
.rb-podium-item.rank-2 {
    border-top: 4px solid #b9c2cc;
}
.rb-podium-item.rank-3 {
    border-top: 4px solid #cd7f32;
}

/* 表格卡片 */
.rb-table-card {
    background: #fff;
    border-radius: 16px;
    padding: 8px 8px 4px;
    box-shadow: 0 6px 20px rgba(17, 24, 39, 0.05);
    border: 1px solid #eef0f4;
}

.rank-board :deep(.el-table) {
    border-radius: 12px;
    overflow: hidden;
}
.rank-board :deep(.el-table th.el-table__cell) {
    background: #f7f9fc;
    color: #4b5563;
    font-weight: 600;
}

/* 排名徽章 */
.rb-rank-badge {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 28px;
    height: 28px;
    border-radius: 8px;
    font-weight: 700;
    font-size: 14px;
    background: #eef1f6;
    color: #6b7280;
}
.rb-rank-badge.rk-1 {
    background: linear-gradient(135deg, #ffd66b, #f5a623);
    color: #fff;
    box-shadow: 0 4px 10px rgba(245, 166, 35, 0.4);
}
.rb-rank-badge.rk-2 {
    background: linear-gradient(135deg, #e3e9ef, #b9c2cc);
    color: #fff;
}
.rb-rank-badge.rk-3 {
    background: linear-gradient(135deg, #e6a86e, #cd7f32);
    color: #fff;
}

/* 名称单元格 */
.rb-cell-name {
    font-weight: 600;
    color: #1f2329;
}
.rb-cell-no {
    font-size: 12px;
    color: #9aa3af;
    margin-top: 2px;
}
.rb-amount-text {
    font-weight: 700;
    color: #1890ff;
}

/* 金额占比条 */
.rb-bar-wrap {
    position: relative;
    display: flex;
    align-items: center;
    height: 22px;
    background: #f1f3f7;
    border-radius: 999px;
    overflow: hidden;
}
.rb-bar {
    height: 100%;
    border-radius: 999px;
    background: linear-gradient(90deg, #1890ff, #7c3aed);
    transition: width 0.8s cubic-bezier(0.16, 1, 0.3, 1);
}
.rb-bar.rk-bar-1 { background: linear-gradient(90deg, #f5a623, #ffd66b); }
.rb-bar.rk-bar-2 { background: linear-gradient(90deg, #b9c2cc, #e3e9ef); }
.rb-bar.rk-bar-3 { background: linear-gradient(90deg, #cd7f32, #e6a86e); }
.rb-bar-pct {
    position: absolute;
    right: 10px;
    font-size: 12px;
    font-weight: 600;
    color: #4b5563;
}
</style>
