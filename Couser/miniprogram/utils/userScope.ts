// 用户作用域（User Scope）：把本地缓存按「登录账号」隔离
//
// 背景：同一台设备可能先后登录多个账号。若足迹 / 收藏 / 购物车 / 订单 / 地址等
//       都存在全局固定的 storage key，换号登录后就会把上一个账号的数据展示给新账号
//       （表现为「重新登录后看到别人的足迹，自己的订单却不显示」）。
//
// 隔离维度：登录手机号（唯一、稳定、可读）。
// 为什么不用 IP：同一 WiFi 下多个账号会互相串号，蜂窝网络 IP 还会变，不适合当主键。
//
// 存储 key 形如：footprints__13800138000
// 未登录时落在 guest 作用域；登录成功后把游客期间的数据并入该账号（见 adoptGuestCaches）。

/** 当前登录账号（退出登录时清除） */
export const LOGIN_MOBILE_KEY = 'loginMobile'
/** 缓存归属账号（退出登录时保留，重新登录同一账号可无缝恢复本地缓存） */
export const CACHE_OWNER_KEY = 'cacheOwner'
/** 游客（未登录）作用域 */
export const GUEST_USER = 'guest'

/** 需要按用户隔离的缓存基础 key */
export const USER_SCOPED_BASES = [
  'footprints',      // 足迹
  'favorites',       // 收藏
  'cart',            // 购物车
  'search_history',  // 搜索历史
  'order_cache',     // 订单缓存（按状态分组的 map）
  'address_list',    // 收货地址缓存
  'checkout_items',  // 结算页临时勾选项
]

/** 旧版本使用的全局 key（升级迁移用，迁移后删除，避免继续串数据） */
const LEGACY_KEYS = [
  'footprints',
  'favorites',
  'cart',
  'search_history',
  'checkout_items',
  'order_cache_',    // 旧实现：'order_cache_' + mobile
  'address_list_',   // 旧实现：'address_list_' + mobile
]

function isEmpty(v: any): boolean {
  return v === '' || v === undefined || v === null
}

/**
 * 当前缓存作用域的用户标识。
 * 优先用已登录账号；已退出但保留 cacheOwner 时仍指向最后登录的账号，
 * 这样「退出登录后订单缓存仍在，重新登录立刻恢复」的设计依然成立。
 */
export function currentUserId(): string {
  const mobile = wx.getStorageSync(LOGIN_MOBILE_KEY)
  if (mobile) return mobile
  const owner = wx.getStorageSync(CACHE_OWNER_KEY)
  if (owner) return owner
  return GUEST_USER
}

/** 生成带用户维度的 storage key */
export function scopedKey(base: string, userId?: string): string {
  return `${base}__${userId || currentUserId()}`
}

/** 标记某账号为当前缓存归属账号（登录成功时调用） */
export function markCacheOwner(userId: string) {
  wx.setStorageSync(CACHE_OWNER_KEY, userId)
}

/** 是否处于登录态（有 token 才算；退出登录后 token 已被清除） */
export function isLoggedIn(): boolean {
  return !!wx.getStorageSync('token')
}

/** 清空指定用户作用域下的全部缓存 */
export function clearUserCaches(userId?: string) {
  const uid = userId || currentUserId()
  USER_SCOPED_BASES.forEach(base => {
    wx.removeStorageSync(scopedKey(base, uid))
  })
}

/**
 * 登录成功后把游客期间的浏览数据（足迹 / 收藏 / 购物车 / 搜索历史）并入该账号。
 * 订单、地址、结算项不并入——它们属于明确的交易数据，必须来自服务端账号。
 */
export function adoptGuestCaches(userId: string) {
  const guestBases = ['footprints', 'favorites', 'cart', 'search_history']
  guestBases.forEach(base => {
    const guestKey = scopedKey(base, GUEST_USER)
    const guestVal = wx.getStorageSync(guestKey)
    if (isEmpty(guestVal)) return
    const ownKey = scopedKey(base, userId)
    const ownVal = wx.getStorageSync(ownKey)
    // 账号维度还没有数据时才并入，避免覆盖该账号已有的真实数据
    if (isEmpty(ownVal)) wx.setStorageSync(ownKey, guestVal)
    wx.removeStorageSync(guestKey)
  })
}

/**
 * 升级迁移：旧版全局 key 的数据并入当前账号，并删除旧 key。
 * 旧实现里订单/地址缓存用的是 'order_cache_' + mobile，而登录写入的是 'loginMobile'，
 * 两者对不上导致后缀恒为空、所有用户共用一份缓存 —— 这里一并修正。
 */
export function migrateLegacyCaches() {
  const uid = currentUserId()
  if (uid === GUEST_USER) return
  LEGACY_KEYS.forEach(key => {
    const val = wx.getStorageSync(key)
    if (isEmpty(val)) return
    // 'order_cache_' / 'address_list_' 对应新的 'order_cache' / 'address_list'
    const base = key.endsWith('_') ? key.slice(0, -1) : key
    if (!USER_SCOPED_BASES.includes(base)) return
    const ownKey = scopedKey(base, uid)
    const ownVal = wx.getStorageSync(ownKey)
    if (isEmpty(ownVal)) wx.setStorageSync(ownKey, val)
    wx.removeStorageSync(key)
  })
}
