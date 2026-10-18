using ECommerce.Api.Admin.Middles.Resilience;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 韧性保护链的可观测端点：查看指标、手动重置，以及用于验证整条链路是否生效的测试接口。
    /// </summary>
    [ApiController]
    [Route("api/Resilience")]
    public class ResilienceController : ControllerBase
    {
        private readonly ResilienceState _state;
        private readonly RedisResilience _redis;

        public ResilienceController(ResilienceState state, RedisResilience redis)
        {
            _state = state;
            _redis = redis;
        }

        /// <summary>查看当前限流/熔断/隔离/重试等实时指标（含 Redis 分布式熔断状态）</summary>
        [HttpGet("stats")]
        public async Task<IActionResult> Stats()
        {
            var snapshot = _state.Snapshot();
            var redisCircuits = _redis.Enabled ? await _redis.GetCircuitsAsync() : new List<object>();
            return Ok(new
            {
                distributed = _redis.Enabled,
                inProcess = snapshot,
                redisCircuits
            });
        }

        /// <summary>重置所有熔断/限流/计数（含 Redis 中的分布式状态，仅演示/排障用）</summary>
        [HttpPost("reset")]
        public async Task<IActionResult> Reset()
        {
            _state.Reset();
            if (_redis.Enabled)
            {
                try
                {
                    var db = _redis.GetDatabase();
                    var server = _redis.GetServer();
                    if (db != null && server != null)
                    {
                        await foreach (var key in server.KeysAsync(pattern: $"{_redis.KeyPrefix}:*"))
                        {
                            await db.KeyDeleteAsync(key);
                        }
                    }
                }
                catch { /* 忽略清理异常 */ }
            }
            return Ok(new { code = 0, message = "已重置韧性状态（含 Redis）" });
        }

        /// <summary>正常请求：用于验证“未触发保护时正常放行”</summary>
        [HttpGet("test/ok")]
        public IActionResult TestOk() => Ok(new { code = 0, message = "ok", data = DateTime.Now });

        /// <summary>慢请求：sleep 超过超时阈值，用于验证“超时 → 快速释放”返回 504</summary>
        [HttpGet("test/delay")]
        public async Task<IActionResult> TestDelay(int ms = 12000)
        {
            await Task.Delay(ms, HttpContext.RequestAborted);
            return Ok(new { code = 0, message = $"delay {ms}ms done" });
        }

        /// <summary>必定失败：用于压测后验证“熔断”被触发（连续失败达到阈值后同组请求返回 503）</summary>
        [HttpGet("test/fail")]
        public IActionResult TestFail() => StatusCode(500, new { code = -500, message = "模拟服务端故障" });

        /// <summary>负载测试用：正常返回 ok，配合脚本高频请求可观察限流/隔离计数</summary>
        [HttpGet("test/load")]
        public IActionResult TestLoad() => Ok(new { code = 0, message = "load ok" });
    }
}
