using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Api.Admin.Middles.Resilience
{
    /// <summary>
    /// 韧性中间件：把“限流 → 隔离 → 超时 → 重试 → 熔断 → 降级”串成一条保护链。
    ///
    /// 执行顺序：
    ///   1) 限流 RateLimit      —— 滑动窗口拦住过量请求（429）。分布式时走 Redis，多实例共享计数。
    ///   2) 熔断(前置) Circuit   —— 已熔断则直接短路（503），半开时放行有限试探。分布式时状态存 Redis。
    ///   3) 隔离 Bulkhead       —— 领取本实例专属并发配额，拿不到则降级（503）。（单实例语义，保持进程内）
    ///   4) 超时 Timeout        —— 单请求超过阈值即取消并快速释放（504）
    ///   5) 重试 Retry          —— 仅“安全方法”的瞬时故障做指数退避重试
    ///   6) 熔断(记录)+降级      —— 成败计入熔断；任何拒绝都返回友好 JSON
    /// </summary>
    public class Resilience_MiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly ResilienceOptions _opt;
        private readonly ResilienceState _state;
        private readonly RedisResilience _redis;
        private readonly ILogger<Resilience_MiddleWare> _logger;
        private readonly IConfiguration _config;
        private readonly bool _useRedis;

        public Resilience_MiddleWare(RequestDelegate next, ResilienceOptions opt, ResilienceState state, RedisResilience redis, ILogger<Resilience_MiddleWare> logger, IConfiguration configuration)
        {
            _next = next;
            _opt = opt;
            _state = state;
            _redis = redis;
            _logger = logger;
            _config = configuration;
            _useRedis = redis.Enabled;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_opt.Enabled || IsExcluded(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var group = GroupKey(context.Request);
            var clientKey = GetRateLimitIdentity(context) + "|" + group;

            // ---------- 1. 限流 ----------
            bool allowed = _useRedis
                ? await _redis.RateLimitAllowAsync(clientKey, _opt.RateLimitPermits, _opt.RateLimitWindowSeconds)
                : _state.GetLimiter(clientKey).TryAcquire(DateTime.UtcNow.Ticks, _opt.RateLimitPermits);
            if (!allowed)
            {
                _state.IncRateLimited();
                _state.IncFallback();
                await WriteFriendly(context, 429, -429, "请求过于频繁，请稍后再试", "RateLimit");
                return;
            }

            // ---------- 2. 熔断（前置短路）----------
            bool circuitAllow = _useRedis
                ? await _redis.CircuitAllowAsync(group, _opt.CircuitFailureThreshold, _opt.CircuitOpenSeconds, _opt.CircuitHalfOpenTrial)
                : CircuitAllowInProcess(group);
            if (!circuitAllow)
            {
                _state.IncCircuitRejected();
                _state.IncFallback();
                await WriteFriendly(context, 503, -503, "服务暂时不可用（熔断保护已开启），请稍后重试", "CircuitBreaker");
                return;
            }

            // ---------- 3. 隔离（舱壁）----------
            var bulkhead = _state.GetBulkhead(group);
            bool acquired;
            try { acquired = await bulkhead.WaitAsync(_opt.BulkheadQueueTimeoutMs); }
            catch { acquired = false; }

            if (!acquired)
            {
                _state.IncBulkheadRejected();
                _state.IncFallback();
                await WriteFriendly(context, 503, -503, "当前请求过多，已为您分配专属资源失败，请稍后重试", "Bulkhead");
                return;
            }
            _state.IncBulkheadInUse(group, 1);

            try
            {
                await ExecuteWithTimeoutAndRetry(context, group);
            }
            finally
            {
                _state.IncBulkheadInUse(group, -1);
                try { bulkhead.Release(); } catch { /* 重复释放保护 */ }
            }
        }

        /// <summary>超时 + 重试执行，并负责“熔断记录 + 降级收尾”。</summary>
        private async Task ExecuteWithTimeoutAndRetry(HttpContext context, string group)
        {
            int attempt = 0;
            while (true)
            {
                Exception? ex = null;
                bool timedOut = false;

                using var cts = new CancellationTokenSource();
                cts.CancelAfter(_opt.TimeoutMs);
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, context.RequestAborted);

                var nextTask = _next(context);
                var timeoutTask = Task.Delay(_opt.TimeoutMs, linked.Token);
                var completed = await Task.WhenAny(nextTask, timeoutTask);

                if (completed == timeoutTask && !nextTask.IsCompleted)
                {
                    // ---------- 4. 超时 ----------
                    cts.Cancel();                 // 通知下游(EF/IO)尽快释放
                    timedOut = !context.Response.HasStarted;
                    // 不等待原任务，避免用户端长时间阻塞（已“快速释放”）
                }
                else
                {
                    try { await nextTask; }
                    catch (OperationCanceledException) when (cts.IsCancellationRequested)
                    {
                        timedOut = !context.Response.HasStarted;
                    }
                    catch (Exception e) { ex = e; }
                }

                bool downstream5xx = context.Response.HasStarted && context.Response.StatusCode >= 500;
                bool transient = timedOut || downstream5xx || IsTransient(ex);

                // ---------- 5. 重试（仅安全方法、响应未写出、瞬时故障）----------
                bool canRetry = (ex != null || downstream5xx || timedOut)
                                && !context.Response.HasStarted
                                && IsSafeMethod(context.Request)
                                && attempt < _opt.RetryCount
                                && transient;

                if (canRetry)
                {
                    attempt++;
                    _state.IncRetry();
                    await Task.Delay(_opt.RetryBaseDelayMs * (1 << (attempt - 1)), context.RequestAborted);
                    // 重试前若响应体被部分写入则无法重试；上面已用 HasStarted 守护
                    continue;
                }

                // ---------- 6. 熔断记录 + 降级 ----------
                bool success = ex == null && !timedOut && !downstream5xx;
                await RecordCircuitAsync(group, success);

                if (timedOut && !context.Response.HasStarted)
                {
                    _state.IncTimeout();
                    _state.IncFallback();
                    await WriteFriendly(context, 504, -504, "请求处理超时，已为您快速释放，请稍后重试", "Timeout");
                    return;
                }

                // 非瞬时异常（下游真实抛出，且非超时、非 5xx 下游）：此前被静默吞掉，
                // 表现为“HTTP 200 空 body”且毫无日志，会掩盖真实报错。
                // 这里强制记录日志，并在响应尚未写出时降级为 500 友好响应；
                // 若响应已经开始写出（如下游部分写体后抛序列化异常）则仅记录，避免破坏已发出的数据。
                if (ex != null && !downstream5xx)
                {
                    WriteErrorLog(context, ex);
                    _logger.LogError(ex,
                        "Resilience 捕获到未处理异常 group={Group} {Method} {Path}：真实错误已被中间件拦截，请排查下游控制器/业务。",
                        group, context.Request.Method, context.Request.Path);
                    if (!context.Response.HasStarted)
                    {
                        _state.IncFallback();
                        await WriteFriendly(context, 500, -500, "服务处理异常，请稍后重试", "UnhandledException");
                        return;
                    }
                    return;
                }
                return;
            }
        }

        // -------------------- 工具方法 --------------------

        private bool CircuitAllowInProcess(string group)
        {
            var c = _state.GetCircuit(group);
            return !(c.IsOpenNow(DateTime.UtcNow) && !c.TryTakeHalfOpenTrial());
        }

        private async Task RecordCircuitAsync(string group, bool success)
        {
            if (_useRedis)
            {
                await _redis.CircuitRecordAsync(group, success, _opt.CircuitFailureThreshold, _opt.CircuitOpenSeconds);
            }
            else
            {
                var c = _state.GetCircuit(group);
                if (success) c.RecordSuccess(); else c.RecordFailure();
            }
        }

        private static bool IsTransient(Exception? ex) =>
            ex is TimeoutException
            or TaskCanceledException
            or OperationCanceledException
            or IOException
            or HttpRequestException;

        private bool IsSafeMethod(HttpRequest req) =>
            _opt.SafeMethods.Contains(req.Method, StringComparer.OrdinalIgnoreCase);

        private static string GroupKey(HttpRequest req)
        {
            // 以 “方法 + 前两段路径” 作为分组，剥离子 id，使同类接口共享同一保护配额
            var seg = req.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Take(2).Select(s => s.ToLowerInvariant()).ToArray() ?? Array.Empty<string>();
            var path = seg.Length > 0 ? string.Join("/", seg) : "root";
            return $"{req.Method.ToUpperInvariant()}:/{path}";
        }

        /// <summary>
        /// 限流身份标识：按配置维度返回 “ip:... / tok:... / u:...”。
        /// 匿名请求（无 Bearer）或解析失败时统一回退到 IP，避免登录等匿名接口被并入同一匿名桶。
        /// 注：本中间件在鉴权之前执行，故直接读取原始 Authorization 头并解码 JWT payload（不验签，验签交给下游）。
        /// </summary>
        private string GetRateLimitIdentity(HttpContext context)
        {
            switch (_opt.RateLimitDimension)
            {
                case RateLimitDimension.Token:
                {
                    var token = ExtractBearerToken(context);
                    return token != null ? "tok:" + Sha256(token) : "ip:" + ClientIp(context);
                }
                case RateLimitDimension.User:
                {
                    var token = ExtractBearerToken(context);
                    if (token != null)
                    {
                        var user = ExtractJwtUser(token);
                        if (!string.IsNullOrWhiteSpace(user)) return "u:" + Sha256(user);
                        return "tok:" + Sha256(token); // 解出令牌但取不到用户标识时退化为令牌维度
                    }
                    return "ip:" + ClientIp(context);
                }
                default: // Ip
                    return "ip:" + ClientIp(context);
            }
        }

        /// <summary>取真实用户端 IP（尊重 X-Forwarded-For，兼容反向代理/NAT 场景）</summary>
        private static string ClientIp(HttpContext context)
        {
            var fwd = context.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(fwd))
                return fwd.Split(',')[0].Trim();
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private static string? ExtractBearerToken(HttpContext context)
        {
            var auth = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return null;
            var token = auth.Substring("Bearer ".Length).Trim();
            return string.IsNullOrWhiteSpace(token) ? null : token;
        }

        /// <summary>解码 JWT payload（中段，base64url），取出用户标识；失败返回 null。仅读声明，不验签。</summary>
        private static string? ExtractJwtUser(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2) return null;
                var payload = parts[1];
                var pad = payload.Length % 4 == 0 ? "" : new string('=', 4 - (payload.Length % 4));
                var json = Encoding.UTF8.GetString(
                    Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/') + pad));
                using var doc = JsonDocument.Parse(json);
                // 优先本项目的 unique_name，其次常见标准/兼容声明名
                foreach (var name in new[] { "unique_name", "sub", "name", "uid", "userId", "nameid",
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" })
                {
                    if (doc.RootElement.TryGetProperty(name, out var v))
                    {
                        var s = v.GetString();
                        if (!string.IsNullOrWhiteSpace(s)) return s;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>SHA256 哈希（十六进制），用于把 token/用户标识转换为定长、不可逆的 key 片段，避免明文密钥/PII 落入 Redis。</summary>
        private static string Sha256(string s)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(s)));
        }

        private static bool IsExcluded(PathString path)
        {
            var p = path.Value ?? string.Empty;
            var prefixes = new[]
            {
                "/swagger", "/knife4j", "/doc.html", "/v3", "/index.html",
                "/images/products", "/api/file", "/favicon.ico", "/health"
            };
            return prefixes.Any(pre => p.StartsWith(pre, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 把被韧性中间件拦截的下游异常按全局异常处理器相同格式写入错误日志文件（Logs:Error_Log/{yyyyMMdd}.txt），
        /// 避免“下游真实异常被静默吞掉、无任何落盘痕迹”的盲区。写入失败不影响响应。
        /// </summary>
        private void WriteErrorLog(HttpContext context, Exception ex)
        {
            try
            {
                var logPath = _config["Logs:Error_Log"];
                if (string.IsNullOrWhiteSpace(logPath)) return;
                var fullPath = System.IO.Path.Combine(logPath, DateTime.Now.ToString("yyyyMMdd") + ".txt");
                var dir = System.IO.Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);

                var fullEx = ex;
                var chain = new System.Text.StringBuilder();
                int depth = 0;
                while (fullEx != null && depth < 5)
                {
                    if (depth > 0) chain.Append("  ---> ");
                    chain.AppendLine(fullEx.GetType().Name + ": " + fullEx.Message);
                    fullEx = fullEx.InnerException;
                    depth++;
                }
                if (ex.StackTrace != null)
                    chain.AppendLine("\r\n[StackTrace] " + ex.StackTrace.Substring(0, Math.Min(ex.StackTrace.Length, 800)));

                System.IO.File.AppendAllText(fullPath,
                    $"时间:{DateTime.Now.ToString()}\r\n地址：{context.Request.Path}\r\n[Resilience拦截] " +
                    $"{context.Request.Method} 被中间件捕获（此前会被静默吞掉，无日志）\r\n{chain}\r\n");
            }
            catch { /* 日志写入失败不应阻塞响应 */ }
        }

        private static async Task WriteFriendly(HttpContext context, int httpStatus, int code, string message, string stage)
        {
            if (context.Response.HasStarted) return;
            // 注意：CORS 中间件已在本中间件之前设置好跨域响应头，这里不调用 Clear()，
            // 以免把 Access-Control-Allow-Origin 等头清掉导致浏览器无法读取降级结果。
            context.Response.StatusCode = httpStatus;
            context.Response.ContentType = "application/json; charset=utf-8";
            var payload = new
            {
                code,
                message,
                data = new { stage, timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}
