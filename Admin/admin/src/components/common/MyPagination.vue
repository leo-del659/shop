<template>
    <div class="my-pagination">
        <div class="mp-left">
            <span class="mp-total">
                共 <em>{{ total }}</em> 条 · 当前第 <em>{{ page }}</em> 页
            </span>
            <el-select
                v-model="sizeModel"
                :size="sizeType"
                class="mp-sizes"
                @change="on_size_change"
            >
                <el-option
                    v-for="s in pageSizes"
                    :key="s"
                    :label="`${s} 条/页`"
                    :value="s"
                />
            </el-select>
        </div>
        <el-pagination
            v-model:current-page="pageModel"
            :page-size="size"
            :pager-count="5"
            background
            :layout="pagerLayout"
            :total="total"
            :small="small"
            @current-change="on_page_change"
        />
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
    page: number
    size: number
    total: number
    pageSizes?: number[]
    small?: boolean
}

const props = withDefaults(defineProps<Props>(), {
    pageSizes: () => [10, 20, 50, 100],
    small: false,
})

const emit = defineEmits<{
    'update:page': [v: number]
    'update:size': [v: number]
    'page-change': [v: number]
    'size-change': [v: number]
}>()

const sizeType = computed(() => (props.small ? 'small' : 'default'))

const pageModel = computed({
    get: () => props.page,
    set: v => emit('update:page', v),
})

const sizeModel = computed({
    get: () => props.size,
    set: v => emit('update:size', v),
})

const pagerLayout = computed(() => 'prev, pager, next, jumper')

const on_page_change = (v: number) => {
    emit('page-change', v)
}

const on_size_change = (v: number) => {
    emit('size-change', v)
}
</script>

<style scoped>
.my-pagination {
    margin-top: 0;
    padding: 12px 16px;
    background: #fff;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: 12px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
    border: 1px solid #ebeef5;
}

.mp-left {
    display: flex;
    align-items: center;
    gap: 16px;
    color: #606266;
    font-size: 14px;
    white-space: nowrap;
}

.mp-total em {
    color: #1890ff;
    font-weight: 600;
    font-style: normal;
    margin: 0 2px;
}

.mp-sizes {
    width: 120px;
}

:deep(.el-pagination) {
    --el-pagination-bg-color: #fcf6ea;
    --el-pagination-button-color: #6B6358;
    --el-pagination-hover-color: var(--accent);
    justify-content: flex-end;
    flex-wrap: wrap;
    row-gap: 4px;
}

:deep(.el-pagination .el-pager li) {
    border-radius: 6px;
    transition: all 0.2s ease;
    font-weight: 500;
}

:deep(.el-pagination .el-pager li:hover) {
    color: #1890ff;
    transform: translateY(-1px);
}

:deep(.el-pagination .el-pager li.is-active) {
    background: linear-gradient(135deg, #1890ff 0%, #096dd9 100%);
    color: #fff;
    box-shadow: 0 2px 6px rgba(24, 144, 255, 0.35);
}

:deep(.el-pagination .btn-prev),
:deep(.el-pagination .btn-next) {
    border-radius: 6px;
    background: #f5f7fa;
}

:deep(.el-pagination .btn-prev:hover),
:deep(.el-pagination .btn-next:hover) {
    background: #e6f7ff;
}

:deep(.el-pagination .el-pagination__jump) {
    margin-left: 12px;
    color: #606266;
}

:deep(.el-pagination .el-input__wrapper) {
    border-radius: 6px;
}
</style>
