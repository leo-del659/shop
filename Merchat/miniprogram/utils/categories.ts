// 商品分类常量（与 goods_category 表数据保持一致，前端筛选/新增共用）
export interface Category {
  id: string
  name: string
}

export const CATEGORIES: Category[] = [
  { id: 'cat00101', name: '时令水果' },
  { id: 'cat00102', name: '新鲜蔬菜' },
  { id: 'cat00103', name: '进口水果' },
  { id: 'cat00201', name: '猪肉牛肉' },
  { id: 'cat00202', name: '鸡鸭禽肉' },
  { id: 'cat00203', name: '海鲜水产' },
  { id: 'cat00204', name: '禽蛋豆制品' },
  { id: 'cat00205', name: '鲜奶乳品' },
  { id: 'cat00301', name: '坚果炒货' },
  { id: 'cat00302', name: '饼干糕点' },
  { id: 'cat00303', name: '糖果巧克力' },
  { id: 'cat00304', name: '饮品果汁' },
  { id: 'cat00401', name: '米面杂粮' },
  { id: 'cat00402', name: '食用油脂' },
  { id: 'cat00403', name: '调味调料' },
  { id: 'cat00501', name: '清洁洗护' },
  { id: 'cat00502', name: '纸品日用' },
  { id: 'cat00601', name: '中式熟食' },
  { id: 'cat00602', name: '烘焙面点' },
]
