import { Api_Code, type Api_Reseponse_Dto } from "@/dto/response/base";
import { ElLoading, ElMessage, type LoadingInstance } from "element-plus"
let loading: LoadingInstance;

/**
 * 打开全局加载
 */
export const open_loading = (msg?: string) => {
    loading = ElLoading.service({
        lock: true,
        text: msg || '请稍后...',
        background: 'rgba(0, 0, 0, 0.7)',
    })
}
/**
 * 关闭全局加载
 */
export const close_loading = () => {
    if (loading)
        loading.close();
}

/**
 * 消息提示
 * @param res 接口返回
 * @param successMsg 成功时的自定义文案（如「保存成功」）；不传则优先用后端 message，后端为默认 "ok" 时降级为「操作成功」
 * @param errorMsg 失败时的自定义文案；不传则用后端 message，兜底「操作失败」
 */
export const show_notice = (res: Api_Reseponse_Dto, successMsg?: string, errorMsg?: string) => {
    if (res.code === Api_Code.ok) {
        const msg = successMsg || (res.message && res.message !== 'ok' ? res.message : '操作成功');
        ElMessage.success(msg);
    } else {
        ElMessage.error(errorMsg || res.message || '操作失败');
    }
}