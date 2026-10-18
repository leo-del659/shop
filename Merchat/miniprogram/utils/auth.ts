import { request } from './request'
import { Merchant } from './types'

const TOKEN_KEY = 'auth_token'
const MERCHANT_KEY = 'merchant_info'

export type Role = 'merchant'

export function getToken(): string {
  return wx.getStorageSync(TOKEN_KEY) || ''
}

export function setToken(t: string): void {
  wx.setStorageSync(TOKEN_KEY, t)
}

export function clearAuth(): void {
  wx.removeStorageSync(TOKEN_KEY)
  wx.removeStorageSync(MERCHANT_KEY)
}

export function isLogin(): boolean {
  return !!getToken()
}

// 当前登录角色：仅商户端
export function getCurrentRole(): Role | null {
  if (getLocalMerchant() && (getLocalMerchant() as any).id) return 'merchant'
  return null
}

// 手机号 + 密码登录
export async function accountLogin(
  account: string,
  password: string,
): Promise<{ token: string; merchant: Merchant }> {
  const res = await request<{ token: string; merchant: Merchant }>({
    path: '/api/merchant/login',
    method: 'POST',
    data: { account, password },
  })
  setToken(res.token)
  if (res.merchant) setLocalMerchant(res.merchant)
  return res
}

// 发送短信验证码（type: 1-商户注册）。开发环境后端直接回传验证码，便于联调。
export async function sendSmsCode(mobile: string, type: number = 1): Promise<string> {
  const res = await request<{ mobile: string; code: string }>({
    path: '/api/Sms/Send',
    method: 'POST',
    data: { mobile, type, ip: '' },
  })
  return res.code || ''
}

// 手机号 + 验证码 + 密码注册，成功后返回登录页自行登录
export async function registerMerchant(
  phone: string,
  code: string,
  password: string,
): Promise<void> {
  await request<{ token?: string; merchant?: Merchant }>({
    path: '/api/merchant/register',
    method: 'POST',
    data: { phone, code, password },
  })
}

export function getLocalMerchant(): Merchant | null {
  return wx.getStorageSync(MERCHANT_KEY) || null
}

export function setLocalMerchant(s: Merchant): void {
  wx.setStorageSync(MERCHANT_KEY, s)
}

export function getCurrentUser(): Merchant | null {
  if (getLocalMerchant() && (getLocalMerchant() as any).id) return getLocalMerchant()
  return null
}

export function logout(): void {
  clearAuth()
}
