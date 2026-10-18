"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.request = request;
exports.uploadFile = uploadFile;
exports.resolveUrl = resolveUrl;
exports.uploadProductImage = uploadProductImage;
const config_1 = require("./config");
const auth_1 = require("./auth");
function redirectToLogin() {
    // 避免在登录页重复跳转
    const pages = getCurrentPages();
    const current = pages.length ? pages[pages.length - 1] : null;
    const route = current ? current.route : '';
    if (route && route.indexOf('pages/login/login') > -1)
        return;
    wx.reLaunch({ url: '/pages/login/login' });
}
// 把后端返回的字段统一转成 camelCase，供小程序端使用。
// 兼容三种格式：snake_case(goods_name)、PascalCase(GoodsName)、已有 camelCase(goodsName)
function toCamelCase(value) {
    if (value === null || value === undefined)
        return value;
    if (Array.isArray(value))
        return value.map(toCamelCase);
    if (typeof value === 'object' && value !== null) {
        const obj = value;
        const out = {};
        Object.keys(obj).forEach((key) => {
            // snake_case → camelCase：goods_name → goodsName
            let camel = key.replace(/_([a-z])/g, (_, ch) => ch.toUpperCase());
            // PascalCase → camelCase：GoodsName → goodsName
            if (/[A-Z]/.test(camel[0])) {
                camel = camel[0].toLowerCase() + camel.slice(1);
            }
            out[camel] = toCamelCase(obj[key]);
        });
        return out;
    }
    return value;
}
// 韧性保护链（用户端侧）：配合后端 限流/隔离/超时/重试/熔断/降级，
// 对可幂等的“安全方法”(GET/HEAD/OPTIONS) 在网络错误或 429/503/504 时做指数退避重试，
// 其余情况给出与后端一致的友好提示。
const RESILIENCE = {
    maxRetry: 2,
    baseDelayMs: 300,
    timeoutMs: 15000,
    safeMethods: ['GET', 'HEAD', 'OPTIONS'],
};
// 把后端保护链返回的 HTTP 状态码，映射成用户能看懂的话
function resilienceMessage(statusCode, fallback) {
    switch (statusCode) {
        case 429: return '请求过于频繁，请稍后再试';
        case 503: return '服务暂时繁忙，请稍后重试';
        case 504: return '请求超时，已为您快速释放，请稍后重试';
        default: return fallback || '请求失败';
    }
}
function isSafeMethod(method) {
    return !!method && RESILIENCE.safeMethods.includes(method.toUpperCase());
}
// 统一请求封装：自动带 Bearer token，按 code===0 判定成功；内置用户端韧性重试。
function request(options) {
    const { path, method = 'GET', data, raw = false } = options;
    const safe = isSafeMethod(method);
    const attemptOnce = (attempt) => new Promise((resolve, reject) => {
        const token = (0, auth_1.getToken)();
        wx.request({
            url: config_1.API_BASE + path,
            method,
            data,
            timeout: RESILIENCE.timeoutMs,
            header: {
                'content-type': 'application/json',
                ...(token ? { Authorization: 'Bearer ' + token } : {}),
            },
            success: (res) => {
                const extractMsg = () => {
                    const b = res.data;
                    return b && typeof b === 'object' && b.message ? b.message : `HTTP ${res.statusCode}`;
                };
                if (res.statusCode === 401 || res.statusCode === 403) {
                    // 优先读取后端返回的 message，登录失败时 backend 会返回"账号或密码错误"
                    const msg = extractMsg();
                    (0, auth_1.clearAuth)();
                    // 延迟跳转，避免在 onLaunch 阶段与页面加载冲突造成闪烁
                    setTimeout(() => redirectToLogin(), 100);
                    reject(new Error(msg));
                    return;
                }
                // 后端保护链触发：429 限流 / 503 熔断或隔离 / 504 超时
                if (res.statusCode === 429 || res.statusCode === 503 || res.statusCode === 504) {
                    const msg = resilienceMessage(res.statusCode, extractMsg());
                    if (safe && attempt < RESILIENCE.maxRetry) {
                        setTimeout(() => attemptOnce(attempt + 1).then(resolve, reject), RESILIENCE.baseDelayMs * Math.pow(2, attempt));
                        return;
                    }
                    reject(new Error(msg));
                    return;
                }
                if (raw) {
                    resolve(toCamelCase(res.data));
                    return;
                }
                const body = res.data;
                if (res.statusCode >= 200 && res.statusCode < 300 && body && typeof body === 'object' && 'code' in body) {
                    if (body.code === 0) {
                        resolve(toCamelCase(body.data));
                    }
                    else {
                        reject(new Error(body.message || '请求失败'));
                    }
                }
                else if (res.statusCode >= 200 && res.statusCode < 300) {
                    resolve(toCamelCase(res.data));
                }
                else {
                    reject(new Error(extractMsg()));
                }
            },
            fail: (err) => {
                const msg = err.errMsg || '';
                if (msg.indexOf('不在以下 request 合法域名') > -1 || msg.indexOf('fail:url not in domain list') > -1) {
                    reject(new Error('请求被微信域名白名单拦截，请在开发者工具「详情 → 本地设置」中勾选「不校验合法域名」'));
                    return;
                }
                // 网络层错误：安全方法可重试（指数退避）
                if (safe && attempt < RESILIENCE.maxRetry) {
                    setTimeout(() => attemptOnce(attempt + 1).then(resolve, reject), RESILIENCE.baseDelayMs * Math.pow(2, attempt));
                    return;
                }
                reject(new Error(msg || '网络错误，请稍后重试'));
            },
        });
    });
    return attemptOnce(0);
}
// 文件上传：form-data 字段 file，响应结构 { code:0, message, data:{ url } }
function uploadFile(filePath) {
    const token = (0, auth_1.getToken)();
    return new Promise((resolve, reject) => {
        wx.uploadFile({
            url: config_1.API_BASE + '/api/File/upload',
            filePath,
            name: 'file',
            header: token ? { Authorization: 'Bearer ' + token } : {},
            success: (res) => {
                try {
                    const body = JSON.parse(res.data);
                    if (body.code === 0 && body.data && body.data.url) {
                        // 仅返回后端相对地址 /api/file/{guid}（与商户端一致），展示时由 resolveUrl 拼完整 URL
                        resolve({ url: body.data.url });
                    }
                    else {
                        reject(new Error(body.message || '上传失败'));
                    }
                }
                catch (e) {
                    reject(new Error('上传响应解析失败'));
                }
            },
            fail: (err) => reject(new Error(err.errMsg || '上传失败')),
        });
    });
}
/**
 * 把后端返回的图片地址解析为可访问的完整 URL（与商户端 Chat 的 resolveUrl 行为一致）
 * - 已是 http(s) 直接返回
 * - 以 /api/file/ 开头的相对地址转成 /api/file/img?name=xxx，
 *   走可靠下载端点（避免 MVC 路由对文件名中的点做截断，直接 /api/file/{name} 不可靠）
 * - 其他以 / 开头的相对地址直接拼接 API_BASE
 * - 逗号分隔的多图地址（如 /api/file/a.webp,/api/file/b.webp）只取第一张
 */
function resolveUrl(url) {
    if (!url)
        return '';
    if (url.startsWith('http'))
        return url;
    // coverImg 等字段可能存的是逗号分隔的多张图片，<image> 只能显示一张
    const first = url.split(',')[0].trim();
    if (first.startsWith('/api/file/')) {
        const fileName = first.slice('/api/file/'.length);
        return `${config_1.API_BASE}/api/file/img?name=${encodeURIComponent(fileName)}`;
    }
    return config_1.API_BASE + first;
}
function uploadProductImage(filePath) {
    const token = (0, auth_1.getToken)();
    return new Promise((resolve, reject) => {
        wx.uploadFile({
            url: config_1.API_BASE + '/api/File/upload-product',
            filePath,
            name: 'file',
            header: token ? { Authorization: 'Bearer ' + token } : {},
            success: (res) => {
                try {
                    const body = JSON.parse(res.data);
                    if (body.code === 0 && body.data && body.data.url) {
                        // 仅返回后端相对地址 /api/file/<hash>（与商户端一致），展示时由 resolveUrl 拼完整 URL
                        resolve({ url: body.data.url });
                    }
                    else {
                        reject(new Error(body.message || '上传失败'));
                    }
                }
                catch (e) {
                    reject(new Error('上传响应解析失败'));
                }
            },
            fail: (err) => reject(new Error(err.errMsg || '上传失败')),
        });
    });
}
