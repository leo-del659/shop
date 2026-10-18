import 'element-plus/dist/index.css'

import './assets/main.css'
import './assets/iconfont.css'

import 'animate.css';

import { createApp } from 'vue'
import App from './App.vue'
import router from './router/index.ts'

import ElementPlus from 'element-plus'
import zhCn from 'element-plus/es/locale/lang/zh-cn'

import { createPinia } from 'pinia'
const pinia = createPinia()



const app = createApp(App)
app.use(pinia)
app.use(router)
app.use(ElementPlus, { locale: zhCn })


app.mount('#app')
