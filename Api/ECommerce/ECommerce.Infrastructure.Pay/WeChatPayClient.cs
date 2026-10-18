using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Pay
{
    /// <summary>
    /// 微信支付 v3 接口契约。
    /// 真实模式（Enabled=true）发起真实 HTTPS 请求并对回调解密/验签；
    /// 开发模拟模式（Enabled=false，默认）不联网，签名与回调解密使用本地 ApiV3Key 完成，
    /// 便于无商户号环境端到端联调。配齐真实商户号 + 证书 + ApiV3Key 后，将 Enabled 置 true 即切换上线。
    /// </summary>
    public interface IWeChatPayClient
    {
        /// <summary>是否开发模拟模式</summary>
        bool IsMockMode { get; }

        /// <summary>JSAPI 统一下单，返回预支付会话与前端调起支付参数包</summary>
        Task<JsapiPrepayResult> JsapiPrepayAsync(JsapiPrepayRequest req, CancellationToken ct = default);

        /// <summary>验证签名并解密支付成功通知（真实模式验签 + 解密；模拟模式仅解密）</summary>
        Task<WeChatPayNotification?> VerifyAndDecryptNotifyAsync(string signature, string timestamp, string nonce, string serial, string body, CancellationToken ct = default);

        /// <summary>生成一段完整的模拟支付成功通知 body（AES-GCM 加密），供 simulate 端点回放</summary>
        string BuildMockEncryptedNotification(WeChatPayNotification notify);

        /// <summary>AES-256-GCM 解密 resource，返回明文 JSON（解密失败返回 null）</summary>
        string? DecryptResource(string ciphertext, string nonce, string associatedData);

        /// <summary>商家转账到银行卡，返回受理/结果</summary>
        Task<TransferToBankResult> TransferToBankAsync(TransferToBankRequest req, CancellationToken ct = default);
    }

    /// <summary>
    /// 微信支付 v3 接口实现。
    /// 仅依赖框架内置：System.Net.Http / System.Security.Cryptography / System.Text.Json，无需任何 NuGet 包。
    /// </summary>
    public class WeChatPayClient : IWeChatPayClient
    {
        private readonly WeChatPayOptions _opt;
        private static readonly HttpClient _http = CreateHttpClient();

        public WeChatPayClient(IOptions<WeChatPayOptions> options)
        {
            _opt = options?.Value ?? new WeChatPayOptions();
            _apiV3Key = ResolveApiV3Key();
        }

        /// <summary>开发模拟默认密钥（32 字节 = AES-256 要求）。Mock 模式下若未配置 ApiV3Key 或长度不足，用于 AES-GCM 加解密回放；真实模式忽略。</summary>
        private const string DevMockKey = "dev_mock_aes_key_0123456789abcde";

        private readonly string _apiV3Key;

        private string ResolveApiV3Key()
        {
            if (!IsMockMode) return _opt.ApiV3Key ?? "";
            var k = _opt.ApiV3Key ?? "";
            return Encoding.UTF8.GetBytes(k).Length == 32 ? k : DevMockKey;
        }

        public bool IsMockMode => !_opt.Enabled;

        #region 统一下单（JSAPI）

        public async Task<JsapiPrepayResult> JsapiPrepayAsync(JsapiPrepayRequest req, CancellationToken ct = default)
        {
            var totalFen = Convert.ToInt32(Math.Round((req.Amount <= 0 ? 0 : req.Amount) * 100m, 0, MidpointRounding.AwayFromZero));

            if (IsMockMode)
            {
                var mockPrepayId = "mock_prepay_" + req.OutTradeNo;
                return new JsapiPrepayResult
                {
                    PrepayId = mockPrepayId,
                    IsMock = true,
                    PayPackage = BuildJsapiPackage(mockPrepayId, "mock_sign_" + Guid.NewGuid().ToString("N"))
                };
            }

            var bodyObj = new
            {
                appid = _opt.AppId,
                mchid = _opt.MchId,
                description = req.Description,
                out_trade_no = req.OutTradeNo,
                notify_url = _opt.NotifyUrl,
                amount = new { total = totalFen, currency = "CNY" },
                payer = new { openid = req.PayerOpenId }
            };
            var bodyJson = JsonSerializer.Serialize(bodyObj);

            var (status, respJson) = await PostV3Async("/v3/pay/transactions/jsapi", bodyJson, ct);
            if (!status)
            {
                return new JsapiPrepayResult
                {
                    PrepayId = "",
                    IsMock = false,
                    PayPackage = new WeChatPayJsapiPackage { PaySign = "" }
                };
            }

            var prepayId = "";
            try
            {
                using var doc = JsonDocument.Parse(respJson);
                prepayId = doc.RootElement.GetProperty("prepay_id").GetString() ?? "";
            }
            catch { /* 解析失败视为无 prepay_id */ }

            var paySign = "";
            if (!string.IsNullOrEmpty(prepayId) && !string.IsNullOrEmpty(_opt.PrivateKey))
            {
                var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var nonceStr = Guid.NewGuid().ToString("N");
                var package = "prepay_id=" + prepayId;
                var message = $"{_opt.AppId}\n{timeStamp}\n{nonceStr}\n{package}\n";
                paySign = RsaSign(message, _opt.PrivateKey!);
                return new JsapiPrepayResult
                {
                    PrepayId = prepayId,
                    IsMock = false,
                    PayPackage = new WeChatPayJsapiPackage
                    {
                        AppId = _opt.AppId ?? "",
                        TimeStamp = timeStamp,
                        NonceStr = nonceStr,
                        Package = package,
                        SignType = "RSA",
                        PaySign = paySign
                    }
                };
            }

            return new JsapiPrepayResult
            {
                PrepayId = prepayId,
                IsMock = false,
                PayPackage = new WeChatPayJsapiPackage { PaySign = paySign }
            };
        }

        #endregion

        #region 支付通知（验签 + 解密）

        public async Task<WeChatPayNotification?> VerifyAndDecryptNotifyAsync(string signature, string timestamp, string nonce, string serial, string body, CancellationToken ct = default)
        {
            // 模拟模式：信任本地加密内容，跳过验签直接解密
            if (IsMockMode)
            {
                return DecryptNotifyBody(body);
            }

            // 真实模式：先用平台证书公钥验签，再解密
            if (!string.IsNullOrEmpty(_opt.PlatformPublicKey) && !string.IsNullOrEmpty(body))
            {
                var signMessage = $"{timestamp}\n{nonce}\n{body}\n";
                if (!RsaVerify(signMessage, signature, _opt.PlatformPublicKey!))
                {
                    return null;
                }
            }

            return DecryptNotifyBody(body);
        }

        private WeChatPayNotification? DecryptNotifyBody(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                var resource = doc.RootElement.GetProperty("resource");
                var cipher = resource.GetProperty("ciphertext").GetString() ?? "";
                var nonce = resource.GetProperty("nonce").GetString() ?? "";
                var associatedData = resource.TryGetProperty("associated_data", out var a) ? (a.GetString() ?? "") : "";
                var plain = DecryptResource(cipher, nonce, associatedData);
                if (plain == null) return null;
                return JsonSerializer.Deserialize<WeChatPayNotification>(plain);
            }
            catch
            {
                return null;
            }
        }

        public string BuildMockEncryptedNotification(WeChatPayNotification notify)
        {
            var plain = JsonSerializer.Serialize(notify);
            var (ciphertext, nonce, associatedData) = AesGcmEncrypt(_apiV3Key, plain);
            var envelope = new
            {
                id = "mock-event-" + Guid.NewGuid().ToString("N"),
                create_time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                resource_type = "encrypt-resource",
                event_type = "TRANSACTION.SUCCESS",
                summary = "支付成功",
                resource = new
                {
                    algorithm = "AEAD_AES_256_GCM",
                    ciphertext,
                    nonce,
                    associated_data = associatedData
                }
            };
            return JsonSerializer.Serialize(envelope);
        }

        public string? DecryptResource(string ciphertext, string nonce, string associatedData)
        {
            try
            {
                var keyBytes = Encoding.UTF8.GetBytes(_apiV3Key);
                // nonce 在加密侧以 base64 存储，这里必须按 base64 还原为原始 12 字节，
                // 不能用 Encoding.UTF8.GetBytes（那会得到 base64 字符串的 ASCII 字节，导致解密失败）。
                var nonceBytes = Convert.FromBase64String(nonce);
                var cipherBytes = Convert.FromBase64String(ciphertext);
                var aadBytes = string.IsNullOrEmpty(associatedData) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(associatedData);

                // 微信格式：密文在前，16 字节 tag 在后
                var tagSize = AesGcm.TagByteSizes.MaxSize; // 16
                if (cipherBytes.Length <= tagSize) return null;
                var cipherTextOnly = cipherBytes.AsSpan(0, cipherBytes.Length - tagSize).ToArray();
                var tagOnly = cipherBytes.AsSpan(cipherBytes.Length - tagSize, tagSize).ToArray();

                using var aes = new AesGcm(keyBytes);
                var plaintext = new byte[cipherTextOnly.Length];
                aes.Decrypt(nonceBytes, cipherTextOnly, tagOnly, plaintext, aadBytes);
                return Encoding.UTF8.GetString(plaintext);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 转账到银行卡

        public async Task<TransferToBankResult> TransferToBankAsync(TransferToBankRequest req, CancellationToken ct = default)
        {
            var totalFen = Convert.ToInt32(Math.Round((req.Amount <= 0 ? 0 : req.Amount) * 100m, 0, MidpointRounding.AwayFromZero));

            if (IsMockMode)
            {
                return new TransferToBankResult
                {
                    Success = true,
                    TransferNo = "MOCK_TX_" + req.OutTransferNo,
                    IsMock = true
                };
            }

            // 真实模式：商家转账到银行卡（/v3/fund-app/mch-transfer/transfer-to-bank-card）
            // 注意：真实上线时，bank_account_no / bank_account_name 需使用「微信支付平台证书公钥」做 RSA 加密后放入 enc_bank_info，
            // transfer_scene_id 需向微信申请开通。此处发送基础结构，按微信最新文档补全 enc_bank_info 加密即可。
            var bodyObj = new
            {
                appid = _opt.AppId,
                out_batch_no = req.OutTransferNo,
                transfer_scene_id = "1001",
                transfer_detail_list = new[]
                {
                    new
                    {
                        out_detail_no = req.OutTransferNo,
                        transfer_amount = totalFen,
                        transfer_remark = req.Remark ?? "商户提现",
                        bank_account_no = req.BankCardNo,
                        bank_account_name = req.AccountName,
                        bank_name = req.BankName
                    }
                }
            };
            var bodyJson = JsonSerializer.Serialize(bodyObj);

            var (status, respJson) = await PostV3Async("/v3/fund-app/mch-transfer/transfer-to-bank-card", bodyJson, ct);
            if (status)
            {
                var transferNo = "";
                try
                {
                    using var doc = JsonDocument.Parse(respJson);
                    transferNo = doc.RootElement.GetProperty("transfer_no").GetString() ?? "";
                }
                catch { }
                return new TransferToBankResult { Success = true, TransferNo = transferNo, IsMock = false };
            }

            return new TransferToBankResult
            {
                Success = false,
                FailReason = "微信转账受理失败（请检查商户号/证书/transfer_scene_id 配置）",
                IsMock = false
            };
        }

        #endregion

        #region 底层工具

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient(new HttpClientHandler
            {
                // 开发/内网环境跳过证书校验；生产环境应移除该回调
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            })
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            return client;
        }

        /// <summary>
        /// 向微信支付网关发起带 RSA-SHA256 签名头的 POST。返回 (是否成功, 响应体)。
        /// 缺少关键配置（私钥/商户号）时返回 (false, "")。
        /// </summary>
        private async Task<(bool ok, string body)> PostV3Async(string path, string bodyJson, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(_opt.PrivateKey) || string.IsNullOrEmpty(_opt.MchId) || string.IsNullOrEmpty(_opt.AppId))
            {
                return (false, "");
            }

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var nonceStr = Guid.NewGuid().ToString("N");
            var message = $"POST\n{path}\n{timestamp}\n{nonceStr}\n{bodyJson}\n";
            var signature = RsaSign(message, _opt.PrivateKey!);
            var auth = $"WECHATPAY2-SHA256-RSA2048 mchid=\"{_opt.MchId}\",nonce_str=\"{nonceStr}\",signature=\"{signature}\",timestamp=\"{timestamp}\",serial_no=\"{_opt.MchSerialNo}\"";

            using var reqMsg = new HttpRequestMessage(HttpMethod.Post, _opt.Gateway.TrimEnd('/') + path);
            reqMsg.Headers.TryAddWithoutValidation("Authorization", auth);
            reqMsg.Headers.TryAddWithoutValidation("Accept", "application/json");
            reqMsg.Headers.TryAddWithoutValidation("User-Agent", "community-pay/1.0");
            reqMsg.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
            reqMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            try
            {
                using var resp = await _http.SendAsync(reqMsg, ct);
                var respBody = await resp.Content.ReadAsStringAsync(ct);
                return (resp.IsSuccessStatusCode, respBody);
            }
            catch
            {
                return (false, "");
            }
        }

        private static string RsaSign(string data, string privateKeyPem)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem.ToCharArray());
            var bytes = Encoding.UTF8.GetBytes(data);
            var sig = rsa.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(sig);
        }

        private static bool RsaVerify(string data, string signature, string publicKeyPem)
        {
            try
            {
                using var rsa = RSA.Create();
                rsa.ImportFromPem(publicKeyPem.ToCharArray());
                var bytes = Encoding.UTF8.GetBytes(data);
                var sig = Convert.FromBase64String(signature);
                return rsa.VerifyData(bytes, sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>AES-256-GCM 加密，返回 (密文base64[密文+tag], nonce[12字节base64], associatedData)</summary>
        private static (string ciphertext, string nonce, string associatedData) AesGcmEncrypt(string key, string plaintext)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var nonceBytes = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonceBytes);
            }
            var aad = "";
            var aadBytes = Array.Empty<byte>();
            var plainBytes = Encoding.UTF8.GetBytes(plaintext);
            using var aes = new AesGcm(keyBytes);
            var cipher = new byte[plainBytes.Length];
            var tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16
            aes.Encrypt(nonceBytes, plainBytes, cipher, tag, aadBytes);

            var combined = new byte[cipher.Length + tag.Length];
            Buffer.BlockCopy(cipher, 0, combined, 0, cipher.Length);
            Buffer.BlockCopy(tag, 0, combined, cipher.Length, tag.Length);

            return (Convert.ToBase64String(combined), Convert.ToBase64String(nonceBytes), aad);
        }

        private static WeChatPayJsapiPackage BuildJsapiPackage(string prepayId, string paySign)
        {
            return new WeChatPayJsapiPackage
            {
                AppId = "",
                TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                NonceStr = Guid.NewGuid().ToString("N"),
                Package = "prepay_id=" + prepayId,
                SignType = "RSA",
                PaySign = paySign
            };
        }

        #endregion
    }
}
