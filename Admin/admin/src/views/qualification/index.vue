<template>
    <div class="l-container">
        <el-select v-model="merchant_id" filterable placeholder="选择商户" style="width:320px" @change="bind">
            <el-option v-for="s in merchants" :key="s.id" :label="s.merchantName" :value="s.id" />
        </el-select>
        <el-button type="primary" @click="bind">查询</el-button>
        <el-button type="success" :disabled="!merchant_id" @click="open_add">新增资质</el-button>
        <el-table :data="list" border height="calc(100vh - 220px)">
            <el-table-column label="序号" type="index" width="60" />
            <el-table-column label="资质类型" prop="qualType" show-overflow-tooltip />
            <el-table-column label="证书编号" prop="qualNo" show-overflow-tooltip />
            <el-table-column label="图片" width="120">
                <template #default="s">
                    <!-- 缩略图：普通 img（不触发 hover preview 遮挡表格），点击时手动打开 ImageViewer -->
                    <template v-if="is_image_url(s.row.fileUrl)">
                        <img
                            :src="img_src(s.row.fileUrl)"
                            style="width:60px;height:60px;border-radius:4px;object-fit:cover;cursor:zoom-in;border:1px solid #e8e8e8;"
                            @click="open_preview(s.row.fileUrl)"
                            @error="(e: any) => { (e.target as HTMLImageElement).style.display = 'none' }"
                        />
                    </template>
                    <template v-else>
                        <span :style="{ color: '#999', fontSize: '12px' }">{{ s.row.fileUrl || '—' }}</span>
                    </template>
                </template>
            </el-table-column>
            <el-table-column label="到期日" prop="expireDate" width="120" />
            <el-table-column label="禁用" width="80">
                <template #default="s">
                    <el-switch
                        :model-value="s.row.isBan === 1"
                        @change="(v: any) => on_toggle_ban(s.row, v)"
                    />
                </template>
            </el-table-column>
            <el-table-column label="操作" width="180" fixed="right">
                <template #default="s">
                    <el-button size="small" @click="open_edit(s.row)">编辑</el-button>
                    <el-button size="small" type="danger" @click="on_del(s.row)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>
        <el-alert v-if="list.length === 0 && merchant_id" title="暂无资质记录" type="info" :closable="false" style="margin-top:8px" />
        <el-alert v-if="!merchant_id" title="请先选择商户" type="info" :closable="false" style="margin-top:8px" />

        <el-dialog v-model="show" :title="form.id ? '编辑资质' : '新增资质'" width="560px">
            <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
                <el-form-item label="资质类型" prop="qualType">
                    <el-input v-model="form.qualType" placeholder="如：食品经营许可证" />
                </el-form-item>
                <el-form-item label="证书编号" prop="qualNo">
                    <el-input v-model="form.qualNo" placeholder="请输入证书编号" />
                </el-form-item>
                <el-form-item label="证书文件" prop="fileUrl">
                    <div style="display:flex;align-items:center;gap:8px;flex-wrap:wrap;">
                        <template v-if="form.fileUrl">
                            <template v-if="is_image_url(form.fileUrl)">
                                <img :src="img_src(form.fileUrl)" style="width:60px;height:60px;border-radius:4px;object-fit:cover;cursor:zoom-in;border:1px solid #e8e8e8;" @click="open_preview(form.fileUrl)" />
                            </template>
                            <span v-else :style="{ color: '#999', fontSize: '12px' }">{{ form.fileUrl }}</span>
                            <el-button link type="danger" @click="form.fileUrl = ''">清除</el-button>
                        </template>
                        <el-upload
                            v-else
                            action="#"
                            :show-file-list="false"
                            :before-upload="before_upload"
                            :http-request="custom_upload"
                            accept="image/*"
                        >
                            <el-button type="primary" plain>选择文件</el-button>
                        </el-upload>
                        <span v-if="uploading" style="color:#999;font-size:12px;">上传中...</span>
                    </div>
                </el-form-item>
                <el-form-item label="到期日" prop="expireDate">
                    <el-date-picker v-model="form.expireDate" type="date" value-format="YYYY-MM-DD" placeholder="选择到期日期" style="width:100%" />
                </el-form-item>
                <el-form-item label="禁用">
                    <el-switch v-model="form.isBan" :active-value="1" :inactive-value="0" />
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="show = false">取消</el-button>
                <el-button type="primary" :loading="uploading" @click="save">保存</el-button>
            </template>
        </el-dialog>

        <!-- 图片预览：手动控制开关，避免 el-image hover 自动弹遮挡表格 -->
        <el-image-viewer
            v-if="show_preview"
            :url-list="preview_urls"
            :initial-index="preview_index"
            @close="show_preview = false"
        />
    </div>
</template>

<script setup lang="ts">
import { get_merchant_list } from '@/api/merchant'
import { add_merchant_qualification, get_merchant_qualifications, remove_merchant_qualification, update_merchant_qualification } from '@/api/merchant'
import { upload_file } from '@/api/file'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { ElImageViewer, ElMessage, ElMessageBox } from 'element-plus'
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'

// 图片访问用 baseURL：后端约定的路径是相对路径 /api/file/xxx，浏览器需要完整 URL
// baseURL 末尾可能带 /（如 VITE_API_BASE_URL=http://localhost:6001/），直接拼接会产生 // 导致 404
const BASE = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '')
// 所有本地相对路径（/api/file/、/uploads/、/images/ 等）统一走后端文件服务 BASE，
// 由后端 FileController 的 GET /api/file/{name} 提供静态文件
const img_src = (rel: string) => {
    if (!rel) return ''
    if (rel.startsWith('http')) return rel
    return `${BASE}${rel.startsWith('/') ? '' : '/'}${rel}`.replace(/([^:])\/\//g, '$1/')
}
// 判断是否为可预览的图片 URL（兼容 .NET /api/file/、小程序 /images/ /uploads/、完整 http URL）
const is_image_url = (url: string) => {
    if (!url) return false
    if (/\.(jpg|jpeg|png|gif|webp|bmp)(\?.*)?$/i.test(url)) return true
    if (url.startsWith('/api/file/')) return true
    if (url.startsWith('/uploads/')) return true
    if (url.startsWith('/images/')) return true
    if (/^https?:\/\//i.test(url)) return true
    return false
}

const merchants = ref<any[]>([])
const merchant_id = ref('')
const pages = reactive({ page: 1, size: 10, total: 0 })
const list = ref<any[]>([])
const show = ref(false)
const form = ref<any>({})
const uploading = ref(false)
const formRef = ref<any>(null)
const rules = {
    qualType: [{ required: true, message: '请输入资质类型', trigger: 'blur' }],
    qualNo: [{ required: true, message: '请输入证书编号', trigger: 'blur' }],
    fileUrl: [{ required: true, message: '请上传证书文件', trigger: 'change' }],
    expireDate: [{ required: true, message: '请选择到期日', trigger: 'change' }]
}
// 图片预览：手动控制，只有用户点缩略图才打开 ElImageViewer
const show_preview = ref(false)
const preview_urls = ref<string[]>([])
const preview_index = ref(0)
const open_preview = (rel: string) => {
    preview_urls.value = [img_src(rel)]
    preview_index.value = 0
    show_preview.value = true
}

const bind = async () => {
    if (!merchant_id.value) { list.value = []; return }
    open_loading(); const res = await get_merchant_qualifications(merchant_id.value); close_loading()
    if (res.code === Api_Code.ok) { list.value = res.data || [] }
}

const open_add = () => {
    if (!merchant_id.value) { show_notice({ code: -1, message: '请先选择商户' } as any); return }
    form.value = { merchantId: merchant_id.value, qualType: '', qualNo: '', fileUrl: '', expireDate: '', isBan: 0 }
    show.value = true
}
const open_edit = (row: any) => {
    form.value = { ...row }
    show.value = true
}

const save = async () => {
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) { ElMessage.warning('必填字段未填，请检查表单'); return }
    open_loading()
    const data: any = { ...form.value, merchantId: merchant_id.value || form.value.merchantId }
    // 清理 UI 内部字段，避免发给后端引起不必要的耦合
    delete data._img_broken
    const res = form.value.id
        ? await update_merchant_qualification(data)
        : await add_merchant_qualification(data)
    close_loading()
    show_notice(res)
    if (res.code === Api_Code.ok) { show.value = false; bind() }
}

const on_del = (row: any) => {
    ElMessageBox.confirm('确认删除该资质？').then(async () => {
        const res = await remove_merchant_qualification(row.id)
        show_notice(res)
        if (res.code === Api_Code.ok) bind()
    })
}

// 直接切换 isBan → 调 update（不进入弹窗）
const on_toggle_ban = async (row: any, v: any) => {
    const res = await update_merchant_qualification({ id: row.id, merchantId: row.merchantId, isBan: v ? 1 : 0 })
    show_notice(res)
    if (res.code === Api_Code.ok) row.isBan = v ? 1 : 0
}

// 上传相关：custom_http-request 必须 opt.onSuccess 否则 el-upload 状态不更新
const before_upload = (file: File) => {
    if (file.size > 5 * 1024 * 1024) { show_notice({ code: -1, message: '文件不能超过 5MB' } as any); return false }
    return true
}
const custom_upload = (opt: any) => {
    uploading.value = true
    upload_file(opt.file).then((res: any) => {
        if (res.code === Api_Code.ok) {
            form.value.fileUrl = res.data?.url || ''
            show_notice({ code: 0, message: '上传成功' } as any)
        } else {
            show_notice(res)
        }
        opt.onSuccess(res)
    }).catch((e: any) => {
        opt.onError(e)
    }).finally(() => { uploading.value = false })
}

// 如果是从 merchant 页带 ?merchantId=xxx 跳过来的，自动选中并加载
const preset_merchant_id = useRoute().query.merchantId as string || ''
onMounted(async () => {
    if (preset_merchant_id) merchant_id.value = preset_merchant_id
    open_loading(); const r = await get_merchant_list({ page: 1, size: 100 }); close_loading()
    if (r.code === Api_Code.ok) {
        merchants.value = r.data.list
        // 默认选第一个商户：进入页面就有数据可看，避免"请先选择商户"空置
        if (!merchant_id.value && merchants.value.length > 0) {
            merchant_id.value = merchants.value[0].id
        }
        if (merchant_id.value) await bind()
    }
})
</script>

<style scoped></style>
