"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.getToken = getToken;
exports.setToken = setToken;
exports.clearAuth = clearAuth;
exports.isLogin = isLogin;
exports.getCurrentRole = getCurrentRole;
exports.accountLogin = accountLogin;
exports.sendSmsCode = sendSmsCode;
exports.registerMerchant = registerMerchant;
exports.getLocalMerchant = getLocalMerchant;
exports.setLocalMerchant = setLocalMerchant;
exports.getCurrentUser = getCurrentUser;
exports.logout = logout;
const request_1 = require("./request");
const TOKEN_KEY = 'auth_token';
const MERCHANT_KEY = 'merchant_info';
function getToken() {
    return wx.getStorageSync(TOKEN_KEY) || '';
}
function setToken(t) {
    wx.setStorageSync(TOKEN_KEY, t);
}
function clearAuth() {
    wx.removeStorageSync(TOKEN_KEY);
    wx.removeStorageSync(MERCHANT_KEY);
}
function isLogin() {
    return !!getToken();
}
// 当前登录角色：仅商户端
function getCurrentRole() {
    if (getLocalMerchant() && getLocalMerchant().id)
        return 'merchant';
    return null;
}
// 手机号 + 密码登录
async function accountLogin(account, password) {
    const res = await (0, request_1.request)({
        path: '/api/merchant/login',
        method: 'POST',
        data: { account, password },
    });
    setToken(res.token);
    if (res.merchant)
        setLocalMerchant(res.merchant);
    return res;
}
// 发送短信验证码（type: 1-商户注册）。开发环境后端直接回传验证码，便于联调。
async function sendSmsCode(mobile, type = 1) {
    const res = await (0, request_1.request)({
        path: '/api/Sms/Send',
        method: 'POST',
        data: { mobile, type, ip: '' },
    });
    return res.code || '';
}
// 手机号 + 验证码 + 密码注册，成功后返回登录页自行登录
async function registerMerchant(phone, code, password) {
    await (0, request_1.request)({
        path: '/api/merchant/register',
        method: 'POST',
        data: { phone, code, password },
    });
}
function getLocalMerchant() {
    return wx.getStorageSync(MERCHANT_KEY) || null;
}
function setLocalMerchant(s) {
    wx.setStorageSync(MERCHANT_KEY, s);
}
function getCurrentUser() {
    if (getLocalMerchant() && getLocalMerchant().id)
        return getLocalMerchant();
    return null;
}
function logout() {
    clearAuth();
}
