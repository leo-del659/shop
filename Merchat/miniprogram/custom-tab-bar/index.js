"use strict";
Component({
    data: {
        selected: 0,
        list: [
            { pagePath: 'pages/home/home', text: '首页', icon: '🏠', activeCls: 'active' },
            { pagePath: 'pages/order/list', text: '订单', icon: '📋', activeCls: '' },
            { pagePath: 'pages/goods/list', text: '商品', icon: '📦', activeCls: '' },
            { pagePath: 'pages/reconcile/reconcile', text: '对账', icon: '💰', activeCls: '' },
            { pagePath: 'pages/profile/profile', text: '我的', icon: '👤', activeCls: '' },
        ],
    },
    observers: {
        selected(selected) {
            const list = this.data.list.map((item, idx) => ({
                ...item,
                activeCls: idx === selected ? 'active' : '',
            }));
            this.setData({ list });
        },
    },
    methods: {
        switchTab(e) {
            const index = e.currentTarget.dataset.index;
            const path = this.data.list[index].pagePath;
            if (!path) {
                wx.showToast({ title: '页面路径错误', icon: 'none' });
                return;
            }
            // 先本地高亮，提升交互反馈
            this.setData({ selected: index });
            // 使用与 tabBar.list 一致的相对路径跳转
            wx.switchTab({
                url: '/' + path,
                fail: (err) => {
                    console.error('[custom-tab-bar] switchTab failed:', err);
                    wx.showToast({ title: '跳转失败：' + (err.errMsg || '未知错误'), icon: 'none' });
                },
            });
        },
    },
});
