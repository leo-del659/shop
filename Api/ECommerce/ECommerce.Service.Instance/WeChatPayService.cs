using System;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Pay;
using ECommerce.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service.Instance
{
    /// <summary>
    /// 微信支付编排实现。
    /// 下单（Prepay）只负责向微信申请预支付会话并返回前端支付参数；
    /// 回调（HandleNotify）解密通知后委托 OrderService.PayCallbackAsync 统一落库（订单支付状态 + 支付流水 + 事件），
    /// 避免支付落库逻辑重复，保证与现有 pay-callback 入口行为一致。
    /// </summary>
    [Inject_]
    public class WeChatPayService : Base_Service, IWeChatPayService
    {
        private readonly _111Context _db;
        private readonly IWeChatPayClient _payClient;
        private readonly IOrderService _orderService;

        public WeChatPayService(_111Context db, IWeChatPayClient payClient, IOrderService orderService)
        {
            _db = db;
            _payClient = payClient;
            _orderService = orderService;
        }

        public async Task<Api_Response> PrepayAsync(string orderId, string openId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return Fail("订单ID不能为空");
            if (string.IsNullOrWhiteSpace(openId))
                return Fail("付款人 openid 不能为空（小程序端 wx.login 获取后传入）");

            var order = await _db.ClientOrders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                return Fail("订单不存在");
            if (order.PayStatus == PayStatus.Paid)
                return Fail("订单已支付，请勿重复下单");
            if (string.IsNullOrWhiteSpace(order.OrderNo))
                return Fail("订单编号缺失，无法发起支付");

            var amount = (decimal)(order.TotalAmount ?? 0);
            if (amount <= 0)
                return Fail("订单金额无效");

            var req = new JsapiPrepayRequest
            {
                OutTradeNo = order.OrderNo,
                Description = "用户订单-" + order.OrderNo,
                Amount = amount,
                PayerOpenId = openId,
                Attach = order.MerchantId
            };

            var result = await _payClient.JsapiPrepayAsync(req);
            if (string.IsNullOrEmpty(result.PrepayId))
                return Fail("微信预下单失败（请检查 WeChatPay 配置或网络）");

            return Ok("预下单成功", new
            {
                orderId = order.Id,
                orderNo = order.OrderNo,
                prepayId = result.PrepayId,
                isMock = result.IsMock,
                payPackage = result.PayPackage
            });
        }

        public async Task<Api_Response> HandleNotifyAsync(string signature, string timestamp, string nonce, string serial, string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return Fail("通知内容为空");

            var notify = await _payClient.VerifyAndDecryptNotifyAsync(signature, timestamp, nonce, serial, body);
            if (notify == null)
                return Fail("通知验签/解密失败");

            if (string.IsNullOrWhiteSpace(notify.OutTradeNo))
                return Fail("通知缺少商户订单号");

            var order = await _db.ClientOrders.FirstOrDefaultAsync(o => o.OrderNo == notify.OutTradeNo);
            if (order == null)
                return Fail("订单不存在：" + notify.OutTradeNo);

            // 已支付：直接幂等返回，避免重复落库
            if (order.PayStatus == PayStatus.Paid)
                return Ok("支付回调成功（已处理）", new { orderId = order.Id, orderNo = order.OrderNo });

            // 金额校验：微信回调金额（分）必须与订单应收金额一致，防止少付/错付被置为已付
            if (notify.Amount != null)
            {
                var expectFen = (int)Math.Round(((decimal)(order.TotalAmount ?? 0)) * 100m, 0, MidpointRounding.AwayFromZero);
                if (notify.Amount.Total != expectFen)
                    return Fail($"支付金额不一致：订单应收 {expectFen} 分，回调通知 {notify.Amount.Total} 分");
            }

            // 复用支付落库逻辑：写订单支付状态 + 支付流水（含微信流水号）+ 发布订单事件
            var result = await _orderService.PayCallbackAsync(order.Id, null, notify.TransactionId);
            if (result.code != Api_Code.ok)
                return Fail("支付回调落库失败：" + result.message);

            return Ok("支付回调成功", new { orderId = order.Id, orderNo = order.OrderNo });
        }

        public async Task<Api_Response> SimulateNotifyAsync(string orderNo, string? transactionId = null)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
                return Fail("订单编号不能为空");

            var order = await _db.ClientOrders.FirstOrDefaultAsync(o => o.OrderNo == orderNo);
            if (order == null)
                return Fail("订单不存在：" + orderNo);

            var totalFen = (int)Math.Round(((decimal)(order.TotalAmount ?? 0)) * 100m, 0, MidpointRounding.AwayFromZero);
            var notify = new WeChatPayNotification
            {
                OutTradeNo = order.OrderNo ?? "",
                TransactionId = transactionId ?? ("MOCK_TXN_" + (order.OrderNo ?? Guid.NewGuid().ToString("N"))),
                TradeState = "SUCCESS",
                TradeStateDesc = "支付成功",
                SuccessTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                Amount = new WeChatPayNotification.AmountInfo { Total = totalFen, Currency = "CNY" }
            };

            var body = _payClient.BuildMockEncryptedNotification(notify);
            return await HandleNotifyAsync("", "", "", "", body);
        }
    }
}
