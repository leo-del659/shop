import { resolveUrl } from './request'
import { CategoryNode, GoodsItem } from '../api/shop'

/**
 * 把后端分类树（含分类图标、商品封面）里的相对图片地址解析为完整可访问 URL。
 * 集中在这一处处理，避免 home / category / 搜索页各自写一遍。
 */
export function resolveCategoryTree(raw: CategoryNode[]): CategoryNode[] {
  return raw.map((c: CategoryNode) => ({
    ...c,
    icon: resolveUrl(c.icon),
    children: (c.children || []).map((sub: any) => ({
      ...sub,
      goods: (sub.goods || []).map((g: GoodsItem) => ({
        ...g,
        coverImg: resolveUrl(g.coverImg)
      }))
    })),
    goods: (c.goods || []).map((g: GoodsItem) => ({
      ...g,
      coverImg: resolveUrl(g.coverImg)
    }))
  }))
}

/**
 * 首页 / 分类页共用的展示分类图片（本地静态资源）。
 */
export const CATEGORY_IMAGES: Record<string, string> = {
  '新鲜水果': '/images/category/fruit.jpg',
  '时令蔬菜': '/images/category/vegetable.webp',
  '肉禽生鲜': '/images/category/meat.webp',
  '蛋品蛋类': '/images/category/egg.jpg',
  '牛奶乳品': '/images/category/milk.jpg',
  '休闲零食': '/images/category/snack.jpg',
  '酒水饮料': '/images/category/drink.jpg',
  '粮油调味': '/images/category/grain-oil.webp',
  '日用百货': '/images/category/daily-goods.jpg',
  '熟食面点': '/images/category/cooked-food.jpg',
}

/** 根据分类名称匹配 Emoji 图标（兜底用） */
export function categoryEmoji(name: string): string {
  const map: Record<string, string> = {
    '果蔬': '🥬', '水果': '🍎', '蔬菜': '🥦', '肉禽': '🥩', '蛋奶': '🥛',
    '猪肉': '🐷', '牛肉': '🥩', '鸡鸭': '🍗', '海鲜': '🦐', '禽蛋': '🥚',
    '蛋': '🥚', '奶': '🥛', '鲜奶': '🥛', '零食': '🍿', '饮料': '🧃',
    '坚果': '🥜', '饼干': '🍪', '糖果': '🍫', '巧克力': '🍫', '饮品': '🧋',
    '粮油': '🌾', '米面': '🍚', '油脂': '🫒', '调味': '🧂',
    '日用': '🧹', '清洁': '🧽', '纸品': '🧻',
    '熟食': '🍱', '烘焙': '🍞', '面点': '🥟', '进口': '🌍', '时令': '🌸',
  }
  for (const [key, emoji] of Object.entries(map)) {
    if (name.includes(key)) return emoji
  }
  return name[0] || '📁'
}

/**
 * 组合类目拆分配置：
 * 一级分类 -> 展示分类列表，每个展示分类再映射到若干后端子分类名。
 * 用于首页宫格和分类页左侧导航统一展示水果/蔬菜/肉禽/蛋/奶/零食/饮料。
 */
export interface DisplaySplitItem { label: string; subNames: string[] }
export const DISPLAY_CATEGORY_GROUPS: Record<string, DisplaySplitItem[]> = {
  '新鲜果蔬': [
    { label: '新鲜水果', subNames: ['时令水果', '进口水果'] },
    { label: '时令蔬菜', subNames: ['新鲜蔬菜'] }
  ],
  '肉禽蛋奶': [
    { label: '肉禽生鲜', subNames: ['猪肉牛肉', '鸡鸭禽肉', '海鲜水产'] },
    { label: '蛋品蛋类', subNames: ['禽蛋豆制品'] },
    { label: '牛奶乳品', subNames: ['鲜奶乳品'] }
  ],
  '零食饮料': [
    { label: '休闲零食', subNames: ['坚果炒货', '饼干糕点', '糖果巧克力'] },
    { label: '酒水饮料', subNames: ['饮品果汁'] }
  ]
}

/** 展示分类节点：在后端 CategoryNode 基础上补充聚合信息 */
export interface DisplayCategory extends CategoryNode {
  /** 该展示分类聚合了哪些后端子分类 id */
  sourceIds: string[]
  /** 当首页/分类页跳转时，默认指向的第一个子分类 id */
  defaultTargetId: string
  /** 无自定义图标时的 emoji 兜底 */
  emoji?: string
}

/**
 * 把后端分类树归一化成前端展示分类树：
 * 1）将「新鲜果蔬/肉禽蛋奶/零食饮料」按 DISPLAY_CATEGORY_GROUPS 拆成展示小类；
 * 2）其余一级类目保持原样；
 * 3）每个展示分类的 children 为其聚合到的后端子分类，goods 为子分类商品聚合。
 */
export function normalizeDisplayCategories(categories: CategoryNode[]): DisplayCategory[] {
  const result: DisplayCategory[] = []

  for (const c of categories) {
    const group = DISPLAY_CATEGORY_GROUPS[c.categoryName || '']
    if (group) {
      for (const item of group) {
        const matchedSubs = (c.children || []).filter(sub => item.subNames.includes(sub.categoryName))
        if (!matchedSubs.length) continue

        const allGoods: GoodsItem[] = []
        for (const sub of matchedSubs) {
          if (sub.goods && sub.goods.length) allGoods.push(...sub.goods)
        }

        result.push({
          id: item.label,
          parentId: c.id,
          categoryName: item.label,
          icon: CATEGORY_IMAGES[item.label] || c.icon || '',
          emoji: categoryEmoji(item.label),
          sort: c.sort,
          isBan: c.isBan,
          children: matchedSubs,
          goods: allGoods,
          sourceIds: matchedSubs.map(s => s.id),
          defaultTargetId: matchedSubs[0].id
        })
      }
    } else {
      // 未拆分的普通一级类目：聚合自身 + 所有子分类的商品，保证「全部」能展示完整列表
      const allGoods: GoodsItem[] = []
      if (c.goods && c.goods.length) allGoods.push(...c.goods)
      for (const sub of c.children || []) {
        if (sub.goods && sub.goods.length) allGoods.push(...sub.goods)
      }
      result.push({
        ...c,
        icon: CATEGORY_IMAGES[c.categoryName || ''] || c.icon || '',
        emoji: categoryEmoji(c.categoryName || ''),
        goods: allGoods,
        sourceIds: (c.children || []).map(s => s.id),
        defaultTargetId: c.children && c.children.length ? c.children[0].id : c.id
      })
    }
  }

  return result
}
