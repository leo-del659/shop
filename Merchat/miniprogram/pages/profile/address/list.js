"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../../utils/api");
const util_1 = require("../../../utils/util");
const auth_1 = require("../../../utils/auth");
Page({
    data: {
        list: [],
        loading: true,
        /** 从补货弹窗进入选择地址模式：来自 detail 写入 __gys_pick_address_return 标记 */
        picking: false,
        /** 删除确认弹窗目标 id；空表示不显示 */
        pendingDeleteId: '',
    },
    onLoad(options) {
        const picking = (options.from || '') === 'replenish';
        this.setData({ picking });
        if (picking) {
            // 标记为选择模式：列表页"点地址项"会回写 __gys_pick_address 并 navigateBack
            wx.setStorageSync('__gys_pick_address_return', true);
        }
    },
    onShow() {
        if (!(0, auth_1.isLogin)()) {
            wx.redirectTo({ url: '/pages/login/login' });
            return;
        }
        this.load();
    },
    onUnload() {
        // 非选择模式离开（如从个人中心 menu 正常浏览/编辑/删除），清理掉选择标记
        if (!this.data.picking) {
            try {
                wx.removeStorageSync('__gys_pick_address_return');
            }
            catch (e) { /* noop */ }
        }
    },
    async load() {
        this.setData({ loading: true });
        try {
            const rows = (await (0, api_1.getAddressList)()) || [];
            const list = rows.map((a) => ({
                ...a,
                // 拼接展示
                region: [a.province, a.city, a.district].filter(Boolean).join(' ') ||
                    (a.fullAddress ? a.fullAddress.slice(0, a.fullAddress.length - (a.detailAddr || '').length) : ''),
            }));
            this.setData({ list, loading: false });
        }
        catch (err) {
            this.setData({ loading: false });
            (0, util_1.toast)(err.message || '加载失败');
        }
    },
    onAdd() {
        wx.navigateTo({ url: '/pages/profile/address/edit' });
    },
    onEdit(e) {
        const id = e.currentTarget.dataset.id;
        if (!id)
            return;
        wx.navigateTo({ url: '/pages/profile/address/edit?id=' + id });
    },
    onSetDefault(e) {
        const id = e.currentTarget.dataset.id;
        if (!id)
            return;
        const item = this.data.list.find((x) => x.id === id);
        if (!item)
            return;
        // 选择模式：选中并直接返回详情页
        if (this.data.picking) {
            wx.removeStorageSync('__gys_pick_address_return');
            wx.setStorageSync('__gys_pick_address', item);
            wx.navigateBack();
            return;
        }
        // 普通模式：设为默认
        if (item.isDefault === 1) {
            // 已经是默认
            return;
        }
        wx.showLoading({ title: '设置中' });
        (0, api_1.setDefaultAddress)(id)
            .then(() => {
            (0, util_1.toast)('已设为默认');
            this.load();
        })
            .catch((err) => (0, util_1.toast)(err.message || '设置失败'))
            .finally(() => wx.hideLoading());
    },
    goBack() {
        const pages = getCurrentPages();
        if (pages.length > 1) {
            wx.navigateBack();
        }
        else {
            wx.switchTab({ url: '/pages/profile/profile' });
        }
    },
    onDeleteTap(e) {
        const id = e.currentTarget.dataset.id;
        if (!id)
            return;
        // 选择模式不允许删除，避免误操作
        if (this.data.picking)
            return;
        this.setData({ pendingDeleteId: id });
    },
    onCancelDelete() {
        this.setData({ pendingDeleteId: '' });
    },
    async onConfirmDelete() {
        const id = this.data.pendingDeleteId;
        if (!id)
            return;
        this.setData({ pendingDeleteId: '' });
        wx.showLoading({ title: '删除中' });
        try {
            await (0, api_1.removeAddress)(id);
            (0, util_1.toast)('已删除');
            this.load();
        }
        catch (err) {
            (0, util_1.toast)(err.message || '删除失败');
        }
        finally {
            wx.hideLoading();
        }
    },
});
