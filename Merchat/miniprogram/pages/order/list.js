"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../utils/api");
const auth_1 = require("../../utils/auth");
const util_1 = require("../../utils/util");
/** order_status → 前端显示文案 + 样式类 */
function mapStatus(os) {
    if (os === 1)
        return { label: '待接单', cls: 'pending' };
    if (os === 0 || os === 8)
        return { label: '待处理', cls: 'pending' };
    if (os === 2)
        return { label: '备货中', cls: 'shipping' };
    if (os === 3)
        return { label: '配送中', cls: 'shipping' };
    if (os === 4 || os === 6)
        return { label: '已完成', cls: 'completed' };
    if (os === 9)
        return { label: '已取消', cls: 'cancelled' };
    return { label: '未知', cls: '' };
}
Page({
    data: {
        filter: 'all',
        list: [],
        statAll: 0,
        statPending: 0,
        statShipping: 0,
        statCompleted: 0,
        statCancelled: 0,
        _sig: '',
        _acting: false,
        fAll: 'active', fPending: '', fShipping: '', fCompleted: '', fCancelled: '',
        emptyTitle: '暂无订单',
        emptySub: '还没有订单记录',
    },
    onLoad() { },
    onShow() {
        const tabBar = this.getTabBar();
        if (tabBar)
            tabBar.setData({ selected: 1 });
        this.load();
    },
    onHide() { },
    load() {
        const token = (0, auth_1.getToken)();
        if (!token) {
            wx.redirectTo({ url: '/pages/login/login' });
            return;
        }
        // 数据源：merchant_order（后端已按当前登录商户 merchant_id 过滤，天然隔离越权）
        const fetch = (0, api_1.getOrderList)(this.data.filter);
        const fetchAll = (0, api_1.getOrderList)('all');
        Promise.all([fetch, fetchAll])
            .then(([raw, allRaw]) => {
            const list = (raw || []).map((b) => {
                const os = Number(b.rawStatus ?? 0);
                const st = b.detailStatusLabel
                    ? { label: String(b.detailStatusLabel), cls: String(b.detailStatusCls || '') }
                    : mapStatus(os);
                const party = b.merchantName || b.merchant || '-';
                const items = b.items || [];
                const goodsName = b.goodsSummary || items.map((i) => i.name).filter(Boolean).join('、') || '-';
                const firstWithImg = items.find((i) => i.img);
                const rawCover = firstWithImg ? String(firstWithImg.img || '') : '';
                const coverImg = rawCover ? (0, api_1.resolveCoverImage)(rawCover, goodsName, b.id) : '';
                const buyNum = Number(b.totalQty ?? items.reduce((s, i) => s + Number(i.qty || 0), 0));
                return {
                    id: String(b.id || ''),
                    billNo: b.orderNo || String(b.id || ''),
                    partyName: party,
                    goodsName,
                    coverImg,
                    hasImage: !!coverImg,
                    emoji: (0, api_1.emojiFor)(goodsName),
                    defaultEmoji: '📦',
                    buyNum,
                    amount: Number(b.total ?? 0),
                    statusLabel: st.label,
                    statusCls: st.cls,
                    orderStatus: os,
                    time: b.time ? String(b.time).slice(0, 16).replace('T', ' ') : '',
                };
            });
            const all = allRaw || [];
            this.setData({
                list,
                _sig: JSON.stringify(list),
                statAll: all.length,
                // 待接单统计需排除未支付订单（未支付订单不进入商户 pending 列表）
                statPending: all.filter((b) => Number(b.rawStatus ?? 0) === 1 && b.payStatus === 'paid').length,
                statShipping: all.filter((b) => [2, 3].includes(Number(b.rawStatus ?? 0))).length,
                statCompleted: all.filter((b) => [4, 6].includes(Number(b.rawStatus ?? 0))).length,
                statCancelled: all.filter((b) => Number(b.rawStatus ?? 0) === 9).length,
                fAll: this.data.filter === 'all' ? 'active' : '',
                fPending: this.data.filter === 'pending' ? 'active' : '',
                fShipping: this.data.filter === 'shipping' ? 'active' : '',
                fCompleted: this.data.filter === 'completed' ? 'active' : '',
                fCancelled: this.data.filter === 'cancelled' ? 'active' : '',
            });
        })
            .catch((err) => {
            if (err && (err.statusCode === 401 || err.statusCode === 403)) {
                (0, auth_1.clearAuth)();
                wx.redirectTo({ url: '/pages/login/login' });
            }
            else {
                (0, util_1.toast)(err.message || '加载失败');
            }
        });
    },
    setFilter(e) {
        const f = e.currentTarget.dataset.filter;
        if (f === this.data.filter)
            return;
        this.setData({ filter: f }, () => this.load());
    },
    // 阻止卡片点击冒泡到详情跳转
    stopProp() { },
    async action(e) {
        const id = e.currentTarget.dataset.id;
        const op = e.currentTarget.dataset.op;
        if (this.data._acting)
            return;
        const labels = { accept: '接单', reject: '拒单', ship: '发货', complete: '完成' };
        let reason;
        if (op === 'reject') {
            reason = await (0, util_1.promptInput)('填写拒单理由', '请输入拒单原因（必填）');
            if (reason === null)
                return; // 用户取消
        }
        this.setData({ _acting: true });
        wx.showLoading({ title: '处理中...', mask: true });
        try {
            await (0, api_1.orderAction)(id, op, reason || undefined);
            wx.hideLoading();
            wx.showToast({ title: (labels[op] || '操作') + '成功', icon: 'success' });
            // 重新拉取列表，实时刷新订单状态与统计
            this.load();
        }
        catch (err) {
            wx.hideLoading();
            // 后端会返回「当前状态不可接单 / 订单不存在或不属于当前商户」等明确原因，直接展示
            (0, util_1.toast)(err?.message || '操作失败');
        }
        finally {
            this.setData({ _acting: false });
        }
    },
    goDetail(e) {
        wx.navigateTo({ url: '/pages/order/detail?id=' + e.currentTarget.dataset.id });
    },
    onImageError(e) {
        const idx = e.currentTarget.dataset.index;
        const key = `list[${idx}].hasImage`;
        this.setData({ [key]: false });
    },
});
