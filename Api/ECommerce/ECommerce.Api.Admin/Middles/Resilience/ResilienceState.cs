using System.Collections.Concurrent;

namespace ECommerce.Api.Admin.Middles.Resilience
{
    /// <summary>
    /// 熔断状态机（按接口分组）：Closed → Open → HalfOpen → Closed。
    /// 所有判断与状态切换都在锁内完成，保证并发安全。
    /// </summary>
    public class CircuitState
    {
        private readonly object _lock = new();
        private readonly ResilienceOptions _opt;

        public string Group { get; }
        public int ConsecutiveFailures { get; private set; }
        public DateTime OpenUntilUtc { get; private set; }
        public CircuitStatus Status { get; private set; } = CircuitStatus.Closed;
        public int HalfOpenTrialsRemaining { get; private set; }

        public CircuitState(string group, ResilienceOptions opt)
        {
            Group = group;
            _opt = opt;
        }

        /// <summary>判断当前是否“熔断开启”，调用方应直接拒绝并返回降级结果。</summary>
        public bool IsOpenNow(DateTime nowUtc)
        {
            lock (_lock)
            {
                if (Status == CircuitStatus.Open)
                {
                    if (nowUtc >= OpenUntilUtc)
                    {
                        // 时间到，转为半开试探
                        Status = CircuitStatus.HalfOpen;
                        HalfOpenTrialsRemaining = _opt.CircuitHalfOpenTrial;
                        return false;
                    }
                    return true; // 仍处开启，拒绝
                }
                return false;
            }
        }

        /// <summary>尝试占用一次半开试探名额；true 表示放行试探，false 表示半开名额已用完（按开启处理）。</summary>
        public bool TryTakeHalfOpenTrial()
        {
            lock (_lock)
            {
                if (Status != CircuitStatus.HalfOpen) return true;
                if (HalfOpenTrialsRemaining > 0)
                {
                    HalfOpenTrialsRemaining--;
                    return true;
                }
                return false;
            }
        }

        /// <summary>记录一次成功：重置失败计数，半开成功则闭合。</summary>
        public void RecordSuccess()
        {
            lock (_lock)
            {
                ConsecutiveFailures = 0;
                if (Status == CircuitStatus.HalfOpen) Status = CircuitStatus.Closed;
            }
        }

        /// <summary>记录一次失败：累计失败；达到阈值则开启熔断。</summary>
        public void RecordFailure()
        {
            lock (_lock)
            {
                ConsecutiveFailures++;
                if (Status == CircuitStatus.HalfOpen)
                {
                    Open(); // 半开试探失败，重新开启
                }
                else if (ConsecutiveFailures >= _opt.CircuitFailureThreshold)
                {
                    Open();
                }
            }
        }

        private void Open()
        {
            Status = CircuitStatus.Open;
            OpenUntilUtc = DateTime.UtcNow.AddSeconds(_opt.CircuitOpenSeconds);
        }
    }

    public enum CircuitStatus { Closed, Open, HalfOpen }

    /// <summary>
    /// 滑动窗口限流计数器：保留窗口内的请求时间戳，按数量判定是否超限。
    /// </summary>
    public class SlidingWindowCounter
    {
        private readonly object _lock = new();
        private readonly List<long> _timestamps = new();
        private readonly long _windowTicks;

        public SlidingWindowCounter(int windowSeconds)
        {
            _windowTicks = TimeSpan.FromSeconds(windowSeconds).Ticks;
        }

        /// <summary>尝试“领取”一个配额；true=放行，false=超限（并清理过期时间戳）。</summary>
        public bool TryAcquire(long nowTicks, int permits)
        {
            lock (_lock)
            {
                long cutoff = nowTicks - _windowTicks;
                _timestamps.RemoveAll(t => t < cutoff);
                if (_timestamps.Count >= permits) return false;
                _timestamps.Add(nowTicks);
                return true;
            }
        }
    }

    /// <summary>
    /// 进程内共享的韧性状态与指标（注册为单例）。
    /// 包含：限流窗口、熔断状态、隔离舱壁信号量、以及各阶段计数。
    /// </summary>
    public class ResilienceState
    {
        private readonly ResilienceOptions _opt;

        // 限流：key = 用户端IP|接口分组
        private readonly ConcurrentDictionary<string, SlidingWindowCounter> _limiters = new();
        // 熔断：key = 接口分组
        private readonly ConcurrentDictionary<string, CircuitState> _circuits = new();
        // 隔离：key = 接口分组 -> 信号量
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _bulkheads = new();
        // 隔离：key = 接口分组 -> 当前占用数（仅展示用）
        private readonly ConcurrentDictionary<string, int> _bulkheadInUse = new();

        // 各阶段触发计数（降级总数）
        private long _cntRateLimited;
        private long _cntBulkheadRejected;
        private long _cntTimeout;
        private long _cntRetry;
        private long _cntCircuitRejected;
        private long _cntFallback;

        public ResilienceState(ResilienceOptions opt) => _opt = opt;

        public SlidingWindowCounter GetLimiter(string key) =>
            _limiters.GetOrAdd(key, _ => new SlidingWindowCounter(_opt.RateLimitWindowSeconds));

        public CircuitState GetCircuit(string group) =>
            _circuits.GetOrAdd(group, g => new CircuitState(g, _opt));

        public SemaphoreSlim GetBulkhead(string group) =>
            _bulkheads.GetOrAdd(group, _ => new SemaphoreSlim(_opt.BulkheadMaxConcurrent, _opt.BulkheadMaxConcurrent + _opt.BulkheadQueue));

        public void IncBulkheadInUse(string group, int delta) =>
            _bulkheadInUse.AddOrUpdate(group, Math.Max(0, delta), (_, v) => Math.Max(0, v + delta));

        public long IncRateLimited() => Interlocked.Increment(ref _cntRateLimited);
        public long IncBulkheadRejected() => Interlocked.Increment(ref _cntBulkheadRejected);
        public long IncTimeout() => Interlocked.Increment(ref _cntTimeout);
        public long IncRetry() => Interlocked.Increment(ref _cntRetry);
        public long IncCircuitRejected() => Interlocked.Increment(ref _cntCircuitRejected);
        public long IncFallback() => Interlocked.Increment(ref _cntFallback);

        public object Snapshot()
        {
            var circuits = _circuits.Select(c => new
            {
                group = c.Key,
                status = c.Value.Status.ToString(),
                consecutiveFailures = c.Value.ConsecutiveFailures,
                halfOpenTrialsRemaining = c.Value.HalfOpenTrialsRemaining
            }).ToList();

            var bulkheads = _bulkheads.Select(b => new
            {
                group = b.Key,
                max = _opt.BulkheadMaxConcurrent,
                inUse = _bulkheadInUse.TryGetValue(b.Key, out var v) ? v : 0,
                available = b.Value.CurrentCount
            }).ToList();

            return new
            {
                options = _opt,
                counters = new
                {
                    rateLimited = Interlocked.Read(ref _cntRateLimited),
                    bulkheadRejected = Interlocked.Read(ref _cntBulkheadRejected),
                    timeout = Interlocked.Read(ref _cntTimeout),
                    retry = Interlocked.Read(ref _cntRetry),
                    circuitRejected = Interlocked.Read(ref _cntCircuitRejected),
                    fallback = Interlocked.Read(ref _cntFallback)
                },
                circuits,
                bulkheads
            };
        }

        public void Reset()
        {
            foreach (var c in _circuits.Values) { c.RecordSuccess(); }
            _circuits.Clear();
            _limiters.Clear();
            Interlocked.Exchange(ref _cntRateLimited, 0);
            Interlocked.Exchange(ref _cntBulkheadRejected, 0);
            Interlocked.Exchange(ref _cntTimeout, 0);
            Interlocked.Exchange(ref _cntRetry, 0);
            Interlocked.Exchange(ref _cntCircuitRejected, 0);
            Interlocked.Exchange(ref _cntFallback, 0);
        }
    }
}
