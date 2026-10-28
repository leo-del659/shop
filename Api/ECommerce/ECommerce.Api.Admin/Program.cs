using ECommerce.Api.Admin.Filters;
using ECommerce.Api.Admin.Middles.Idempotency;
using ECommerce.Api.Admin.Middles.Resilience;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.MQ;
using ECommerce.Infrastructure.Pay;
using ECommerce.Infrastructure.Tools;
using ECommerce.Repository.Instance;
using ECommerce.Repository.Interface;
using ECommerce.Service.Instance;
using ECommerce.Service.Interface;
using IGeekFan.AspNetCore.Knife4jUI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



//指定端口号运行
builder.WebHost.UseUrls(builder.Configuration["host"]);


//builder.Services.AddScoped_Extension();

//批量注入EFCore
builder.Services.AddScoped_Extension(Assembly.Load("ECommerce.Service.EFCore"), Assembly.Load("ECommerce.Service.EFCore"));


//Service
builder.Services.AddScoped_Extension(Assembly.Load("ECommerce.Service.Interface"), Assembly.Load("ECommerce.Service.Instance"));

//builder.Services.AddScoped<Sys_User_EFCore>();

//builder.Services.AddScoped<Sys_Login_Log_EFCore>();


builder.Services.AddScoped<JWT_Helper>();

//泛型注入
builder.Services.AddScoped(typeof(I_Repository<>), typeof(MySql_Repository<>));

//连接字符串在哪？
builder.Services.AddDbContext<_111Context>();
builder.Services.AddScoped<DbContext, _111Context>();

//扩展方法 一次性注入一个程序集 


var provider = builder.Services.BuildServiceProvider();//手动注入



#region Swagger 配置，开启Token授权输入框
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "后台WebApi",
        Version = "v1",
        Description = "接口文档"
    });

    // 【可选】加载项目XML注释，显示接口注释
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, true);

    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "ECommerce.Infrastructure.Dto.xml"), true);

    // 定义JWT授权方案
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "请输入Token"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    // 全局应用授权，所有接口自动识别需要Token
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });


    // 关键配置（解决接口id、服务地址异常）
    c.AddServer(new OpenApiServer { Url = "" });
    c.CustomOperationIds(d =>
    {
        var action = d.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        return $"{action.ControllerName}_{action.ActionName}";
    });



});
#endregion




// 读取Jwt配置
var jwtConfig = builder.Configuration.GetSection("Jwt");
string secretKey = jwtConfig["SecretKey"]!;
byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);

// 注册JWT认证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            // 校验发行者
            ValidateIssuer = true,
            ValidIssuer = jwtConfig["Issuer"],
            // 校验受众
            ValidateAudience = true,
            ValidAudience = jwtConfig["Audience"],
            // 校验密钥
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            // 校验过期时间
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // 取消容错缓冲，到点立即失效
        };


        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var url = context.Request.Path.Value ?? string.Empty;
                var checks = (builder.Configuration["check_login"] ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.ToUpperInvariant())
                    .ToArray();

                if (!checks.Contains(url.ToUpperInvariant()))
                {
                    return Task.CompletedTask;
                }

                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (string.IsNullOrWhiteSpace(authHeader))
                {
                    return Task.CompletedTask;
                }

                if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.CompletedTask;
                }

                var tokenStr = authHeader["Bearer ".Length..].Trim();
                if (string.IsNullOrWhiteSpace(tokenStr))
                {
                    return Task.CompletedTask;
                }

                var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenStr);
                var claim = token.Claims.FirstOrDefault(o => o.Type == "unique_name");
                if (claim == null)
                {
                    context.Request.Headers["Authorization"] = string.Empty;
                    return Task.CompletedTask;
                }

                var roleClaim = token.Claims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value;
                if (string.Equals(roleClaim, "custom", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.CompletedTask;
                }

                var _loginService = provider.GetService<I_Login_Service>() as I_Login_Service;
                if (_loginService == null || _loginService.Check_Login(claim.Value) == null)
                {
                    context.Request.Headers["Authorization"] = string.Empty;
                }

                return Task.CompletedTask;
            }
        };

    });

builder.Services.AddScoped<IGoodsService, GoodsService>();

// 实时通知器：进程内 SSE 广播，单例跨越所有请求共享订阅者
builder.Services.AddSingleton<ECommerce.Service.Interface.IRealtimeNotifier, ECommerce.Service.Instance.RealtimeNotifier>();

// ===== RabbitMQ 订单消息队列 =====
// 配置：appsettings "RabbitMQ" 段；密码可用环境变量 COMMUNITY_RABBITMQ_PASSWORD 覆盖（不落源码）
builder.Services.Configure<RabbitMQOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.PostConfigure<RabbitMQOptions>(opt =>
{
    var pwd = System.Environment.GetEnvironmentVariable("COMMUNITY_RABBITMQ_PASSWORD");
    if (!string.IsNullOrWhiteSpace(pwd)) opt.Password = pwd;
});
// 连接管理（单例，懒连接 + 失败降级）与拓扑声明
builder.Services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();
builder.Services.AddSingleton<RabbitMQTopology>();
// 进程内降级总线（RabbitMQ 不可用时保证订单事件链路不断）
builder.Services.AddSingleton<InProcessOrderEventBus>();
// 订单事件发布器（下单/支付/取消/收货/商户动作统一入口）
builder.Services.AddSingleton<IOrderEventPublisher, OrderEventPublisher>();
// 订单事件消费者（结算 / 自动关单 / 库存预警 / 跨实例 SSE 广播）
builder.Services.AddHostedService<ECommerce.Service.Instance.OrderSettlementConsumerHost>();
builder.Services.AddHostedService<ECommerce.Service.Instance.OrderAutoCancelConsumerHost>();
builder.Services.AddHostedService<ECommerce.Service.Instance.OrderStockAlertConsumerHost>();
builder.Services.AddHostedService<ECommerce.Service.Instance.OrderRealtimeFanoutConsumerHost>();

// ===== 微信支付 v3（JSAPI 下单 / 回调 / 转账到银行卡）=====
// 配置：appsettings "WeChatPay" 段；私钥可用环境变量 COMMUNITY_WXPAY_PRIVATEKEY 覆盖（不落源码）
builder.Services.Configure<WeChatPayOptions>(builder.Configuration.GetSection("WeChatPay"));
builder.Services.PostConfigure<WeChatPayOptions>(opt =>
{
    var pk = System.Environment.GetEnvironmentVariable("COMMUNITY_WXPAY_PRIVATEKEY");
    if (!string.IsNullOrWhiteSpace(pk)) opt.PrivateKey = pk;
});
// 微信支付模块（Enabled=false 走开发模拟模式，无需商户号即可联调；配齐凭证置 true 即上线）
builder.Services.AddScoped<IWeChatPayClient, WeChatPayClient>();
// 支付编排服务（IWeChatPayService 已通过下方 AddScoped_Extension 按 [Provider_]/[Inject_] 自动注册）

// 韧性保护链：限流 → 隔离 → 超时 → 重试 → 熔断 → 降级
var resilienceOpt = builder.Configuration.GetSection("Resilience").Get<ResilienceOptions>() ?? new ResilienceOptions();
// 支持通过环境变量覆盖 Redis 连接（方便不同环境切换地址/密码，且密码不进入源码/配置文件）：
//   COMMUNITY_REDIS_CONNECTION  完整连接串，存在则覆盖 appsettings 的 Redis:Configuration
//   COMMUNITY_REDIS_PASSWORD    单独的密码，会注入到连接串（与地址分离管理，更利于保密）
{
    var envConn = System.Environment.GetEnvironmentVariable("COMMUNITY_REDIS_CONNECTION");
    if (string.IsNullOrWhiteSpace(envConn)) envConn = resilienceOpt.Redis.Configuration;
    var envPwd = System.Environment.GetEnvironmentVariable("COMMUNITY_REDIS_PASSWORD");
    if (!string.IsNullOrWhiteSpace(envPwd))
    {
        var segs = envConn.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(p => !p.StartsWith("password=", StringComparison.OrdinalIgnoreCase)).ToArray();
        envConn = string.Join(",", segs) + (segs.Length > 0 ? "," : "") + "password=" + envPwd;
    }
    resilienceOpt.Redis.Configuration = envConn;
}
builder.Services.AddSingleton<ResilienceOptions>(resilienceOpt);
builder.Services.AddSingleton<ResilienceState>();
// 分布式（Redis）实现：限流/熔断状态存放到 Redis，多实例共享；配置为空时自动回退进程内。
builder.Services.AddSingleton<RedisResilience>();

// 幂等保护：复用 Resilience 的 Redis 配置（多实例共享/进程内兜底）
builder.Services.AddSingleton<IdempotencyStore>(sp =>
{
    var ro = sp.GetRequiredService<ResilienceOptions>();
    return new IdempotencyStore(ro.Redis);
});

// 聊天 WebSocket 连接管理器（单例）：维护在线连接，实时推送买卖家消息
builder.Services.AddSingleton<ECommerce.Service.Interface.WebSockets.ChatWebSocketManager>();



//配置跨域选项

// 1. 注册跨域服务
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevAll", policy =>
    {
        policy.AllowAnyOrigin()    // 允许任意前端地址
              .AllowAnyHeader()    // 允许任意请求头（含Token）
              .AllowAnyMethod();   // 允许GET/POST/PUT/DELETE等所有请求方式
    });
});



builder.Services.AddControllers(opt =>
{
    // 关闭“非可空引用类型(string Id 等)被隐式视为必填”的默认行为：
    // 新增(Add)时前端不传 Id，由服务端 service 用 Config.NewSequentialGuid() 生成，避免 400 "Id field is required"
    opt.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    // 全局注册，所有Controller/Action自动生效
    opt.Filters.Add<Action_Filter>();
    // 接口/按钮级权限过滤器（复杂 RBAC：[Permission_] 标记校验）
    opt.Filters.Add<Permission_Filter>();
    // 幂等过滤器（仅对标记 [Idempotent_] 的写接口生效；Redis 不可用时自动回退进程内）
    opt.Filters.Add<Idempotency_Filter>();
})
// 全局 JSON 序列化配置：camelCase 命名（与前端 JS 对齐）
.AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);






var app = builder.Build();

// RabbitMQ 启动预热：立即建连并声明拓扑。失败不阻断启动——运行期消费者会自动重连，
// 期间订单事件走进程内降级链路，功能不中断。
try
{
    var mqWarmup = app.Services.GetRequiredService<IRabbitMQConnection>();
    using var warmChannel = mqWarmup.TryCreateModel();
    if (warmChannel != null)
        app.Logger.LogInformation("RabbitMQ 订单消息队列就绪（exchange={Exchange}）",
            builder.Configuration["RabbitMQ:Exchange"] ?? "community.order");
    else
        app.Logger.LogWarning("RabbitMQ 当前不可用：订单事件暂时走进程内降级链路，后台将自动重连");
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "RabbitMQ 启动预热异常（忽略，运行期自动重连）");
}

app.UseMiddleware<ECommerce.Api.Admin.Middles.Exception_MiddleWare>();//异常

app.Use((context, next) =>
{
    context.Request.EnableBuffering();//允许重复读取参数
    return next(context);
});

// 2. 启用跨域中间件（顺序关键！）跨域 可以仅次于异常中间件
app.UseCors("DevAll");

// 韧性保护链（限流/隔离/超时/重试/熔断/降级）。放在 CORS 之后、鉴权与控制器之前，
// 使得“限流”以最低成本拦住过量请求，且整条下游管道（鉴权+业务）都受到保护；
// 同时复用 CORS 已写入的响应头，保证浏览器能正常读取降级结果。
app.UseMiddleware<ECommerce.Api.Admin.Middles.Resilience.Resilience_MiddleWare>();

// 3. 静态文件映射：让数据库里小程序风格的 /images/products/xxx.webp 路径
// 在后台也能直接访问，指向与 FileController 相同的 uploads 目录。
var uploadDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "uploads");
if (!Directory.Exists(uploadDir))
{
    Directory.CreateDirectory(uploadDir);
}
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/images/products",
    FileProvider = new PhysicalFileProvider(uploadDir)
});


// 开发环境启用文档
if (app.Environment.IsDevelopment())
{

}



// 启用 WebSocket（聊天实时通信）：保活 120s，避免运营商/NAT 过早回收连接
// 注意：必须注册在 Swagger / Knife4UI 之前——Knife4UI 的 RoutePrefix 被设为空字符串，
// 会拦截所有非 /api 开头的路径并直接返回 404，导致 /ws/chat 握手根本到不了聊天中间件。
// 聊天中间件内部自带 JWT 鉴权（query 传 token），不依赖框架的 UseAuthentication/UseAuthorization。
app.UseWebSockets(new Microsoft.AspNetCore.Builder.WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
});
// 聊天 WebSocket 中间件（仅匹配 /ws/chat，其余请求放行给后续管道）
app.UseMiddleware<ECommerce.Api.Admin.WebSockets.ChatWebSocketMiddleware>();

//开发 生产

app.UseSwagger();
//接口文档
app.UseKnife4UI(d =>
{
    d.RoutePrefix = String.Empty;
    d.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});

// 启用认证授权中间件（顺序不能颠倒！）
app.UseAuthentication();//这里实现了token 认证 但是 它会认证什么东西？只会去确认token 是否有效
app.UseAuthorization();

app.MapControllers();

app.Run();
