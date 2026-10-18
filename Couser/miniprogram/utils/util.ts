/**
 * 将对象/数组中所有键名从 PascalCase 转为 camelCase（递归）
 * 解决后端 C# 实体默认返回 PascalCase、前端期望 camelCase 的兼容问题
 */
export function camelCaseKeys<T = any>(data: T): T {
  if (data === null || data === undefined) return data
  if (Array.isArray(data)) return data.map(item => camelCaseKeys(item)) as unknown as T
  if (typeof data !== 'object') return data

  const obj = data as Record<string, any>
  const result: Record<string, any> = {}
  for (const key of Object.keys(obj)) {
    // 首字母小写：ImgUrl → imgUrl, GoodsName → goodsName
    const camelKey = key.charAt(0).toLowerCase() + key.slice(1)
    result[camelKey] = camelCaseKeys(obj[key])
  }
  return result as T
}

/**
 * 把商品已售数量格式化为营销展示文案：
 *   几十 → 30+，几百 → 300+，几千 → 3千+，几万 → 3万+，几十万及以上 → 30万+
 * 规则：按数量级四舍五入保留 1 位有效数字，并追加 “+”（表示“至少这么多”）。
 */
export function formatSoldCount(input: number | string | null | undefined): string {
  const num = typeof input === 'number'
    ? input
    : parseInt(String(input ?? '').trim(), 10)

  if (!num || num <= 0) return '0'

  const round1 = (x: number): number => {
    if (x <= 0) return 0
    const mag = Math.pow(10, Math.floor(Math.log10(x)))
    return Math.round(x / mag) * mag
  }

  if (num < 1000) {
    return `${round1(num)}+`
  }
  if (num < 10000) {
    return `${round1(num / 1000)}千+`
  }
  return `${round1(num / 10000)}万+`
}
