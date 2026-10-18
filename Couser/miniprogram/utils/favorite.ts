// 本地收藏（storage 持久化）

export interface FavoriteItem {
  goodsId: string
  goodsName: string
  coverImg: string
  salePrice: number
  createTime: number
}

const KEY = 'favorites'

export function getFavorites(): FavoriteItem[] {
  return wx.getStorageSync(KEY) || []
}

export function saveFavorites(list: FavoriteItem[]) {
  wx.setStorageSync(KEY, list)
}

/** 是否已收藏 */
export function isFavorite(goodsId: string): boolean {
  return getFavorites().some(f => f.goodsId === goodsId)
}

/** 切换收藏状态：已收藏则取消，未收藏则加入 */
export function toggleFavorite(goods: { id: string; goodsName: string; coverImg: string; salePrice?: number; price?: number }): boolean {
  const list = getFavorites()
  const idx = list.findIndex(f => f.goodsId === goods.id)
  if (idx >= 0) {
    list.splice(idx, 1)
    saveFavorites(list)
    return false
  }
  list.unshift({
    goodsId: goods.id,
    goodsName: goods.goodsName,
    coverImg: goods.coverImg || '',
    salePrice: goods.salePrice || goods.price || 0,
    createTime: Date.now()
  })
  saveFavorites(list)
  return true
}

export function removeFavorite(goodsId: string) {
  const list = getFavorites().filter(f => f.goodsId !== goodsId)
  saveFavorites(list)
}

/** 批量删除（管理模式下使用） */
export function removeFavorites(ids: string[]) {
  if (!ids || !ids.length) return
  const set = new Set(ids)
  const list = getFavorites().filter(f => !set.has(f.goodsId))
  saveFavorites(list)
}

export function favoriteCount(): number {
  return getFavorites().length
}
