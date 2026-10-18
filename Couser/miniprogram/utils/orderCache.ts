// 订单本地缓存（storage 持久化）
// 设计目标：退出登录后保留订单数据（含历史翻页），再次登录时全量即时恢复。
// 按「手机号 + 订单状态」隔离缓存；第 1 页整体替换，翻页时按 id 去重追加。
import { OrderSummary } from '../api/order'

type OrderCacheMap = Record<number, OrderSummary[]>

function storageKey(): string {
  const mobile = wx.getStorageSync('mobile') || ''
  return 'order_cache_' + mobile
}

/** 读取某状态下的订单缓存（登出后保留的全量数据） */
export function getCachedOrders(status: number): OrderSummary[] {
  const map: OrderCacheMap = wx.getStorageSync(storageKey()) || {}
  return map[status] || []
}

/** 第 1 页整体替换缓存 */
export function cacheOrdersPage(status: number, list: OrderSummary[]) {
  if (!wx.getStorageSync('mobile')) return // 未登录不缓存
  const map: OrderCacheMap = wx.getStorageSync(storageKey()) || {}
  map[status] = list || []
  wx.setStorageSync(storageKey(), map)
}

/** 翻页时追加：按 id 去重，避免恢复缓存后再次拉取已缓存页造成重复 */
export function appendCachedOrders(status: number, list: OrderSummary[]) {
  if (!wx.getStorageSync('mobile')) return
  const map: OrderCacheMap = wx.getStorageSync(storageKey()) || {}
  const exist = map[status] || []
  const seen = new Set(exist.map(o => o.id))
  const merged = exist.concat((list || []).filter(o => !seen.has(o.id)))
  map[status] = merged
  wx.setStorageSync(storageKey(), map)
}
