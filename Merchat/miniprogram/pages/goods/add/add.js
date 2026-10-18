"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../../utils/api");
const request_1 = require("../../../utils/request");
const auth_1 = require("../../../utils/auth");
const util_1 = require("../../../utils/util");
const categories_1 = require("../../../utils/categories");
Page({
    data: {
        saving: false,
        uploading: false,
        isEdit: false,
        goodsId: '',
        categories: categories_1.CATEGORIES,
        categoryIndex: 0,
        form: {
            name: '',
            price: '',
            marketPrice: '',
            stock: '',
            unit: '',
            spec: '',
            description: '',
        },
        coverImg: '',
        coverImgFull: '',
        // computed display texts (avoid single-quote literals in WXML)
        uploadIco: '\u{1F4F7}',
        uploadText: '点击上传图片',
        tipsText: '提交审核后将自动关联到当前商户',
        saveBtnText: '提交审核',
    },
    async onLoad(options) {
        const id = options.id;
        if (id) {
            this.setData({ isEdit: true, goodsId: id });
            this._updateTexts(true, false);
            wx.setNavigationBarTitle({ title: '编辑商品' });
            await this.loadGoods(id);
        }
        else {
            this._updateTexts(false, false);
        }
    },
    async loadGoods(id) {
        try {
            const g = await (0, api_1.getGoodsDetail)(id);
            if (!g) {
                (0, util_1.toast)('商品不存在');
                return;
            }
            const catIdx = categories_1.CATEGORIES.findIndex((c) => c.id === g.category);
            this.setData({
                categoryIndex: catIdx >= 0 ? catIdx : 0,
                form: {
                    name: g.name || '',
                    price: g.price ? String(g.price) : '',
                    marketPrice: g.marketPrice ? String(g.marketPrice) : '',
                    stock: g.stock != null ? String(g.stock) : '',
                    unit: g.unit || '',
                    spec: g.spec || '',
                    description: g.description || '',
                },
                coverImg: g.coverImg || '',
                coverImgFull: (0, request_1.resolveUrl)(g.coverImg),
            });
        }
        catch (err) {
            (0, util_1.toast)(err.message || '加载商品失败');
        }
    },
    goBack() {
        wx.navigateBack();
    },
    onInput(e) {
        const field = e.currentTarget.dataset.field;
        this.setData({ [`form.${field}`]: e.detail.value });
    },
    onCategoryChange(e) {
        this.setData({ categoryIndex: e.detail.value });
    },
    async chooseImage() {
        if (this.data.uploading)
            return;
        try {
            const res = await wx.chooseMedia({
                count: 1,
                mediaType: ['image'],
                sourceType: ['album', 'camera'],
            });
            const tempFile = res.tempFiles[0];
            if (!tempFile?.tempFilePath) {
                (0, util_1.toast)('未选择图片');
                return;
            }
            this.setData({ uploading: true });
            this._updateTexts(this.data.isEdit, true);
            const uploaded = await (0, request_1.uploadProductImage)(tempFile.tempFilePath);
            // coverImg 保留后端相对地址（提交时用），coverImgFull 经 resolveUrl 拼成完整展示地址（与商户端一致）
            this.setData({ coverImg: uploaded.url, coverImgFull: (0, request_1.resolveUrl)(uploaded.url), uploading: false });
            this._updateTexts(this.data.isEdit, false);
            (0, util_1.toast)('上传成功');
        }
        catch (err) {
            this.setData({ uploading: false });
            this._updateTexts(this.data.isEdit, false);
            (0, util_1.toast)(err.message || '上传失败');
        }
    },
    previewImage() {
        const url = this.data.coverImgFull;
        if (!url)
            return;
        wx.previewImage({ urls: [url], current: url });
    },
    async save() {
        if (this.data.saving)
            return;
        // 立即置位，防止按钮连点导致并发提交
        this.setData({ saving: true });
        this._updateTexts(this.data.isEdit, false);
        try {
            const { form, categoryIndex, coverImg, isEdit, goodsId } = this.data;
            const name = (form.name || '').trim();
            if (!name) {
                (0, util_1.toast)('请输入商品名称');
                return;
            }
            const price = parseFloat(form.price);
            if (isNaN(price) || price <= 0) {
                (0, util_1.toast)('请输入正确的售价');
                return;
            }
            const marketPrice = parseFloat(form.marketPrice || '0') || price;
            const stock = parseInt(form.stock, 10);
            if (isNaN(stock) || stock < 0) {
                (0, util_1.toast)('请输入正确的库存');
                return;
            }
            if (!coverImg) {
                (0, util_1.toast)('请上传商品图片');
                return;
            }
            const merchant = (0, auth_1.getLocalMerchant)();
            const merchantId = merchant ? String(merchant.id) : '';
            const payload = {
                name,
                categoryId: categories_1.CATEGORIES[categoryIndex].id,
                price,
                marketPrice,
                stock,
                unit: (form.unit || '份').trim(),
                spec: (form.spec || '').trim(),
                description: (form.description || '').trim(),
                coverImg,
                // 新增商品统一提交审核，默认不上架；后端会强制 audit_status=0
                isOnSale: 0,
                merchantId: merchantId,
            };
            if (isEdit) {
                await (0, api_1.updateGoods)(goodsId, payload);
                (0, util_1.toast)('保存成功');
            }
            else {
                await (0, api_1.addGoods)(payload);
                (0, util_1.toast)('提交审核成功');
            }
            // 不再手动 prev.load()：返回列表页时 onShow 会自动 load 一次，避免双重刷新
            setTimeout(() => wx.navigateBack(), 600);
        }
        catch (err) {
            (0, util_1.toast)(err.message || (this.data.isEdit ? '保存失败' : '新增失败'));
        }
        finally {
            this.setData({ saving: false });
            this._updateTexts(this.data.isEdit, false);
        }
    },
    /** compute all display texts for WXML (avoid single-quote literals) */
    _updateTexts(isEdit, uploading) {
        this.setData({
            uploadIco: uploading ? '\u23F3' : '\u{1F4F7}',
            uploadText: uploading ? '上传中...' : '点击上传图片',
            tipsText: isEdit ? '保存后将更新商品信息' : '提交审核后将自动关联到当前商户',
            saveBtnText: uploading
                ? (isEdit ? '保存中...' : '提交中...')
                : (isEdit ? '保存商品' : '提交审核'),
        });
    },
});
