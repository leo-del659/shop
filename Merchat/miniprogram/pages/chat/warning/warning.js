"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../../utils/api");
/** 格式化时间展示：今天显示 HH:mm，昨天显示「昨天 HH:mm」，更早显示 MM-dd HH:mm */
function bubbleTimeText(raw) {
    if (!raw)
        return '';
    const d = new Date(raw.replace(' ', 'T'));
    if (isNaN(d.getTime()))
        return raw;
    const now = new Date();
    const p = (n) => (n < 10 ? '0' : '') + n;
    const hm = `${p(d.getHours())}:${p(d.getMinutes())}`;
    const startOfDay = (x) => new Date(x.getFullYear(), x.getMonth(), x.getDate()).getTime();
    const diffDays = Math.round((startOfDay(now) - startOfDay(d)) / 86400000);
    if (diffDays === 0)
        return hm;
    if (diffDays === 1)
        return '昨天 ' + hm;
    return `${p(d.getMonth() + 1)}-${p(d.getDate())} ` + hm;
}
/** 两条消息间隔超过 5 分钟时显示时间分隔 */
function gapOver5Min(prev, next) {
    if (!prev || !next)
        return true;
    const a = new Date(prev.replace(' ', 'T')).getTime();
    const b = new Date(next.replace(' ', 'T')).getTime();
    if (isNaN(a) || isNaN(b))
        return false;
    return b - a > 5 * 60 * 1000;
}
Page({
    data: {
        /** 库存预警消息（按时间正序，气泡形式展示） */
        messages: [],
        loading: true,
        scrollInto: '',
        scrollHeight: 600,
    },
    onLoad() {
        // 可视区域高度：窗口高度 - 底部提示栏高度
        const info = wx.getSystemInfoSync();
        this.setData({ scrollHeight: info.windowHeight - 110 });
        this.load();
    },
    async load() {
        try {
            const res = await (0, api_1.getMerchantNotices)();
            const list = (res && res.notices) ? res.notices : [];
            // 后端按时间倒序返回，聊天界面需要正序（旧的在上，新的在下）
            const ordered = list.slice().reverse();
            const view = ordered.map((n, i) => ({
                ...n,
                timeText: bubbleTimeText(n.createTime),
                showTime: i === 0 || gapOver5Min(ordered[i - 1].createTime, n.createTime),
            }));
            this.setData({ messages: view, loading: false });
            // 滚动到最新一条
            if (view.length) {
                this.setData({ scrollInto: 'msg-' + view[view.length - 1].id });
            }
            // 进入即标记已读（失败不影响查看）
            try {
                await (0, api_1.readMerchantNotices)();
            }
            catch (e) {
                // ignore
            }
        }
        catch (e) {
            this.setData({ loading: false });
        }
    },
});
