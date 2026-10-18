using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using ECommerce.Api.Admin.Middles.Resilience;
using StackExchange.Redis;

namespace ECommerce.Api.Admin.Middles.Idempotency
{
    /// <summary>幂等键检查的结果状态。</summary>
    public enum IdemState
    {
        /// <summary>首次请求，已获取处理锁，可正常执行。</summary>
        Locked,
        /// <summary>并发重复请求，首次仍在处理中。</summary>
        Conflict,
        /// <summary>已命中缓存，直接返回此前结果。</summary>
        Done
    }

    /// <summary>幂等键检查结果。</summary>
    public class IdemCheckResult
    {
        public IdemState State { get; set; }
        public int StatusCode { get; set; }
        public string? BodyJson { get; set; }
    }

    /// <summary>
    /// 幂等结果存储：优先 Redis（多实例共享、带 TTL），Redis 不可用/未配置时回退进程内字典。
    /// 容错：任何异常都 fail-open（放行，不阻断业务）。
    /// 记录结构（JSON）：{ "s": "processing"|"done", "c": 状态码, "b": 响应体JSON字符串 }
    /// </summary>
    public sealed class IdempotencyStore
    {
        private readonly RedisOptions _redis;
        private readonly object _connLock = new();
        private IConnectionMultiplexer? _mux;
        private bool _initTried;

        // 进程内兜底（Redis 未配置或不可用时）
        private readonly ConcurrentDictionary<string, MemEntry> _mem = new();
        private DateTime _lastSweep = DateTime.UtcNow;

        private class MemEntry
        {
            public string Status { get; set; } = "processing";
            public int StatusCode { get; set; }
            public string? BodyJson { get; set; }
            public DateTime ExpireAt { get; set; }
        }

        public bool RedisEnabled => !string.IsNullOrWhiteSpace(_redis.Configuration);

        public IdempotencyStore(RedisOptions redis) => _redis = redis;

        // ---------------- Redis 懒连接（与 RedisResilience 同风格） ----------------
        private IDatabase? Db
        {
            get
            {
                if (!RedisEnabled) return null;
                if (_mux == null && !_initTried)
                {
                    lock (_connLock)
                    {
                        if (_mux == null && !_initTried)
                        {
                            try
                            {
                                var cfg = ConfigurationOptions.Parse(_redis.Configuration);
                                cfg.AbortOnConnectFail = false; // 连接失败不抛，交给 fail-open
                                _mux = ConnectionMultiplexer.Connect(cfg);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"[IdempotencyStore] Redis 连接失败，回退进程内: {ex.Message}");
                                _mux = null;
                            }
                            finally { _initTried = true; }
                        }
                    }
                }
                if (_mux != null && !_mux.IsConnected)
                {
                    try { _mux = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(_redis.Configuration)); }
                    catch { _mux = null; }
                }
                return _mux?.GetDatabase();
            }
        }

        private string FullKey(string key) => $"{_redis.KeyPrefix}:idem:{key}";

        // 原子地：存在 done→返回记录；存在 processing→CONFLICT；不存在→SET processing(NX)→LOCKED
        private const string AcquireScript = @"
local cur = redis.call('GET', KEYS[1])
if cur then
  local obj = cjson.decode(cur)
  if obj.s == 'done' then return cur end
  return 'CONFLICT'
end
local ok = redis.call('SET', KEYS[1], ARGV[1], 'NX', 'PX', ARGV[2])
if ok then return 'LOCKED' end
local cur2 = redis.call('GET', KEYS[1])
if cur2 then
  local obj = cjson.decode(cur2)
  if obj.s == 'done' then return cur2 end
end
return 'CONFLICT'";

        /// <summary>尝试获取锁：LOCKED=首次；DONE=返回已缓存结果；CONFLICT=并发处理中。</summary>
        public async Task<IdemCheckResult> AcquireOrGetAsync(string key, int lockSeconds)
        {
            var db = Db;
            if (db != null)
            {
                try
                {
                    var processing = JsonSerializer.Serialize(new { s = "processing" });
                    var res = (string?)await db.ScriptEvaluateAsync(AcquireScript,
                        new RedisKey[] { FullKey(key) },
                        new RedisValue[] { processing, lockSeconds * 1000L });
                    if (res == "LOCKED") return new IdemCheckResult { State = IdemState.Locked };
                    if (res == "CONFLICT") return new IdemCheckResult { State = IdemState.Conflict };
                    // 否则为 done 记录 JSON
                    var obj = JsonSerializer.Deserialize<IdemRecord>(res!);
                    if (obj != null && obj.s == "done")
                        return new IdemCheckResult { State = IdemState.Done, StatusCode = obj.c, BodyJson = obj.b };
                    return new IdemCheckResult { State = IdemState.Conflict };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[IdempotencyStore] Acquire 异常，回退进程内: {ex.Message}");
                }
            }
            return AcquireOrGetMemory(key, lockSeconds);
        }

        /// <summary>写入成功结果（覆盖 processing 为 done，带 TTL）。</summary>
        public async Task StoreAsync(string key, int statusCode, string bodyJson, int ttlSeconds)
        {
            var db = Db;
            if (db != null)
            {
                try
                {
                    var record = JsonSerializer.Serialize(new IdemRecord { s = "done", c = statusCode, b = bodyJson });
                    await db.StringSetAsync(FullKey(key), record, TimeSpan.FromSeconds(ttlSeconds));
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[IdempotencyStore] Store 异常，回退进程内: {ex.Message}");
                }
            }
            StoreMemory(key, statusCode, bodyJson, ttlSeconds);
        }

        /// <summary>移除处理中锁（业务失败/异常时，允许用户端修正后重试）。</summary>
        public async Task RemoveAsync(string key)
        {
            var db = Db;
            if (db != null)
            {
                try { await db.KeyDeleteAsync(FullKey(key)); return; }
                catch (Exception ex) { Debug.WriteLine($"[IdempotencyStore] Remove 异常: {ex.Message}"); }
            }
            _mem.TryRemove(key, out _);
        }

        // ---------------- 进程内回退 ----------------
        private IdemCheckResult AcquireOrGetMemory(string key, int lockSeconds)
        {
            SweepMemory();
            lock (_mem)
            {
                if (_mem.TryGetValue(key, out var e) && e.ExpireAt > DateTime.UtcNow)
                {
                    if (e.Status == "done")
                        return new IdemCheckResult { State = IdemState.Done, StatusCode = e.StatusCode, BodyJson = e.BodyJson };
                    return new IdemCheckResult { State = IdemState.Conflict };
                }
                _mem[key] = new MemEntry { Status = "processing", ExpireAt = DateTime.UtcNow.AddSeconds(lockSeconds) };
                return new IdemCheckResult { State = IdemState.Locked };
            }
        }

        private void StoreMemory(string key, int statusCode, string bodyJson, int ttlSeconds)
        {
            _mem[key] = new MemEntry
            {
                Status = "done",
                StatusCode = statusCode,
                BodyJson = bodyJson,
                ExpireAt = DateTime.UtcNow.AddSeconds(ttlSeconds)
            };
        }

        private void SweepMemory()
        {
            if ((DateTime.UtcNow - _lastSweep).TotalSeconds < 60) return;
            _lastSweep = DateTime.UtcNow;
            var now = DateTime.UtcNow;
            foreach (var kv in _mem)
                if (kv.Value.ExpireAt <= now) _mem.TryRemove(kv.Key, out _);
        }

        private class IdemRecord
        {
            public string s { get; set; } = "";
            public int c { get; set; }
            public string? b { get; set; }
        }
    }
}
