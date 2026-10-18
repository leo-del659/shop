"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../utils/api");
const auth_1 = require("../../utils/auth");
const util_1 = require("../../utils/util");
const request_1 = require("../../utils/request");
Page({
    data: {
        displayName: '',
        subLine: '',
        phone: '',
        address: '',
        avatarText: '',
        avatarUrl: '',
        statOrders: 0,
        statRevenue: '',
        statProducts: 0,
        statPending: 0,
    },
    onShow() {
        const tabBar = this.getTabBar();
        if (tabBar)
            tabBar.setData({ selected: 4 });
        this.load();
    },
    async load() {
        if (!(0, auth_1.isLogin)()) {
            wx.redirectTo({ url: '/pages/login/login' });
            return;
        }
        const d = await (0, api_1.getProfile)();
        const s = d.merchant;
        this.setData({
            displayName: s.merchantName,
            subLine: s.merchantNo || s.id || '',
            phone: s.account ? String(s.account).replace(/^(\d{3})\d{4}(\d{4})$/, '$1****$2') : (s.phone ? String(s.phone).replace(/^(\d{3})\d{4}(\d{4})$/, '$1****$2') : ''),
            address: s.address || '',
            avatarText: (s.merchantName || '商').charAt(0),
            avatarUrl: s.avatar ? (0, request_1.resolveUrl)(s.avatar) : '',
            statOrders: d.stats.orders,
            statRevenue: (0, util_1.formatWan)(d.stats.revenue),
            statProducts: d.stats.products,
            statPending: d.stats.pending,
        });
    },
    goBack() {
        const pages = getCurrentPages();
        if (pages.length > 1) {
            wx.navigateBack();
        }
        else {
            wx.switchTab({ url: '/pages/home/home' });
        }
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
    goDashboard() {
        wx.switchTab({ url: '/pages/home/home' });
    },
    onEditProfile() {
        wx.navigateTo({ url: '/pages/profile/edit/edit' });
    },
    onMenu(e) {
        const urls = {
            address: '/pages/profile/address/list',
            security: '/pages/profile/security/security',
            bind: '/pages/profile/bind/bind',
            qualification: '/pages/profile/qualification/qualification',
        };
        const key = e.currentTarget.dataset.key;
        const url = urls[key];
        if (url) {
            wx.navigateTo({ url });
        }
        else {
            (0, util_1.toast)('功能开发中');
        }
    },
    async onLogout() {
        const ok = await (0, util_1.confirmDialog)('确认退出登录？');
        if (!ok)
            return;
        (0, auth_1.logout)();
        wx.reLaunch({ url: '/pages/login/login' });
    },
});
