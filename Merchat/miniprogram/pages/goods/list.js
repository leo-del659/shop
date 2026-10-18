"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../utils/api");
const util_1 = require("../../utils/util");
const auth_1 = require("../../utils/auth");
const categories_1 = require("../../utils/categories");
Page({
    data: {
        canManage: true,
        filter: 'all',
        keyword: '',
        categories: categories_1.CATEGORIES,
        // picker 选项（纯字符串数组，onLoad 中构建）
        categoryNames: [],
        categoryIndex: 0,
        category: '',
        list: [],
        statTotal: 0,
        statPublished: 0,
        statLow: 0,
        statDraft: 0,
        // computed display texts
        fAll: 'active', fPublished: '', fDraft: '', fDisabled: '', fLowStock: '',
        defaultEmoji: '\u{1F4E6}',
    },
    _searchTimer: 0,
    onLoad() {
        this.setData({
            canManage: true,
            categoryNames: ['全部', ...categories_1.CATEGORIES.map(c => c.name)],
        });
        this.load();
    },
    onShow() {
        const tabBar = this.getTabBar();
        if (tabBar)
            tabBar.setData({ selected: 2 });
    },
    async load() {
        if (!(0, auth_1.isLogin)()) {
            wx.redirectTo({ url: '/pages/login/login' });
            return;
        }
        const { keyword, category, filter } = this.data;
        const opts = { keyword: keyword || undefined, category: category || undefined };
        const api = api_1.getGoodsList;
        const [raw, all, drafts] = await Promise.all([
            api(filter, opts),
            api('all'),
            api('draft'),
        ]);
        const list = raw.map((g) => {
            const isLow = g.stock < 50;
            const isPub = g.status === 'published';
            return {
                ...g,
                coverImgFull: g.coverImg || '',
                stockClass: (0, util_1.getStockClass)(g.stock, g.safeStock || 50),
                stockLabel: (0, util_1.getStockLabel)(g.stock, g.safeStock || 50),
                isLowStock: isLow,
                statusText: isPub ? '已上架' : g.status === 'draft' ? '待审核' : '已下架',
                stockCls: isLow ? 'low' : 'normal',
                defaultEmoji: '\u{1F4E6}',
            };
        });
        this.setData({
            list,
            statTotal: all.length,
            statPublished: all.filter((g) => g.status === 'published').length,
            statLow: all.filter((g) => g.stock < 50).length,
            statDraft: drafts.length,
        });
        this._updateFilterCls();
    },
    onSearchInput(e) {
        const keyword = (e.detail.value || '').trim();
        this.setData({ keyword });
        if (this._searchTimer)
            clearTimeout(this._searchTimer);
        this._searchTimer = setTimeout(() => this.load(), 350);
    },
    onSearchConfirm() {
        if (this._searchTimer)
            clearTimeout(this._searchTimer);
        this.load();
    },
    clearSearch() {
        if (this._searchTimer)
            clearTimeout(this._searchTimer);
        this.setData({ keyword: '' }, () => this.load());
    },
    onCategoryChange(e) {
        const idx = Number(e.detail.value);
        const catId = idx === 0 ? '' : (this.data.categories[idx - 1]?.id || '');
        this.setData({ categoryIndex: idx, category: catId }, () => this.load());
    },
    setFilter(e) {
        const f = e.currentTarget.dataset.filter;
        if (f === this.data.filter)
            return;
        this.setData({ filter: f }, () => this.load());
    },
    _updateFilterCls() {
        const f = this.data.filter;
        this.setData({
            fAll: f === 'all' ? 'active' : '',
            fPublished: f === 'published' ? 'active' : '',
            fDraft: f === 'draft' ? 'active' : '',
            fDisabled: f === 'disabled' ? 'active' : '',
            fLowStock: f === 'low-stock' ? 'active' : '',
        });
    },
    onImageError(e) {
        const id = e.currentTarget.dataset.id;
        const list = this.data.list.map((item) => item.id === id ? { ...item, imgError: true } : item);
        this.setData({ list });
    },
    goDetail(e) {
        const id = e.currentTarget.dataset.id;
        const idx = this.data.list.findIndex((g) => String(g.id) === String(id));
        wx.navigateTo({ url: '/pages/goods/detail?id=' + id + '&idx=' + (idx >= 0 ? idx : 0) });
    },
    goAdd() {
        wx.navigateTo({ url: '/pages/goods/add/add' });
    },
    stopPropagation() { },
    async onToggleOff(e) {
        const id = e.currentTarget.dataset.id;
        const ok = await new Promise((resolve) => {
            wx.showModal({
                title: '确认下架',
                content: '下架后该商品将不再对商户可见，确认？',
                success: (res) => resolve(res.confirm),
            });
        });
        if (!ok)
            return;
        wx.showLoading({ title: '处理中' });
        try {
            await (0, api_1.toggleGoodsStatus)(id);
            (0, util_1.toast)('已下架');
            this.load();
        }
        catch (err) {
            (0, util_1.toast)(err.message || '操作失败');
        }
        finally {
            wx.hideLoading();
        }
    },
    async onToggleOn(e) {
        const id = e.currentTarget.dataset.id;
        wx.showLoading({ title: '处理中' });
        try {
            await (0, api_1.toggleGoodsStatus)(id);
            (0, util_1.toast)('已上架');
            this.load();
        }
        catch (err) {
            (0, util_1.toast)(err.message || '操作失败');
        }
        finally {
            wx.hideLoading();
        }
    },
});
