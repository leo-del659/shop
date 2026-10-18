<template>
    <div class="layout">
        <!-- 顶部 Header -->
        <div class="l-head">
            <MyHead></MyHead>
        </div>
        <!-- 主体：左侧菜单 + 右侧内容区 -->
        <div class="l-body">
            <div class="lb-left" :style="{ width: isContractile ? '64px' : '220px' }">
                <MyMenu></MyMenu>
            </div>
            <div class="lb-right">
                <!-- 标签页导航 -->
                <div class="nav-container">
                    <div
                        @click="go(item.url)"
                        v-for="item in navs"
                        :key="item.id"
                        class="nav-tag"
                        :class="item.active ? 'nav-tag-active' : ''"
                    >
                        <span class="nav-tag-name">{{ item.name }}</span>
                        <el-icon @click.stop="close_router(item.url)" class="nav-tag-close" v-if="navs.length > 1">
                            <Close />
                        </el-icon>
                    </div>
                </div>
                <!-- 页面内容 -->
                <div class="page-content">
                    <RouterView v-slot="{ Component }">
                        <transition name="fade" mode="out-in">
                            <component :is="Component" />
                        </transition>
                    </RouterView>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import MyHead from '@/components/layout/MyHead.vue';
import MyMenu from '@/components/layout/MyMenu.vue';
import router from '@/router';
import { useUserStore } from '@/stores/user';
import { Close } from '@element-plus/icons-vue';
import { computed } from 'vue';

const userStore = useUserStore();
const name = computed(() => userStore.get_name);
const navs = computed<any[]>(() => userStore.get_navs);
const isContractile = computed(() => userStore.get_IsContractile);

const go = (url: string) => {
    router.push(url);
};

const close_router = (url: string) => {
    if (navs.value.length <= 1) return;
    const n = navs.value.find(d => d.url === url);
    for (let i = 0; i < navs.value.length; i++) {
        if (navs.value[i].id === n.id) {
            navs.value.splice(i, 1);
            break;
        }
    }
    if (n.active && navs.value.length > 0) {
        navs.value[0].active = true;
        go(navs.value[0].url);
    }
    userStore.set_navs(navs);
};
</script>

<style scoped>
.layout {
    width: 100vw;
    height: 100vh;
    overflow: hidden;
    display: flex;
    flex-direction: column;
}

/* ===== 顶部 Header ===== */
.l-head {
    height: 60px;
    flex-shrink: 0;
}

/* ===== 主体区域 ===== */
.l-body {
    flex: 1;
    display: flex;
    overflow: hidden;
}

.lb-left {
    height: 100%;
    flex-shrink: 0;
    transition: width 0.28s ease;
}

.lb-right {
    flex: 1;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    background-color: var(--bg-page);
    transition: background-color 0.35s ease;
}

/* ===== 标签页导航栏 ===== */
.nav-container {
    flex-shrink: 0;
    height: 42px;
    padding: 6px 12px 0;
    background-color: var(--bg-surface);
    border-bottom: 1px solid var(--border);
    display: flex;
    gap: 6px;
    overflow-x: auto;
    transition: background-color 0.35s ease, border-color 0.35s ease;
}

.nav-container::-webkit-scrollbar {
    height: 3px;
}

.nav-container::-webkit-scrollbar-thumb {
    background-color: var(--text-4);
    border-radius: 2px;
}

.nav-tag {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    height: 30px;
    padding: 0 10px;
    font-size: 13px;
    color: var(--text-2);
    background-color: var(--bg-surface-2);
    border: 1px solid var(--border);
    border-radius: 6px;
    cursor: pointer;
    white-space: nowrap;
    transition: all 0.2s;
    flex-shrink: 0;
}

.nav-tag:hover {
    color: var(--accent);
    border-color: var(--accent);
    background-color: color-mix(in srgb, var(--accent) 14%, transparent);
}

.nav-tag-active {
    color: #fff;
    background-color: var(--accent);
    border-color: var(--accent);
}

.nav-tag-active:hover {
    color: #fff;
    background-color: var(--el-color-primary-light-3);
    border-color: var(--el-color-primary-light-3);
}

.nav-tag-name {
    line-height: 1;
}

.nav-tag-close {
    font-size: 12px;
    border-radius: 50%;
    transition: all 0.15s;
    padding: 1px;
}

.nav-tag-close:hover {
    background-color: rgba(255, 255, 255, 0.3);
    color: #fff;
}

.nav-tag-active .nav-tag-close:hover {
    background-color: rgba(255, 255, 255, 0.25);
}

/* ===== 内容区 ===== */
.page-content {
    flex: 1;
    overflow-y: auto;
    padding: 16px;
}

.page-content::-webkit-scrollbar {
    width: 6px;
}

.page-content::-webkit-scrollbar-thumb {
    background-color: var(--text-4);
    border-radius: 3px;
}

/* ===== 路由切换过渡动画 ===== */
.fade-enter-active,
.fade-leave-active {
    transition: opacity 0.18s ease;
}

.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}
</style>
