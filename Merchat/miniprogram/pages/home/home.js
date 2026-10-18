"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../utils/api");
const auth_1 = require("../../utils/auth");
const request_1 = require("../../utils/request");
// 公告类型文案（与管理端 notice.vue 类型选项一致）
const NOTICE_TYPE_TEXT = { 1: '公告', 2: '通知', 3: '系统' };
// 通知滚动计时器句柄（模块级，单页场景安全）
let noticeTimer = null;
function greetingByHour(h) {
    if (h < 6)
        return '夜深了';
    if (h < 11)
        return '早上好';
    if (h < 13)
        return '中午好';
    if (h < 18)
        return '下午好';
    return '晚上好';
}
/** "2026-10-18T14:30:00" / "2026-10-18 14:30:00" → "10-18 14:30" */
function shortTime(t) {
    if (!t)
        return '';
    const s = t.replace('T', ' ');
    return s.length >= 16 ? s.slice(5, 16) : s;
}
Page({
    data: {
        greeting: '你好',
        // 顶部商户信息（仅首页展示）
        merchantName: '',
        merchantAvatar: '',
        avatarChar: '',
        merchantLevel: '',
        shopStatus: '营业中',
        // 今日数据（含昨日对比）
        todayOrders: 0,
        todayRevenue: '0.00',
        yesterdayOrders: 0,
        yesterdayRevenue: '0.00',
        kpiPending: 0,
        // 资金账户
        balanceAvailable: '0.00',
        balanceTotalPaid: '0.00',
        balanceWithdrawn: '0.00',
        // 公告通知
        notices: [],
        // 动态通知滚动状态
        noticeIndex: 0,
        noticeAnim: '',
        // 最新订单（取前 3 条）
        recentOrders: [],
        // 在售商品
        goodsSectionTitle: '在售商品',
        products: [],
        // 页面状态：加载中 / 加载失败
        loading: true,
        loadError: '',
    },
    onShow() {
        const tabBar = this.getTabBar();
        if (tabBar)
            tabBar.setData({ selected: 0 });
        this.setData({ greeting: greetingByHour(new Date().getHours()) });
        this.load();
    },
    onHide() {
        this.stopMarquee();
    },
    onUnload() {
        this.stopMarquee();
    },
    async load() {
        if (!(0, auth_1.isLogin)()) {
            wx.redirectTo({ url: '/pages/login/login' });
            return;
        }
        this.setData({ loading: true, loadError: '' });
        try {
            // 三个数据源并行；余额/订单失败时降级为默认值，不阻塞主内容
            const [home, balance, orders] = await Promise.all([
                (0, api_1.getHome)(),
                (0, api_1.getBalance)().catch(() => null),
                (0, api_1.getOrderList)('all').catch(() => []),
            ]);
            const m = home.merchant || {};
            const t = home.today || {};
            const name = m.merchantName || '商户';
            const notices = (home.notices || []).map((n) => ({
                id: n.id,
                title: n.title || '',
                content: n.content || '',
                type: n.type || 1,
                typeText: n.typeText || NOTICE_TYPE_TEXT[n.type || 1] || '公告',
                createTime: n.createTime || '',
            }));
            const products = (home.products || []).map((p) => ({
                ...p,
                hasImage: !!(p.coverImg || '').trim(),
                defaultEmoji: '\u{1F4EA}',
            }));
            const recentOrders = orders.slice(0, 3).map((o) => ({
                id: o.id,
                orderNo: o.orderNo || o.id,
                time: shortTime(o.time),
                total: Number(o.total || 0).toFixed(2),
                statusLabel: o.detailStatusLabel || '',
                statusCls: o.detailStatusCls || '',
            }));
            this.setData({
                loading: false,
                merchantName: name,
                merchantAvatar: m.avatar ? (0, request_1.resolveUrl)(m.avatar) : '',
                avatarChar: name.charAt(0),
                merchantLevel: m.level || '',
                shopStatus: m.shopStatus || '营业中',
                todayOrders: Number(t.orders || 0),
                todayRevenue: Number(t.revenue || 0).toFixed(2),
                yesterdayOrders: Number(t.yesterdayOrders || 0),
                yesterdayRevenue: Number(t.yesterdayRevenue || 0).toFixed(2),
                kpiPending: Number((home.kpi && home.kpi.pending) || 0),
                balanceAvailable: Number((balance && balance.available) || 0).toFixed(2),
                balanceTotalPaid: Number((balance && balance.totalPaid) || 0).toFixed(2),
                balanceWithdrawn: Number((balance && balance.withdrawn) || 0).toFixed(2),
                notices,
                noticeIndex: 0,
                noticeAnim: '',
                recentOrders,
                products,
                goodsSectionTitle: '在售商品',
            });
            this.startMarquee();
        }
        catch (e) {
            this.setData({
                loading: false,
                loadError: (e && e.message) || '加载失败，请稍后重试',
            });
        }
    },
    // 公告列表点击：弹窗查看全文
    onNoticeTap(e) {
        const idx = e.currentTarget.dataset.index;
        const n = this.data.notices[idx];
        if (!n)
            return;
        wx.showModal({
            title: n.title || n.typeText,
            content: n.content || '',
            showCancel: false,
            confirmText: '知道了',
        });
    },
    // 顶部通知条点击：查看当前这条全文
    onMarqueeTap() {
        const n = this.data.notices[this.data.noticeIndex];
        if (!n)
            return;
        wx.showModal({
            title: n.title || n.typeText,
            content: n.content || '',
            showCancel: false,
            confirmText: '知道了',
        });
    },
    startMarquee() {
        this.stopMarquee();
        if (this.data.notices.length <= 1)
            return;
        noticeTimer = setInterval(() => this.rollNotice(), 3200);
    },
    stopMarquee() {
        if (noticeTimer) {
            clearInterval(noticeTimer);
            noticeTimer = null;
        }
    },
    // 纵向滚到下一条：先滑出，换内容后从下方滑入
    rollNotice() {
        if (this.data.noticeAnim)
            return;
        const total = this.data.notices.length;
        this.setData({ noticeAnim: 'out' });
        setTimeout(() => {
            const next = (this.data.noticeIndex + 1) % total;
            this.setData({ noticeIndex: next, noticeAnim: 'in' });
            setTimeout(() => this.setData({ noticeAnim: '' }), 30);
        }, 360);
    },
    goOrders() {
        wx.switchTab({ url: '/pages/order/list' });
    },
    goGoods() {
        wx.switchTab({ url: '/pages/goods/list' });
    },
    goReconcile() {
        wx.switchTab({ url: '/pages/reconcile/reconcile' });
    },
    goGoodsAdd() {
        wx.navigateTo({ url: '/pages/goods/add/add' });
    },
    goBill() {
        wx.navigateTo({ url: '/pages/bill/list' });
    },
    goAddress() {
        wx.navigateTo({ url: '/pages/profile/address/list' });
    },
    goQualification() {
        wx.navigateTo({ url: '/pages/profile/qualification/qualification' });
    },
    goGoodsDetail(e) {
        wx.navigateTo({ url: '/pages/goods/detail?id=' + e.currentTarget.dataset.id });
    },
    goOrderDetail(e) {
        wx.navigateTo({ url: '/pages/order/detail?id=' + e.currentTarget.dataset.id });
    },
    onRetry() {
        this.load();
    },
    onImageError(e) {
        const idx = e.currentTarget.dataset.index;
        const key = `products[${idx}].hasImage`;
        this.setData({ [key]: false });
    },
});
