# shop · 商城系统

这个仓库放的是商城项目的**前端和客户端**部分：一个管理后台（Web），加两个微信小程序（用户端、商家端）。后端 API 是另一个仓库，单独维护，不在这里。

## 目录结构

```
shop/
├── Admin/              管理后台（Web）
│   └── admin/          Vue 3 + Vite 项目，源码和构建配置都在这一层
├── Couser/             微信小程序 · 用户端
│   └── miniprogram/
├── Merchat/            微信小程序 · 商家端
│   └── miniprogram/
└── Api/                （不在此仓库，后端独立维护）
```

## 技术栈

| 模块 | 技术 |
| --- | --- |
| 管理后台 | Vue 3 + Vite + TypeScript |
| 用户端小程序 | 微信小程序原生（TypeScript + Less） |
| 商家端小程序 | 微信小程序原生（TypeScript） |
| 后端 API | .NET（独立仓库，提供接口和数据） |

## 本地运行

### 管理后台
```bash
cd Admin/admin
npm install
npm run dev      # 启动开发服务器，默认本地预览
npm run build    # 打包，产物在 dist/
```
接口地址在 `.env.development`（本地）和 `.env.production`（线上）里，用 `VITE_API_BASE_URL` 配置，按你自己的部署环境改一下就行。

### 微信小程序
用**微信开发者工具**打开对应目录：
- 用户端：导入 `Couser` 文件夹，小程序根目录填 `Couser/miniprogram`
- 商家端：导入 `Merchat` 文件夹，小程序根目录填 `Merchat/miniprogram`

第一次打开工具会提示「构建 npm」，点一下让它重新生成 `miniprogram_npm`。两个小程序的 `appid` 写在各自的 `project.config.json` 里，记得换成你自己的。

## 几点说明
- 本仓库只放前端和客户端源码，不含任何后端密钥。
- 后端那个仓库的 `appsettings.json` 里带 JWT 等密钥，部署时请走环境变量或密钥管理，别直接提交到公开仓库。
- `node_modules`、`dist`、小程序构建产物、`project.private.config.json` 这些已经在 `.gitignore` 里忽略掉了，不会进版本库。
