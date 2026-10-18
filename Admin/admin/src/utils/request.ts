import router from "@/router";
import { useUserStore } from "@/stores/user";
import { close_loading } from "@/utils/message";
import { ElMessage } from "element-plus";
import axios from "axios";

// service 服务 instance 实例
const instance = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL,
    timeout: 20000,
});

// 韧性保护链（用户端侧）配置：配合后端 限流/隔离/超时/重试/熔断/降级
const RESILIENCE = {
    maxRetry: 2,
    baseDelay: 300,
    safeMethods: ["get", "head", "options"], // 仅幂等方法在 429/503/504 或网络错误时重试
};

// 把后端保护链返回的 HTTP 状态码，映射成用户能看懂的话
function resilienceMessage(status?: number): string | null {
    if (status === 429) return "请求过于频繁，请稍后再试";
    if (status === 503) return "服务暂时繁忙，请稍后重试";
    if (status === 504) return "请求超时，已为您快速释放，请稍后重试";
    return null;
}

// 添加请求拦截器
instance.interceptors.request.use(function (config) {
    const userStore = useUserStore();
    const token = userStore.get_token;
    if (token) {
        config.headers.set("Authorization", "Bearer " + token);
    }
    return config;
}, function (error) {
    return Promise.reject(error);
});

// 防止并发 401 时反复跳转登录页
let isRedirectingLogin = false;

// 添加响应拦截器
instance.interceptors.response.use(function (response) {
    // 2xx 范围内的状态码都会触发该函数。
    return response.data;
}, async function (error) {
    // 任何请求异常都先关闭全局 loading，避免列表页请求失败后全屏遮罩卡死
    close_loading();

    const status = error.response?.status;
    const config = error.config || {};

    // 401：登录态失效，清除缓存并跳登录页（不重试）
    if (status === 401) {
        const userStore = useUserStore();
        userStore.login_out();
        userStore.set_menus([]);
        if (!isRedirectingLogin && router.currentRoute.value.path !== "/login") {
            isRedirectingLogin = true;
            router.replace("/login").finally(() => { isRedirectingLogin = false; });
        }
        return Promise.reject(error);
    }

    // 后端保护链触发 或 网络层错误：安全方法做指数退避重试
    const isNetworkError = !error.response;
    const isResilienceStatus = status === 429 || status === 503 || status === 504;
    const method = (config.method || "get").toLowerCase();
    const safe = RESILIENCE.safeMethods.includes(method);
    const retryCount = (config as any).__retryCount || 0;

    if ((isResilienceStatus || isNetworkError) && safe && retryCount < RESILIENCE.maxRetry) {
        (config as any).__retryCount = retryCount + 1;
        const backoff = RESILIENCE.baseDelay * Math.pow(2, retryCount);
        await new Promise((r) => setTimeout(r, backoff));
        return instance.request(config);
    }

    // 不再重试 → 友好提示（覆盖后端 429/503/504 与网络错误）
    const friendly = resilienceMessage(status) || (isNetworkError ? "网络错误，请稍后重试" : (error.response?.data?.message || "请求失败"));
    ElMessage.error(friendly);
    return Promise.reject(error);
});

export default instance;

