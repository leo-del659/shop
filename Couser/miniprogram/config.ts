// 后端基础地址
// ⚠️ 重要：部署到真机/线上时，localhost 指向的是「用户手机自己」，下单等所有请求都会失败，
// 表现为「小程序下了单、后端却看不见」。因此正式版/体验版必须使用真实后端域名（HTTPS），
// 并在微信公众平台「开发管理 → 开发设置 → 服务器域名 → request 合法域名」中完成备案。
//
// 开发阶段：微信开发者工具中勾选「不校验合法域名/TLS」，使用本机地址：
//  - 模拟器：http://localhost:6001
//  - 真机预览（开发版）：http://你的局域网IP:6001

// 开发环境地址（开发者工具 / 开发版）
const DEV_BASE_URL = 'http://localhost:6001'

// TODO: 替换为你的生产环境后端地址（必须是 HTTPS，且已在微信公众平台配置为 request 合法域名）
const PROD_BASE_URL = 'https://your-prod-domain.com'

/**
 * 按小程序运行环境自动选择后端地址：
 *  - develop = 开发者工具 / 开发版  → 用本地地址
 *  - trial   = 体验版              → 用生产地址
 *  - release = 正式版（线上）       → 用生产地址
 * 这样上传体验版/正式版时无需手动改代码，避免「忘了把 localhost 改成生产域名」导致订单无法落库。
 */
function resolveBaseUrl(): string {
  try {
    const env = (wx.getAccountInfoSync() as any)?.miniProgram?.envVersion
    if (env === 'develop') return DEV_BASE_URL
    return PROD_BASE_URL
  } catch {
    return DEV_BASE_URL
  }
}

export const BASE_URL = resolveBaseUrl()
