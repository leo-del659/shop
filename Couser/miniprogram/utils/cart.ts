// 本地购物车（storage 持久化）
import { GoodsItem } from '../api/shop'

export interface CartItem {
  goodsId: string
  goodsName: string
  coverImg: string
  price: number
  buyNum: number
  skuId?: string
  skuName?: string
  specName?: string
  /** 下单时锁定库存上限（来自商品/SKU 的 stock）。缺省 undefined 表示不限制（兼容历史购物车数据） */
  stock?: number
}

const KEY = 'cart'

/**
 * 购物车「去重键」：商品 + 规格。
 * 全局必须只用这一个规则，否则不同页面会对「同一行」产生不同定义，导致数量错乱。
 * skuName 缺省按空串处理，保证「无规格商品」与「带规格商品」不会混为同一行。
 */
function sameCartLine(c: CartItem, goodsId: string, skuName = ''): boolean {
  return c.goodsId === goodsId && (c.skuName || '') === (skuName || '')
}

export function getCart(): CartItem[] {
  return wx.getStorageSync(KEY) || []
}

/** 购物车 tab 在 app.json tabBar.list 中的索引（0:首页 1:分类 2:购物车 3:我的） */
const CART_TAB_INDEX = 2

export function saveCart(list: CartItem[]) {
  wx.setStorageSync(KEY, list)
  syncCartBadge()
}

/** 同步购物车数量到 tabBar 角标 */
export function syncCartBadge() {
  const count = cartCount()
  const setBadge = () => {
    if (count > 0) {
      wx.setTabBarBadge({
        index: CART_TAB_INDEX,
        text: String(count),
        fail: (err) => {
          console.warn('setTabBarBadge fail', err)
          // tabBar 未初始化时稍后重试
          if (err.errMsg && err.errMsg.includes('not ready')) {
            setTimeout(setBadge, 500)
          }
        }
      })
    } else {
      wx.removeTabBarBadge({
        index: CART_TAB_INDEX,
        fail: () => { /* 无角标时移除失败可忽略 */ }
      })
    }
  }
  setBadge()
}

/** 加入购物车，相同 商品+规格 累加数量 */
export function addToCart(item: CartItem) {
  const list = getCart()
  const idx = list.findIndex(c => sameCartLine(c, item.goodsId, item.skuName))
  if (idx >= 0) {
    const line = list[idx]
    // 以最新写入的库存为准（后端实时库存）
    if (item.stock !== undefined) line.stock = item.stock
    line.buyNum += item.buyNum
    // 累加后不得超过库存
    if (line.stock !== undefined && line.stock > 0 && line.buyNum > line.stock) {
      line.buyNum = line.stock
    }
  } else {
    list.push(item)
  }
  saveCart(list)
}

export function updateCart(list: CartItem[]) {
  saveCart(list)
}

export function clearCart() {
  saveCart([])
}

export function cartCount(): number {
  return getCart().reduce((sum, c) => sum + c.buyNum, 0)
}

/** 获取某商品在购物车中的数量（可按规格精确匹配） */
export function getCartQuantity(goodsId: string, skuName = ''): number {
  const item = getCart().find(c => sameCartLine(c, goodsId, skuName))
  return item ? item.buyNum : 0
}

/** 增加某商品购物车数量（+1），按 商品+规格 精确定位某一行 */
export function incCartItem(goodsId: string, skuName = ''): number {
  const list = getCart()
  const idx = list.findIndex(c => sameCartLine(c, goodsId, skuName))
  if (idx >= 0) {
    const line = list[idx]
    // 库存上限：达到库存则不再增加，提示库存不足
    if (line.stock !== undefined && line.stock > 0 && line.buyNum >= line.stock) {
      wx.showToast({ title: '库存不足', icon: 'none' })
      return line.buyNum
    }
    line.buyNum += 1
  } else {
    // 不在购物车中，不应走到这里（UI 应隐藏 stepper）
    list.push({ goodsId, goodsName: '', coverImg: '', price: 0, buyNum: 1, skuName })
  }
  saveCart(list)
  return list[idx >= 0 ? idx : list.length - 1].buyNum
}

/** 减号移除该商品对应的「这一行」（按 商品+规格 精确定位，不会误删其它规格行），返回 0 */
export function decCartItem(goodsId: string, skuName = ''): number {
  const list = getCart()
  const idx = list.findIndex(c => sameCartLine(c, goodsId, skuName))
  if (idx < 0) return 0
  list.splice(idx, 1)
  saveCart(list)
  return 0
}

/**
 * 由商品对象构造一个购物车项（取售价、兜底封面图）。
 * 统一 home / category / 搜索页的加购逻辑，避免三处重复写。
 */
export function buildCartItem(g: GoodsItem, buyNum = 1): CartItem {
  return {
    goodsId: g.id,
    goodsName: g.goodsName,
    coverImg: g.coverImg || '',
    price: g.salePrice || g.price,
    buyNum,
    stock: (g as any).stock
  }
}
