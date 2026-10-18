using ECommerce.Infrastructure.Dto.Response;
using System.Net;
using System.Text.Json;

namespace ECommerce.Api.Admin.Middles
{
    /// <summary>
    /// 
    /// </summary>
    public class Exception_MiddleWare
    {
        private readonly RequestDelegate _next;

        public Exception_MiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                //异常也要写两处 第一就是日志 第二记录数据库

                // 记录日志

                await HandleExceptionAsync(context, ex, configuration);
            }
        }

        /// <summary>
        /// 统一格式化返回
        /// </summary>
        private static Task HandleExceptionAsync(HttpContext context, Exception ex, IConfiguration configuration)
        {
            //日志记录异常
            var log_path = configuration["Logs:Error_Log"];
            var fileName = DateTime.Now.ToString("yyyyMMdd") + ".txt";
            var fullPath = Path.Combine(log_path!, fileName);
            try
            {
                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            // 构建完整异常链（含 InnerException），EF Core 的真正数据库错误在内层
            var fullEx = ex;
            var exChain = new System.Text.StringBuilder();
            int depth = 0;
            while (fullEx != null && depth < 5)
            {
                if (depth > 0) exChain.Append("  ---> ");
                exChain.AppendLine(fullEx.GetType().Name + ": " + fullEx.Message);
                fullEx = fullEx.InnerException;
                depth++;
            }
            if (ex.StackTrace != null)
            {
                exChain.AppendLine("\r\n[StackTrace] " + ex.StackTrace.Substring(0, Math.Min(ex.StackTrace.Length, 800)));
            }

            System.IO.File.AppendAllText(fullPath, $"时间:{DateTime.Now.ToString()}\r\n" +
                $"地址：{context.Request.Path}" +
                $"\r\n{exChain}\r\n");
            }
            catch
            {
                // 日志写入失败不应阻塞响应
            }

            // 若响应已开始写出（如 SSE / 已部分返回），不再二次写入，避免异常或破坏已发数据
            if (context.Response.HasStarted)
            {
                return Task.CompletedTask;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            // 自定义返回模型
            var result = new Api_Response
            {
                code = Api_Code.error,
                message = "操作异常",
            };

            var json = JsonSerializer.Serialize(result);
            return context.Response.WriteAsync(json);
        }

    }
}
