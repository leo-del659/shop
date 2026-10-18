<template>
    <div class="menu">
        <el-menu router :default-active="route.path" :collapse="isContractile" class="el-menu-vertical-demo">
            <!-- 首页/运营看板 - 固定置顶，不依赖后端菜单 -->
            <el-menu-item index="/">
                <i class="menu-icon">🏠</i>
                <template #title>首页</template>
            </el-menu-item>

            <template v-for="item in displayMenus" :key="item.id">
                <template v-if="!item.children || item.children.length === 0">
                    <el-menu-item :index="item.path || item.id">
                        <img v-if="isImageUrl(item.icon)" :src="get_image_url(item.icon)" class="menu-icon-img" />
                        <i v-else class="menu-icon">{{ getMenuIcon(item) }}</i>
                        <template #title>{{ cleanMenuName(item.name) }}</template>
                    </el-menu-item>
                </template>
                <template v-else>
                    <el-sub-menu :index="item.path || item.id">
                        <template #title>
                            <img v-if="isImageUrl(item.icon)" :src="get_image_url(item.icon)" class="menu-icon-img" />
                            <i v-else class="menu-icon">{{ getMenuIcon(item) }}</i>
                            <span>{{ cleanMenuName(item.name) }}</span>
                        </template>
                        <el-menu-item-group>
                            <el-menu-item v-for="cd in item.children" :key="cd.id" :index="cd.path || cd.id">
                                <img v-if="isImageUrl(cd.icon)" :src="get_image_url(cd.icon)" class="menu-icon-img" />
                                <i v-else class="menu-icon">{{ getMenuIcon(cd) }}</i>
                                <span>{{ cleanMenuName(cd.name) }}</span>
                            </el-menu-item>
                        </el-menu-item-group>
                    </el-sub-menu>
                </template>
            </template>
        </el-menu>
        <!-- 底部品牌 -->
        <div class="sidebar-footer" v-show="!isContractile">
            <span class="sidebar-footer-text">Community Admin v1.0</span>
        </div>
    </div>

</template>

<script setup lang="ts">
import { useUserStore } from '@/stores/user';
import { get_image_url } from '@/utils/image';
import { computed, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';

//状态 不是指单一的变量 
const userStore = useUserStore();
const isContractile = computed(state => userStore.get_IsContractile)

const menus = computed<any[]>(state => userStore.get_menus);

// 菜单 path -> emoji 图标映射（后端未配置 icon 时按 path 兜底，比 name 更稳）
const iconMapByPath: Record<string, string> = {
    '/': '🏠',
    '/goods': '🛍️',
    '/goods/list': '📋',
    '/goods/audit': '✅',
    '/goods/sku': '🔧',
    '/qualification': '📜',
    '/qualification/merchant': '🏪',
    '/merchant': '🏪',
    '/merchant/rank': '🏆',
    '/settle': '💰',
    '/settle/booth': '🏟️',
    '/settle/boothrent': '📅',
    '/settle/feeconfig': '⚖️',
    '/settle/receipt': '🧾',
    '/settle/merchantbill': '🧾',
    '/order': '📄',
    '/order/list': '📋',
    '/order/refund': '💸',
    '/order/refund/list': '💰',
    '/order/review': '💸',
    '/logistics': '🚚',
    '/stock': '📦',
    '/sys': '⚙️',
    '/sys/org': '🏢',
    '/sys/menu': '📑',
    '/sys/notice': '🔔',
    '/sys/right': '🔐',
}

// 按 name 兜底匹配（path 未命中时使用）
const iconMapByName: Record<string, string> = {
    '首页': '🏠',
    '商品管理': '🛍️',
    '商品列表': '📋',
    '商品审核': '✅',
    'sku规格': '🔧',
    'sku': '🔧',
    '资质管理': '📜',
    '商户资质': '🏪',
    '商户管理': '🏪',
    '商户排行榜': '🏆',
    '结算管理': '💰',
    '摊位管理': '🏟️',
    '摊位租赁': '📅',
    '费用配置': '⚖️',
    '收据管理': '🧾',
    '订单管理': '📄',
    '订单列表': '📋',
    '退款管理': '💸',
    '订单退款': '💰',
    '退款审核': '💸',
    '物流管理': '🚚',
    '库存管理': '📦',
    '系统管理': '⚙️',
    '组织管理': '🏢',
    '菜单管理': '📑',
    '通知管理': '🔔',
    '权限管理': '🔐',
}

// 后端部分菜单 name 带英文前缀（如 "goods 商品管理" / "olist 订单列表"），显示前清理
// 先 trim 再匹配，避免前导空格导致正则失效
const cleanMenuName = (name: string): string => {
    return (name || '').trim().replace(/^[a-zA-Z0-9_]+\s+/, '').trim()
}

const isImageUrl = (icon?: string): boolean => !!icon && /^(\/|https?:\/\/)/.test(icon)

const getMenuIcon = (item: any): string => {
    // 优先使用数据库 icon 字段里的图片 URL；否则走 emoji 映射兜底
    if (isImageUrl(item.icon)) return ''
    const p = (item.path || '').trim()
    const n = cleanMenuName(item.name || '').toLowerCase()
    return iconMapByPath[p]
        || iconMapByPath[p.replace(/\/[^/]*$/, '')]
        || iconMapByName[n]
        || '📁'
}

// 过滤掉需要隐藏的菜单项：
//   1. 路径含 /sys/dict（数据字典已下线）
//   2. 名称和路径都为空的"脏记录"（常见于权限页误操作创建的空菜单，渲染出来只有孤立的 icon）
const isHidden = (item: any) => {
    const p = (item.path || '').trim()
    const n = cleanMenuName(item.name || '').trim()
    if (p.indexOf('/sys/dict') >= 0) return true
    if (!p && !n) return true
    return false
}

const displayMenus = computed<any[]>(() => {
    const raw = (menus.value || [])
        .filter(m => !isHidden(m))
        .map(m => ({
            ...m,
            name: cleanMenuName(m.name),
            children: (m.children || [])
                .filter((c: any) => !isHidden(c))
                .map((c: any) => ({ ...c, name: cleanMenuName(c.name) }))
        }))

    // 菜单完全由后端 sys_menu 驱动（含排序与权限），前端仅做 name 清理与图标渲染，
    // 不再硬编码注入任何子菜单。相关 seed 见 db/seed_*.sql。
    return raw
})

const navs = userStore.get_navs;
const route = useRoute(); // 获取当前路由信息

const bind_navs = () => {
    let menu_item = displayMenus.value.find(d => d.path === route.fullPath);
    if (menu_item === undefined) {
        for (let i = 0; i < displayMenus.value.length; i++) {
            menu_item = displayMenus.value[i].children.find((d: any) => d.path === route.fullPath);
            if (menu_item != undefined) {
                break;
            }
        }
    }

    // 首页等硬编码菜单项不在后端数据中，跳过导航记录
    if (!menu_item) {
        navs.forEach(d => { d.active = false; });
        userStore.set_navs(navs);
        return;
    }

    let isExists = navs.find(d => d.id === menu_item.id);
    if (isExists === undefined) {
        navs.forEach(d => {
            d.active = false;
        });
        if (menu_item) {
            navs.push({
                id: menu_item.id,
                url: menu_item.path,
                name: menu_item.name,
                active: true
            });
        }
    } else {
        navs.forEach(d => {
            d.active = false;
        });
        isExists.active = true;
    }



    userStore.set_navs(navs);


}


watch(() => route.path, (newPath) => {
    bind_navs();

});


onMounted(() => {
    bind_navs();
});


</script>

<style scoped>
/* ===== 白色风格侧边栏（令牌驱动，浅色风格）===== */
.menu {
    height: calc(100vh - 60px);
    display: flex;
    flex-direction: column;
    background-color: var(--sidebar-bg);
    border-right: 1px solid var(--sidebar-border);
    transition: background-color 0.35s ease, border-color 0.35s ease;
}

:deep(.el-menu) {
    border-right: 0;
    background-color: transparent;
}

/* 菜单图标：emoji / 上传图片与文字保持间距 */
.menu-icon,
:deep(.el-menu-item i),
:deep(.el-sub-menu__title i) {
    margin-right: 8px;
    font-style: normal;
    font-size: 18px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 20px;
}

.menu-icon-img {
    margin-right: 8px;
    width: 20px;
    height: 20px;
    object-fit: contain;
    display: inline-block;
    vertical-align: middle;
}

/* 一级菜单项文字颜色 */
:deep(.el-menu-item) {
    color: var(--sidebar-text);
}

:deep(.el-menu-item:hover) {
    color: var(--sidebar-text-hover);
    background-color: var(--sidebar-hover-bg);
}

:deep(.el-menu-item.is-active) {
    color: var(--sidebar-active-text);
    background-color: var(--sidebar-active-bg);
    font-weight: 600;
}

/* 子菜单标题 */
:deep(.el-sub-menu__title) {
    color: var(--sidebar-text);
}

:deep(.el-sub-menu__title:hover) {
    color: var(--sidebar-text-hover);
    background-color: var(--sidebar-hover-bg);
}

/* 子菜单展开后的内部菜单 */
:deep(.el-sub-menu .el-menu) {
    background-color: transparent;
}

/* 子菜单内的菜单项 */
:deep(.el-sub-menu .el-menu-item) {
    color: var(--sidebar-text);
}

:deep(.el-sub-menu .el-menu-item:hover) {
    color: var(--sidebar-text-hover);
    background-color: var(--sidebar-hover-bg);
}

:deep(.el-sub-menu .el-menu-item.is-active) {
    color: var(--sidebar-active-text);
    background-color: var(--sidebar-active-bg);
    font-weight: 600;
}

/* el-menu-item-group 默认会带一个隐藏的 title，去掉它占的空间 */
:deep(.el-menu-item-group__title) {
    display: none;
    padding: 0;
    height: 0;
}

/* ===== 底部品牌 ===== */
.sidebar-footer {
    flex-shrink: 0;
    padding: 16px;
    text-align: center;
    border-top: 1px solid var(--sidebar-border);
    transition: border-color 0.35s ease;
}

.sidebar-footer-text {
    font-size: 11px;
    color: var(--sidebar-footer-text);
    letter-spacing: 1px;
}
</style>
