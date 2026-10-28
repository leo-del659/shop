using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerce.Infrastructure.Tools
{
    public class JWT_Helper
    {
        private readonly IConfiguration _config;

        public JWT_Helper(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(string code, string role, string? mobile = null)
        {
            var jwtSection = _config.GetSection("Jwt");
            string secret = jwtSection["SecretKey"]!;
            string issuer = jwtSection["Issuer"]!;
            string audience = jwtSection["Audience"]!;
            int expireMin = int.Parse(jwtSection["ExpiresMinutes"]!);

            List<Claim> claims = new List<Claim>()
            {
                new Claim("unique_name", code),
                new Claim(ClaimTypes.Name, code),
                new Claim(ClaimTypes.Role, role)
            };

            // 写入手机号声明，供订单等查询按「同手机号下的所有历史 u_id」兜底，
            // 解决用户体系重构后旧订单 owner 仍是旧 u_id 导致查不到的问题。
            if (!string.IsNullOrWhiteSpace(mobile))
            {
                claims.Add(new Claim("mobile", mobile));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(expireMin),
                SigningCredentials = credentials
            };

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(securityToken);
        }

        /// <summary>
        /// 校验 JWT 是否有效（签名/签发方/受众/过期），并取出 unique_name 声明（登录账号）。
        /// 用于 SSE 等无法携带 Authorization 头（EventSource 不支持自定义头）的场景：
        /// 调用方可从 cookie(admin_token) 或 query(?token=) 读取令牌后在此校验。
        /// </summary>
        /// <param name="token">待校验的 JWT 字符串</param>
        /// <param name="userName">校验通过时回写 unique_name 声明值，失败为 null</param>
        /// <returns>有效返回 true，否则 false</returns>
        public bool TryValidateToken(string token, out string? userName)
        {
            userName = null;
            if (string.IsNullOrWhiteSpace(token)) return false;

            try
            {
                var jwtSection = _config.GetSection("Jwt");
                var secret = jwtSection["SecretKey"]!;
                var issuer = jwtSection["Issuer"]!;
                var audience = jwtSection["Audience"]!;
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                var handler = new JwtSecurityTokenHandler();

                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                // JwtSecurityTokenHandler 会做「入站声明类型映射」：
                // token 里的 "unique_name" 会被映射成 ClaimTypes.Name，
                // 因此直接 FindFirst("unique_name") 取不到值，会把合法令牌误判为无效。
                // 这里按 ClaimTypes.Name → unique_name → Identity.Name 的顺序兜底取值。
                userName = principal.FindFirst(ClaimTypes.Name)?.Value
                        ?? principal.FindFirst("unique_name")?.Value
                        ?? principal.Identity?.Name;
                return !string.IsNullOrWhiteSpace(userName);
            }
            catch
            {
                return false;
            }
        }
    }
}
