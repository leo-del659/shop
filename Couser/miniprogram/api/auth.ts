// 认证相关接口
import http from '../utils/request'

/**
 * 发送短信验证码（模拟接收：开发环境后端直接把验证码返回，前端弹窗展示）
 * 后端路由：POST /api/Sms/Send
 * 响应：{ mobile, code }（code 仅开发环境返回，生产由短信网关下发）
 */
export function sendSmsCode(mobile: string) {
  return http.post<{ mobile: string; code?: string }>(
    '/api/Sms/Send',
    { mobile, type: 3 },
    false
  )
}

/**
 * 消费者手机号 + 验证码登录
 * 后端路由：POST /api/Login/Login
 * 响应：{ token, code }（code 为登录凭据）
 */
export function smsLogin(mobile: string, code: string) {
  return http.post<{ token: string; code: string }>(
    '/api/Login/Login',
    { mobile, code },
    false
  )
}

/**
 * 消费者手机号 + 验证码注册（未注册手机号首次登录时调用，注册后自动登录）
 * 后端路由：POST /api/Login/Register
 * 响应：{ token, code }
 */
export function registerBySms(mobile: string, code: string) {
  return http.post<{ token: string; code: string }>(
    '/api/Login/Register',
    { mobile, code },
    false
  )
}

/**
 * 获取当前登录消费者的用户资料（client_login 表）
 * 后端路由：GET /api/Login/Client_Profile
 * 响应：{ account, name, avatar }
 */
export function getClientProfile() {
  return http.get<{ account: string; name: string; avatar: string }>(
    '/api/Login/Client_Profile'
  )
}

/**
 * 更新用户名称 / 头像（写入 client_login 表）
 * 后端路由：POST /api/Login/Update_Client_Profile
 * 请求体：{ name?: string, avatar?: string }（null/不传不修改，avatar 传 "" 清除）
 * 响应：{ account, name, avatar }
 */
export function updateClientProfile(data: { name?: string; avatar?: string }) {
  return http.post<{ account: string; name: string; avatar: string }>(
    '/api/Login/Update_Client_Profile',
    data
  )
}
