"use strict";
// 通用工具：金额格式化、状态映射、库存判定、toast
Object.defineProperty(exports, "__esModule", { value: true });
exports.WITHDRAW_STATUS = exports.GOODS_STATUS = exports.PAY_STATUS = exports.ORDER_STATUS = void 0;
exports.formatMoney = formatMoney;
exports.formatWan = formatWan;
exports.toast = toast;
exports.confirmDialog = confirmDialog;
exports.promptInput = promptInput;
exports.orderStatusDetail = orderStatusDetail;
exports.getStockClass = getStockClass;
exports.getStockLabel = getStockLabel;
exports.buildLogistics = buildLogistics;
function formatMoney(n) {
    return '¥' + Math.round(n || 0).toLocaleString('zh-CN');
}
function formatWan(n, precision = 0) {
    if (n >= 10000)
        return (n / 10000).toFixed(Math.max(1, precision)) + '万';
    return (n || 0).toFixed(precision);
}
function toast(msg) {
    wx.showToast({ title: msg, icon: 'none', duration: 2000 });
}
function confirmDialog(content) {
    return new Promise((resolve) => {
        wx.showModal({
            title: '提示',
            content,
            success: (r) => resolve(!!r.confirm),
            fail: () => resolve(false),
        });
    });
}
/**
 * 弹出带输入框的确认框，返回用户输入内容；取消/关闭返回 null。
 * 兼容不支持 editable 的基础库：回退到普通 modal，reason 由调用方给默认值。
 */
function promptInput(title, placeholder = '请输入', defaultValue = '') {
    return new Promise((resolve) => {
        const cfg = {
            title,
            placeholderText: placeholder,
            editable: true,
            success: (r) => {
                if (!r.confirm)
                    return resolve(null);
                // content 可能为 undefined（老基础库不支持 editable）
                const v = typeof r.content === 'string' ? r.content.trim() : defaultValue;
                resolve(v || defaultValue || null);
            },
            fail: () => resolve(null),
        };
        wx.showModal(cfg);
    });
}
// 订单状态
exports.ORDER_STATUS = {
    pending: { label: '待处理', cls: 'pending' },
    shipping: { label: '配送中', cls: 'shipping' },
    completed: { label: '已完成', cls: 'completed' },
    cancelled: { label: '已取消', cls: 'cancelled' },
};
// 付款状态（结算状态，非支付状态；详情页不再用其充当订单状态展示）
exports.PAY_STATUS = {
    paid: { label: '已付款', cls: 'paid' },
    unpaid: { label: '待付款', cls: 'unpaid' },
    partial: { label: '部分付款', cls: 'partial' },
    overdue: { label: '逾期', cls: 'overdue' },
};
// 订单状态（明细版，对应数据库 order_status 数值）
// 1 待处理 / 2 已接单 / 3 已发货 / 4,6 已完成 / 9 已取消
function orderStatusDetail(raw) {
    switch (raw) {
        case 1: return { label: '待处理', cls: 'pending' };
        case 2: return { label: '已接单', cls: 'shipping' };
        case 3: return { label: '已发货', cls: 'shipping' };
        case 4:
        case 6: return { label: '已完成', cls: 'completed' };
        case 9: return { label: '已取消', cls: 'cancelled' };
        default: return { label: '未知', cls: '' };
    }
}
// 商品状态
exports.GOODS_STATUS = {
    published: { label: '已上架', cls: 'published' },
    draft: { label: '草稿', cls: 'draft' },
    disabled: { label: '已下架', cls: 'disabled' },
};
// 提现状态
exports.WITHDRAW_STATUS = {
    success: { label: '成功', cls: 'success' },
    pending: { label: '处理中', cls: 'pending' },
    failed: { label: '失败', cls: 'failed' },
};
// 库存状态：库存 <=0 缺货；<20 严重不足；<safe 偏低；否则充足
function getStockClass(stock, safe = 50) {
    if (stock <= 0)
        return 'critical';
    if (stock < 20)
        return 'critical';
    if (stock < safe)
        return 'low';
    return 'normal';
}
function getStockLabel(stock, safe = 50) {
    if (stock <= 0)
        return '缺货';
    if (stock < 20)
        return '严重不足';
    if (stock < safe)
        return '库存偏低';
    return '库存充足';
}
// 根据订单状态数值推导物流进度节点（与订单详情页时间线一致）
function buildLogistics(rawStatus) {
    const base = [
        { text: '订单已创建，等待商家确认', time: '' },
        { text: '商家已接单，正在备货中', time: '' },
        { text: '订单已发货，预计3天内送达', time: '' },
        { text: '已签收，订单完成', time: '' },
    ];
    let activeIndex = 0;
    if (rawStatus === 1)
        activeIndex = 0;
    else if (rawStatus === 2)
        activeIndex = 1;
    else if (rawStatus === 3)
        activeIndex = 2;
    else if (rawStatus === 4 || rawStatus === 6)
        activeIndex = 3;
    else
        activeIndex = -1;
    return base.map((n, i) => ({
        ...n,
        active: rawStatus === 9 ? false : i <= activeIndex,
        current: i === activeIndex && rawStatus !== 4 && rawStatus !== 6 && rawStatus !== 9,
    }));
}
