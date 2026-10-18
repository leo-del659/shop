<template>
    <div class="l-container">
        <el-input v-model="key" placeholder="商品名称" style="max-width:240px" clearable @keyup.enter="bind" @clear="bind" />
        <el-button type="primary" @click="bind">查询</el-button>
        <el-button type="success" @click="open_add">新增商品</el-button>
        <!-- 卡片网格：封面图 + 名称 + 分类/商户 + 价格/库存 + 审核状态 + 操作 -->
        <div class="goods-grid" ref="grid_el">
            <div v-for="row in list" :key="row.id" class="goods-card">
                <div class="gc-cover" @click="preview_image(first_cover_url(row.coverImg))">
                    <img v-if="first_cover_url(row.coverImg)" :src="first_cover_url(row.coverImg)" class="gc-cover-img" />
                    <div v-else class="gc-cover-empty">无图</div>
                    <span class="gc-sale-badge" :class="row.isSale === 1 ? 'on' : 'off'">{{ row.isSale === 1 ? '已上架' : '未上架' }}</span>
                </div>
                <div class="gc-body">
                    <div class="gc-name" :title="row.goodsName">{{ row.goodsName }}</div>
                    <div class="gc-meta">
                        <span class="gc-tag">分类：{{ cat_name(row.categoryId) }}</span>
                        <span class="gc-tag">商户：{{ row.merchantName || merchant_name(row.merchantId) || '-' }}</span>
                    </div>
                    <div class="gc-price">
                        <span class="gc-price-now">¥{{ row.salePrice }}</span>
                        <span class="gc-price-old">¥{{ row.price }}</span>
                        <span class="gc-unit">{{ row.unit }}</span>
                        <span class="gc-stock">库存 {{ row.stock }}</span>
                    </div>
                    <div class="gc-status">
                        <el-tag :type="audit_tag(row.auditStatus)" size="small">{{ audit_text(row.auditStatus) }}</el-tag>
                        <span class="gc-time">{{ format_date(row.createTime) }}</span>
                    </div>
                    <div class="gc-actions">
                        <el-button size="small" :type="row.isSale === 1 ? 'info' : 'success'" :loading="sale_loading[row.id]" @click="on_toggle_sale(row)">{{ row.isSale === 1 ? '下架' : '上架' }}</el-button>
                        <el-button size="small" plain @click="open_edit(row)">编辑</el-button>
                        <!-- 已通过(1) 和待审核(0) 禁用；未提交(null) / 已驳回(2) 可提交 -->
                        <el-tooltip content="提交审核" placement="top">
                            <el-button size="small" type="warning" plain :disabled="row.auditStatus === 0 || row.auditStatus === 1" :loading="submit_loading[String(row.id)]" @click="on_submit(row)">送审</el-button>
                        </el-tooltip>
                        <el-button size="small" type="danger" plain @click="on_del(row)">删除</el-button>
                    </div>
                </div>
            </div>
            <div v-if="!list.length" class="goods-empty">暂无数据</div>
        </div>
        <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />

        <el-dialog v-model="show" :title="form.id ? '编辑商品' : '新增商品'" width="520px" top="3vh">
            <el-form ref="formRef" :model="form" :rules="rules" label-width="90px" class="goods-form">
                <el-form-item label="商品名称" prop="goodsName"><el-input v-model="form.goodsName" /></el-form-item>
                <el-form-item label="分类" prop="categoryId">
                    <el-tree-select v-model="form.categoryId" :data="category_options" :props="{ label: 'categoryName', value: 'id', children: 'children' }" check-strictly clearable placeholder="选择分类（可搜索）" filterable style="width:100%" />
                </el-form-item>
                <el-form-item label="商户" prop="merchantId">
                    <el-select v-model="form.merchantId" filterable clearable placeholder="选择商户（可搜索）" style="width:100%" :disabled="is_edit">
                        <el-option v-for="s in merchant_options" :key="s.id" :label="s.merchantName" :value="s.id" />
                    </el-select>
                </el-form-item>
                <el-form-item label="价格" prop="price"><el-input-number v-model="form.price" :precision="2" /></el-form-item>
                <el-form-item label="售价" prop="salePrice"><el-input-number v-model="form.salePrice" :precision="2" /></el-form-item>
                <el-form-item label="库存" prop="stock">
                    <template v-if="has_skus">
                        <div class="stock-readonly">
                            <span class="stock-num">{{ total_stock }}</span>
                            <span class="stock-hint"></span>
                        </div>
                    </template>
                    <el-input-number v-else v-model="form.stock" :min="0" placeholder="单规格商品填总库存" :disabled="is_edit" />
                </el-form-item>
                <el-form-item label="单位" prop="unit"><el-input v-model="form.unit" /></el-form-item>
                <el-form-item label="封面图" class="full">
                    <div class="cover-list">
                        <div v-for="(url, i) in cover_urls" :key="url + i" class="cover-preview">
                            <img :src="url" class="cover-img" />
                            <div class="cover-actions">
                                <el-button size="small" type="danger" plain @click="remove_cover(Number(i))">删除</el-button>
                            </div>
                        </div>
                        <el-upload :http-request="upload_cover" :show-file-list="false" accept="image/*" :before-upload="before_upload_cover" multiple>
                            <el-icon class="cover-uploader-trigger"><Plus /></el-icon>
                        </el-upload>
                    </div>
                    <div style="font-size:12px;color:#999;margin-top:4px">支持 png/jpg/jpeg/gif/webp/bmp，≤ 5MB，可上传多张</div>
                </el-form-item>
                <el-divider>多规格（每种规格不同价格）</el-divider>
                <!-- 列名说明 -->
                <div class="sku-header">
                    <div class="sku-col sku-col-name">规格值</div>
                    <div class="sku-col sku-col-price">
                        价格
                        <el-tooltip content="该规格的独立售价，买家选择该规格时按此价格成交" placement="top">
                            <el-icon class="sku-help"><QuestionFilled /></el-icon>
                        </el-tooltip>
                    </div>
                    <div class="sku-col sku-col-stock">
                        库存
                        <el-tooltip content="该规格的独立库存，与商品总库存分开计算" placement="top">
                            <el-icon class="sku-help"><QuestionFilled /></el-icon>
                        </el-tooltip>
                    </div>
                    <div class="sku-col sku-col-op"></div>
                </div>
                <div v-for="(s, i) in form.skus" :key="i" class="sku-row">
                    <el-input v-model="s.skuName" placeholder="如：250g/罐" class="sku-col sku-col-name" />
                    <el-input-number v-model="s.price" :precision="2" placeholder="售价" class="sku-col sku-col-price" :controls-position="'right'" />
                    <el-input-number v-model="s.stock" :min="0" placeholder="库存" class="sku-col sku-col-stock" :controls-position="'right'" />
                    <el-button type="danger" circle class="sku-col sku-col-op" @click="del_sku(Number(i))">
                        <el-icon><Delete /></el-icon>
                    </el-button>
                </div>
                <el-button type="primary" plain @click="add_sku">
                    <el-icon><Plus /></el-icon><span>添加规格</span>
                </el-button>
            </el-form>
            <template #footer>
                <el-button @click="show = false">取消</el-button>
                <el-button type="primary" @click="save">保存</el-button>
            </template>
        </el-dialog>

        <!-- 图片预览：点击商品缩略图时打开 -->
        <el-image-viewer
            v-if="show_preview"
            :url-list="preview_urls"
            :initial-index="preview_index"
            @close="show_preview = false"
        />
    </div>
</template>

<script setup lang="ts">
import MyPagination from '@/components/common/MyPagination.vue'
import { get_category_tree } from '@/api/category'
import { upload_file } from '@/api/file'
import { add_goods, get_goods, get_goods_list, remove_goods, set_goods_sale, submit_audit, update_goods } from '@/api/goods'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { format_date } from '@/utils/format'
import { Delete, Plus, QuestionFilled } from '@element-plus/icons-vue'
import { ElImageViewer, ElMessage, ElMessageBox } from 'element-plus'
import { computed, nextTick, onBeforeUnmount, onMounted, reactive, ref } from 'vue'

const list = ref<any[]>([])
const key = ref('')
const pages = reactive({ page: 1, size: 10, total: 0 })
const show = ref(false)
const form = ref<any>({})
const formRef = ref<any>(null)
const submit_loading = ref<Record<string, boolean>>({})
const sale_loading = ref<Record<string, boolean>>({})
const rules = {
    goodsName: [{ required: true, message: '请输入商品名称', trigger: 'blur' }],
    categoryId: [{ required: true, message: '请选择分类', trigger: 'change' }],
    merchantId: [{ required: true, message: '请选择商户', trigger: 'change' }],
    price: [
        { required: true, message: '请输入价格', trigger: 'blur' },
        { type: 'number', min: 0, message: '价格不能小于0', trigger: 'blur' }
    ],
    salePrice: [
        { required: true, message: '请输入售价', trigger: 'blur' },
        { type: 'number', min: 0, message: '售价不能小于0', trigger: 'blur' }
    ],
    unit: [{ required: true, message: '请输入单位', trigger: 'blur' }],
    stock: [
        { required: true, message: '请输入库存', trigger: 'blur' },
        { type: 'number', min: 0, message: '库存不能小于0', trigger: 'blur' }
    ]
}

// 图片预览
const show_preview = ref(false)
const preview_urls = ref<string[]>([])
const preview_index = ref(0)
const preview_image = (url: string) => {
    preview_urls.value = [url]
    preview_index.value = 0
    show_preview.value = true
}
// 把相对图片路径转成可访问的完整 URL。
// 所有本地相对路径（/api/file/、/uploads/、/images/ 等）统一走后端文件服务 API_BASE，
// 由后端 FileController 的 GET /api/file/{name} 提供静态文件（含 GYS images/products 兜底目录）
const resolve_img_url = (raw: string): string => {
    if (!raw) return ''
    if (raw.startsWith('http')) return raw
    const path = raw.startsWith('/') ? raw : '/' + raw
    return `${API_BASE}${path}`.replace(/([^:])\/\//g, '$1/')
}
// 取商品第一张封面图完整 URL（coverImg 兼容逗号分隔字符串 / 数组 / null）
const first_cover_url = (coverImg: any): string => {
    if (!coverImg) return ''
    const arr = Array.isArray(coverImg) ? coverImg : String(coverImg).split(',').map(s => s.trim()).filter(Boolean)
    const first = arr[0]
    if (!first) return ''
    return resolve_img_url(first)
}

// 编辑态（有 id）时「商户」「库存」只读，不允许在商品页改动
const is_edit = computed(() => !!form.value.id)
// 是否有有效多规格（用于决定「总库存」是否自动汇总而非手填）
const has_skus = computed(() => {
    const skus = form.value.skus || []
    return skus.length > 0 && skus.some((s: any) => s.skuName)
})
// 总库存 = 各规格库存之和（有规格时）；单规格时退回手填的 stock
const total_stock = computed(() => {
    if (!has_skus.value) return Number(form.value.stock) || 0
    return (form.value.skus || [])
        .filter((s: any) => s.skuName)
        .reduce((a: number, s: any) => a + (Number(s.stock) || 0), 0)
})

// 分类树（懒加载，打开对话框时拉一次）
const category_options = ref<any[]>([])
// 分类 ID → 名称 映射（列表页渲染分类列用，递归扁平化整棵树）
const category_map = ref<Record<string, string>>({})
// 商户列表（懒加载，打开对话框时拉一次）
const merchant_options = ref<any[]>([])
// 商户 ID → 名称 映射（列表页渲染商户列用）
const merchant_map = ref<Record<string, string>>({})

// 后端 baseURL（用于拼接图片等相对路径，trim 末尾 / 避免拼出 //）
const API_BASE = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '')
// 封面图 URL 列表（coverImgs 为数组；兼容老数据 coverImg 逗号分隔字符串）
const cover_urls = computed(() => {
    const arr = form.value.coverImgs || []
    return arr.filter(Boolean).map((c: string) => resolve_img_url(c))
})
const remove_cover = (i: number) => { form.value.coverImgs.splice(i, 1) }

// 列表用：ID → 名称 查不到时回退显示 ID 文本，避免空白
const cat_name = (id: any) => id ? (category_map.value[String(id)] || id) : '-'
const merchant_name = (id: any) => id ? (merchant_map.value[String(id)] || id) : ''

// 递归把分类树拍扁成 {id: name} 映射（支持任意层级）
const flatten_category = (nodes: any[], out: Record<string, string>) => {
    for (const n of nodes || []) {
        if (n && n.id != null) out[String(n.id)] = n.categoryName ?? ''
        if (n && Array.isArray(n.children) && n.children.length) flatten_category(n.children, out)
    }
}

const load_category_options = async () => {
    if (category_options.value.length) return
    const res = await get_category_tree()
    if (res.code === Api_Code.ok) {
        category_options.value = res.data || []
        // 同步扁平化一份 map 供列表渲染
        const map: Record<string, string> = {}
        flatten_category(category_options.value, map)
        category_map.value = map
    }
}
const load_merchant_options = async () => {
    if (merchant_options.value.length) return
    const { get_merchant_list } = await import('@/api/merchant')
    const r = await get_merchant_list({ page: 1, size: 1000 })
    if (r.code === Api_Code.ok) {
        merchant_options.value = r.data?.list || []
        const map: Record<string, string> = {}
        for (const s of merchant_options.value) if (s && s.id != null) map[String(s.id)] = s.merchantName ?? ''
        merchant_map.value = map
    }
}

// 自定义上传：走我们的 axios 拦截器（自动带 token），成功后追加到 coverImgs
const upload_cover = async (opt: any) => {
    open_loading()
    const res = await upload_file(opt.file)
    close_loading()
    if (res.code === 0) {
        if (!form.value.coverImgs) form.value.coverImgs = []
        form.value.coverImgs.push(res.data.url)
        opt.onSuccess(res)
    } else {
        ElMessage.error(res.message || '上传失败')
        opt.onError(new Error(res.message))
    }
}
// 上传前校验：5MB 内 + 必须是图片
const before_upload_cover = (file: File) => {
    if (file.size > 5 * 1024 * 1024) { ElMessage.warning('图片不能超过 5MB'); return false }
    if (!file.type.startsWith('image/')) { ElMessage.warning('请选择图片文件'); return false }
    return true
}

const audit_text = (v: number | null | undefined) =>
    v == null ? '未提交' : v === 0 ? '待审核' : v === 1 ? '已通过' : v === 2 ? '已拒绝' : '未知'
const audit_tag = (v: number | null | undefined) =>
    v == null ? 'info' : v === 0 ? 'warning' : v === 1 ? 'success' : v === 2 ? 'danger' : ''

const bind = async () => {
    open_loading()
    const res = await get_goods_list({ page: pages.page, size: pages.size, keyword: key.value || undefined })
    close_loading()
    if (res.code === Api_Code.ok) {
        list.value = res.data.list
        pages.total = res.data.total
    }
}

/* ============================================================
 * 让每页条数随卡片列数动态调整
 * 目的：卡片网格按「列数 × 整数行」铺满，避免最后一行剩 1~2 张
 *      造成右侧大片空白（CSS 无法感知末行剩余卡片数，只能从条数入手）
 * 做法：ResizeObserver 监听网格宽度 -> 推算列数 -> 每页 = 列数 × 2 行
 * ============================================================ */
const grid_el = ref<HTMLElement | null>(null)
// 与 .goods-grid 的 CSS 保持一致：minmax(260px, 1fr) + gap 16px
const CARD_MIN_W = 260
const GRID_GAP = 16
// 每个视口下希望铺满的行数（2 行在 1080p 下最接近一屏）
const ROWS_PER_PAGE = 2

// 由容器宽度推算 CSS Grid 实际会排出的列数（与 auto-fill 算法一致）
const calc_cols = (width: number): number => {
    if (!width || width < CARD_MIN_W) return 1
    return Math.max(1, Math.floor((width + GRID_GAP) / (CARD_MIN_W + GRID_GAP)))
}

// 依据当前列数计算「无空洞」的每页条数，并做去重避免抖动
const sync_page_size = () => {
    const el = grid_el.value
    if (!el) return
    const cols = calc_cols(el.clientWidth)
    const want = cols * ROWS_PER_PAGE
    if (want > 0 && want !== pages.size) {
        pages.size = want
        // 换页数后当前页可能越界，回到第 1 页重新拉取
        pages.page = 1
        bind()
    }
}

let grid_observer: ResizeObserver | null = null

const open_add = async () => {
    form.value = { stock: 0, price: 0, salePrice: 0, skus: [], coverImgs: [] }
    show.value = true
    // 先等下拉数据加载完再让 el-select 拿到 label 匹配（否则会回退显示原始 value）
    await Promise.all([load_category_options(), load_merchant_options()])
}
const open_edit = async (row: any) => {
    // 先拉详情,确认 id;同时把下拉数据并行加载
    const [res] = await Promise.all([get_goods(row.id), load_category_options(), load_merchant_options()])
    if (res.code === Api_Code.ok && res.data) {
        const good = res.data.good || row
        const skus = (res.data.skus || []).map((s: any) => ({ skuName: s.skuName, price: s.price, stock: s.stock }))
        const specs = res.data.specs || []
        // 多图统一从 good.img_list（逗号分隔）读取，不再依赖 goods_image 表
        const rawList = good.imgList || good.coverImg || ''
        const coverImgs = rawList ? rawList.split(',').map((s: string) => s.trim()).filter(Boolean) : []
        form.value = {
            ...good,
            skus: skus,
            coverImgs,
            merchantId: res.data.merchantId ?? good.merchantId ?? ''
        }
    } else {
        form.value = { ...row, coverImgs: row.coverImg ? String(row.coverImg).split(',').map((s: string) => s.trim()).filter(Boolean) : [] }
    }
    show.value = true
}

const add_sku = () => {
    if (!form.value.skus) form.value.skus = []
    // 新增规格默认继承当前商品售价，减少重复输入
    form.value.skus.push({ skuName: '', price: Number(form.value.salePrice) || 0, stock: 0 })
}
const del_sku = (i: number) => { form.value.skus.splice(i, 1) }

const save = async () => {
    // 方案 A：有规格时总库存强制 = 各规格库存之和，避免两处数字不一致
    if (has_skus.value) form.value.stock = total_stock.value
    // 先校验表单必填字段
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) {
        ElMessage.warning('必填字段未填，请检查表单')
        return
    }
    open_loading()
    // form 走 camelCase；api 层会转成后端需要的 snake_case（见 api/goods.ts to_save_payload）
    const res = form.value.id ? await update_goods(form.value) : await add_goods(form.value)
    close_loading()
    show_notice(res, '保存成功')
    if (res.code === Api_Code.ok) { show.value = false; bind() }
}

const on_size = () => { pages.page = 1; bind() }

const on_submit = async (row: any) => {
    const id = String(row.id)
    submit_loading.value[id] = true
    try {
        const res = await submit_audit(row.id)
        show_notice(res, '提交成功')
        if (res.code === Api_Code.ok) {
            // 就地更新为「待审核」即可，避免再调 bind() 触发整表重渲染与全屏 loading 遮罩闪烁（动画异常）
            row.auditStatus = 0
        }
    } finally {
        submit_loading.value[id] = false
    }
}

const on_toggle_sale = async (row: any) => {
    const next = row.isSale === 1 ? 0 : 1
    sale_loading.value[row.id] = true
    try {
        const res = await set_goods_sale(row.id, next)
        show_notice(res, next === 1 ? '上架成功' : '下架成功')
        if (res.code === Api_Code.ok) {
            row.isSale = next
        }
    } finally {
        sale_loading.value[row.id] = false
    }
}

const on_del = async (row: any) => {
    ElMessageBox.confirm('确认删除该商品？').then(async () => {
        const res = await remove_goods(row.id)
        show_notice(res, '删除成功')
        if (res.code === Api_Code.ok) bind()
    })
}

onMounted(async () => {
    // 预加载分类/商户映射（失败也不阻塞列表渲染，各自 catch）
    load_category_options().catch(() => {})
    load_merchant_options().catch(() => {})
    // 直接拉商品列表，商户/分类映射加载完会回填名称，否则显示 ID 回退文本
    await bind()
    // 网格宽度变化（窗口缩放 / 侧边栏折叠）时同步每页条数，消除末行空洞
    await nextTick()
    if (grid_el.value && typeof ResizeObserver !== 'undefined') {
        grid_observer = new ResizeObserver(() => sync_page_size())
        grid_observer.observe(grid_el.value)
        sync_page_size()
    }
})

onBeforeUnmount(() => {
    grid_observer?.disconnect()
    grid_observer = null
})
</script>

<style scoped>
/* ===== 商品卡片网格 ===== */
.goods-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
    gap: 16px;
    padding: 4px 0;
    align-items: stretch;
    /* 撑满容器剩余高度：视口 - 头部60 - 标签栏42 - 页面内边距32 - 容器外边距32
       - 容器内边距与边框34 - 工具栏32 - 间距16 - 分页条58 = 306 */
    min-height: calc(100vh - 306px);
}
.goods-card {
    background: #fff;
    border: 1px solid #e8e8e8;
    border-radius: 10px;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    transition: box-shadow 0.2s, transform 0.2s, border-color 0.2s;
}
.goods-card:hover {
    box-shadow: 0 6px 18px rgba(0, 0, 0, 0.12);
    transform: translateY(-2px);
    border-color: var(--accent, #c0821b);
}
.gc-cover {
    position: relative;
    width: 100%;
    /* 行高被拉伸时封面吸收多余空间（基准 150px，不低于 150px） */
    flex: 1 1 150px;
    min-height: 150px;
    background: #f5f7fa;
    cursor: zoom-in;
    overflow: hidden;
}
.gc-cover-img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    display: block;
    transition: transform 0.3s ease;
}
.gc-cover:hover .gc-cover-img { transform: scale(1.06); }
.gc-cover-empty {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #c0c4cc;
    font-size: 13px;
}
.gc-sale-badge {
    position: absolute;
    top: 8px;
    left: 8px;
    font-size: 12px;
    padding: 2px 8px;
    border-radius: 10px;
    color: #fff;
    box-shadow: 0 1px 4px rgba(0, 0, 0, 0.18);
}
.gc-sale-badge.on { background: var(--success, #67c23a); }
.gc-sale-badge.off { background: rgba(0, 0, 0, 0.5); }
.gc-body {
    padding: 12px 14px;
    display: flex;
    flex-direction: column;
    gap: 8px;
    flex: 0 0 auto;
}
.gc-name {
    font-size: 15px;
    font-weight: 600;
    color: #303133;
    line-height: 1.4;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    min-height: 21px;
}
.gc-meta {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
    font-size: 12px;
    color: #606266;
}
.gc-tag {
    background: #f4f6fa;
    border-radius: 4px;
    padding: 2px 6px;
    max-width: 100%;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
.gc-price {
    display: flex;
    align-items: baseline;
    flex-wrap: wrap;
    gap: 6px;
}
.gc-price-now { color: #f56c6c; font-size: 18px; font-weight: 700; }
.gc-price-old { color: #909399; font-size: 12px; text-decoration: line-through; }
.gc-unit { color: #909399; font-size: 12px; }
.gc-stock { color: #909399; font-size: 12px; margin-left: auto; }
.gc-status {
    display: flex;
    align-items: center;
    justify-content: space-between;
    font-size: 12px;
    color: #909399;
}
.gc-time { font-size: 12px; }
/* 操作区：4 列等分网格，按钮恒定一行不换行 */
.gc-actions {
    display: grid;
    grid-template-columns: repeat(4, minmax(0, 1fr));
    gap: 6px;
    margin-top: auto;
    padding-top: 8px;
    border-top: 1px dashed #ebeef5;
}
/* Element Plus 全局 .el-button + .el-button { margin-left: 12px }，
   在 grid 里会额外撑开间距甚至把按钮顶出格子，这里必须归零 */
.gc-actions :deep(.el-button) {
    margin-left: 0;
    width: 100%;
    padding: 5px 4px;
    font-size: 12px;
}
.gc-actions :deep(.el-button span) {
    margin-left: 0;
}
.goods-empty {
    grid-column: 1 / -1;
    text-align: center;
    color: #909399;
    padding: 40px 0;
    font-size: 14px;
}

.cover-list {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    align-items: flex-start;
}
.cover-preview {
    position: relative;
    width: 100px;
    height: 100px;
    border-radius: 8px;
    overflow: hidden;
    border: 1px solid #e4e7ed;
}
.cover-img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    display: block;
}
.cover-actions {
    position: absolute;
    inset: 0;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    opacity: 0;
    transition: opacity 0.2s;
}
.cover-preview:hover .cover-actions { opacity: 1; }
.cover-uploader-trigger {
    width: 100px;
    height: 100px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px dashed #d9d9d9;
    border-radius: 8px;
    cursor: pointer;
    color: #999;
    font-size: 24px;
}
.cover-uploader-trigger:hover { border-color: #1890ff; color: #1890ff; }

/* 多规格列表样式 - 4 列对齐：规格值 / 价格 / 库存 / 操作 */
.sku-header {
    display: flex;
    gap: 8px;
    align-items: center;
    padding: 0 0 6px;
    font-size: 12px;
    color: #909399;
    border-bottom: 1px dashed #e4e7ed;
    margin-bottom: 8px;
}
.sku-row {
    display: flex;
    gap: 8px;
    align-items: center;
    margin-bottom: 8px;
}
.sku-col {
    display: flex;
    align-items: center;
    gap: 4px;
}
.sku-col-name {
    width: 180px;
    flex: 0 0 180px;
}
.sku-col-price {
    width: 150px;
    flex: 0 0 150px;
}
.sku-col-stock {
    width: 150px;
    flex: 0 0 150px;
}
.sku-col-op {
    width: 32px;
    flex: 0 0 32px;
    justify-content: center;
}
.sku-help {
    color: #c0c4cc;
    cursor: help;
    font-size: 14px;
    transition: color 0.2s;
}
.sku-help:hover { color: #1890ff; }
/* 只读总库存展示（有规格时自动汇总） */
.stock-readonly {
    display: flex;
    align-items: baseline;
    gap: 8px;
}
.stock-num {
    font-size: 18px;
    font-weight: 600;
    color: #1890ff;
    line-height: 1;
}
.stock-hint {
    font-size: 12px;
    color: #909399;
}

/* 商品编辑弹窗：两列网格，压缩高度，内容一屏显示完 */
.goods-form {
    display: grid;
    grid-template-columns: 1fr 1fr;
    column-gap: 12px;
}
/* 默认非表单项（多规格分隔线/表头/行/按钮）与标记 .full 的项占满整行 */
.goods-form > * {
    grid-column: 1 / -1;
}
.goods-form > .el-form-item:not(.full) {
    grid-column: span 1;
    margin-bottom: 14px;
}
.goods-form > .el-form-item.full {
    margin-bottom: 14px;
}
.goods-form :deep(.el-form-item__label) {
    padding-right: 8px;
}
.goods-form :deep(.el-input),
.goods-form :deep(.el-select),
.goods-form :deep(.el-input-number) {
    width: 100%;
}
</style>
