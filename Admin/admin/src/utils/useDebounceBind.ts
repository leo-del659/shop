import { type Ref, watch } from 'vue'

/**
 * 对一组响应式查询条件做防抖监听，条件变化时自动调用 bind()。
 * 常用于列表页的搜索输入框 / 下拉选择：用户停止输入 delay 毫秒后触发查询。
 */
export function useDebounceBind(sources: Ref<any> | Ref<any>[], bind: () => void, delay = 400) {
    let timer: any
    const run = () => {
        clearTimeout(timer)
        timer = setTimeout(() => bind(), delay)
    }
    const refs = Array.isArray(sources) ? sources : [sources]
    refs.forEach(r => watch(r, run))
}
