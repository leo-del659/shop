using ECommerce.Infrastructure.Dto.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace ECommerce.Api.Admin.Controllers
{
    /// <summary>
    /// 文件上传 / 下载控制器
    /// 用于消费者端头像等小文件的上传；上传需登录，下载（GET）开放匿名以便 image 标签直接展示
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private static readonly string[] AllowedExt = { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp" };

        // 统一放到项目根目录 uploads，避免 Debug/Release/publish 各自一份导致文件找不到
        private static string UploadDir =>
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "uploads");

        // 备用图片目录：商户端小程序内置的商品图片（开发环境兼容）
        private static readonly string[] FallbackDirs = new[]
        {
            // GYS 项目内置商品图片
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "WX", "GYS", "miniprogram", "images", "products"),
        };

        /// <summary>
        /// 查找文件：主目录 → 各备用目录
        /// </summary>
        private static string ResolveFilePath(string fileName)
        {
            // 1. 主 uploads 目录
            var primary = Path.Combine(UploadDir, fileName);
            if (System.IO.File.Exists(primary)) return primary;

            // 2. 备用目录
            foreach (var dir in FallbackDirs)
            {
                if (string.IsNullOrWhiteSpace(dir)) continue;
                var fallback = Path.Combine(dir, fileName);
                if (System.IO.File.Exists(fallback)) return fallback;
            }

            return null;
        }

        /// <summary>
        /// 上传文件（需登录）
        /// 保存到程序目录下的 uploads 文件夹，返回可访问的相对地址 /api/file/{fileName}
        /// </summary>
        [HttpPost("upload")]
        [Authorize]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Ok(new Api_Response { code = Api_Code.fail, message = "未选择文件" });
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExt.Contains(ext))
            {
                return Ok(new Api_Response { code = Api_Code.fail, message = "仅支持图片格式(png/jpg/jpeg/gif/webp/bmp)" });
            }

            // 限制大小 5MB
            if (file.Length > 5 * 1024 * 1024)
            {
                return Ok(new Api_Response { code = Api_Code.fail, message = "文件不能超过 5MB" });
            }

            if (!Directory.Exists(UploadDir))
            {
                Directory.CreateDirectory(UploadDir);
            }

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(UploadDir, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var url = $"/api/file/{fileName}";
            return Ok(new Api_Response { code = Api_Code.ok, message = "ok", data = new { url } });
        }

        /// <summary>
        /// 商户端商品图片上传别名（与 /api/File/upload 行为一致）
        /// </summary>
        [HttpPost("upload-product")]
        [Authorize]
        public IActionResult UploadProduct(IFormFile file) => Upload(file);

        /// <summary>
        /// 下载 / 预览文件（匿名开放）
        /// 供小程序 image 组件直接通过 URL 展示头像等图片
        /// </summary>
        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping() => Ok("pong");

        [HttpGet("img")]
        [AllowAnonymous]
        public IActionResult GetImage(string name) => GetFileContent(name);

        /// <summary>
        /// 兼容上传接口返回的 /api/file/{fileName} 路径，直接按文件名返回文件
        /// </summary>
        [HttpGet("{name}")]
        [AllowAnonymous]
        public IActionResult GetByName(string name) => GetFileContent(name);

        private IActionResult GetFileContent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return NotFound();
            }

            // 防目录穿越：仅允许纯文件名，拒绝包含路径分隔符或 ".." 的输入
            var fileName = Path.GetFileName(name);
            if (string.IsNullOrWhiteSpace(fileName) || fileName != name)
            {
                return BadRequest("invalid file name");
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AllowedExt.Contains(ext))
            {
                return BadRequest("unsupported file type");
            }

            // 查找文件（主 uploads 目录 + 备用目录）
            var fullPath = ResolveFilePath(fileName);
            if (fullPath == null)
            {
                return NotFound();
            }

            var contentType = ext switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };

            return PhysicalFile(fullPath, contentType);
        }
    }
}
