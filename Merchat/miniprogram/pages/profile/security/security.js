"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const api_1 = require("../../../utils/api");
const util_1 = require("../../../utils/util");
Page({
    data: {
        saving: false,
        saveBtnText: '确认修改',
        errorMsg: '',
        form: {
            oldPassword: '',
            newPassword: '',
            confirmPassword: '',
        },
    },
    goBack() {
        wx.navigateBack();
    },
    onInput(e) {
        const field = e.currentTarget.dataset.field;
        this.setData({ [`form.${field}`]: e.detail.value, errorMsg: '' });
    },
    async save() {
        if (this.data.saving)
            return;
        const { oldPassword, newPassword, confirmPassword } = this.data.form;
        if (!oldPassword || !newPassword) {
            this.setData({ errorMsg: '请输入原密码和新密码' });
            (0, util_1.toast)('请输入原密码和新密码');
            return;
        }
        if (newPassword.length < 6) {
            this.setData({ errorMsg: '新密码长度不能少于6位' });
            (0, util_1.toast)('新密码长度不能少于6位');
            return;
        }
        if (newPassword !== confirmPassword) {
            this.setData({ errorMsg: '两次输入的新密码不一致' });
            (0, util_1.toast)('两次输入的新密码不一致');
            return;
        }
        const ok = await (0, util_1.confirmDialog)('修改密码后需要重新登录，是否继续？');
        if (!ok)
            return;
        this.setData({ saving: true, saveBtnText: '保存中...', errorMsg: '' });
        try {
            await (0, api_1.changePassword)(oldPassword, newPassword);
            (0, util_1.toast)('密码修改成功，请重新登录');
            setTimeout(() => {
                wx.reLaunch({ url: '/pages/login/login' });
            }, 800);
        }
        catch (err) {
            const msg = err.message || '修改失败';
            this.setData({ errorMsg: msg });
            (0, util_1.toast)(msg);
        }
        finally {
            this.setData({ saving: false, saveBtnText: '确认修改' });
        }
    },
});
