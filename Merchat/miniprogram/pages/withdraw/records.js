"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../utils/api");
const auth_1 = require("../../utils/auth");
const util_1 = require("../../utils/util");
Page({
    data: {
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
            const rows = await (0, api_1.getWithdrawals)();
            const list = (rows || []).map((w) => {
                const s = util_1.WITHDRAW_STATUS[w.status] || { label: w.status, cls: '' };
                return {
                    ...w,
                    wdLabel: s.label,
                    wdCls: s.cls,
                    amountText: '-¥' + Number(w.amount || 0).toFixed(2),
                    timeText: w.time ? String(w.time).slice(0, 16).replace('T', ' ') : '',
                };
            });
            this.setData({ list });
        }
        catch (e) {
            (0, util_1.toast)('加载失败：' + (e.message || '未知错误'));
        }
    },
});
