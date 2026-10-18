import { logins, check_login } from "@/api/login"
import type { Login_Request_Dto } from "@/dto/request/base"
import { Api_Code, type User_Dto } from "@/dto/response/base"
import { getToken, removeToken, setToken } from "@/utils/auth"
import { close_loading, open_loading } from "@/utils/message"
import { ElMessage } from "element-plus"
import { defineStore } from "pinia"

const MENU_CACHE_KEY = "admin_menus_v2"
const MENU_CACHE_VERSION = "10"

// 递归清洗菜单名称：去掉英文/数字/下划线前缀及后续空格
const cleanMenuName = (name: any): string => {
    return (name || '').toString().trim().replace(/^[a-zA-Z0-9_]+\s+/, '').trim()
}

const cleanMenus = (menus: any[]): any[] => {
    return (menus || []).map(m => ({
        ...m,
        name: cleanMenuName(m.name),
        children: cleanMenus(m.children)
    }))
}

const loadCachedMenus = (): any[] => {
    try {
        const raw = sessionStorage.getItem(MENU_CACHE_KEY)
        if (!raw) return []
        const parsed = JSON.parse(raw)
        if (parsed && parsed.__v === MENU_CACHE_VERSION && Array.isArray(parsed.data)) {
            return parsed.data
        }
    } catch { }
    return []
}

export const useUserStore = defineStore('user', {
    state: () => ({
        token: getToken() || "",
        routerLoading: true,
        user: {
            account: '',
            name: ''
        },
        isContractile: false,
        menus: loadCachedMenus(),
        navs: [] as any[],
        lastCheck: 0,
        freshLogin: false,
    }),
    getters: {
        get_navs(): any[] {
            return this.navs;
        },
        get_menus(): any[] {
            return this.menus;
        },
        get_IsContractile(): boolean {
            return this.isContractile;
        },
        get_user(): User_Dto {
            return this.user;
        },
        get_name(): string {
            return this.user.name;
        },
        get_account(): string {
            return this.user.account;
        },
        get_RouterLoading(): boolean {
            return this.routerLoading
        },
        get_token(): string {
            return this.token;
        }
    },
    actions: {
        set_navs(val: any) {
            this.navs = val;
        },
        set_menus(val: any) {
            const cleaned = cleanMenus(val || [])
            this.menus = cleaned;
            sessionStorage.setItem(MENU_CACHE_KEY, JSON.stringify({ __v: MENU_CACHE_VERSION, data: cleaned }));
        },
        set_IsContractile(val: boolean) {
            this.isContractile = val;
        },
        login_out() {
            this.token = '';
            this.user = { account: '', name: '' };
            this.menus = [];
            this.navs = [];
            this.lastCheck = 0;
            this.freshLogin = false;
            sessionStorage.removeItem(MENU_CACHE_KEY);
            removeToken();
        },
        set_user(val: User_Dto) {
            this.user = val;
        },
        set_RouterLoading(val: boolean) {
            this.routerLoading = val;
        },
        async login(data: Login_Request_Dto): Promise<Api_Code> {
            open_loading();
            try {
                const res = await logins(data);
                if (res.code === Api_Code.fail) {
                    ElMessage.error(res.message);
                } else {
                    this.token = res.data;
                    setToken(this.token);
                    // 登录期间顺带拉取用户信息（隐藏在 loading 中，避免跳转首页时再卡一轮）
                    try {
                        const info = await check_login();
                        if (info.code === Api_Code.ok) {
                            this.user = info.data;
                            this.lastCheck = Date.now();
                        }
                    } catch { /* 失败不阻断登录，下次进入受保护页会重试 */ }
                    this.freshLogin = true;
                }
                return res.code;
            } catch (error: any) {
                ElMessage.error('网络请求失败，请检查后端服务是否运行');
                console.error('Login error:', error);
                return Api_Code.fail;
            } finally {
                close_loading();
            }
        }
    },
})