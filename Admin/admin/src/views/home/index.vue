<template>
    <div class="dashboard">
        <!-- 统计卡片 -->
        <div class="stat-row">
            <div class="stat-col" v-for="s in stats" :key="s.label" @click="go(s.path)">
                <div class="stat-card" :style="{ '--tint': s.bg, '--accent': s.color }">
                    <div class="stat-icon">
                        <el-icon :size="24" color="#fff"><component :is="s.icon" /></el-icon>
                    </div>
                    <div class="stat-meta">
                        <div class="stat-value">{{ s.value }}</div>
                        <div class="stat-label">{{ s.label }}</div>
                    </div>
                    <div class="stat-arrow">
                        <el-icon :size="14" color="#fff"><ArrowRight /></el-icon>
                    </div>
                </div>
            </div>
        </div>

        <!-- 四方格看板：销售趋势 / 分类占比 / 退款趋势 / 商户排行 -->
        <div class="body-row">
            <div class="body-col">
                <div class="panel">
                    <div class="panel-header">
                        <div class="panel-title-wrap">
                            <span class="panel-title">近七周订单销售趋势</span>
                            <span class="panel-sub">实时 · 每周 00:00 更新</span>
                        </div>
                        <div class="panel-legend">
                            <span class="legend-item order"><i></i>订单量</span>
                            <span class="legend-item amount"><i></i>销售额（元）</span>
                        </div>
                    </div>
                    <div class="panel-body">
                        <div ref="salesChartRef" class="mini-chart"></div>
                    </div>
                </div>
            </div>
            <div class="body-col">
                <div class="panel">
                    <div class="panel-header">
                        <div class="panel-title-wrap">
                            <span class="panel-title">分类销售占比</span>
                            <span class="panel-sub">按商户账单金额汇总</span>
                        </div>
                    </div>
                    <div class="panel-body">
                        <div ref="categoryChartRef" class="mini-chart"></div>
                    </div>
                </div>
            </div>
        </div>

        <div class="body-row">
            <div class="body-col">
                <div class="panel">
                    <div class="panel-header">
                        <div class="panel-title-wrap">
                            <span class="panel-title">近七周退款趋势</span>
                            <span class="panel-sub">已退款单按周汇总</span>
                        </div>
                        <div class="panel-legend">
                            <span class="legend-item order"><i></i>退款笔数</span>
                            <span class="legend-item amount"><i></i>退款金额（元）</span>
                        </div>
                    </div>
                    <div class="panel-body">
                        <div ref="refundChartRef" class="mini-chart"></div>
                    </div>
                </div>
            </div>
            <div class="body-col">
                <div class="panel">
                    <div class="panel-header">
                        <div class="panel-title-wrap">
                            <span class="panel-title">商户销售排行 Top10</span>
                            <span class="panel-sub">按账单金额汇总</span>
                        </div>
                    </div>
                    <div class="panel-body">
                        <div ref="merchantChartRef" class="mini-chart"></div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, markRaw, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { get_goods_list } from '@/api/goods'
import { get_order_list, get_refund_list, get_dashboard } from '@/api/order'
import { get_merchant_list } from '@/api/merchant'
import { pending_list } from '@/api/goodsAudit'
import { getToken } from '@/utils/auth'
import * as echarts from 'echarts/core'
import { LineChart, BarChart, PieChart } from 'echarts/charts'
import {
    GridComponent, TooltipComponent, LegendComponent, GraphicComponent, TitleComponent
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import type { EChartsType } from 'echarts/core'
import {
    Goods, List, Shop, OfficeBuilding, Warning, Refresh, ArrowRight
} from '@element-plus/icons-vue'

// 按需注册 ECharts 模块，减小首页包体积
echarts.use([
    LineChart, BarChart, PieChart,
    GridComponent, TooltipComponent, LegendComponent, GraphicComponent, TitleComponent,
    CanvasRenderer
])

const router = useRouter()
const userStore = useUserStore()
const salesChartRef = ref<HTMLElement | null>(null)
const categoryChartRef = ref<HTMLElement | null>(null)
const refundChartRef = ref<HTMLElement | null>(null)
const merchantChartRef = ref<HTMLElement | null>(null)
let salesChart: EChartsType | null = null
let categoryChart: EChartsType | null = null
let refundChart: EChartsType | null = null
let merchantChart: EChartsType | null = null

// 渐变色定义
const gradients = {
    blue: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    green: 'linear-gradient(135deg, #42d392 0%, #1a9c6c 100%)',
    orange: 'linear-gradient(135deg, #f6ad55 0%, #dd6b20 100%)',
    gray: 'linear-gradient(135deg, #a0aec0 0%, #718096 100%)',
    red: 'linear-gradient(135deg, #fc8181 0%, #e53e3e 100%)',
    purple: 'linear-gradient(135deg, #b794f6 0%, #805ad5 100%)',
    cyan: 'linear-gradient(135deg, #63b3ed 0%, #3182ce 100%)',
    yellow: 'linear-gradient(135deg, #fbd38d 0%, #d69e2e 100%)',
    dark: 'linear-gradient(135deg, #718096 0%, #2d3748 100%)'
}

const statDefs = reactive([
    { label: '商品总数', value: 0, icon: markRaw(Goods), color: '#409EFF', bg: '#ecf5ff', gradient: gradients.blue, path: '/goods/list', api: () => get_goods_list({ page: 1, size: 1 }) },
    { label: '订单总数', value: 0, icon: markRaw(List), color: '#67C23A', bg: '#f0f9eb', gradient: gradients.green, path: '/order/list', api: () => get_order_list({ page: 1, size: 1 }) },
    { label: '商户总数', value: 0, icon: markRaw(Shop), color: '#E6A23C', bg: '#fdf6ec', gradient: gradients.orange, path: '/merchant', api: () => get_merchant_list({ page: 1, size: 1 }) },
    { label: '待审核商品', value: 0, icon: markRaw(Warning), color: '#F56C6C', bg: '#fef0f0', gradient: gradients.red, path: '/goods/audit', api: () => pending_list({ page: 1, size: 1 }) },
    { label: '待处理退款', value: 0, icon: markRaw(Refresh), color: '#9254DE', bg: '#f9f0ff', gradient: gradients.purple, path: '/order/refund', api: () => get_refund_list({ page: 1, size: 1, status: 0 }) },
])

const hasMenu = (path: string): boolean => {
    const walk = (nodes: any[]): boolean => {
        for (const n of nodes || []) {
            if (n.path === path) return true
            if (n.children?.length && walk(n.children)) return true
        }
        return false
    }
    return walk(userStore.get_menus || [])
}

const stats = computed(() => statDefs.map(s => ({
    ...s,
    value: hasMenu(s.path) ? s.value : '--'
})))

const go = (path: string) => router.push(path)

const hexToRgba = (hex: string, alpha: number): string => {
    const clean = hex.replace('#', '')
    if (clean.length !== 6) return hex
    const r = parseInt(clean.substring(0, 2), 16)
    const g = parseInt(clean.substring(2, 4), 16)
    const b = parseInt(clean.substring(4, 6), 16)
    return `rgba(${r}, ${g}, ${b}, ${alpha})`
}

const safeTotal = (res: any): number => {
    if (res && res.code === 0 && res.data) {
        if (typeof res.data.total === 'number') return res.data.total
        if (Array.isArray(res.data)) return res.data.length
    }
    return 0
}

// 看板多维数据（后端 /api/Order/Dashboard 聚合返回）
const dashboard = reactive({
    salesTrend: [] as { label: string; orderCount: number; salesAmount: number }[],
    refundTrend: [] as { label: string; refundCount: number; refundAmount: number }[],
    categorySales: [] as { categoryId: string; categoryName: string; amount: number; count: number }[],
    topMerchants: [] as { merchantId: string; merchantName: string; amount: number; orderCount: number }[]
})

// 真正实时：通过 SSE（Server-Sent Events）订阅后端事件流。
//  - 小程序新订单/支付/取消时，后端实时推送 order.* 事件，前端立即静默刷新
//  - EventSource 自带断线自动重连；若持续失败（如 token 失效 401）则降级为 15s 轮询兜底
//  - 页面卸载时关闭连接；标签页切回前台时若连接已断则重连
const AUTO_REFRESH_INTERVAL = 15000
let es: EventSource | null = null
let pollTimer: number | undefined
let autoRunning = false

const refresh = async (silent = false) => {
    if (autoRunning) return  // 上一轮还没跑完就跳过，避免请求堆积
    autoRunning = true
    try {
        await refresh_core(silent)
    } finally {
        autoRunning = false
    }
}

const refresh_core = async (_silent: boolean) => {
    const statTasks: Promise<any>[] = []
    const statIdx: number[] = []
    statDefs.forEach((s, i) => {
        if (hasMenu(s.path)) {
            statIdx.push(i)
            statTasks.push(s.api())
        } else {
            s.value = 0
        }
    })
    const statResults = await Promise.allSettled(statTasks)
    statResults.forEach((r, i) => {
        const def = statDefs[statIdx[i] as number]
        if (def) def.value = r.status === 'fulfilled' ? safeTotal(r.value) : 0
    })

    await loadDashboard()
}

// 拉取后端多维分析看板数据（替代原有的本地聚合演示）
const loadDashboard = async () => {
    try {
        const res: any = await get_dashboard()
        if (res && res.code === 0 && res.data) {
            dashboard.salesTrend = Array.isArray(res.data.salesTrend) ? res.data.salesTrend : []
            dashboard.refundTrend = Array.isArray(res.data.refundTrend) ? res.data.refundTrend : []
            dashboard.categorySales = Array.isArray(res.data.categorySales) ? res.data.categorySales : []
            dashboard.topMerchants = Array.isArray(res.data.topMerchants) ? res.data.topMerchants : []
        }
    } catch {
        // 接口异常时保留上一次数据，不强制清空，避免闪烁
    }
    renderAllCharts()
}

const updateTodaySales = () => {
    // SSE 即时反馈：当前周（最后一个点）销售额 +1 单、金额小幅累加；下一次真实刷新会覆盖
    const list = dashboard.salesTrend
    if (list.length === 0) return
    const today = list[list.length - 1]
    if (!today) return
    today.orderCount += 1
    today.salesAmount = Math.round((today.salesAmount + Math.random() * 80 + 20) * 100) / 100
    renderSalesChart()
}

/* ===================== 图表渲染 ===================== */

const baseGrid = { left: 48, right: 56, top: 36, bottom: 28 }
const axisLineColor = '#e0d8cd'
const axisLabelColor = '#8a7f72'
const splitLineColor = '#f3ece1'

const renderSalesChart = () => {
    if (!salesChart) return
    const list = dashboard.salesTrend || []
    const labels = list.map(d => d.label)
    const counts = list.map(d => d.orderCount)
    const amounts = list.map(d => d.salesAmount)
    const option: any = {
        tooltip: {
            trigger: 'axis',
            backgroundColor: 'rgba(255,255,255,0.96)',
            borderColor: '#f0e9df',
            textStyle: { color: '#2f2820' },
            formatter: (params: any) => {
                const dateItem = list[params[0]?.dataIndex]
                const lines = params.map((p: any) => {
                    const marker = `<span style="display:inline-block;width:8px;height:8px;border-radius:50%;background:${p.color};margin-right:6px;"></span>`
                    const val = p.seriesName === '销售额（元）' ? `¥${Number(p.value).toFixed(2)}` : `${p.value} 单`
                    return `<div style="display:flex;align-items:center;justify-content:space-between;gap:16px;margin:4px 0;">${marker}<span>${p.seriesName}</span><b>${val}</b></div>`
                })
                return `<div style="font-weight:600;margin-bottom:4px;">${dateItem?.label}</div>${lines.join('')}`
            }
        },
        grid: { ...baseGrid, right: 64 },
        legend: { show: false },
        xAxis: {
            type: 'category',
            boundaryGap: false,
            data: labels,
            axisLine: { lineStyle: { color: axisLineColor } },
            axisLabel: { color: axisLabelColor },
            axisTick: { show: false }
        },
        yAxis: [
            {
                type: 'value',
                name: '订单量',
                minInterval: 1,
                splitLine: { lineStyle: { color: splitLineColor } },
                axisLabel: { color: axisLabelColor },
                nameTextStyle: { color: axisLabelColor, padding: [0, 30, 0, 0] }
            },
            {
                type: 'value',
                name: '销售额',
                position: 'right',
                splitLine: { show: false },
                axisLabel: { color: axisLabelColor, formatter: (v: number) => `¥${v}` },
                nameTextStyle: { color: axisLabelColor, padding: [0, 0, 0, 30] }
            }
        ],
        series: [
            {
                name: '订单量',
                type: 'line',
                smooth: true,
                showSymbol: true,
                symbolSize: 8,
                data: counts,
                itemStyle: { color: '#409EFF' },
                lineStyle: { width: 3, shadowColor: 'rgba(64,158,255,0.3)', shadowBlur: 10 },
                areaStyle: {
                    color: new (echarts as any).graphic.LinearGradient(0, 0, 0, 1, [
                        { offset: 0, color: 'rgba(64,158,255,0.25)' },
                        { offset: 1, color: 'rgba(64,158,255,0.02)' }
                    ])
                }
            },
            {
                name: '销售额（元）',
                type: 'line',
                smooth: true,
                yAxisIndex: 1,
                showSymbol: true,
                symbolSize: 8,
                data: amounts,
                itemStyle: { color: '#E6A23C' },
                lineStyle: { width: 3, shadowColor: 'rgba(230,162,60,0.3)', shadowBlur: 10 },
                areaStyle: {
                    color: new (echarts as any).graphic.LinearGradient(0, 0, 0, 1, [
                        { offset: 0, color: 'rgba(230,162,60,0.25)' },
                        { offset: 1, color: 'rgba(230,162,60,0.02)' }
                    ])
                }
            }
        ]
    }
    salesChart.setOption(option, true)
}

const categoryPalette = ['#4F46E5', '#0EA5E9', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6', '#EC4899', '#14B8A6', '#F97316', '#64748B']

const renderCategoryChart = () => {
    if (!categoryChart) return
    const list = dashboard.categorySales || []
    const data = list.map(d => ({ name: d.categoryName || '未分类', value: Number(d.amount ?? 0) }))
    const option: any = {
        tooltip: {
            trigger: 'item',
            backgroundColor: 'rgba(255,255,255,0.96)',
            borderColor: '#f0e9df',
            textStyle: { color: '#2f2820' },
            formatter: (p: any) => `${p.name}<br/>¥${Number(p.value).toFixed(2)} (${p.percent}%)`
        },
        legend: { bottom: 0, textStyle: { color: axisLabelColor }, type: 'scroll' },
        color: categoryPalette,
        series: [
            {
                name: '分类销售占比',
                type: 'pie',
                radius: ['42%', '66%'],
                center: ['50%', '44%'],
                avoidLabelOverlap: true,
                itemStyle: { borderColor: '#fff', borderWidth: 2, borderRadius: 6 },
                label: { show: false },
                emphasis: {
                    label: { show: true, fontSize: 13, fontWeight: 'bold', color: '#2f2820', formatter: '{b}\n{d}%' }
                },
                data: data.length ? data : [{ name: '暂无数据', value: 1 }]
            }
        ]
    }
    categoryChart.setOption(option, true)
}

const renderRefundChart = () => {
    if (!refundChart) return
    const list = dashboard.refundTrend || []
    const labels = list.map(d => d.label)
    const counts = list.map(d => d.refundCount)
    const amounts = list.map(d => d.refundAmount)
    const option: any = {
        tooltip: {
            trigger: 'axis',
            backgroundColor: 'rgba(255,255,255,0.96)',
            borderColor: '#f0e9df',
            textStyle: { color: '#2f2820' },
            formatter: (params: any) => {
                const lines = params.map((p: any) => {
                    const marker = `<span style="display:inline-block;width:8px;height:8px;border-radius:50%;background:${p.color};margin-right:6px;"></span>`
                    const val = p.seriesName === '退款金额（元）' ? `¥${Number(p.value).toFixed(2)}` : `${p.value} 笔`
                    return `<div style="display:flex;align-items:center;justify-content:space-between;gap:16px;margin:4px 0;">${marker}<span>${p.seriesName}</span><b>${val}</b></div>`
                })
                return `<div style="font-weight:600;margin-bottom:4px;">${list[params[0]?.dataIndex]?.label}</div>${lines.join('')}`
            }
        },
        grid: { ...baseGrid, right: 64 },
        xAxis: {
            type: 'category',
            boundaryGap: false,
            data: labels,
            axisLine: { lineStyle: { color: axisLineColor } },
            axisLabel: { color: axisLabelColor },
            axisTick: { show: false }
        },
        yAxis: [
            {
                type: 'value',
                name: '笔数',
                minInterval: 1,
                splitLine: { lineStyle: { color: splitLineColor } },
                axisLabel: { color: axisLabelColor },
                nameTextStyle: { color: axisLabelColor, padding: [0, 24, 0, 0] }
            },
            {
                type: 'value',
                name: '退款额',
                position: 'right',
                splitLine: { show: false },
                axisLabel: { color: axisLabelColor, formatter: (v: number) => `¥${v}` },
                nameTextStyle: { color: axisLabelColor, padding: [0, 0, 0, 24] }
            }
        ],
        series: [
            {
                name: '退款笔数',
                type: 'line',
                smooth: true,
                showSymbol: true,
                symbolSize: 7,
                data: counts,
                itemStyle: { color: '#F56C6C' },
                lineStyle: { width: 3, shadowColor: 'rgba(245,108,108,0.3)', shadowBlur: 10 },
                areaStyle: {
                    color: new (echarts as any).graphic.LinearGradient(0, 0, 0, 1, [
                        { offset: 0, color: 'rgba(245,108,108,0.25)' },
                        { offset: 1, color: 'rgba(245,108,108,0.02)' }
                    ])
                }
            },
            {
                name: '退款金额（元）',
                type: 'line',
                smooth: true,
                yAxisIndex: 1,
                showSymbol: true,
                symbolSize: 7,
                data: amounts,
                itemStyle: { color: '#E6A23C' },
                lineStyle: { width: 3, shadowColor: 'rgba(230,162,60,0.3)', shadowBlur: 10 }
            }
        ]
    }
    refundChart.setOption(option, true)
}

const renderMerchantChart = () => {
    if (!merchantChart) return
    const list = [...(dashboard.topMerchants || [])].sort((a, b) => a.amount - b.amount) // 横向柱：底部最大
    const names = list.map(d => d.merchantName || d.merchantId)
    const amounts = list.map(d => Number(d.amount ?? 0))
    const option: any = {
        tooltip: {
            trigger: 'axis',
            axisPointer: { type: 'shadow' },
            backgroundColor: 'rgba(255,255,255,0.96)',
            borderColor: '#f0e9df',
            textStyle: { color: '#2f2820' },
            formatter: (params: any) => {
                const p = params[0]
                const item = list[p.dataIndex]
                return `${p.name}<br/>¥${Number(p.value).toFixed(2)} · ${item?.orderCount ?? 0} 笔`
            }
        },
        grid: { left: 96, right: 56, top: 16, bottom: 24 },
        xAxis: {
            type: 'value',
            splitLine: { lineStyle: { color: splitLineColor } },
            axisLabel: { color: axisLabelColor, formatter: (v: number) => `¥${v}` }
        },
        yAxis: {
            type: 'category',
            data: names,
            axisLine: { lineStyle: { color: axisLineColor } },
            axisLabel: { color: axisLabelColor, fontSize: 11 },
            axisTick: { show: false }
        },
        series: [
            {
                type: 'bar',
                barWidth: '58%',
                data: amounts,
                itemStyle: {
                    color: new (echarts as any).graphic.LinearGradient(0, 0, 1, 0, [
                        { offset: 0, color: '#4F46E5' },
                        { offset: 1, color: '#7C9CF8' }
                    ]),
                    borderRadius: [0, 6, 6, 0]
                },
                label: {
                    show: true,
                    position: 'right',
                    color: '#5c5247',
                    fontSize: 11,
                    formatter: (p: any) => `¥${Number(p.value).toFixed(0)}`
                }
            }
        ]
    }
    merchantChart.setOption(option, true)
}

const renderAllCharts = () => {
    renderSalesChart()
    renderCategoryChart()
    renderRefundChart()
    renderMerchantChart()
}

const initCharts = () => {
    if (salesChartRef.value) salesChart = echarts.init(salesChartRef.value)
    if (categoryChartRef.value) categoryChart = echarts.init(categoryChartRef.value)
    if (refundChartRef.value) refundChart = echarts.init(refundChartRef.value)
    if (merchantChartRef.value) merchantChart = echarts.init(merchantChartRef.value)
    renderAllCharts()
    window.addEventListener('resize', onResize)
}

const onResize = () => {
    salesChart?.resize()
    categoryChart?.resize()
    refundChart?.resize()
    merchantChart?.resize()
}

// 建立 SSE 长连接
const connect_sse = () => {
    const token = getToken()
    if (!token) { start_fallback_poll(); return }
    const base = (import.meta.env.VITE_API_BASE_URL as string) || ''
    const url = `${base}/api/Realtime/Stream?token=${encodeURIComponent(token)}`
    try {
        es = new EventSource(url)
    } catch {
        start_fallback_poll()
        return
    }
    es.onopen = () => { stop_fallback_poll() }
    es.onmessage = (e: MessageEvent) => {
        try {
            const evt = JSON.parse(e.data)
            // 订单相关事件：即时刷新看板统计 + 累加今日销售折线图
            if (evt && (evt.type === 'order.created' || evt.type === 'order.paid' || evt.type === 'order.updated')) {
                updateTodaySales()
                refresh(true)
            }
        } catch { /* 忽略非法帧 */ }
    }
    es.onerror = () => {
        // 浏览器会自动重连；但持续失败（如 401）时降级为轮询，避免一直空转
        if (es) { es.close(); es = null }
        start_fallback_poll()
    }
}

// 降级轮询：SSE 不可用时每 15s 拉一次
const start_fallback_poll = () => {
    stop_fallback_poll()
    pollTimer = window.setInterval(() => {
        if (document.visibilityState === 'visible') refresh(true)
    }, AUTO_REFRESH_INTERVAL)
}
const stop_fallback_poll = () => {
    if (pollTimer) { window.clearInterval(pollTimer); pollTimer = undefined }
}

// 标签页切回前台：若 SSE 已断开则重连
const on_visibility_change = () => {
    if (document.visibilityState === 'visible' && !es) {
        refresh(true)
        connect_sse()
    }
}

onMounted(() => {
    initCharts()
    refresh(false)
    connect_sse()
    document.addEventListener('visibilitychange', on_visibility_change)
})

onBeforeUnmount(() => {
    if (es) { es.close(); es = null }
    stop_fallback_poll()
    document.removeEventListener('visibilitychange', on_visibility_change)
    window.removeEventListener('resize', onResize)
    salesChart?.dispose(); salesChart = null
    categoryChart?.dispose(); categoryChart = null
    refundChart?.dispose(); refundChart = null
    merchantChart?.dispose(); merchantChart = null
})
</script>

<style scoped>
.dashboard {
    padding: 20px;
    background: #faf7f2;
}

/* 统计卡片：柔和底色 + 同色图标 */
.stat-row {
    display: grid;
    grid-template-columns: repeat(6, 1fr);
    gap: 16px;
    margin-bottom: 16px;
}

.stat-col { cursor: pointer; }

.stat-card {
    position: relative;
    display: flex;
    align-items: center;
    gap: 14px;
    padding: 22px 18px;
    border-radius: 18px;
    background: var(--tint, #fff);
    border: 1px solid rgba(0, 0, 0, 0.03);
    overflow: hidden;
    transition: transform 0.25s cubic-bezier(0.22, 1, 0.36, 1), box-shadow 0.25s ease;
}
.stat-card::after {
    content: '';
    position: absolute;
    left: 0; top: 0; bottom: 0;
    width: 4px;
    background: var(--accent, #e07a5f);
    opacity: 0.85;
}

.stat-card:hover {
    transform: translateY(-4px);
    box-shadow: 0 18px 36px rgba(91, 70, 54, 0.12);
}

.stat-icon {
    width: 48px;
    height: 48px;
    border-radius: 14px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    background: var(--accent, #e07a5f);
    box-shadow: 0 8px 18px color-mix(in srgb, var(--accent, #e07a5f) 35%, transparent);
}

.stat-meta { flex: 1; min-width: 0; }

.stat-value {
    font-size: 26px;
    font-weight: 800;
    line-height: 1.1;
    color: #2f2820;
}

.stat-label {
    font-size: 12px;
    color: #8a7f72;
    margin-top: 4px;
    font-weight: 600;
}

.stat-arrow {
    opacity: 0;
    transform: translateX(-4px);
    transition: all 0.25s ease;
}
.stat-card:hover .stat-arrow { opacity: 1; transform: translateX(0); }

/* 内容区：四方格布局（每行两块看板） */
.body-row {
    display: flex;
    gap: 20px;
    margin-bottom: 20px;
}

.body-col { flex: 1; min-width: 0; }

/* 面板：暖白底 + 细描边 */
.panel {
    background: #fff;
    border-radius: 20px;
    box-shadow: 0 10px 30px rgba(91, 70, 54, 0.06);
    border: 1px solid #f0e9df;
    overflow: hidden;
    transition: box-shadow 0.3s ease;
}
.panel:hover {
    box-shadow: 0 16px 40px rgba(91, 70, 54, 0.1);
}

.panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 20px;
    border-bottom: 1px solid #f3ece1;
}

.panel-title {
    font-weight: 700;
    font-size: 15px;
    color: #2f2820;
}

.panel-body { padding: 20px; }

.more { font-size: 13px; font-weight: 500; }

.panel-title-wrap {
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.panel-sub {
    font-size: 12px;
    color: #a99e90;
    font-weight: 500;
}

.panel-legend {
    display: flex;
    align-items: center;
    gap: 18px;
}

.legend-item {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 13px;
    color: #5c5247;
    font-weight: 500;
}

.legend-item i {
    width: 10px;
    height: 10px;
    border-radius: 50%;
}

.legend-item.order i { background: #409EFF; }
.legend-item.amount i { background: #E6A23C; }

.mini-chart {
    width: 100%;
    height: 320px;
}

/* 响应式：卡片堆叠 */
@media (max-width: 1180px) {
    .stat-row { grid-template-columns: repeat(3, 1fr); }
}
@media (max-width: 760px) {
    .stat-row { grid-template-columns: repeat(2, 1fr); }
    .body-row { flex-direction: column; }
    .mini-chart { height: 280px; }
}
</style>
