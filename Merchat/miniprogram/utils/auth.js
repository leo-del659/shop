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
/** 地址页之间传递选中地址用的临时 key（切换商户后必须清理，否则会把上个商户的地址带过来） */
const PICK_ADDRESS_KEY = '__gys_pick_address';
const PICK_ADDRESS_RETURN_KEY = '__gys_pick_address_return';
function getToken() {
    return wx.getStorageSync(TOKEN_KEY) || '';
}
function setToken(t) {
    wx.setStorageSync(TOKEN_KEY, t);
}
function clearAuth() {
    wx.removeStorageSync(TOKEN_KEY);
    wx.removeStorageSync(MERCHANT_KEY);
    // 清理跨页面的地址传参残留
    wx.removeStorageSync(PICK_ADDRESS_KEY);
    wx.removeStorageSync(PICK_ADDRESS_RETURN_KEY);
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
    // 切换商户时清理上个商户遗留的地址传参
    wx.removeStorageSync(PICK_ADDRESS_KEY);
    wx.removeStorageSync(PICK_ADDRESS_RETURN_KEY);
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
