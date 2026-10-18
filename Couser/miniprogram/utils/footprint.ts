// 本地足迹（浏览记录，storage 持久化）

export interface FootprintItem {
  goodsId: string
  goodsName: string
  coverImg: string
  salePrice: number
  viewTime: number
}

const KEY = 'footprints'
const MAX = 50 // 最多保留最近 50 条

export function getFootprints(): FootprintItem[] {
  return wx.getStorageSync(KEY) || []
}

export function saveFootprints(list: FootprintItem[]) {
  wx.setStorageSync(KEY, list)
}

/** 记录一条足迹：去重并放在最前，超出上限移除最旧 */
export function addFootprint(goods: { id: string; goodsName: string; coverImg: string; salePrice?: number; price?: number }) {
  const list = getFootprints().filter(f => f.goodsId !== goods.id)
  list.unshift({
    goodsId: goods.id,
    goodsName: goods.goodsName,
    coverImg: goods.coverImg || '',
    salePrice: goods.salePrice || goods.price || 0,
    viewTime: Date.now()
  })
  if (list.length > MAX) list.length = MAX
  saveFootprints(list)
}

export function removeFootprint(goodsId: string) {
  const list = getFootprints().filter(f => f.goodsId !== goodsId)
  saveFootprints(list)
}

/** 批量删除（管理模式下使用） */
export function removeFootprints(ids: string[]) {
  if (!ids || !ids.length) return
  const set = new Set(ids)
  const list = getFootprints().filter(f => !set.has(f.goodsId))
  saveFootprints(list)
}

export function clearFootprints() {
  saveFootprints([])
}

export function footprintCount(): number {
  return getFootprints().length
}
