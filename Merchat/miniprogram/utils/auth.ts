import { request } from './request'
import { Merchant } from './types'

const TOKEN_KEY = 'auth_token'
const MERCHANT_KEY = 'merchant_info'

/** 地址页之间传递选中地址用的临时 key（切换商户后必须清理，否则会把上个商户的地址带过来） */
const PICK_ADDRESS_KEY = '__gys_pick_address'
const PICK_ADDRESS_RETURN_KEY = '__gys_pick_address_return'

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
  // 清理跨页面的地址传参残留
  wx.removeStorageSync(PICK_ADDRESS_KEY)
  wx.removeStorageSync(PICK_ADDRESS_RETURN_KEY)
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
  // 切换商户时清理上个商户遗留的地址传参
  wx.removeStorageSync(PICK_ADDRESS_KEY)
  wx.removeStorageSync(PICK_ADDRESS_RETURN_KEY)
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
