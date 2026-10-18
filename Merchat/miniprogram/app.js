"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const auth_1 = require("./utils/auth");
const request_1 = require("./utils/request");
App({
    globalData: {
        loggedIn: false,
        userType: 'merchant',
    },
    onLaunch() {
        // 未登录时直接留在登录页（login 是首页）
        if (!(0, auth_1.isLogin)()) {
            this.globalData.loggedIn = false;
            return;
        }
        const token = (0, auth_1.getToken)();
        if (!token) {
            (0, auth_1.clearAuth)();
            this.globalData.loggedIn = false;
            return;
        }
        // 以本地存储的商户信息判断角色（仅商户端）
        const merchant = (0, auth_1.getLocalMerchant)();
        const role = merchant && merchant.id ? 'merchant' : null;
        this.globalData.userType = role || 'merchant';
        // 本地已有身份信息 → 直接视为已登录，不额外请求后端（避免网络抖动把已登录用户踢出）
        if (role === 'merchant' && merchant && merchant.id) {
            this.globalData.loggedIn = true;
            return;
        }
        // 无本地缓存时才拉取；失败时不粗暴 clearAuth，保留 token（下次进入页面自然触发 401 再跳登录）
        (0, request_1.request)({ path: '/api/merchant/me', method: 'GET' })
            .then(() => {
            this.globalData.loggedIn = true;
        })
            .catch(() => {
            this.globalData.loggedIn = false;
        });
    },
});
