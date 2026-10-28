"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../utils/api");
const auth_1 = require("../../utils/auth");
const util_1 = require("../../utils/util");
/** order_status → 前端显示文案 + 样式类（与列表页一致） */
function mapStatus(os) {
    if (os === 1)
        return { label: '待接单', cls: 'pending' };
    if (os === 0 || os === 8)
        return { label: '待处理', cls: 'pending' };
    if (os === 2 || os === 3)
        return { label: '配送中', cls: 'shipping' };
    if (os === 4 || os === 6)
        return { label: '已完成', cls: 'completed' };
    if (os === 9)
        return { label: '已取消', cls: 'cancelled' };
    return { label: '未知', cls: '' };
}
Page({
    data: {
        id: '',
        bill: null,
    },
    onLoad(options) {
        if (!options.id) {
            wx.showToast({ title: '缺少订单ID', icon: 'none' });
            return;
        }
        this.setData({ id: options.id });
        this.load();
    },
    onShow() { },
    async load() {
        if (!(0, auth_1.isLogin)()) {
            wx.redirectTo({ url: '/pages/login/login' });
            return;
        }
        try {
            // 数据源：merchant_order（后端按当前商户 merchant_id 校验并返回，不含其他商户订单）
            const raw = await (0, api_1.getOrderDetail)(this.data.id);
            if (!raw) {
                wx.showToast({ title: '订单不存在', icon: 'none' });
                return;
            }
            const o = raw;
            const os = Number(o.rawStatus ?? 0);
            const st = o.detailStatusLabel
                ? { label: String(o.detailStatusLabel), cls: String(o.detailStatusCls || '') }
                : mapStatus(os);
            const amt = Number(o.total ?? 0);
            const ct = o.time ? String(o.time) : '';
            const items = (o.items || []).map((it) => {
                const qty = Number(it.qty ?? 0);
                const price = Number(it.price ?? 0);
                return {
                    name: it.name || '-',
                    price,
                    qty,
                    total: price * qty,
                };
            });
            const goodsName = items.map((it) => it.name).filter(Boolean).join('、') || '-';
            const buyNum = Number(o.totalQty ?? items.reduce((s, it) => s + it.qty, 0));
            const view = {
                id: String(o.id || ''),
                billNo: o.orderNo || String(o.id || ''),
                merchantName: o.merchantName || '-',
                merchantContact: o.merchantContact || '',
                merchantPhone: o.merchantPhone || '',
                merchantAddress: o.merchantAddress || '',
                goodsName,
                buyNum,
                amount: amt,
                orderStatus: os,
                payStatusText: o.payStatus === 'paid' ? '已付款' : '待付款',
                statusLabel: st.label,
                statusCls: st.cls,
                createTime: ct,
                updateTime: '',
                shipTime: o.shipTime ? String(o.shipTime) : '',
                deliveryTime: o.shipTime ? String(o.shipTime).slice(0, 16).replace('T', ' ') : '',
                remark: o.remark || '',
                rejectReason: o.rejectReason || '',
                clientId: o.clientId || '',
                items,
                logistics: (o.logistics || []).map((node) => ({
                    text: node.text || '',
                    time: node.time || '',
                    current: !!node.current,
                    active: !!node.active,
                })),
                // booleans for WXML conditionals
                isPending: [0, 1, 8].includes(os),
                // 只有买家已支付后才能接单（未支付订单留在买家「未付款」页）
                canAccept: os === 1 && o.payStatus === 'paid',
                canShip: os === 2,
                isShipping: [2, 3].includes(os),
                isCompleted: [4, 6].includes(os),
                isCancelled: os === 9,
                totalText: '¥' + amt.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                createTimeFormatted: ct ? ct.slice(0, 16).replace('T', ' ') : '',
            };
            this.setData({ bill: view });
        }
        catch (err) {
            wx.showModal({
                title: '加载失败',
                content: err.message || '订单详情加载失败，请稍后重试',
                showCancel: false,
                success: () => {
                    const pages = getCurrentPages();
                    if (pages.length > 1)
                        wx.navigateBack();
                    else
                        wx.switchTab({ url: '/pages/order/list' });
                },
            });
        }
    },
    goBack() {
        const pages = getCurrentPages();
        if (pages.length > 1)
            wx.navigateBack();
        else
            wx.switchTab({ url: '/pages/order/list' });
    },
    /** 联系买家：跳转到该订单买家的聊天界面 */
    goChat() {
        const bill = this.data.bill;
        if (!bill || !bill.clientId) {
            wx.showToast({ title: '未找到买家信息', icon: 'none' });
            return;
        }
        const params = [
            'clientId=' + bill.clientId,
            'name=' + encodeURIComponent(bill.merchantContact || ''),
        ].join('&');
        wx.navigateTo({ url: '/pages/chat/chat?' + params });
    },
    /** 接单 / 拒单 / 发货 */
    async action(e) {
        const op = e.currentTarget.dataset.op;
        const id = e.currentTarget.dataset.id || this.data.id;
        const tips = {
            accept: '确认接单并备货？',
            reject: '确认拒单？拒单后订单将取消',
            ship: '确认发货？',
        };
        const labels = { accept: '接单', reject: '拒单', ship: '发货' };
        let reason;
        if (op === 'reject') {
            reason = await (0, util_1.promptInput)('填写拒单理由', '请输入拒单原因（必填）');
            if (reason === null)
                return; // 用户取消
        }
        const ok = await (0, util_1.confirmDialog)(tips[op] || '确认操作？');
        if (!ok)
            return;
        wx.showLoading({ title: '处理中...', mask: true });
        try {
            await (0, api_1.orderAction)(id, op, reason || undefined);
            wx.hideLoading();
            wx.showToast({ title: (labels[op] || '操作') + '成功', icon: 'success' });
            this.load();
        }
        catch (err) {
            wx.hideLoading();
            wx.showToast({ title: err?.message || '操作失败', icon: 'none' });
        }
    },
});
