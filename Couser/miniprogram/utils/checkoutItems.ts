// 结算页临时勾选项（storage 传递数据）
// 按登录账号隔离：否则上一个账号勾选的商品会被带进新账号的结算页。
import { scopedKey } from './userScope'

const KEY_BASE = 'checkout_items'

function key(): string {
  return scopedKey(KEY_BASE)
}

/** 写入待结算商品 */
export function setCheckoutItems(items: any[]) {
  wx.setStorageSync(key(), items || [])
}

/** 读取待结算商品 */
export function getCheckoutItems<T = any>(): T[] {
  return (wx.getStorageSync(key()) || []) as T[]
}

/** 清空待结算商品（下单成功后调用） */
export function clearCheckoutItems() {
  wx.removeStorageSync(key())
}
