// 商城（商品浏览）接口
import http from '../utils/request'

export interface GoodsItem {
  id: string
  categoryId: string
  goodsName: string
  price: number
  salePrice: number
  stock: number
  coverImg: string
  unit?: string
  spec?: string
  imgList?: string
}

export interface CategoryNode {
  id: string
  parentId?: string
  categoryName: string
  icon?: string
  sort?: number
  isBan?: number
  children: CategoryNode[]
  goods: GoodsItem[]
}

/** 首页：分类树（每个分类附带商品） */
export function getHome() {
  return http.get<CategoryNode[]>('/api/shop/home')
}

/**
 * 分类商品树（用于分类详情页的树状卡片 + 筛选）
 * 不传 categoryId 时返回完整分类树；传入时返回该分类（含子分类）的子树
 * 返回结构：数组。传 categoryId 时为长度 1 的数组（根节点）；不传时为一级分类数组
 */
export function getCategoryTree(categoryId?: string) {
  return http.get<CategoryNode[]>('/api/shop/cattree', categoryId ? { categoryId } : undefined)
}

/** 商品列表（仅已上架），支持分类/关键词/排序分页 */
export function getGoodsList(params: {
  categoryId?: string
  keyword?: string
  sort?: 'price_asc' | 'price_desc' | ''
  page?: number
  size?: number
}) {
  return http.get<{ total: number; list: GoodsItem[] }>('/api/shop/goods', params)
}

/** 商品详情：主信息 + 图片 + SKU + 规格 */
export function getGoodsDetail(id: string) {
  return http.get<{
    good: any
    images: { id: string; imgUrl: string; sort: number }[]
    skus: { id: string; specId: string; skuName: string; price: number; stock: number }[]
    specs: { id: string; specName: string }[]
  }>('/api/shop/goods/' + id)
}
