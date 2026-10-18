using System;

namespace ECommerce.Infrastructure.Attributes
{
    /// <summary>
    /// 标记某写接口（POST/PUT/PATCH/DELETE）需要幂等保护。
    /// 机制：以「用户标识 + 接口路径 + Idempotency-Key 请求头」作为幂等键；
    /// 若请求头缺失，则自动以「方法 + 路径 + 请求体 SHA256」派生键，
    /// 使“完全一致的重复提交”也能去重（防误点、防前端重试）。
    /// 同一幂等键的重复请求：首次正常执行并缓存成功响应（默认 24h）；
    /// 后续重复请求直接返回首次的缓存结果（HTTP 状态码与响应体一致），不再执行业务。
    /// 并发重复请求（首次仍在处理中）返回 409，提示用户端稍后重试。
    /// 业务失败（HTTP 非 2xx 或 Api_Response.code != ok）不缓存，允许用户端修正后重试。
    /// 示例：
    ///   [Idempotent_]                       // 默认 Ttl=24h，头名 Idempotency-Key
    ///   [Idempotent_(TtlSeconds = 3600)]    // 自定义缓存 1h
    /// 用户端建议：每次提交生成一个 UUID 放入 Idempotency-Key 头；重试时复用同一 UUID。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class Idempotent_Attribute : Attribute
    {
        /// <summary>成功响应在缓存中的存活时间（秒），默认 86400（24h）。</summary>
        public int TtlSeconds { get; set; } = 86400;

        /// <summary>幂等键请求头名称，默认 "Idempotency-Key"（大小写不敏感）。</summary>
        public string HeaderName { get; set; } = "Idempotency-Key";

        /// <summary>
        /// 处理中锁的存活时间（秒），需大于单次请求最大耗时，默认 60。
        /// 期间到达的并发重复请求会收到 409。
        /// </summary>
        public int LockSeconds { get; set; } = 60;
    }
}
