import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base'

/**
 * 获取商品分类树（用于商品表单选择分类）
 * 后端接口：/api/shop/cattree
 * 返回的 data 是树形结构：[{ id, categoryName, children: [...] }]
 */
export const get_category_tree = (): Promise<Api_Reseponse_Dto> => {
    return request({
        url: '/api/shop/cattree',
        method: 'get',
        params: { categoryId: '' }
    })
}
