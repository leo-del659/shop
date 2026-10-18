using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace ECommerce.Api.Admin.Middles.Idempotency
{
    /// <summary>
    /// 幂等过滤器：仅对标记 [Idempotent_] 的“写方法”(POST/PUT/PATCH/DELETE) 生效。
    /// 全局注册，对未标记的接口零影响。
    /// - 命中缓存：直接返回首次成功响应（HTTP 状态码 + 响应体完全一致）。
    /// - 并发处理中：返回 409，提示用户端稍后重试。
    /// - 业务失败/异常：移除处理锁，允许用户端修正后重试（不缓存失败）。
    /// </summary>
    public class Idempotency_Filter : IAsyncActionFilter
    {
        private static readonly string[] MutatingMethods = { "POST", "PUT", "PATCH", "DELETE" };
        private readonly IdempotencyStore _store;
        private readonly JsonSerializerOptions _json;

        public Idempotency_Filter(IdempotencyStore store, IOptions<JsonOptions> jsonOpt)
        {
            _store = store;
            _json = jsonOpt.Value?.JsonSerializerOptions
                    ?? new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var attr = context.ActionDescriptor.EndpointMetadata
                .FirstOrDefault(m => m is Idempotent_Attribute) as Idempotent_Attribute;
            if (attr == null) { await next(); return; }

            var method = context.HttpContext.Request.Method.ToUpperInvariant();
            if (Array.IndexOf(MutatingMethods, method) < 0) { await next(); return; }

            var key = BuildKey(context, attr.HeaderName);

            var check = await _store.AcquireOrGetAsync(key, attr.LockSeconds);
            if (check.State == IdemState.Done)
            {
                // 命中缓存：直接返回首次的成功响应，HTTP 状态码与响应体完全一致
                context.Result = new ContentResult
                {
                    StatusCode = check.StatusCode,
                    ContentType = "application/json; charset=utf-8",
                    Content = check.BodyJson
                };
                return;
            }
            if (check.State == IdemState.Conflict)
            {
                context.Result = new JsonResult(new Api_Response
                {
                    code = Api_Code.fail,
                    message = "请求正在处理中（幂等键冲突），请稍后重试"
                })
                { StatusCode = 409 };
                return;
            }

            // 首次执行
            var executed = await next();
            if (TryGetCacheable(executed, out var statusCode, out var value))
            {
                var bodyJson = JsonSerializer.Serialize(value, _json);
                await _store.StoreAsync(key, statusCode, bodyJson, attr.TtlSeconds);
            }
            else
            {
                // 业务失败/异常：移除锁，允许用户端修正后重试
                await _store.RemoveAsync(key);
            }
        }

        /// <summary>
        /// 仅当 HTTP 2xx 且（无 Api_Response 或 Api_Response.code==ok）时才可缓存。
        /// 业务失败（code!=ok）或异常不缓存，避免把失败锁死导致无法重试。
        /// </summary>
        private static bool TryGetCacheable(ActionExecutedContext executed, out int statusCode, out object? value)
        {
            statusCode = 200;
            value = null;
            var obj = executed.Result as ObjectResult;
            if (obj == null) return false;
            statusCode = obj.StatusCode ?? 200;
            if (statusCode < 200 || statusCode >= 300) return false;
            value = obj.Value;
            if (value is Api_Response api && api.code != Api_Code.ok) return false;
            return true;
        }

        private static string BuildKey(ActionExecutingContext context, string headerName)
        {
            var req = context.HttpContext.Request;
            var userId = context.HttpContext.User?.FindFirst("unique_name")?.Value
                       ?? context.HttpContext.User?.FindFirst(ClaimsIdentity.DefaultNameClaimType)?.Value
                       ?? "anon";
            var source = req.Headers[headerName].ToString().Trim();
            if (string.IsNullOrWhiteSpace(source))
            {
                // 无显式幂等键：以「请求体 SHA256」派生，使完全一致的重复提交去重
                source = "h:" + HashBody(req);
            }
            var path = req.Path.ToString();
            return $"{userId}:{req.Method.ToUpperInvariant()}:{path}:{source}";
        }

        private static string HashBody(HttpRequest req)
        {
            try
            {
                req.EnableBuffering();
                req.Body.Seek(0, SeekOrigin.Begin);
                using var sha = SHA256.Create();
                var bytes = sha.ComputeHash(req.Body);
                req.Body.Seek(0, SeekOrigin.Begin);
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
            catch
            {
                // 读取失败（如空 body）时用随机键，退化为“不幂等”，不影响正常请求
                return "r:" + Guid.NewGuid().ToString("N");
            }
        }
    }
}
