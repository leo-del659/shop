namespace ECommerce.Api.Admin.Middles.Resilience
{
    /// <summary>限流计数维度：决定“一次请求按谁来计算配额”。</summary>
    public enum RateLimitDimension
    {
        /// <summary>按用户端 IP（默认，兼容现状）。NAT/代理后多用户共享 IP 时会互相影响。</summary>
        Ip,
        /// <summary>按登录令牌(Bearer)。不同登录用户各自独立配额；匿名(无 token)请求回退到 IP。</summary>
        Token,
        /// <summary>按 JWT 中的用户标识(unique_name 等)。最精确地按“账号”限流；解析失败或匿名时回退 Token→IP。</summary>
        User
    }

    /// <summary>
    /// 韧性（保护链）配置，从 appsettings.json 的 "Resilience" 节点绑定。
    /// 链顺序：限流 → 隔离 → 超时 → 重试 → 熔断 → 降级。
    /// </summary>
    public class ResilienceOptions
    {
        /// <summary>是否整体开启保护链（关闭后中间件直接放行）</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>限流：每个滑动窗口允许的请求数（按 用户端+接口分组 统计）</summary>
        public int RateLimitPermits { get; set; } = 60;

        /// <summary>限流：滑动窗口大小（秒）</summary>
        public int RateLimitWindowSeconds { get; set; } = 10;

        /// <summary>隔离（舱壁）：每个接口分组允许的最大并发数，超出进入等待队列</summary>
        public int BulkheadMaxConcurrent { get; set; } = 30;

        /// <summary>隔离（舱壁）：每个接口分组允许排队等待的额外请求数</summary>
        public int BulkheadQueue { get; set; } = 10;

        /// <summary>隔离（舱壁）：排队获取资源的最长等待时间（毫秒），超时即降级</summary>
        public int BulkheadQueueTimeoutMs { get; set; } = 500;

        /// <summary>超时：单个请求的最大处理时间（毫秒），到点立即取消并快速释放</summary>
        public int TimeoutMs { get; set; } = 8000;

        /// <summary>重试：最大重试次数（仅对“安全方法”的瞬时故障生效）</summary>
        public int RetryCount { get; set; } = 2;

        /// <summary>重试：退避基数（毫秒），实际等待 = 基数 * 2^(尝试序号)</summary>
        public int RetryBaseDelayMs { get; set; } = 200;

        /// <summary>熔断：连续失败达到该阈值后，熔断开启（进入 Open 状态）</summary>
        public int CircuitFailureThreshold { get; set; } = 5;

        /// <summary>熔断：Open 状态持续时长（秒），之后进入 HalfOpen 试探</summary>
        public int CircuitOpenSeconds { get; set; } = 30;

        /// <summary>熔断：HalfOpen 状态下允许的试探请求数</summary>
        public int CircuitHalfOpenTrial { get; set; } = 1;

        /// <summary>
        /// 安全方法（幂等）：仅这些方法在发生瞬时故障时执行“重试”，避免非幂等写操作被重复执行。
        /// 默认 GET/HEAD/OPTIONS。
        /// </summary>
        public string[] SafeMethods { get; set; } = new[] { "GET", "HEAD", "OPTIONS" };

        /// <summary>需要从保护链中排除的路径前缀（文档/静态资源/健康检查），逗号分隔</summary>
        public string[] ExcludePathPrefixes { get; set; } = new[]
        {
            "/swagger", "/knife4j", "/doc.html", "/v3", "/index.html",
            "/images/products", "/api/file", "/favicon.ico", "/health"
        };

        /// <summary>
        /// 限流计数维度（仅影响“限流”，不影响熔断/隔离）。
        /// Ip=按用户端IP（默认，兼容现状）；Token=按登录令牌区分不同用户；User=按 JWT 用户标识。
        /// 无论哪种维度，最终 key 仍为 “身份|接口分组”，即“每身份每接口”独立配额。
        /// 匿名请求（无 token）一律回退到 IP，避免登录等匿名接口被并入同一匿名桶。
        /// </summary>
        public RateLimitDimension RateLimitDimension { get; set; } = RateLimitDimension.Ip;

        /// <summary>Redis 分布式配置：限流与熔断的状态存放到 Redis，实现多实例共享。空=关闭，回退进程内。</summary>
        public RedisOptions Redis { get; set; } = new();
    }

    /// <summary>分布式（Redis）相关配置。Configuration 为空表示不使用 Redis，限流/熔断回退为进程内实现。</summary>
    public class RedisOptions
    {
        /// <summary>
        /// StackExchange.Redis 连接串，例如 "localhost:6379"、"127.0.0.1:6379,password=xxx,abortConnect=false"。
        /// 该值优先被环境变量 COMMUNITY_REDIS_CONNECTION（完整串）或 COMMUNITY_REDIS_PASSWORD（仅密码）覆盖，
        /// 以便不同环境切换 Redis 地址/密码，且密码不进入源代码/配置文件。
        /// </summary>
        public string Configuration { get; set; } = "";

        /// <summary>Redis key 前缀，避免与其它业务 key 冲突</summary>
        public string KeyPrefix { get; set; } = "community:resilience";

        /// <summary>是否启用分布式（连接串非空即启用）</summary>
        public bool Enabled => !string.IsNullOrWhiteSpace(Configuration);
    }
}
