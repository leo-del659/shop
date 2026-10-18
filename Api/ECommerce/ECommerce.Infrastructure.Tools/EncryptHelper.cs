using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;

namespace ECommerce.Infrastructure.Tools
{
    public static class EncryptHelper
    {

        /// <summary>Argon2 哈希串前缀（PHC 串行格式，如 $argon2id$v=19$m=19456,t=2,p=1$...）</summary>
        public const string Argon2Prefix = "$argon2";

        // ===== Argon2id 参数（OWASP 密码存储推荐配置：m=19MiB, t=2, p=1）=====
        private const int MemorySizeKiB = 19456;   // 内存成本 19 MiB
        private const int Iterations = 2;          // 时间成本（迭代次数）
        private const int DegreeOfParallelism = 1; // 并行度
        private const int SaltSize = 16;           // 随机盐 128 位
        private const int HashSize = 32;           // 摘要 256 位

        /// <summary>
        /// Argon2id 密码哈希（PHC 串行格式，base64 去填充），整串约 97 字符：
        /// $argon2id$v=19$m=19456,t=2,p=1$&lt;salt&gt;$&lt;hash&gt;
        /// 盐值与算法参数全部内嵌于哈希串，无需额外盐列；
        /// pepper 为服务端全局加权串（appsettings Login:Keys），拼接到明文尾部，
        /// 与旧版「加权」思路一致，数据库泄露后仍需服务端密钥才能暴力破解。
        /// </summary>
        public static string HashPassword(string plainPassword, string? pepper = null)
        {
            if (string.IsNullOrEmpty(plainPassword))
                throw new ArgumentNullException(nameof(plainPassword));

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = ComputeArgon2id(plainPassword, salt, MemorySizeKiB, Iterations, DegreeOfParallelism, HashSize, pepper);

            // 与 Argon2 参考实现一致：base64 不补 '=' 填充
            return string.Concat(
                Argon2Prefix, "id$v=19$m=", MemorySizeKiB.ToString(),
                ",t=", Iterations.ToString(),
                ",p=", DegreeOfParallelism.ToString(),
                "$", ToBase64NoPad(salt),
                "$", ToBase64NoPad(hash));
        }

        /// <summary>
        /// 校验密码（仅 Argon2id）：
        /// - $argon2 前缀哈希 → Argon2id 校验，返回 Argon2Matched / Failed；
        /// - 其它格式（含存量 MD5 / 明文）一律按校验失败处理。
        /// 存量 MD5 / 明文已于 2026-10-16 全部迁移为 Argon2id 并移除兼容分支。
        /// </summary>
        public static PasswordVerifyOutcome Verify(string plainPassword, string? storedHash, string? legacySalt = null, string? pepper = null)
        {
            if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(storedHash))
                return PasswordVerifyOutcome.Failed;

            return VerifyArgon2(plainPassword, storedHash, pepper)
                ? PasswordVerifyOutcome.Argon2Matched
                : PasswordVerifyOutcome.Failed;
        }

        /// <summary>是否为 Argon2 新哈希串（以此区分新旧算法，实现存量账号平滑迁移）</summary>
        public static bool IsArgon2Hash(string? storedHash)
            => !string.IsNullOrEmpty(storedHash)
               && storedHash.StartsWith(Argon2Prefix, StringComparison.Ordinal);

        /// <summary>密码校验结果</summary>
        public enum PasswordVerifyOutcome
        {
            /// <summary>校验失败</summary>
            Failed = 0,
            /// <summary>Argon2id 哈希校验通过</summary>
            Argon2Matched = 1
        }

        #region Argon2 内部实现

        private static byte[] ComputeArgon2id(string plainPassword, byte[] salt, int memoryKiB, int iterations, int parallelism, int hashSize, string? pepper)
        {
            // pepper（Login:Keys）拼接到明文尾部，等价于旧方案的「加权」思路
            byte[] pwdBytes = Encoding.UTF8.GetBytes(plainPassword + (pepper ?? string.Empty));
            using var argon2 = new Argon2id(pwdBytes);
            argon2.Salt = salt;
            argon2.MemorySize = memoryKiB;
            argon2.Iterations = iterations;
            argon2.DegreeOfParallelism = parallelism;
            return argon2.GetBytes(hashSize);
        }

        /// <summary>
        /// 解析 PHC 串并按串内参数重算比对（固定时间比较防时序攻击）。
        /// 哈希串损坏/被篡改一律按校验失败处理，不向外抛异常。
        /// </summary>
        private static bool VerifyArgon2(string plainPassword, string storedHash, string? pepper)
        {
            try
            {
                // PHC 格式：$argon2id$v=19$m=19456,t=2,p=1$<salt>$<hash>
                var parts = storedHash.Split('$');
                if (parts.Length != 6) return false;
                if (!string.Equals(parts[1], "argon2id", StringComparison.Ordinal)) return false;

                int memoryKiB = 0, iterations = 0, parallelism = 0;
                foreach (var p in parts[3].Split(','))
                {
                    var kv = p.Split('=');
                    if (kv.Length != 2) continue;
                    switch (kv[0])
                    {
                        case "m": int.TryParse(kv[1], out memoryKiB); break;
                        case "t": int.TryParse(kv[1], out iterations); break;
                        case "p": int.TryParse(kv[1], out parallelism); break;
                    }
                }
                if (memoryKiB <= 0 || iterations <= 0 || parallelism <= 0) return false;

                byte[] salt = FromBase64NoPad(parts[4]);
                byte[] expected = FromBase64NoPad(parts[5]);
                if (salt.Length == 0 || expected.Length == 0) return false;

                byte[] actual = ComputeArgon2id(plainPassword, salt, memoryKiB, iterations, parallelism, expected.Length, pepper);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch
            {
                return false;
            }
        }

        private static string ToBase64NoPad(byte[] data)
            => Convert.ToBase64String(data).TrimEnd('=');

        private static byte[] FromBase64NoPad(string s)
        {
            if (string.IsNullOrEmpty(s)) return Array.Empty<byte>();
            return Convert.FromBase64String(s.PadRight((s.Length + 3) / 4 * 4, '='));
        }

        #endregion


    }
}
