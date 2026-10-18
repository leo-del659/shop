// 统一请求封装：自动拼接 BASE_URL、携带 token、解包 Api_Response
import { BASE_URL } from '../config'
import { getAppToken, clearAppToken, currentPageUrl } from '../app'

export interface ApiResponse<T = any> {
  code: number
  message: string
  data: T
}

type Method = 'GET' | 'POST' | 'PUT' | 'DELETE'

/**
 * 命中即视为「登录态失效，需要重新登录」的业务码。
 * 不同后端约定不同：有的用 HTTP 401（见下方 statusCode 判断），
 * 有的在 HTTP 200 下用业务码（如 401 / 1001）表示。请按你的后端返回调整这里。
 */
const AUTH_EXPIRED_CODES = [401]

/**
 * 安全调用 wx.showToast：
 * 1. 截断超长 title（微信限制约 7 个汉字，超长会报「操作异常」）
 * 2. 页面销毁等异常场景下静默失败，避免把 toast 错误抛成未捕获异常
 */
function safeToast(title: string, icon: 'none' | 'success' | 'loading' = 'none') {
  if (!title) return
  const maxLen = 20
  const safe = title.length > maxLen ? title.slice(0, maxLen) + '…' : title
  try {
    wx.showToast({ title: safe, icon })
  } catch (e) {
    // 忽略页面不在前台等导致的 toast 异常
  }
}

/**
 * 登录态失效的统一处理：清掉本地 token，并跳登录页。
 * 用「当前页是否已在登录页」做防抖，避免并发请求同时 401 时堆叠多个登录页。
 */
function handleAuthFailure() {
  clearAppToken()
  const pages = getCurrentPages()
  const top = pages[pages.length - 1]
  if (top && top.route === 'pages/login/login') return
  const redirect = currentPageUrl()
  const url =
    '/pages/login/login' + (redirect ? `?redirect=${encodeURIComponent(redirect)}` : '')
  wx.navigateTo({ url })
}

interface RequestOptions {
  url: string
  method?: Method
  data?: any
  /** 是否需要携带登录 token，默认 true */
  auth?: boolean
}

// 韧性保护链（用户端侧）：配合后端 限流/隔离/超时/重试/熔断/降级，
// 对可幂等的“安全方法”(GET/HEAD/OPTIONS) 在 429/503/504 或网络错误时做指数退避重试。
const RESILIENCE = {
  maxRetry: 2,
  baseDelayMs: 300,
  timeoutMs: 15000,
  safeMethods: ['GET', 'HEAD', 'OPTIONS'],
}

function isSafeMethod(method?: string): boolean {
  return !!method && RESILIENCE.safeMethods.includes((method || '').toUpperCase())
}

// 把后端保护链返回的 HTTP 状态码，映射成用户能看懂的话
function resilienceMessage(statusCode: number, fallback?: string | null): string {
  switch (statusCode) {
    case 429: return '请求过于频繁，请稍后再试'
    case 503: return '服务暂时繁忙，请稍后重试'
    case 504: return '请求超时，已为您快速释放，请稍后重试'
    default: return fallback || '请求失败，请稍后重试'
  }
}

function request<T = any>(options: RequestOptions): Promise<T> {
  const auth = options.auth !== false
  const fullUrl = BASE_URL + options.url
  const safe = isSafeMethod(options.method)
  console.log('[request]', options.method || 'GET', fullUrl, auth ? 'with token' : 'no auth')

  const attemptOnce = (attempt: number): Promise<T> =>
    new Promise<T>((resolve, reject) => {
      wx.request({
        url: fullUrl,
        method: options.method || 'GET',
        data: options.data,
        timeout: RESILIENCE.timeoutMs,
        header: {
          'Content-Type': 'application/json',
          ...(auth ? { Authorization: 'Bearer ' + getAppToken() } : {})
        },
        success(res) {
          const body = res.data as ApiResponse<T>
          // 登录态失效：HTTP 401，或业务码命中（按后端约定调整 AUTH_EXPIRED_CODES）
          const authExpired =
            res.statusCode === 401 ||
            (body && typeof body.code === 'number' && AUTH_EXPIRED_CODES.includes(body.code))
          if (authExpired) {
            safeToast('登录已过期，请重新登录')
            handleAuthFailure()
            reject(new Error('AUTH_EXPIRED'))
            return
          }
          // 后端保护链触发：429 限流 / 503 熔断或隔离 / 504 超时 → 安全方法重试，否则友好提示
          if (res.statusCode === 429 || res.statusCode === 503 || res.statusCode === 504) {
            if (safe && attempt < RESILIENCE.maxRetry) {
              setTimeout(() => attemptOnce(attempt + 1).then(resolve, reject), RESILIENCE.baseDelayMs * Math.pow(2, attempt))
              return
            }
            const msg = resilienceMessage(res.statusCode, body?.message)
            safeToast(msg)
            reject(new Error(msg))
            return
          }
          // 后端统一返回 { code, message, data }
          if (body && typeof body.code === 'number') {
            if (body.code === 0) {
              resolve(body.data)
            } else {
              const msg = friendlyMessage(body.message) || '请求失败'
              safeToast(msg)
              reject(new Error(msg))
            }
          } else {
            // 非标准结构直接返回
            resolve(res.data as T)
          }
        },
        fail(err) {
          // 网络层错误：安全方法可重试（指数退避）
          if (safe && attempt < RESILIENCE.maxRetry) {
            setTimeout(() => attemptOnce(attempt + 1).then(resolve, reject), RESILIENCE.baseDelayMs * Math.pow(2, attempt))
            return
          }
          safeToast('网络错误，请稍后重试')
          reject(err)
        }
      })
    })

  return attemptOnce(0)
}

export const http = {
  get: <T = any>(url: string, data?: any, auth = true) =>
    request<T>({ url, method: 'GET', data, auth }),
  post: <T = any>(url: string, data?: any, auth = true) =>
    request<T>({ url, method: 'POST', data, auth }),
  put: <T = any>(url: string, data?: any, auth = true) =>
    request<T>({ url, method: 'PUT', data, auth }),
  delete: <T = any>(url: string, data?: any, auth = true) =>
    request<T>({ url, method: 'DELETE', data, auth })
}

/**
 * 上传图片文件（头像等），需登录
 * @param filePath 本地临时文件路径（wx.chooseMedia 返回的 tempFilePath）
 * @returns 后端返回的相对地址，如 /api/file/{guid}.png（展示时需用 resolveUrl 拼接 BASE_URL）
 */
export function uploadFile(filePath: string): Promise<{ url: string }> {
  return new Promise((resolve, reject) => {
    wx.uploadFile({
      url: BASE_URL + '/api/file/upload',
      filePath,
      name: 'file',
      header: {
        Authorization: 'Bearer ' + getAppToken()
      },
      success(res) {
        try {
          const body = JSON.parse(res.data) as ApiResponse<{ url: string }>
          // 登录态失效同样处理
          if (
            res.statusCode === 401 ||
            (body && typeof body.code === 'number' && AUTH_EXPIRED_CODES.includes(body.code))
          ) {
            safeToast('登录已过期，请重新登录')
            handleAuthFailure()
            reject(new Error('AUTH_EXPIRED'))
            return
          }
          if (body && body.code === 0) {
            resolve(body.data)
          } else {
            const msg = friendlyMessage(body?.message) || '上传失败'
            safeToast(msg)
            reject(new Error(msg))
          }
        } catch (e) {
          safeToast('上传解析失败')
          reject(e)
        }
      },
      fail(err) {
        safeToast('网络错误，上传失败')
        reject(err)
      }
    })
  })
}

/** 兜底错误文案：把后端的"操作异常"等无意义提示换成用户能看懂的话 */
function friendlyMessage(msg?: string | null): string {
  if (!msg) return '请求失败，请稍后重试'
  const meaningless = ['操作异常', '系统异常', '服务器内部错误', 'error', 'internal server error']
  if (meaningless.some(m => msg.toLowerCase().includes(m))) return '请求失败，请稍后重试'
  return msg
}

/**
 * 把后端返回的图片地址解析为可访问的完整 URL
 * - 已是 http(s) 直接返回
 * - 以 /api/file/ 开头的相对地址转成 /api/file/download?name=xxx 以避免 MVC 路由对文件名中的点做截断
 * - 其他以 / 开头的相对地址直接拼接 BASE_URL
 * - 逗号分隔的多图地址（如 /api/a.webp,/api/b.webp）只取第一张
 */
export function resolveUrl(url?: string | null): string {
  if (!url) return ''
  if (url.startsWith('http')) return url
  // coverImg 等字段可能存的是逗号分隔的多张图片，<image> 只能显示一张
  const first = url.split(',')[0].trim()
  if (first.startsWith('/api/file/')) {
    const fileName = first.slice('/api/file/'.length)
    return `${BASE_URL}/api/file/img?name=${encodeURIComponent(fileName)}`
  }
  return BASE_URL + first
}

export default http
