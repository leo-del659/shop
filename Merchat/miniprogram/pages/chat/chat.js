"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../utils/api");
const request_1 = require("../../utils/request");
const auth_1 = require("../../utils/auth");
const config_1 = require("../../utils/config");
/** 断线重连最大尝试次数（仅 WebSocket 自身重试，不再降级轮询） */
const MAX_RECONNECT = 5;
/** 重连退避基数（毫秒） */
const RECONNECT_BASE = 2000;
/** 把 HTTP(S) 基址转换为 WebSocket 基址（ws / wss） */
function wsBaseUrl() {
    if (config_1.API_BASE.startsWith('https'))
        return 'wss' + config_1.API_BASE.slice(5);
    return 'ws' + config_1.API_BASE.slice(4);
}
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
        clientId: '',
        peerName: '',
        peerAvatar: '',
        avatarChar: '买',
        messages: [],
        loading: true,
        input: '',
        canSend: false,
        scrollInto: '',
        scrollHeight: 600,
        wsConnected: false
    },
    /** WebSocket 连接实例 */
    socketTask: null,
    /** 重连尝试次数 */
    retryCount: 0,
    /** 是否用户主动关闭（离页），用于区分正常关闭与异常断开 */
    closedByUser: false,
    onLoad(options) {
        const clientId = options.clientId || '';
        const name = decodeURIComponent(options.name || '');
        const avatar = decodeURIComponent(options.avatar || '');
        this.setData({
            clientId,
            peerName: name,
            peerAvatar: avatar,
            avatarChar: (name || '买')[0],
        });
        if (name)
            wx.setNavigationBarTitle({ title: name });
        // 可视区域高度：窗口高度 - 输入栏高度
        const info = wx.getSystemInfoSync();
        this.setData({ scrollHeight: info.windowHeight - 110 });
        if (!clientId) {
            wx.showToast({ title: '买家不存在', icon: 'none' });
            return;
        }
        this.initChat();
    },
    onUnload() {
        // 用户主动离开：关闭 WebSocket 连接
        this.closedByUser = true;
        if (this.socketTask) {
            try {
                this.socketTask.close({});
            }
            catch (e) { /* 忽略 */ }
            this.socketTask = null;
        }
    },
    /** 初始化：拉取历史消息（含对端信息）+ 建立实时连接 */
    async initChat() {
        try {
            const res = await (0, api_1.getChatMessages)(this.data.clientId);
            const peer = res.peer;
            if (peer) {
                const avatar = (0, request_1.resolveUrl)(peer.peerAvatar || '');
                this.setData({
                    peerName: peer.peerName || '买家',
                    peerAvatar: avatar,
                    avatarChar: (peer.peerName || '买')[0],
                });
                wx.setNavigationBarTitle({ title: peer.peerName || '买家' });
            }
            this.applyMessages(res.messages || []);
            this.setData({ loading: false });
            this.scrollBottom();
        }
        catch (e) {
            this.setData({ loading: false });
        }
        // 建立 WebSocket 实时通道（无轮询兜底）
        this.connectSocket();
    },
    /** 建立 WebSocket 实时接收通道 */
    connectSocket() {
        const token = (0, auth_1.getToken)();
        const peerId = this.data.clientId;
        if (!token || !peerId) {
            // 无登录态 / 无对端，无法建立连接（不再降级轮询）
            return;
        }
        this.closedByUser = false;
        const url = `${wsBaseUrl()}/ws/chat?token=${encodeURIComponent(token)}` +
            `&role=merchant&peerId=${encodeURIComponent(peerId)}`;
        let task;
        try {
            task = wx.connectSocket({ url });
        }
        catch (e) {
            // 连接建立失败：不再降级轮询，等待下次进入/重连
            return;
        }
        this.socketTask = task;
        task.onOpen(() => {
            this.retryCount = 0;
            this.setData({ wsConnected: true });
            // 连接 / 重连成功：补拉断连期间可能漏掉的消息（一次性，非轮询）
            this.catchUp();
        });
        task.onMessage((res) => this.onSocketMessage(res));
        task.onClose(() => {
            this.setData({ wsConnected: false });
            if (!this.closedByUser)
                this.scheduleReconnect();
        });
        task.onError(() => {
            this.setData({ wsConnected: false });
            // 连接异常：仅尝试 WebSocket 重连，不降级轮询
            this.scheduleReconnect();
        });
    },
    /** 断线退避重连；超过上限则停止（无轮询兜底） */
    scheduleReconnect() {
        if (this.retryCount >= MAX_RECONNECT)
            return;
        this.retryCount++;
        const delay = RECONNECT_BASE * this.retryCount;
        setTimeout(() => {
            if (this.closedByUser)
                return;
            this.connectSocket();
        }, delay);
    },
    /** 解析 WS 推送：{type:'msg', message} → 增量追加 */
    onSocketMessage(res) {
        let payload;
        try {
            payload = JSON.parse(res.data);
        }
        catch (e) {
            return;
        }
        if (!payload || payload.type !== 'msg' || !payload.message)
            return;
        this.appendMessages([payload.message]);
    },
    /** 连接 / 重连成功后一次性补拉：拉取最后一条之后的新消息（非轮询） */
    async catchUp() {
        const msgs = this.data.messages;
        const last = msgs.length ? msgs[msgs.length - 1] : null;
        try {
            const res = await (0, api_1.getChatMessages)(this.data.clientId, last ? last.createTime : undefined);
            const fresh = res.messages || [];
            if (fresh.length)
                this.appendMessages(fresh);
        }
        catch (e) {
            // 补拉失败静默，下次重连再尝试
        }
    },
    /** 合并新消息（按 id 去重）并渲染 */
    appendMessages(list) {
        if (!list || !list.length)
            return;
        const known = new Set(this.data.messages.map((m) => m.id));
        const add = list.filter((m) => !known.has(m.id));
        if (!add.length)
            return;
        const merged = this.data.messages.concat(add);
        this.applyMessages(merged);
        this.scrollBottom();
    },
    /** 统一格式化消息（方向 / 时间文案 / 时间分隔）并 setData */
    applyMessages(list) {
        const view = list.map((m, i) => ({
            ...m,
            self: m.senderType === 2,
            timeText: bubbleTimeText(m.createTime),
            showTime: i === 0 || gapOver5Min(list[i - 1].createTime, m.createTime),
        }));
        this.setData({ messages: view });
    },
    /** 滚动到底部 */
    scrollBottom() {
        const msgs = this.data.messages;
        if (!msgs.length)
            return;
        this.setData({ scrollInto: 'msg-' + msgs[msgs.length - 1].id });
    },
    onInput(e) {
        const v = e.detail.value || '';
        this.setData({ input: v, canSend: v.trim().length > 0 });
    },
    /** 发送消息（走 HTTP，拿到含 id 的落库结果，避免乐观更新 id 缺失导致重复） */
    async onSend() {
        const content = this.data.input.trim();
        if (!content)
            return;
        // 先本地清空输入框，提升手感；失败再提示
        this.setData({ input: '', canSend: false });
        try {
            const msg = await (0, api_1.sendChatMessage)(this.data.clientId, content);
            if (msg && msg.id && !this.data.messages.some((m) => m.id === msg.id)) {
                const merged = this.data.messages.concat([msg]);
                this.applyMessages(merged);
                this.scrollBottom();
            }
        }
        catch (e) {
            wx.showToast({ title: '发送失败，请重试', icon: 'none' });
        }
    },
});
