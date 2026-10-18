"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../../utils/api");
const request_1 = require("../../../utils/request");
const util_1 = require("../../../utils/util");
Page({
    data: {
        saving: false,
        saveBtnText: '保存',
        // avatarUrl 为完整展示地址，avatarFile 为后端相对地址（提交用）
        avatarUrl: '',
        avatarFile: '',
        avatarText: '商',
        merchantName: '',
        uploading: false,
    },
    onLoad() {
        this.load();
    },
    async load() {
        const d = await (0, api_1.getProfile)();
        const s = d.merchant;
        this.setData({
            merchantName: s.merchantName || '',
            avatarText: (s.merchantName || '商').charAt(0),
            avatarUrl: s.avatar ? (0, request_1.resolveUrl)(s.avatar) : '',
            avatarFile: s.avatar || '',
        });
    },
    goBack() {
        wx.navigateBack();
    },
    // 选择并上传头像
    chooseAvatar() {
        if (this.data.uploading || this.data.saving)
            return;
        wx.chooseMedia({
            count: 1,
            mediaType: ['image'],
            sourceType: ['album', 'camera'],
            success: (res) => {
                const tempFilePath = res.tempFiles[0] && res.tempFiles[0].tempFilePath;
                if (!tempFilePath)
                    return;
                this.uploadAvatar(tempFilePath);
            },
            fail: () => (0, util_1.toast)('选择图片失败'),
        });
    },
    async uploadAvatar(tempFilePath) {
        this.setData({ uploading: true });
        wx.showLoading({ title: '上传中', mask: true });
        try {
            const result = await (0, request_1.uploadFile)(tempFilePath);
            this.setData({
                avatarFile: result.url,
                avatarUrl: (0, request_1.resolveUrl)(result.url),
            });
            (0, util_1.toast)('头像上传成功');
        }
        catch (err) {
            (0, util_1.toast)(err.message || '头像上传失败');
        }
        finally {
            wx.hideLoading();
            this.setData({ uploading: false });
        }
    },
    onInput(e) {
        const field = e.currentTarget.dataset.field;
        this.setData({ [field]: e.detail.value });
    },
    async save() {
        if (this.data.saving)
            return;
        const merchantName = (this.data.merchantName || '').trim();
        if (!merchantName) {
            (0, util_1.toast)('请输入商户名称');
            return;
        }
        this.setData({ saving: true, saveBtnText: '保存中...' });
        try {
            await (0, api_1.updateProfile)({ merchantName, avatar: this.data.avatarFile || '' });
            (0, util_1.toast)('保存成功');
            setTimeout(() => wx.navigateBack(), 600);
        }
        catch (err) {
            (0, util_1.toast)(err.message || '保存失败');
        }
        finally {
            this.setData({ saving: false, saveBtnText: '保存' });
        }
    },
});
