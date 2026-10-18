"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const auth_1 = require("../../utils/auth");
// 短信验证码类型：1-商户注册
const SMS_TYPE = 1;
// 重新获取验证码倒计时（秒）
const COUNTDOWN_SECONDS = 60;
// 计时器句柄（模块级，单页场景安全）
let smsTimer = null;
// 校验 11 位手机号（1 开头）
function isPhoneValid(phone) {
    return /^1\d{10}$/.test(phone);
}
Page({
    data: {
        phone: '',
        code: '',
        password: '',
        confirmPassword: '',
        countdown: 0, // >0 时禁用"获取验证码"
        sending: false,
        submitting: false,
        errorMsg: '',
    },
    onLoad() {
        // 若上一次残留计时器，先清掉
        if (smsTimer) {
            clearInterval(smsTimer);
            smsTimer = null;
        }
    },
    onUnload() {
        if (smsTimer) {
            clearInterval(smsTimer);
            smsTimer = null;
        }
    },
    onInput(e) {
        const field = e.currentTarget.dataset.field;
        this.setData({ [field]: e.detail.value, errorMsg: '' });
    },
    // 返回登录页
    goBack() {
        wx.navigateBack({
            fail: () => wx.redirectTo({ url: '/pages/login/login' }),
        });
    },
    async onSendCode() {
        if (this.data.sending || this.data.countdown > 0)
            return;
        const phone = this.data.phone;
        if (!isPhoneValid(phone)) {
            this.setData({ errorMsg: '请输入正确的 11 位手机号' });
            return;
        }
        this.setData({ sending: true, errorMsg: '' });
        wx.showLoading({ title: '发送中', mask: true });
        try {
            const code = await (0, auth_1.sendSmsCode)(phone, SMS_TYPE);
            wx.hideLoading();
            // 开发环境用弹窗模拟短信，由用户手动输入验证码
            if (code) {
                wx.showModal({
                    title: '模拟短信',
                    content: `验证码：${code}\n（开发环境模拟接收，请输入）`,
                    showCancel: false,
                    confirmText: '知道了'
                });
            }
            else {
                wx.showToast({ title: '验证码已发送', icon: 'none' });
            }
            this.startCountdown();
        }
        catch (err) {
            wx.hideLoading();
            this.setData({ sending: false, errorMsg: err.message || '验证码发送失败' });
        }
    },
    startCountdown() {
        this.setData({ countdown: COUNTDOWN_SECONDS, sending: false });
        smsTimer = setInterval(() => {
            const left = this.data.countdown - 1;
            if (left <= 0) {
                clearInterval(smsTimer);
                smsTimer = null;
                this.setData({ countdown: 0 });
            }
            else {
                this.setData({ countdown: left });
            }
        }, 1000);
    },
    async onRegister() {
        if (this.data.submitting)
            return;
        const { phone, code, password, confirmPassword } = this.data;
        if (!isPhoneValid(phone)) {
            this.setData({ errorMsg: '请输入正确的 11 位手机号' });
            return;
        }
        if (!code) {
            this.setData({ errorMsg: '请输入短信验证码' });
            return;
        }
        if (!password || password.length < 6) {
            this.setData({ errorMsg: '密码长度不能少于 6 位' });
            return;
        }
        if (password !== confirmPassword) {
            this.setData({ errorMsg: '两次输入的密码不一致' });
            return;
        }
        this.setData({ submitting: true, errorMsg: '' });
        wx.showLoading({ title: '注册中', mask: true });
        try {
            await (0, auth_1.registerMerchant)(phone, code, password);
            wx.hideLoading();
            wx.showToast({ title: '注册成功', icon: 'success' });
            // 注册成功后返回登录页并带入手机号，方便直接登录
            setTimeout(() => {
                wx.redirectTo({ url: '/pages/login/login?phone=' + phone });
            }, 800);
        }
        catch (err) {
            wx.hideLoading();
            this.setData({ submitting: false, errorMsg: err.message || '注册失败' });
        }
    },
});
