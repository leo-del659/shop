<template>
    <div class="l-container">
        <el-card class="welcome-card" shadow="never">
            <div class="welcome-title">💰 结算管理</div>
            <div class="welcome-desc">请选择下方功能模块进入对应账单或配置页面。</div>
            <div class="module-grid">
                <div class="module-item" @click="go('/settle/merchantbill')">
                    <div class="module-icon">🧾</div>
                    <div class="module-name">商户账单</div>
                </div>
                <div class="module-item" @click="go('/settle/receipt')">
                    <div class="module-icon">🧾</div>
                    <div class="module-name">收据管理</div>
                </div>
                <div class="module-item" @click="go('/settle/booth')">
                    <div class="module-icon">🏟️</div>
                    <div class="module-name">摊位管理</div>
                </div>
                <div class="module-item" @click="go('/settle/boothrent')">
                    <div class="module-icon">📅</div>
                    <div class="module-name">摊位租赁</div>
                </div>
                <div class="module-item" @click="go('/settle/feeconfig')">
                    <div class="module-icon">⚖️</div>
                    <div class="module-name">费用配置</div>
                </div>
            </div>
        </el-card>
    </div>
</template>

<script setup lang="ts">
import router from '@/router'

const go = (path: string) => {
    router.push(path)
}
</script>

<style scoped>
.welcome-card {
    max-width: 900px;
    margin: 40px auto;
    padding: 24px;
    background-color: var(--bg-surface);
    border: 1px solid var(--border);
}

.welcome-title {
    font-size: 22px;
    font-weight: 600;
    color: var(--text-1);
    margin-bottom: 8px;
}

.welcome-desc {
    font-size: 14px;
    color: var(--text-2);
    margin-bottom: 24px;
}

.module-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
    gap: 16px;
}

.module-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 24px 16px;
    border-radius: 12px;
    background-color: var(--bg-surface-2);
    border: 1px solid var(--border);
    cursor: pointer;
    transition: all 0.2s ease;
}

.module-item:hover {
    border-color: var(--accent);
    background-color: var(--sidebar-active-bg);
    transform: translateY(-2px);
}

.module-icon {
    font-size: 32px;
    margin-bottom: 8px;
}

.module-name {
    font-size: 14px;
    color: var(--text-1);
    font-weight: 500;
}
</style>
