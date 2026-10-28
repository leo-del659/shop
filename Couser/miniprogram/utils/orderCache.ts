// 订单本地缓存（storage 持久化）
// 设计目标：退出登录后保留订单数据（含历史翻页），再次登录时全量即时恢复。
// 按「登录账号 + 订单状态」隔离缓存；第 1 页整体替换，翻页时按 id 去重追加。
//
// 历史 Bug：旧实现用 'order_cache_' + wx.getStorageSync('mobile') 拼 key，
// 而登录写入的是 'loginMobile'，两者对不上 → 后缀恒为空 → 所有账号共用一份缓存，
// 表现为「重新登录后看到别人的订单，自己的订单却不显示」。现统一走 userScope。
import { OrderSummary } from '../api/order'
import { scopedKey, isLoggedIn } from './userScope'

type OrderCacheMap = Record<number, OrderSummary[]>

function storageKey(): string {
  return scopedKey('order_cache')
}

/** 读取某状态下的订单缓存（登出后保留的全量数据） */
export function getCachedOrders(status: number): OrderSummary[] {
  const map: OrderCacheMap = wx.getStorageSync(storageKey()) || {}
  return map[status] || []
}

/** 第 1 页整体替换缓存 */
export function cacheOrdersPage(status: number, list: OrderSummary[]) {
  if (!isLoggedIn()) return // 未登录不缓存
  const map: OrderCacheMap = wx.getStorageSync(storageKey()) || {}
  map[status] = list || []
  wx.setStorageSync(storageKey(), map)
}

/** 翻页时追加：按 id 去重，避免恢复缓存后再次拉取已缓存页造成重复 */
export function appendCachedOrders(status: number, list: OrderSummary[]) {
  if (!isLoggedIn()) return
  const map: OrderCacheMap = wx.getStorageSync(storageKey()) || {}
  const exist = map[status] || []
  const seen = new Set(exist.map(o => o.id))
  const merged = exist.concat((list || []).filter(o => !seen.has(o.id)))
  map[status] = merged
  wx.setStorageSync(storageKey(), map)
}
