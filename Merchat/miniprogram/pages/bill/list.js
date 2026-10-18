"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../utils/api");
const auth_1 = require("../../utils/auth");
const util_1 = require("../../utils/util");
Page({
    data: {
        filter: 'all',
        filters: [
            { key: 'all', label: '全部' },
            { key: 'pending', label: '待处理' },
            { key: 'shipping', label: '配送中' },
            { key: 'completed', label: '已完成' },
            { key: 'cancelled', label: '已取消' },
        ],
        list: [],
    },
    onShow() {
        if (!(0, auth_1.isLogin)()) {
            wx.redirectTo({ url: '/pages/login/login' });
            return;
        }
        this.load();
    },
    async load() {
        try {
            const rows = await (0, api_1.getBillList)(this.data.filter);
            const list = (rows || []).map((b) => {
                const os = Number(b.orderStatus ?? 0);
                const st = b.statusLabel && b.statusCls
                    ? { label: b.statusLabel, cls: b.statusCls }
                    : (0, util_1.orderStatusDetail)(os);
                return {
                    id: String(b.id || ''),
                    billNo: b.billNo || String(b.id || ''),
                    merchantName: b.merchantName || '-',
                    goodsName: b.goodsName || '-',
                    buyNum: Number(b.buyNum ?? 0),
                    amount: Number(b.amount ?? 0),
                    oLabel: st.label,
                    oCls: st.cls,
                    time: b.createTime ? String(b.createTime).slice(0, 16).replace('T', ' ') : '',
                };
            });
            this.setData({ list });
        }
        catch (e) {
            (0, util_1.toast)('加载失败：' + (e.message || '未知错误'));
        }
    },
    setFilter(e) {
        const key = e.currentTarget.dataset.key;
        if (key === this.data.filter)
            return;
        this.setData({ filter: key });
        this.load();
    },
});
