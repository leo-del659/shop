import { createWebHistory, createRouter } from 'vue-router'
import routes from './routes'
import NProgress from 'nprogress'
import 'nprogress/nprogress.css'
import { useUserStore } from '@/stores/user'
import { check_login, get_menus } from '@/api/login'
import { Api_Code } from '@/dto/response/base'
const router = createRouter({
    history: createWebHistory(),
    routes,
})

router.beforeEach(async (to) => {
    const userStore = useUserStore();
    userStore.set_RouterLoading(true);
    NProgress.start();

    //判断是否需要登陆
    if (to.meta.check) {
        if (!userStore.get_token) {
            return '/login';
        }
        // 刚登录 / 60 秒内已验证过 → 跳过 check_login，直接放行，避免每次跳转都卡一轮网络
        const needCheck = !userStore.freshLogin && (Date.now() - userStore.lastCheck > 60000);
        if (needCheck) {
            //检查登陆是否过期（后端对无效 token 返回 401，拦截器会清态并跳转，这里用 try/catch 兜底）
            try {
                const res = await check_login();
                if (res.code !== Api_Code.ok) {
                    return '/login';
                } else {
                    userStore.set_user(res.data);
                    userStore.lastCheck = Date.now();
                }
            } catch {
                return '/login';
            }
        } else {
            userStore.freshLogin = false;
        }

        //判断菜单是否被缓存 如果缓存了就不再重新获取
        if (userStore.get_menus.length === 0) {
            try {
                const menu_res = await get_menus();
                if (menu_res.code === Api_Code.ok) {
                    userStore.set_menus(menu_res.data);
                }
            } catch {
                // 菜单获取失败不阻断导航，下次进入受保护页会重试
            }
        }
    }

    // 返回 undefined 或 true 表示放行
})

router.afterEach((to, from) => {

    const userSotre = useUserStore();
    userSotre.set_RouterLoading(false);
    NProgress.done();
})

export default router
