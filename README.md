# shop · 商城系统

一个商城项目的完整源码：后端 API、管理后台（Web），加上两个微信小程序（用户端、商家端），都放在这一个仓库里。

## 目录结构

```
shop/
├── Api/                   后端（.NET 8 / ASP.NET Core）
│   ├── ECommerce/        解决方案，按分层拆成多个类库 + Api.Admin 启动工程
│   └── SqlScript/         数据库脚本
├── Admin/                管理后台（Web）
│   └── admin/            Vue 3 + Vite 项目
├── Couser/               微信小程序 · 用户端
│   └── miniprogram/
└── Merchat/              微信小程序 · 商家端
    └── miniprogram/
```

## 技术栈

| 模块 | 技术 |
| --- | --- |
| 后端 API | .NET 8（ASP.NET Core）+ EF Core + MySQL |
| 管理后台 | Vue 3 + Vite + TypeScript |
| 用户端小程序 | 微信小程序原生（TypeScript + Less） |
| 商家端小程序 | 微信小程序原生（TypeScript） |

## 本地运行

### 后端
```bash
cd Api/ECommerce/ECommerce.Api.Admin
dotnet restore
dotnet run            # 默认监听 http://localhost:6001
```
首次运行前，把 `appsettings.example.json` 复制成 `appsettings.json`，把里面的 `Jwt:SecretKey` 换成你自己的密钥，并按本机改数据库连接等配置。`appsettings.json` 不会进版本库，放心放本地配置。

### 管理后台
```bash
cd Admin/admin
npm install
npm run dev          # 本地开发预览
npm run build        # 打包，产物在 dist/
```
接口地址在 `.env.development`（本地）和 `.env.production`（线上）里用 `VITE_API_BASE_URL` 配置，按部署环境改一下。

### 微信小程序
用**微信开发者工具**打开对应目录：
- 用户端：导入 `Couser` 文件夹，小程序根目录填 `Couser/miniprogram`
- 商家端：导入 `Merchat` 文件夹，小程序根目录填 `Merchat/miniprogram`

第一次打开工具会提示「构建 npm」，点一下让它重新生成 `miniprogram_npm`。两个小程序的 `appid` 写在各自的 `project.config.json` 里，记得换成你自己的。

## 配置与密钥
- 后端 `appsettings.json`（含 JWT 密钥、RabbitMQ 等）已通过 `.gitignore` 排除，仓库只保留脱敏的 `appsettings.example.json`。**部署到公开仓库前，请确认没有把真实密钥提交进去，并建议把现有 JWT 密钥轮换掉。**
- 管理后台的接口地址、小程序的 `appid` 都属于环境相关配置，按自己的环境修改。
- `node_modules`、`dist`、小程序构建产物、`project.private.config.json`、`.NET` 的 `bin/obj/.vs` 这些已在 `.gitignore` 里忽略，不会进版本库。
