<template>
    <div class="login-wrap">
        <!-- 有机背景：暖色块 + 柔光 -->
        <div class="blob blob-a"></div>
        <div class="blob blob-b"></div>
        <div class="blob blob-c"></div>
        <div class="grain"></div>

        <!-- 左侧品牌叙事 -->
        <aside class="brand-side">
            <div class="brand-mark">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
                    <path d="M9 22V12h6v10"></path>
                </svg>
            </div>
            <h1 class="brand-name">集邻生活</h1>
            <p class="brand-desc">把邻里之间的每一次购物，<br />都变成有温度的相遇。</p>
        </aside>

        <!-- 右侧登录卡 -->
        <main class="form-side">
            <div class="login-card">
                <div class="card-head">
                    <h2>欢迎回来</h2>
                    <p>请登录您的运营账户</p>
                </div>

                <el-form :model="request" @submit.prevent="login" class="login-form">
                    <div class="field">
                        <label>账号</label>
                        <div class="field-box">
                            <svg class="field-ico" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                            <el-input v-model="request.account" placeholder="请输入账号" size="large" />
                        </div>
                    </div>

                    <div class="field">
                        <label>密码</label>
                        <div class="field-box">
                            <svg class="field-ico" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                            <el-input v-model="request.password" type="password" placeholder="请输入密码" size="large" show-password @keyup.enter="login" />
                        </div>
                    </div>

                    <el-button type="primary" size="large" :loading="loading" class="login-btn" @click="login">
                        {{ loading ? '登录中…' : '登 录' }}
                    </el-button>
                </el-form>

            </div>
        </main>
    </div>
</template>

<script setup lang="ts">
import type { Login_Request_Dto } from '@/dto/request/base';
import { Api_Code } from '@/dto/response/base';
import router from '@/router';
import { useUserStore } from '@/stores/user';
import { reactive, ref } from 'vue';

const userStore = useUserStore();
const loading = ref(false);

const request = reactive<Login_Request_Dto>({
    account: '13700000001',
    password: '123456'
});

const login = async () => {
    if (!request.account || !request.password) return;
    loading.value = true;
    try {
        const res = await userStore.login(request);
        if (res === Api_Code.ok) {
            router.push('/');
        }
    } finally {
        loading.value = false;
    }
}
</script>

<style scoped>
/* 暖色编辑风：奶油底 + 珊瑚/鼠尾草/琥珀三色有机块（上下布局） */
.login-wrap {
    width: 100vw;
    min-height: 100vh;
    height: auto;
    overflow-x: hidden;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    position: relative;
    background: #faf7f2;
    font-family: "PingFang SC", "Microsoft YaHei", -apple-system, system-ui, sans-serif;
    padding: 48px 20px;
    box-sizing: border-box;
}

/* 背景有机色块：放在四角，避免中间主体区域遮挡 */
.blob {
    position: absolute;
    border-radius: 42% 58% 63% 37% / 42% 38% 62% 58%;
    filter: blur(8px);
    opacity: 0.5;
    pointer-events: none;
}
.blob-a {
    width: 420px; height: 420px;
    top: -160px; right: -120px;
    background: #f3c9b1;
    animation: float 19s ease-in-out infinite;
}
.blob-b {
    width: 360px; height: 360px;
    bottom: -140px; left: -110px;
    background: #cfe0cf;
    animation: float 24s ease-in-out infinite reverse;
}
.blob-c {
    width: 240px; height: 240px;
    top: 12%; right: 8%;
    background: #f6e2b0;
    opacity: 0.45;
    animation: float 16s ease-in-out infinite 2s;
}
@keyframes float {
    0%, 100% { transform: translate(0, 0) rotate(0deg); }
    50% { transform: translate(26px, -30px) rotate(8deg); }
}

/* 颗粒质感 */
.grain {
    position: absolute;
    inset: 0;
    opacity: 0.035;
    pointer-events: none;
    background-image: radial-gradient(#5b4636 1px, transparent 1px);
    background-size: 4px 4px;
}

/* 上方品牌叙事 */
.brand-side {
    position: relative;
    z-index: 1;
    flex: 0 0 auto;
    width: 100%;
    max-width: 480px;
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    padding: 0 0 32px 0;
    color: #41372e;
}
.brand-mark {
    width: 56px; height: 56px;
    border-radius: 18px;
    background: #e07a5f;
    color: #fff;
    display: flex; align-items: center; justify-content: center;
    box-shadow: 0 12px 26px rgba(224, 122, 95, 0.34);
    margin-bottom: 20px;
}
.brand-mark svg { width: 28px; height: 28px; }
.brand-name {
    font-size: 36px;
    font-weight: 800;
    letter-spacing: 2px;
    margin: 0 0 10px;
    color: #2f2820;
}
.brand-desc {
    font-size: 15px;
    line-height: 1.7;
    color: #7a6f63;
    margin: 0 0 24px;
}
/* 下方表单 */
.form-side {
    position: relative;
    z-index: 1;
    flex: 0 0 auto;
    width: 100%;
    max-width: 400px;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0;
}
.login-card {
    width: 100%;
    max-width: 400px;
    background: #ffffff;
    border-radius: 24px;
    padding: 32px 28px 24px;
    box-shadow: 0 24px 60px rgba(91, 70, 54, 0.12), 0 2px 8px rgba(91, 70, 54, 0.05);
    border: 1px solid #f0e9df;
    animation: cardIn 0.7s cubic-bezier(0.22, 1, 0.36, 1);
}
@keyframes cardIn {
    from { opacity: 0; transform: translateY(28px) scale(0.98); }
    to { opacity: 1; transform: translateY(0) scale(1); }
}
.card-head { margin-bottom: 26px; text-align: center; }
.card-head h2 {
    font-size: 24px;
    font-weight: 800;
    color: #2f2820;
    margin: 0 0 6px;
}
.card-head p {
    font-size: 14px;
    color: #9c9183;
    margin: 0;
}

.login-form { width: 100%; }
.field { margin-bottom: 18px; }
.field label {
    display: block;
    font-size: 13px;
    font-weight: 600;
    color: #5c5247;
    margin-bottom: 8px;
    padding-left: 2px;
}
.field-box {
    position: relative;
    display: flex;
    align-items: center;
}
.field-ico {
    position: absolute;
    left: 15px;
    width: 18px; height: 18px;
    color: #bcae9c;
    z-index: 1;
    transition: color 0.25s ease;
    pointer-events: none;
}
.field-box:focus-within .field-ico { color: #e07a5f; }
.field-box :deep(.el-input__wrapper) {
    padding-left: 44px;
    border-radius: 13px;
    box-shadow: 0 0 0 1px #ece3d6 inset;
    background: #fdfbf8;
    transition: all 0.25s ease;
}
.field-box :deep(.el-input__wrapper:hover) {
    box-shadow: 0 0 0 1px #d9cdbb inset;
    background: #fff;
}
.field-box :deep(.el-input__wrapper.is-focus) {
    box-shadow: 0 0 0 2px rgba(224, 122, 95, 0.16), 0 0 0 1px #e07a5f inset !important;
    background: #fff;
}

.login-btn {
    width: 100%;
    margin-top: 8px;
    height: 50px;
    font-size: 16px;
    font-weight: 700;
    letter-spacing: 4px;
    border: none;
    border-radius: 13px;
    color: #fff;
    background: linear-gradient(135deg, #e8927a 0%, #e07a5f 100%);
    box-shadow: 0 10px 24px rgba(224, 122, 95, 0.32);
    transition: all 0.3s cubic-bezier(0.22, 1, 0.36, 1);
}
.login-btn:hover {
    transform: translateY(-2px);
    box-shadow: 0 14px 32px rgba(224, 122, 95, 0.4);
}
.login-btn:active { transform: translateY(0); }

/* 窄屏：保持上下布局，仅微调字号与间距 */
@media (max-width: 480px) {
    .login-wrap { padding: 32px 16px; }
    .brand-side { padding-bottom: 24px; }
    .brand-mark { width: 48px; height: 48px; border-radius: 16px; }
    .brand-name { font-size: 28px; }
    .brand-desc { font-size: 14px; margin-bottom: 18px; }
    .login-card { padding: 26px 20px 20px; border-radius: 20px; }
    .card-head h2 { font-size: 22px; }
}
</style>
