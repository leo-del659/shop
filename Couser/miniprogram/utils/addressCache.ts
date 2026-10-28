// 收货地址本地缓存（storage 持久化）
// 设计目标：退出登录后保留地址数据，再次登录时即时恢复、无需等待网络。
// 按登录账号隔离，避免同一设备切换账号时串数据。
//
// 历史 Bug：旧实现用 'address_list_' + wx.getStorageSync('mobile') 拼 key，
// 而登录写入的是 'loginMobile' → 后缀恒为空 → 所有账号共用一份地址。现统一走 userScope。
import { AddressItem } from '../api/address'
import { scopedKey, isLoggedIn } from './userScope'

function storageKey(): string {
  return scopedKey('address_list')
}

/** 读取本地缓存的地址列表（登出后保留的数据） */
export function getCachedAddresses(): AddressItem[] {
  return wx.getStorageSync(storageKey()) || []
}

/** 把最新地址列表写入本地缓存 */
export function cacheAddresses(list: AddressItem[]) {
  if (!isLoggedIn()) return // 未登录不缓存
  wx.setStorageSync(storageKey(), list || [])
}
