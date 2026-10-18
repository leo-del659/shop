<template>
    <div class="admin-header">
        <!-- 左侧：Logo + 折叠按钮 -->
        <div class="ah-left" :style="{ width: isContractile ? '64px' : '220px' }">
            <div class="ah-logo" @click="goHome">
                <el-icon v-if="isContractile" class="ah-logo-icon">
                    <Shop />
                </el-icon>
                <template v-else>
                    <el-icon class="ah-logo-icon">
                        <Shop />
                    </el-icon>
                    <span class="ah-logo-text">集邻生活</span>
                    <span class="ah-logo-sub">管理后台</span>
                </template>
            </div>
        </div>

        <!-- 中间：折叠按钮 -->
        <div class="ah-center">
            <el-icon @click="on_Contractile" class="ah-collapse-btn">
                <Expand v-if="isContractile" />
                <Fold v-else />
            </el-icon>
        </div>

        <!-- 右侧：通知 + 用户信息 -->
        <div class="ah-right">
            <el-dropdown trigger="click" class="ah-notice-dropdown" @command="onNoticeCommand" @visible-change="onNoticeDropdownChange">
                <div class="ah-notice">
                    <el-badge :value="noticeCount" :hidden="noticeCount === 0" :max="99">
                        <el-icon class="ah-notice-icon"><Bell /></el-icon>
                    </el-badge>
                </div>
                <template #dropdown>
                    <div class="ah-notice-panel">
                        <div class="ah-notice-header">
                            <span>系统通知</span>
                            <el-link type="primary" underline="never" @click="goNotice">查看全部</el-link>
                        </div>
                        <el-dropdown-item
                            v-for="item in notices"
                            :key="item.id"
                            :command="item"
                            class="ah-notice-item"
                        >
                            <div class="ah-notice-title">{{ item.title }}</div>
                            <div class="ah-notice-time">{{ format_date(item.createTime) }}</div>
                        </el-dropdown-item>
                        <el-dropdown-item v-if="notices.length === 0" disabled>
                            暂无通知
                        </el-dropdown-item>
                    </div>
                </template>
            </el-dropdown>

            <el-dialog v-model="noticeVisible" :title="activeNotice?.title || '通知详情'" width="420px">
                <div v-if="activeNotice">
                    <div style="color:#909399;font-size:12px;margin-bottom:8px;">{{ format_date(activeNotice.createTime) }}</div>
                    <div style="color:#303133;font-size:14px;line-height:1.6;white-space:pre-wrap;">{{ activeNotice.content }}</div>
                </div>
            </el-dialog>

            <div class="ah-user-info">
                <el-icon class="ah-user-avatar"><User /></el-icon>
                <span class="ah-user-name">{{ name }}</span>
                <el-dropdown trigger="click" @command="handleCommand">
                    <span class="ah-user-action">
                        <el-icon><ArrowDown /></el-icon>
                    </span>
                    <template #dropdown>
                        <el-dropdown-menu>
                            <el-dropdown-item disabled>
                                <el-icon><UserFilled /></el-icon> 账号：{{ account }}
                            </el-dropdown-item>
                            <el-dropdown-item divided command="logout">
                                <el-icon><SwitchButton /></el-icon> 退出登录
                            </el-dropdown-item>
                        </el-dropdown-menu>
                    </template>
                </el-dropdown>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, watch, ref } from 'vue';
import { useRoute } from 'vue-router';
import { Expand, Fold, Shop, User, UserFilled, ArrowDown, SwitchButton, Bell } from '@element-plus/icons-vue';
import { useUserStore } from '@/stores/user';
import { useNoticeStore } from '@/stores/notice';
import { format_date } from '@/utils/format';
import router from '@/router';
import { ElMessage } from 'element-plus';

const userStore = useUserStore();
const noticeStore = useNoticeStore();
const route = useRoute();

const name = computed(() => userStore.get_name);
const account = computed(() => userStore.get_account);
const isContractile = computed(() => userStore.get_IsContractile);

const on_Contractile = () => userStore.set_IsContractile(!isContractile.value);
const goHome = () => router.push('/');

const handleCommand = (command: string) => {
    if (command === 'logout') {
        userStore.login_out();
        ElMessage.success('已退出登录');
        setTimeout(() => router.push('/login'), 500);
    }
};

/* 通知：读取 notice store */
const notices = computed(() => noticeStore.notices);
const noticeCount = computed(() => noticeStore.unreadCount);
const goNotice = () => {
    noticeStore.markAllRead();
    router.push('/sys/notice');
};

/* 点击单条通知：弹出内容并标记该条已读，不跳转 */
const noticeVisible = ref(false);
const activeNotice = ref<any>(null);
const onNoticeCommand = (item: any) => {
    activeNotice.value = item;
    noticeVisible.value = true;
    if (item?.id != null) {
        noticeStore.markRead(item.id);
    }
};

/* 下拉打开时让红点 -1（数量为 2 则变 1，数量为 1 则变 0 自动隐藏） */
const onNoticeDropdownChange = (visible: boolean) => {
    if (visible) noticeStore.peek();
};

onMounted(noticeStore.fetchNotices);
watch(() => route.fullPath, noticeStore.fetchNotices);
</script>

<style scoped>
.admin-header {
    display: flex;
    align-items: center;
    height: 60px;
    padding: 0 16px;
    background: var(--header-bg);
    color: var(--header-text);
    border-bottom: 1px solid var(--header-border);
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
    position: relative;
    z-index: 100;
    transition: background-color 0.35s ease, border-color 0.35s ease, color 0.35s ease;
}

/* ===== 左侧 Logo 区 ===== */
.ah-left {
    flex-shrink: 0;
    transition: width 0.28s ease;
}

.ah-logo {
    display: flex;
    align-items: center;
    gap: 8px;
    cursor: pointer;
    padding: 4px 12px;
    border-radius: 8px;
    transition: background-color 0.2s;
    height: 52px;
}

.ah-logo:hover {
    background-color: var(--header-hover-bg);
}

.ah-logo-icon {
    font-size: 28px;
    color: var(--accent);
    flex-shrink: 0;
}

.ah-logo-text {
    font-size: 18px;
    font-weight: 700;
    letter-spacing: 2px;
    white-space: nowrap;
}

.ah-logo-sub {
    font-size: 12px;
    color: var(--header-sub-text);
    margin-left: 2px;
    white-space: nowrap;
    position: relative;
    top: -1px;
}

/* ===== 中间折叠按钮 ===== */
.ah-center {
    flex-shrink: 0;
    margin-left: 8px;
}

.ah-collapse-btn {
    font-size: 20px;
    cursor: pointer;
    padding: 6px;
    border-radius: 6px;
    color: var(--header-icon);
    transition: all 0.2s;
}

.ah-collapse-btn:hover {
    background-color: var(--header-hover-bg);
    color: var(--header-text);
}

/* ===== 右侧用户区 ===== */
.ah-right {
    margin-left: auto;
    flex-shrink: 0;
    display: flex;
    align-items: center;
    gap: 16px;
}

.ah-user-info {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 6px 14px;
    border-radius: 8px;
    background-color: var(--header-user-bg);
    border: 1px solid var(--header-border);
    transition: background-color 0.2s;
}

.ah-user-info:hover {
    background-color: var(--header-hover-bg);
}

.ah-user-avatar {
    font-size: 18px;
    color: var(--accent);
}

.ah-user-name {
    font-size: 14px;
    font-weight: 500;
    max-width: 100px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.ah-user-action {
    cursor: pointer;
    display: flex;
    align-items: center;
    color: var(--header-sub-text);
    transition: color 0.2s;
}

.ah-user-action:hover {
    color: var(--header-text);
}

/* ===== 通知图标 ===== */
.ah-notice {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 40px;
    border-radius: 8px;
    cursor: pointer;
    transition: background-color 0.2s;
}

.ah-notice:hover {
    background-color: var(--header-hover-bg);
}

.ah-notice-icon {
    font-size: 20px;
    color: var(--header-icon);
}

.ah-notice-panel {
    width: 280px;
    max-height: 360px;
    overflow-y: auto;
}

.ah-notice-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px 16px;
    border-bottom: 1px solid #eee;
    font-weight: 500;
}

.ah-notice-item {
    padding: 12px 16px;
    cursor: pointer;
    white-space: normal;
    line-height: 1.4;
}

.ah-notice-item:hover {
    background-color: #f5f7fa;
}

.ah-notice-title {
    color: #303133;
    font-size: 14px;
}

.ah-notice-time {
    color: #909399;
    font-size: 12px;
    margin-top: 4px;
}
</style>
