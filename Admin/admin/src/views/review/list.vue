<template>
  <div class="l-container">
    <el-input v-model="key" placeholder="评价内容关键词" style="max-width:200px" clearable @keyup.enter="bind" />
    <el-select v-model="tier" placeholder="评分" clearable style="width:120px" @change="bind">
      <el-option v-for="o in tier_opts" :key="o.v" :label="o.l" :value="o.v" />
    </el-select>
    <el-select v-model="tag" placeholder="标签" clearable style="width:140px" @change="bind">
      <el-option v-for="o in tag_opts" :key="o" :label="o" :value="o" />
    </el-select>
    <el-select v-model="status" placeholder="状态" clearable style="width:120px" @change="bind">
      <el-option v-for="o in status_opts" :key="o.v" :label="o.l" :value="o.v" />
    </el-select>
    <el-button type="primary" @click="bind">查询</el-button>
    <el-button @click="reset">重置</el-button>

    <el-table :data="list" border height="calc(100vh - 220px)">
      <el-table-column label="序号" type="index" width="60" />
      <el-table-column label="评价人" width="120">
        <template #default="s">{{ s.row.memberName || '匿名' }}</template>
      </el-table-column>
      <el-table-column label="商品" min-width="180">
        <template #default="s">
          <div>{{ s.row.goodsName }}</div>
          <div class="muted" v-if="s.row.skuName">{{ s.row.skuName }}</div>
        </template>
      </el-table-column>
      <el-table-column label="评分" width="160">
        <template #default="s">
          <el-rate :model-value="s.row.rating" disabled />
        </template>
      </el-table-column>
      <el-table-column label="内容" prop="content" show-overflow-tooltip min-width="200" />
      <el-table-column label="标签" min-width="160">
        <template #default="s">
          <el-tag v-for="t in splitTags(s.row.tags)" :key="t" size="small" class="tag">{{ t }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="状态" width="90">
        <template #default="s">
          <el-tag :type="s.row.status === 1 ? 'success' : (s.row.status === 2 ? 'danger' : 'warning')">
            {{ status_text(s.row.status) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="时间" prop="createTime" width="170" />
      <el-table-column label="操作" width="240" fixed="right">
        <template #default="s">
          <el-button size="small" @click="open_detail(s.row)">查看</el-button>
          <el-button size="small" type="success" :disabled="s.row.status === 1" @click="on_pass(s.row)">通过</el-button>
          <el-button size="small" type="danger" :disabled="s.row.status === 2" @click="on_reject(s.row)">驳回</el-button>
          <el-button size="small" type="danger" plain @click="on_del(s.row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <MyPagination v-model:page="pages.page" v-model:size="pages.size" :total="pages.total" @page-change="bind" @size-change="on_size" />

    <!-- 查看 / 回复 -->
    <el-dialog v-model="show" title="评价详情" width="560px">
      <template v-if="curr">
        <div class="muted" style="margin-bottom:6px;">{{ curr.memberName || '匿名' }} · {{ curr.goodsName }}
          <span v-if="curr.skuName">（{{ curr.skuName }}）</span>
        </div>
        <el-rate :model-value="curr.rating" disabled />
        <div style="margin:8px 0;">{{ curr.content || '（无文字评价）' }}</div>
        <div v-if="curr.tags" style="margin-bottom:8px;">
          <el-tag v-for="t in splitTags(curr.tags)" :key="t" size="small" class="tag">{{ t }}</el-tag>
        </div>
        <div v-if="curr.images" style="display:flex;gap:8px;flex-wrap:wrap;margin-bottom:8px;">
          <img v-for="(img, i) in splitImgs(curr.images)" :key="i" :src="imgUrl(img)" class="rv-img" mode="aspectFill" />
        </div>
        <el-divider />
        <el-input v-model="reply_text" type="textarea" :rows="3" placeholder="商家回复内容" />
      </template>
      <template #footer>
        <el-button @click="show = false">关闭</el-button>
        <el-button type="primary" @click="on_reply(curr)">回复并设为通过</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import MyPagination from '@/components/common/MyPagination.vue'
import { get_review_list, audit_review, reply_review, remove_review } from '@/api/review'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { ElMessageBox } from 'element-plus'
import { onMounted, reactive, ref } from 'vue'

const list = ref<any[]>([])
const key = ref('')
const tier = ref<string | undefined>(undefined)
const tag = ref<string | undefined>(undefined)
const status = ref<number | undefined>(undefined)

const tier_opts = [
  { v: 'good', l: '好评(≥4)' },
  { v: 'mid', l: '中评(3)' },
  { v: 'bad', l: '差评(≤2)' }
]
const tag_opts = ['口感好', '包装精美', '物流快', '性价比高', '新鲜', '份量偏少']
const status_opts = [
  { v: 0, l: '待审核' },
  { v: 1, l: '通过' },
  { v: 2, l: '驳回' }
]

const pages = reactive({ page: 1, size: 10, total: 0 })
const show = ref(false)
const curr = ref<any>(null)
const reply_text = ref('')

const splitTags = (v: string | null) => (v ? String(v).split(',').filter(Boolean) : [])
const splitImgs = (v: string | null) => (v ? String(v).split(',').filter(Boolean) : [])
const imgUrl = (v: string) => (v.startsWith('http') ? v : import.meta.env.VITE_API_BASE_URL + v)
const status_text = (v: number) => (status_opts.find(o => o.v === v)?.l) || '未知'

const bind = async () => {
  open_loading()
  const res = await get_review_list({
    page: pages.page,
    size: pages.size,
    keyword: key.value || undefined,
    tier: tier.value,
    tag: tag.value,
    status: status.value
  })
  close_loading()
  if (res.code === Api_Code.ok) {
    list.value = res.data.list
    pages.total = res.data.total
  }
}

const reset = () => {
  key.value = ''
  tier.value = undefined
  tag.value = undefined
  status.value = undefined
  bind()
}

const open_detail = (row: any) => {
  curr.value = row
  reply_text.value = row.reply || ''
  show.value = true
}

const on_pass = async (row: any) => {
  const res = await audit_review({ id: row.id, status: 1 })
  show_notice(res)
  if (res.code === Api_Code.ok) bind()
}

const on_reject = (row: any) => {
  ElMessageBox.prompt('请输入驳回原因', '驳回评价', {
    inputValidator: (val: string) => {
      if (!val || !val.trim()) return '驳回原因不能为空'
      return true
    },
    inputErrorMessage: '驳回原因不能为空'
  }).then(async ({ value }) => {
    const res = await audit_review({ id: row.id, status: 2, reject_reason: value.trim() })
    show_notice(res)
    if (res.code === Api_Code.ok) bind()
  }).catch(() => { })
}

const on_reply = async (row: any) => {
  if (!reply_text.value.trim()) {
    show_notice({ code: Api_Code.fail, message: '请输入回复内容' } as any)
    return
  }
  const res = await reply_review({ id: row.id, reply: reply_text.value })
  show_notice(res)
  if (res.code === Api_Code.ok) { show.value = false; bind() }
}

const on_del = async (row: any) => {
  ElMessageBox.confirm('确认删除该评价？').then(async () => {
    const res = await remove_review(row.id)
    show_notice(res)
    if (res.code === Api_Code.ok) bind()
  }).catch(() => { })
}

const on_size = () => { pages.page = 1; bind() }

onMounted(bind)
</script>

<style scoped>
.tag { margin-right: 4px; }
.muted { color: #909399; font-size: 12px; }
.rv-img { width: 72px; height: 72px; border-radius: 6px; object-fit: cover; }
</style>
