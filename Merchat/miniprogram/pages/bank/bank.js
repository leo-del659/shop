"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../utils/api");
const auth_1 = require("../../utils/auth");
const util_1 = require("../../utils/util");
Page({
    data: {
        saving: false,
        bankName: '',
        bankAccount: '',
        accountName: '',
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
            const bank = await (0, api_1.getBank)();
            if (bank) {
                this.setData({
                    bankName: bank.bankName || '',
                    bankAccount: bank.bankAccount || '',
                    accountName: bank.accountName || '',
                });
            }
        }
        catch (e) {
            (0, util_1.toast)('加载失败：' + (e.message || '未知错误'));
        }
    },
    onInput(e) {
        const field = e.currentTarget.dataset.field;
        this.setData({ [field]: e.detail.value });
    },
    async save() {
        if (this.data.saving)
            return;
        const { bankName, bankAccount, accountName } = this.data;
        if (!bankName.trim() || !bankAccount.trim() || !accountName.trim()) {
            (0, util_1.toast)('请填写完整的开户行、账号和开户名');
            return;
        }
        this.setData({ saving: true });
        try {
            await (0, api_1.updateBank)({
                bankName: bankName.trim(),
                bankAccount: bankAccount.trim(),
                accountName: accountName.trim(),
            });
            (0, util_1.toast)('保存成功');
            setTimeout(() => wx.navigateBack(), 600);
        }
        catch (e) {
            (0, util_1.toast)(e.message || '保存失败');
        }
        finally {
            this.setData({ saving: false });
        }
    },
});
