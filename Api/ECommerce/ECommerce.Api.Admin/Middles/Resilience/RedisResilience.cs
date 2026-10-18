using System.Diagnostics;
using StackExchange.Redis;

namespace ECommerce.Api.Admin.Middles.Resilience
{
    /// <summary>
    /// 基于 Redis 的分布式限流与熔断实现。
    /// - 限流：滑动窗口（ZSET + Lua 原子脚本），多实例共享同一计数。
    /// - 熔断：状态机（Closed/Open/HalfOpen）存于 Redis Hash，用 Lua 保证“判定+状态切换”原子，避免多实例竞态。
    ///
    /// 容错策略：Redis 不可用时“放行(fail-open)”，即降级为不拦截，避免保护链本身成为单点故障拖垮整站。
    /// （隔离/超时/重试/降级仍由进程内中间件负责，隔离本就是单实例线程池语义，不应跨实例共享。）
    /// </summary>
    public class RedisResilience
    {
        private readonly ResilienceOptions _opt;
        private readonly object _connLock = new();
        private IConnectionMultiplexer? _mux;
        private bool _initTried;

        public bool Enabled => _opt.Redis.Enabled;

        public string KeyPrefix => _opt.Redis.KeyPrefix;

        public RedisResilience(ResilienceOptions opt) => _opt = opt;

        /// <summary>暴露底层 Database（供 /reset 清理使用；Redis 不可用返回 null）</summary>
        public IDatabase? GetDatabase() => Db;

        /// <summary>暴露一个非副本 Server（供 SCAN 清理/读取；不可用返回 null）</summary>
        public IServer? GetServer()
        {
            if (_mux == null || !_mux.IsConnected) return null;
            return _mux.GetServers().FirstOrDefault(s => !s.IsReplica);
        }

        private IDatabase? Db
        {
            get
            {
                if (!Enabled) return null;
                if (_mux == null && !_initTried)
                {
                    lock (_connLock)
                    {
                        if (_mux == null && !_initTried)
                        {
                            try
                            {
                                var cfg = ConfigurationOptions.Parse(_opt.Redis.Configuration);
                                cfg.AbortOnConnectFail = false; // 连接失败不抛，交给 fail-open
                                _mux = ConnectionMultiplexer.Connect(cfg);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"[RedisResilience] 连接失败，已 fail-open: {ex.Message}");
                                _mux = null;
                            }
                            finally { _initTried = true; }
                        }
                    }
                }
                // 连接断开后尝试重连一次
                if (_mux != null && !_mux.IsConnected)
                {
                    try { _mux = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(_opt.Redis.Configuration)); }
                    catch { _mux = null; }
                }
                return _mux?.GetDatabase();
            }
        }

        private string RlKey(string clientKey) => $"{_opt.Redis.KeyPrefix}:rl:{clientKey}";
        private string CbKey(string group) => $"{_opt.Redis.KeyPrefix}:cb:{group}";

        // ---------------- 限流：滑动窗口 ----------------
        // KEYS[1]=key  ARGV[1]=nowMs  ARGV[2]=windowMs  ARGV[3]=permits
        // 用 INCR 生成唯一 member（Redis 的 Lua math.random 是固定种子，不能作为唯一标识），
        // 否则同一毫秒/多次 EVAL 会因 member 相同而只更新 score，导致计数偏小、限流失效。
        private const string RateLimitScript = @"
redis.call('ZREMRANGEBYSCORE', KEYS[1], 0, ARGV[1] - ARGV[2])
local c = redis.call('ZCARD', KEYS[1])
if c >= tonumber(ARGV[3]) then return 0 end
local seq = redis.call('INCR', KEYS[1] .. ':seq')
redis.call('ZADD', KEYS[1], ARGV[1], ARGV[1] .. ':' .. seq)
redis.call('PEXPIRE', KEYS[1], ARGV[2])
redis.call('PEXPIRE', KEYS[1] .. ':seq', ARGV[2])
return 1";

        /// <summary>限流判定：true=放行，false=超限。Redis 不可用时返回 true（fail-open）。</summary>
        public async Task<bool> RateLimitAllowAsync(string clientKey, int permits, int windowSeconds)
        {
            var db = Db;
            if (db == null) return true;
            try
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var windowMs = windowSeconds * 1000L;
                var res = (long)await db.ScriptEvaluateAsync(RateLimitScript,
                    new RedisKey[] { RlKey(clientKey) },
                    new RedisValue[] { now, windowMs, permits });
                return res == 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RedisResilience] 限流脚本异常，fail-open: {ex.Message}");
                return true;
            }
        }

        // ---------------- 熔断：状态机 ----------------
        // KEYS[1]=key  ARGV[1]=threshold ARGV[2]=openMs ARGV[3]=trial ARGV[4]=nowMs
        private const string CircuitAllowScript = @"
local key = KEYS[1]
local threshold = tonumber(ARGV[1])
local openMs = tonumber(ARGV[2])
local trial = tonumber(ARGV[3])
local now = tonumber(ARGV[4])
local d = redis.call('HMGET', key, 'status', 'failures', 'openedAt', 'trial')
local status = d[1] or 'closed'
local openedAt = tonumber(d[3] or 0)
local trialRem = tonumber(d[4] or 0)
if status == 'open' then
  if now - openedAt >= openMs then
    redis.call('HSET', key, 'status', 'halfopen', 'trial', trial)
    return 1
  end
  return 0
elseif status == 'halfopen' then
  if trialRem > 0 then
    redis.call('HINCRBY', key, 'trial', -1)
    return 1
  end
  return 0
end
return 1";

        // KEYS[1]=key  ARGV[1]=success(1/0) ARGV[2]=threshold ARGV[3]=openMs ARGV[4]=nowMs
        private const string CircuitRecordScript = @"
local key = KEYS[1]
local success = tonumber(ARGV[1])
local threshold = tonumber(ARGV[2])
local openMs = tonumber(ARGV[3])
local now = tonumber(ARGV[4])
if success == 1 then
  redis.call('HSET', key, 'status', 'closed', 'failures', 0, 'trial', 0)
  return
end
local d = redis.call('HMGET', key, 'status', 'failures')
local status = d[1] or 'closed'
local failures = tonumber(d[2] or 0) + 1
if status == 'halfopen' then
  redis.call('HSET', key, 'status', 'open', 'openedAt', now, 'failures', failures, 'trial', 0)
elseif failures >= threshold then
  redis.call('HSET', key, 'status', 'open', 'openedAt', now, 'failures', failures, 'trial', 0)
else
  redis.call('HSET', key, 'failures', failures)
end";

        /// <summary>熔断前置判定：true=放行（含半开试探名额），false=已熔断拒绝。Redis 不可用时返回 true（fail-open）。</summary>
        public async Task<bool> CircuitAllowAsync(string group, int threshold, int openSeconds, int halfOpenTrial)
        {
            var db = Db;
            if (db == null) return true;
            try
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var openMs = openSeconds * 1000L;
                var res = (long)await db.ScriptEvaluateAsync(CircuitAllowScript,
                    new RedisKey[] { CbKey(group) },
                    new RedisValue[] { threshold, openMs, halfOpenTrial, now });
                return res == 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RedisResilience] 熔断判定异常，fail-open: {ex.Message}");
                return true;
            }
        }

        /// <summary>记录一次结果到熔断状态机。success=true 闭合/重置，false 累计失败（达阈值则开启）。</summary>
        public async Task CircuitRecordAsync(string group, bool success, int threshold, int openSeconds)
        {
            var db = Db;
            if (db == null) return;
            try
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var openMs = openSeconds * 1000L;
                await db.ScriptEvaluateAsync(CircuitRecordScript,
                    new RedisKey[] { CbKey(group) },
                    new RedisValue[] { success ? 1 : 0, threshold, openMs, now });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RedisResilience] 熔断记录异常（忽略）: {ex.Message}");
            }
        }

        /// <summary>读取 Redis 中当前所有熔断分组的状态，供 /stats 观测。无可用时返回空列表。</summary>
        public async Task<List<object>> GetCircuitsAsync()
        {
            var db = Db;
            var list = new List<object>();
            if (db == null) return list;
            try
            {
                var server = _mux!.GetServers().FirstOrDefault(s => !s.IsReplica);
                if (server == null) return list;
                var pattern = $"{_opt.Redis.KeyPrefix}:cb:*";
                await foreach (var key in server.KeysAsync(pattern: pattern))
                {
                    var h = await db.HashGetAllAsync(key);
                    var dict = h.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
                    list.Add(new
                    {
                        group = key.ToString().Replace($"{_opt.Redis.KeyPrefix}:cb:", ""),
                        status = dict.GetValueOrDefault("status", "closed"),
                        consecutiveFailures = dict.GetValueOrDefault("failures", "0"),
                        halfOpenTrialsRemaining = dict.GetValueOrDefault("trial", "0")
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RedisResilience] 读取熔断状态异常: {ex.Message}");
            }
            return list;
        }
    }
}
