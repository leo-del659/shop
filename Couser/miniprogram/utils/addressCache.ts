// 收货地址本地缓存（storage 持久化）
// 设计目标：退出登录后保留地址数据，再次登录时即时恢复、无需等待网络。
// 缓存按手机号隔离，避免同一设备切换账号时串数据。
import { AddressItem } from '../api/address'

function storageKey(): string {
  const mobile = wx.getStorageSync('mobile') || ''
  return 'address_list_' + mobile
}

/** 读取本地缓存的地址列表（登出后保留的数据） */
export function getCachedAddresses(): AddressItem[] {
  return wx.getStorageSync(storageKey()) || []
}

/** 把最新地址列表写入本地缓存 */
export function cacheAddresses(list: AddressItem[]) {
  if (!wx.getStorageSync('mobile')) return // 未登录不缓存
  wx.setStorageSync(storageKey(), list || [])
}
