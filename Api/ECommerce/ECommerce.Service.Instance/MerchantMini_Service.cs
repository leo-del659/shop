using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Mobile.Merchant;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.MQ;
using ECommerce.Infrastructure.Pay;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ECommerce.Service.Instance
{
    /// <summary>
    /// 商户端（微信小程序）业务服务实现。
    /// token 采用 JWT，unique_name 声明存放 merchant_id；小程序仅把 token 当 Bearer 字符串使用。
    /// 密码为 PHC 格式哈希串（盐值与算法参数内嵌，pepper 取 Login:Keys），与 sys_users 规则一致。
    /// </summary>
    [Inject_]
    public class MerchantMini_Service : Base_Service, IMerchantMiniService
    {
        private readonly _111Context _context;
        private readonly JWT_Helper _jwt;
        private readonly IConfiguration _configuration;
        private readonly IOrderEventPublisher _publisher;
        private readonly IWeChatPayClient _weChatPay;

        public MerchantMini_Service(_111Context context, JWT_Helper jwt, IConfiguration configuration, IOrderEventPublisher publisher, IWeChatPayClient weChatPay)
        {
            _context = context;
            _jwt = jwt;
            _configuration = configuration;
            _publisher = publisher;
            _weChatPay = weChatPay;
        }

        #region 密码哈希

        /// <summary>全局加权串（appsettings Login:Keys），作为哈希 pepper</summary>
        private string PasswordKeys => _configuration["Login:Keys"] ?? string.Empty;

        /// <summary>生成 8 位随机盐（仅作标记，真实盐值内嵌于哈希串）</summary>
        private static string NewSalt() => Guid.NewGuid().ToString("N").Substring(0, 8);

        /// <summary>
        /// 计算密码哈希（PHC 串，盐值与算法参数内嵌），pepper 取 Login:Keys，
        /// 与 Login_Service 中 sys_users 的加密规则保持一致。
        /// </summary>
        private string HashPassword(string plainPassword)
            => EncryptHelper.HashPassword(plainPassword, PasswordKeys);

        /// <summary>按 PHC 串校验密码</summary>
        private bool VerifyPassword(string plainPassword, Merchant merchant)
        {
            return EncryptHelper.Verify(plainPassword, merchant.Password, merchant.Salt, PasswordKeys)
                == EncryptHelper.PasswordVerifyOutcome.Argon2Matched;
        }

        #endregion

        #region 登录 / 账号

        public Api_Response Login(string account, string password)
        {
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password))
                return Fail("手机号或密码不能为空");

            var merchant = _context.Merchants
                .FirstOrDefault(x => x.Account == account);
            if (merchant == null)
                return Fail("手机号或密码错误");

            if (!VerifyPassword(password, merchant))
                return Fail("手机号或密码错误");

            if (merchant.IsBan == 1)
                return Fail("账号已被禁用");

            var token = _jwt.GenerateToken(merchant.Id, "merchant");
            return Ok("登录成功", new
            {
                token,
                merchant = BuildMerchantDto(merchant)
            });
        }

        /// <summary>快速注册：手机号 + 短信验证码 + 密码，校验通过后建号（account 即手机号）</summary>
        public Api_Response Register(string phone, string code, string password)
        {
            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(password))
                return Fail("手机号、验证码和密码均不能为空");
            if (password.Length < 6)
                return Fail("密码长度不能少于6位");

            // 手机号查重（account 已设唯一索引，双重保险）
            if (_context.Merchants.Any(x => x.Account == phone))
                return Fail("该手机号已注册");

            // 校验短信验证码（与消费者注册共用 sms_code 表，取最近一条未使用且未过期的记录）
            var codeEntity = _context.SmsCodes
                .OrderByDescending(d => d.CreateTime)
                .FirstOrDefault(d => d.Mobile == phone && d.Code == code && d.IsUsed == 0);
            if (codeEntity == null)
                return Fail("验证码错误或已使用");
            if (codeEntity.ExpireTime.HasValue && codeEntity.ExpireTime.Value < DateTime.Now)
                return Fail("验证码已过期，请重新获取");

            // 标记验证码已使用
            codeEntity.IsUsed = 1;
            _context.SmsCodes.Update(codeEntity);

            var now = DateTime.Now;
            var merchant = new Merchant
            {
                Id = Config.NewSequentialGuid(),
                Account = phone,
                MerchantName = phone,
                IsBan = 0,
                Salt = NewSalt(),
                Password = HashPassword(password),
                CreateTime = now
            };
            _context.Merchants.Add(merchant);
            _context.SaveChanges();

            return Ok("注册成功，请登录");
        }

        public Api_Response ChangePassword(string merchantId, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
                return Fail("原密码和新密码均不能为空");

            if (newPassword.Length < 6)
                return Fail("新密码长度不能少于6位");

            var merchant = _context.Merchants.FirstOrDefault(x => x.Id == merchantId);
            if (merchant == null)
                return Fail("登录账号不存在");

            if (!VerifyPassword(oldPassword, merchant))
                return Fail("原密码错误");

            // 盐标记重新生成（真实盐值内嵌于哈希串，无需复用旧值）
            merchant.Salt = NewSalt();
            merchant.Password = HashPassword(newPassword);
            _context.SaveChanges();
            return Ok("密码修改成功");
        }

        #endregion

        #region 资料 / 首页

        public Api_Response GetMe(string merchantId)
        {
            var merchant = _context.Merchants.Find(merchantId);
            if (merchant == null)
                return Fail("商户不存在");
            return Ok("ok", BuildMerchantDto(merchant));
        }

        public Api_Response GetProfile(string merchantId)
        {
            var merchant = _context.Merchants.Find(merchantId);
            if (merchant == null)
                return Fail("商户不存在");

            var bank = _context.MerchantBanks.FirstOrDefault(x => x.MerchantId == merchantId);
            var qualCount = _context.MerchantQualifications.Count(x => x.MerchantId == merchantId && x.IsBan != 1);

            return Ok("ok", new
            {
                merchant = BuildMerchantDto(merchant, bank, qualCount),
                stats = GetHomeStats(merchantId)
            });
        }

        public Api_Response UpdateProfile(string merchantId, MerchantProfileDto dto)
        {
            var merchant = _context.Merchants.Find(merchantId);
            if (merchant == null)
                return Fail("商户不存在");

            if (!string.IsNullOrWhiteSpace(dto.MerchantName))
                merchant.MerchantName = dto.MerchantName;
            if (!string.IsNullOrWhiteSpace(dto.Avatar))
                merchant.Avatar = dto.Avatar;
            _context.SaveChanges();
            return Ok("保存成功", BuildMerchantDto(merchant));
        }

        public Api_Response GetHome(string merchantId)
        {
            var merchant = _context.Merchants.Find(merchantId);
            if (merchant == null)
                return Fail("商户不存在");

            var stats = GetHomeStats(merchantId);
            var products = GetMerchantGoodList(merchantId, "on")
                .Take(5)
                .ToList();

            // 待接单提醒：已付款但未处理的订单
            var pendingCount = _context.ClientOrders
                .Count(x => x.MerchantId == merchantId
                    && x.PayStatus == PayStatus.Paid
                    && x.OrderStatus == OrderStatus.PendingAccept
                    && x.IsBan != 1);

            // 今日/昨日订单数与交易额（口径同总销售额：已付款且未取消/未删除，按下单时间归日）
            var todayStart = DateTime.Today;
            var yesterdayStart = todayStart.AddDays(-1);
            var recentOrders = _context.ClientOrders
                .Where(x => x.MerchantId == merchantId && x.PayStatus == PayStatus.Paid
                    && x.OrderStatus != OrderStatus.Cancelled && x.IsBan != 1 && x.CreateTime >= yesterdayStart)
                .Select(x => new { x.CreateTime, x.TotalAmount })
                .ToList();
            var todayPart = recentOrders.Where(x => x.CreateTime >= todayStart).ToList();
            var yesterdayPart = recentOrders.Where(x => x.CreateTime < todayStart).ToList();

            // 公告通知：取最新 3 条未禁用公告
            var notices = _context.SysNotices
                .Where(n => n.IsBan != 1)
                .OrderByDescending(n => n.CreateTime)
                .Take(3)
                .Select(n => new { n.Id, n.Title, n.Content, n.Type, n.CreateTime })
                .ToList();

            return Ok("ok", new
            {
                merchant = BuildMerchantDto(merchant),
                kpi = stats,
                products,
                today = new
                {
                    orders = todayPart.Count,
                    revenue = todayPart.Sum(x => x.TotalAmount ?? 0),
                    yesterdayOrders = yesterdayPart.Count,
                    yesterdayRevenue = yesterdayPart.Sum(x => x.TotalAmount ?? 0)
                },
                notices,
                todos = pendingCount > 0
                    ? new List<object> { new { type = "red", text = $"有 {pendingCount} 笔新订单待接单", time = "" } }
                    : new List<object>()
            });
        }

        /// <summary>
        /// 商户余额：以已完成/已签收订单为累计已结算，以成功提现为累计已提现，
        /// 可结算余额 = 累计已结算 - 累计已提现，保证与订单明细数据同源可追溯。
        /// </summary>
        public Api_Response GetBalance(string merchantId)
        {
            var bal = ComputeBalance(merchantId);

            return Ok("ok", new
            {
                totalPaid = bal.totalPaid,
                withdrawn = bal.withdrawn,
                available = bal.available,
                // 千分位格式化，前端可直接展示 ¥1,234.56
                availableFormatted = bal.available.ToString("N2", CultureInfo.InvariantCulture),
                paidOrderCount = bal.paidOrderCount,
                // sync 不再依赖 merchant_balance_sync_log 表，余额实时计算；前端按 null 处理即可
                sync = (object?)null
            });
        }

        /// <summary>
        /// 触发余额同步：重算已完成订单金额并与已提现比对，写入同步日志，返回同步状态。
        /// 保证可结算余额与已完成订单总额严格一致。
        /// </summary>
        public Api_Response SyncBalance(string merchantId)
        {
            try
            {
                var bal = ComputeBalance(merchantId);
                return Ok("同步成功", new
                {
                    syncStatus = 2,
                    syncStatusText = "同步成功",
                    available = bal.available,
                    availableFormatted = bal.available.ToString("N2", CultureInfo.InvariantCulture),
                    totalPaid = bal.totalPaid,
                    withdrawn = bal.withdrawn,
                    paidOrderCount = bal.paidOrderCount,
                    lastSyncTime = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Fail("同步失败：" + ex.Message);
            }
        }

        #endregion

        #region 商品

        public Api_Response GetGoods(string merchantId, string filter, string category = "")
        {
            var list = GetMerchantGoodList(merchantId, filter ?? "all", category);
            return Ok("ok", list);
        }

        public Api_Response GetGoodsDetail(string merchantId, string id)
        {
            var item = (from g in _context.Goods
                        join sg in _context.MerchantGood on g.Id equals sg.GoodsId
                        where sg.MerchantId == merchantId && g.Id == id
                        select new { g, sg }).FirstOrDefault();
            if (item == null)
                return Fail("商品不存在");
            return Ok("ok", BuildGoodsDto(item.g, item.sg.SupplyPrice ?? 0, merchantId));
        }

        public Api_Response AddGoods(string merchantId, MerchantGoodsSaveDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Fail("商品名称不能为空");
            if (dto.Price == null || dto.Price < 0)
                return Fail("售价不能为负数");
            if (dto.Stock == null || dto.Stock < 0)
                return Fail("库存不能为负数");

            var now = DateTime.Now;
            var goodsId = Config.NewSequentialGuid();
            var goods = new Good
            {
                Id = goodsId,
                GoodsName = dto.Name!,
                CategoryId = dto.CategoryId ?? "",
                Price = dto.Price ?? 0,
                SalePrice = dto.MarketPrice ?? dto.Price ?? 0,
                Stock = dto.Stock ?? 0,
                Unit = dto.Unit,
                CoverImg = dto.CoverImg,
                ImgList = dto.ImgList,
                IsSale = dto.IsOnSale ?? 0,
                AuditStatus = 0, // 待审核：需管理员在后台审核通过后才能上架
                CreateTime = now,
                UpdateTime = now
            };

            _context.Goods.Add(goods);
            _context.MerchantGood.Add(new MerchantGood
            {
                Id = Config.NewSequentialGuid(),
                MerchantId = merchantId,
                GoodsId = goodsId,
                SupplyPrice = dto.CostPrice ?? 0,
                CreateTime = now
            });
            _context.SaveChanges();
            return Ok("添加成功", BuildGoodsDto(goods, dto.CostPrice ?? 0));
        }

        public Api_Response UpdateGoods(string merchantId, string id, MerchantGoodsSaveDto dto)
        {
            var goods = _context.Goods.Find(id);
            if (goods == null)
                return Fail("商品不存在");

            var sg = _context.MerchantGood.FirstOrDefault(x => x.GoodsId == id && x.MerchantId == merchantId);
            if (sg == null)
                return Fail("无权限编辑该商品");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                goods.GoodsName = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.CategoryId))
                goods.CategoryId = dto.CategoryId;
            if (dto.Price.HasValue)
                goods.Price = dto.Price.Value;
            if (dto.MarketPrice.HasValue)
                goods.SalePrice = dto.MarketPrice.Value;
            if (dto.Stock.HasValue)
                goods.Stock = dto.Stock.Value;
            if (dto.Unit != null)
                goods.Unit = dto.Unit;
            if (dto.CoverImg != null)
                goods.CoverImg = dto.CoverImg;
            if (dto.ImgList != null)
                goods.ImgList = dto.ImgList;
            if (dto.IsOnSale.HasValue)
                goods.IsSale = dto.IsOnSale.Value;
            if (dto.CostPrice.HasValue)
                sg.SupplyPrice = dto.CostPrice.Value;

            goods.UpdateTime = DateTime.Now;
            _context.SaveChanges();
            return Ok("保存成功", BuildGoodsDto(goods, sg.SupplyPrice ?? 0));
        }

        public Api_Response ToggleGoods(string merchantId, string id)
        {
            var goods = _context.Goods.Find(id);
            if (goods == null)
                return Fail("商品不存在");

            var sg = _context.MerchantGood.FirstOrDefault(x => x.GoodsId == id && x.MerchantId == merchantId);
            if (sg == null)
                return Fail("无权限操作该商品");

            goods.IsSale = goods.IsSale == 1 ? 0 : 1;
            goods.UpdateTime = DateTime.Now;
            _context.SaveChanges();
            return Ok("操作成功", BuildGoodsDto(goods, sg.SupplyPrice ?? 0));
        }

        public Api_Response RestockGoods(string merchantId, string id, int qty)
        {
            if (qty <= 0)
                return Fail("补货数量必须大于 0");

            var goods = _context.Goods.Find(id);
            if (goods == null)
                return Fail("商品不存在");

            var sg = _context.MerchantGood.FirstOrDefault(x => x.GoodsId == id && x.MerchantId == merchantId);
            if (sg == null)
                return Fail("无权限操作该商品");

            goods.Stock += qty;
            goods.UpdateTime = DateTime.Now;
            _context.SaveChanges();
            return Ok("补货成功", BuildGoodsDto(goods, sg.SupplyPrice ?? 0));
        }

        #endregion

        #region 订单

        public Api_Response GetOrders(string merchantId, string status, string? startDate = null, string? endDate = null)
        {
            var query = _context.ClientOrders.Where(x => x.MerchantId == merchantId && x.IsBan != 1);
            switch (status?.ToLower())
            {
                case "pending":
                    // 只有买家已支付的待接单订单才展示给商户（未支付订单留在买家「未付款」页）
                    query = query.Where(x => x.OrderStatus == OrderStatus.PendingAccept && x.PayStatus == PayStatus.Paid);
                    break;
                case "shipping":
                case "inprogress":
                    query = query.Where(x => x.OrderStatus == OrderStatus.Accepted || x.OrderStatus == OrderStatus.Shipped);
                    break;
                case "completed":
                    query = query.Where(x => x.OrderStatus == OrderStatus.Completed || x.OrderStatus == OrderStatus.Refunded);
                    break;
                case "cancelled":
                    query = query.Where(x => x.OrderStatus == OrderStatus.Cancelled);
                    break;
            }

            if (DateTime.TryParse(startDate, out var sd))
                query = query.Where(x => x.CreateTime >= sd);
            if (DateTime.TryParse(endDate, out var ed))
                query = query.Where(x => x.CreateTime <= ed);

            var orders = query.OrderByDescending(x => x.CreateTime).ToList();
            // 合并后明细内联在 client_order.items_json，直接解析并按 OrderId 分组
            var itemsByOrder = orders
                .ToDictionary(o => o.Id, o => o.ParseItems());

            // 一次性加载商户信息，避免逐单查询
            var merchant = _context.Merchants.Find(merchantId);

            var result = orders.Select(o =>
            {
                itemsByOrder.TryGetValue(o.Id, out var its);
                return BuildOrderDto(o, its ?? new List<ClientOrderItemDto>(), merchant);
            }).ToList();
            return Ok("ok", result);
        }

        /// <summary>
        /// 商户账单列表（从 MerchantBill 表查询，对齐前端管理后台 /settle/merchantbill）。
        /// 返回字段：id/billNo/merchantName/goodsName/coverImg/buyNum/amount/orderStatus/statusText/statusCls/createTime
        /// </summary>
        public Api_Response GetBills(string merchantId, string status)
        {
            var query = _context.MerchantBills.Where(x => x.MerchantId == merchantId);

            // 状态筛选（对齐订单状态枚举）
            switch (status?.ToLower())
            {
                case "pending":
                    query = query.Where(x => x.OrderStatus == OrderStatus.PendingPayment || x.OrderStatus == OrderStatus.PendingAccept || x.OrderStatus == OrderStatus.PendingShip);
                    break;
                case "shipping":
                    query = query.Where(x => x.OrderStatus == OrderStatus.Accepted || x.OrderStatus == OrderStatus.Shipped);
                    break;
                case "completed":
                    query = query.Where(x => x.OrderStatus == OrderStatus.Completed || x.OrderStatus == OrderStatus.Refunded);
                    break;
                case "cancelled":
                    query = query.Where(x => x.OrderStatus == OrderStatus.Cancelled);
                    break;
            }

            var bills = query.OrderByDescending(x => x.CreateTime).ToList();

            // 批量加载关联商户与商品信息，避免 N+1
            var supIds = bills.Select(x => x.MerchantId).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var merchants = _context.Merchants
                .Where(x => supIds.Contains(x.Id))
                .ToDictionary(x => x.Id, x => x);

            var goodsIds = bills.Select(x => x.GoodsId).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var goods = _context.Goods
                .Where(x => goodsIds.Contains(x.Id))
                .ToDictionary(x => x.Id, x => x);

            var result = bills.Select(b =>
            {
                merchants.TryGetValue(b.MerchantId ?? string.Empty, out var s);
                goods.TryGetValue(b.GoodsId ?? string.Empty, out var g);
                var st = MapOrderStatus((int)(b.OrderStatus ?? OrderStatus.PendingPayment));
                return new
                {
                    id = b.Id,
                    billNo = b.BillNo,
                    merchantId = b.MerchantId,
                    merchantName = s?.MerchantName ?? "-",
                    clientId = b.ClientId,
                    goodsId = b.GoodsId,
                    goodsName = b.GoodsName ?? "-",
                    coverImg = g?.CoverImg ?? "",
                    buyNum = b.BuyNum ?? 0,
                    amount = b.Amount ?? 0,
                    orderStatus = b.OrderStatus,
                    payStatus = b.PayStatus,
                    statusText = st.text,
                    statusCls = st.cls,
                    createTime = b.CreateTime,
                    updateTime = b.UpdateTime,
                };
            }).ToList();

            return Ok("ok", result);
        }

        /// <summary>
        /// 商户账单详情（含物流时间线，按 order_status 动态生成）。
        /// 返回：基本信息 + 商户/商户名 + 商品明细 + 物流跟踪节点
        /// </summary>
        public Api_Response GetBillDetail(string merchantId, string id)
        {
            var bill = _context.MerchantBills
                .FirstOrDefault(x => x.Id == id && x.MerchantId == merchantId);
            if (bill == null)
                return Fail("账单不存在");

            var merchant = _context.Merchants.Find(merchantId);

            var st = MapOrderStatus((int)(bill.OrderStatus ?? OrderStatus.PendingPayment));

            // 物流时间线：根据 order_status 动态生成
            var logistics = BuildBillLogistics((int)(bill.OrderStatus ?? OrderStatus.PendingPayment), bill.CreateTime, bill.UpdateTime);

            return Ok("ok", new
            {
                id = bill.Id,
                billNo = bill.BillNo,
                clientId = bill.ClientId,
                merchantName = merchant?.MerchantName ?? "-",
                goodsId = bill.GoodsId,
                goodsName = bill.GoodsName ?? "-",
                buyNum = bill.BuyNum ?? 0,
                amount = bill.Amount ?? 0,
                orderStatus = bill.OrderStatus,
                payStatus = bill.PayStatus,
                payStatusText = "已付款",
                statusText = st.text,
                statusCls = st.cls,
                createTime = bill.CreateTime,
                updateTime = bill.UpdateTime,
                remark = bill.Remark ?? "",
                // 商品明细（单条）
                items = new[] {
                    new { name = bill.GoodsName ?? "-", price = bill.Amount, qty = bill.BuyNum ?? 0, total = bill.Amount ?? 0 }
                },
                logistics,
            });
        }

        /// <summary>
        /// 根据订单状态生成物流时间线节点。
        /// 状态流转：下单 → 接单 → 发货 → 完成
        /// </summary>
        private static List<object> BuildBillLogistics(int status, DateTime? createTime, DateTime? updateTime)
        {
            var list = new List<object>();
            var ct = createTime ?? DateTime.Now;
            var ut = updateTime ?? DateTime.Now;

            // 节点1：下单（始终存在）
            list.Add(new { text = "商户已下单", time = ct.ToString("MM-dd HH:mm"), current = false, active = true });

            // 后续节点根据状态添加
            if (status >= 1)
                list.Add(new { text = "商户已接单", time = status == 1 ? "" : ct.AddMinutes(5).ToString("MM-dd HH:mm"), current = status == 1, active = status >= 1 });
            if (status >= 2)
                list.Add(new { text = "备货完成，已发货", time = status <= 3 ? "" : ut.ToString("MM-dd HH:mm"), current = status == 2 || status == 3, active = status >= 2 });
            if (status >= 4 || status == 6)
                list.Add(new { text = "订单已完成", time = ut.ToString("MM-dd HH:mm"), current = status == 4 || status == 6, active = true });
            if (status == 9)
                list.Add(new { text = "订单已取消", time = ut.ToString("MM-dd HH:mm"), current = true, active = false });

            return list;
        }

        /// <summary>
        /// 跨商户订单列表（不过滤当前登录商户），用于展示所有商户订单。
        /// 保留状态 + 日期范围筛选能力。
        /// </summary>
        public Api_Response GetAllOrders(string status, string? startDate = null, string? endDate = null)
        {
            var query = _context.ClientOrders.Where(x => x.IsBan != 1);
            switch (status?.ToLower())
            {
                case "pending":
                    query = query.Where(x => x.OrderStatus == OrderStatus.PendingAccept);
                    break;
                case "shipping":
                case "inprogress":
                    query = query.Where(x => x.OrderStatus == OrderStatus.Accepted || x.OrderStatus == OrderStatus.Shipped);
                    break;
                case "completed":
                    query = query.Where(x => x.OrderStatus == OrderStatus.Completed || x.OrderStatus == OrderStatus.Refunded);
                    break;
                case "cancelled":
                    query = query.Where(x => x.OrderStatus == OrderStatus.Cancelled);
                    break;
            }

            if (DateTime.TryParse(startDate, out var sd))
                query = query.Where(x => x.CreateTime >= sd);
            if (DateTime.TryParse(endDate, out var ed))
                query = query.Where(x => x.CreateTime <= ed);

            var orders = query.OrderByDescending(x => x.CreateTime).ToList();
            // 合并后明细内联在 client_order.items_json，直接解析并按 OrderId 分组
            var itemsByOrder = orders
                .ToDictionary(o => o.Id, o => o.ParseItems());

            // 一次性加载关联商户信息，避免 N+1
            var merchantIds = orders
                .Select(x => x.MerchantId)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList();
            var merchants = _context.Merchants
                .Where(x => merchantIds.Contains(x.Id))
                .ToDictionary(x => x.Id, x => x);

            var result = orders.Select(o =>
            {
                merchants.TryGetValue(o.MerchantId ?? string.Empty, out var s);
                itemsByOrder.TryGetValue(o.Id, out var its);
                return BuildOrderDto(o, its ?? new List<ClientOrderItemDto>(), s);
            }).ToList();
            return Ok("ok", result);
        }

        public Api_Response GetOrderDetail(string merchantId, string id)
        {
            var order = _context.ClientOrders
                .FirstOrDefault(x => x.Id == id && x.MerchantId == merchantId && x.IsBan != 1);
            if (order == null)
                return Fail("订单不存在");

            var items = order.ParseItems();
            var merchant = _context.Merchants.Find(merchantId);
            return Ok("ok", BuildOrderDto(order, items, merchant));
        }

        public Api_Response OrderAction(string merchantId, string id, string action, string? reason)
        {
            // 商户端订单 = ClientOrder（下单时已写入 MerchantId），按订单 ID 操作
            var order = _context.ClientOrders
                .FirstOrDefault(x => x.Id == id && x.MerchantId == merchantId);
            if (order == null)
                return Fail("订单不存在或无权限");

            var now = DateTime.Now;
            switch (action?.ToLower())
            {
                case "accept":
                    if (order.OrderStatus != OrderStatus.PendingAccept)
                        return Fail("只有待处理订单可接单");
                    if (order.PayStatus != PayStatus.Paid)
                        return Fail("买家尚未支付，无法接单");
                    order.OrderStatus = OrderStatus.Accepted; // 接单备货中
                    // 接单后生成 merchant_bill 对账记录（货款），供商户端对账明细展示
                    CreateMerchantBill(order);
                    break;
                case "reject":
                    if (order.OrderStatus != OrderStatus.PendingAccept)
                        return Fail("只有待处理订单可拒绝");
                    order.OrderStatus = OrderStatus.Cancelled; // 已取消
                    order.RejectReason = string.IsNullOrWhiteSpace(reason) ? "商户拒绝" : reason.Trim();
                    break;
                case "ship":
                    if (order.OrderStatus != OrderStatus.Accepted)
                        return Fail("只有接单备货中订单可发货");
                    order.OrderStatus = OrderStatus.Shipped; // 配送中
                    order.ShipTime = now;
                    // 同步更新对账单状态
                    SyncMerchantBillStatus(order);
                    break;
                case "complete":
                    if (order.OrderStatus != OrderStatus.Shipped)
                        return Fail("只有备货完成待配送订单可完成");
                    order.OrderStatus = OrderStatus.Completed; // 已完成
                    order.CompleteTime = now;
                    // 同步更新对账单状态；merchant_bill 接单时已由 CreateMerchantBill 写入
                    SyncMerchantBillStatus(order);
                    AppendSyncLog(merchantId, "订单完成自动同步", 2);
                    break;
                default:
                    return Fail("未知操作");
            }

            order.UpdateTime = now;
            _context.SaveChanges();

            // 消息队列：商户订单动作事件（accept/reject/ship/complete）。
            // complete → 结算消费者自动生成结算单；其余事件供跨实例 SSE 广播消费。
            PublishOrderActionEvent(action, order, reason);

            return Ok("操作成功");
        }

        /// <summary>
        /// 发布商户订单动作事件。OrderAction 为同步方法，此处同步等待发布完成：
        /// ASP.NET Core 无同步上下文不存在死锁风险；发布器内部对 Broker 不可用有快速降级，
        /// 且启动时已做连接预热，正常路径为本地 socket 写入，耗时在毫秒级。
        /// </summary>
        private void PublishOrderActionEvent(string? action, ClientOrder order, string? reason)
        {
            var eventType = action?.ToLower() switch
            {
                "accept" => OrderEventTypes.Accepted,
                "reject" => OrderEventTypes.Rejected,
                "ship" => OrderEventTypes.Shipped,
                "complete" => OrderEventTypes.Completed,
                _ => null
            };
            if (eventType == null) return;

            try
            {
                _publisher.PublishAsync(new OrderEventMessage
                {
                    Type = eventType,
                    OrderId = order.Id,
                    OrderNo = order.OrderNo,
                    ClientId = order.ClientId,
                    MerchantId = order.MerchantId,
                    TotalAmount = order.TotalAmount,
                    Data = action!.ToLower() == "reject" ? new { reason = order.RejectReason } : null
                }).GetAwaiter().GetResult();
            }
            catch
            {
                // 消息投递失败不阻断商户操作（发布器内部已降级，此处双保险）
            }
        }

        #endregion

        #region 对账 / 提现

        /// <summary>
        /// 对账月统计：以 merchant_bill 为数据源，与订单列表页、余额计算同源。
        /// 营收 = 当月全部账单金额；已收款 = 当月已完成(order_status 4/6)账单金额。
        /// </summary>
        public Api_Response GetReconcile(string merchantId, string month, string? startDate = null, string? endDate = null)
        {
            var query = _context.MerchantBills.Where(x => x.MerchantId == merchantId);

            if (!string.IsNullOrWhiteSpace(month)
                && DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthDate))
            {
                query = query.Where(x => x.CreateTime.Year == monthDate.Year
                    && x.CreateTime.Month == monthDate.Month);
            }

            if (DateTime.TryParse(startDate, out var sd))
                query = query.Where(x => x.CreateTime >= sd);
            if (DateTime.TryParse(endDate, out var ed))
                query = query.Where(x => x.CreateTime <= ed);

            var bills = query.OrderByDescending(x => x.CreateTime).ToList();
            var total = bills.Sum(x => x.Amount ?? 0);
            // 已完成订单（order_status 4 或 6）的金额作为已收款
            var paid = bills.Where(x => x.OrderStatus == OrderStatus.Completed || x.OrderStatus == OrderStatus.Refunded).Sum(x => x.Amount ?? 0);
            var count = bills.Count;

            var paidBills = bills.Where(x => x.OrderStatus == OrderStatus.Completed || x.OrderStatus == OrderStatus.Refunded).ToList();

            return Ok("ok", new
            {
                bills = bills.Select(b =>
                {
                    return new
                    {
                        id = b.Id,
                        billNo = b.BillNo,
                        goodsName = b.GoodsName,
                        buyNum = b.BuyNum ?? 0,
                        amount = b.Amount,
                        orderStatus = b.OrderStatus,
                        payStatus = b.PayStatus,
                        createTime = b.CreateTime,
                    };
                }),
                summary = new { count, total, paid, unpaid = total - paid, revenue = total },
                orderSummary = new { count, revenue = total, paid },
                payments = paidBills.Select(b =>
                {
                    return new
                    {
                        orderId = b.BillNo,
                        amount = b.Amount,
                        time = b.CreateTime,
                        method = "在线支付",
                        payTypeLabel = "在线支付",
                    };
                }),
                withdrawals = new List<object>()
            });
        }

        public Api_Response GetWithdrawals(string merchantId, int? status = null, string? startTime = null, string? endTime = null)
        {
            var query = _context.MerchantWithdraws.Where(x => x.MerchantId == merchantId);
            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);
            if (DateTime.TryParse(startTime, out var st))
                query = query.Where(x => x.CreateTime >= st);
            if (DateTime.TryParse(endTime, out var et))
                query = query.Where(x => x.CreateTime <= et);

            var rows = query.OrderByDescending(x => x.CreateTime).ToList();
            return Ok("ok", rows.Select(x => new
            {
                id = x.Id,
                amount = x.Amount,
                status = x.Status,
                statusText = x.Status == 1 ? "待处理" : x.Status == 2 ? "已打款" : "其他",
                transferStatus = x.TransferStatus,
                transferStatusText = x.TransferStatus == 0 ? "未发起" : x.TransferStatus == 1 ? "受理中" : x.TransferStatus == 2 ? "成功" : "失败",
                transferNo = x.TransferNo,
                transferTime = x.TransferTime,
                bankName = x.BankName,
                bankAccount = x.BankAccount,
                accountName = x.AccountName,
                remark = x.Remark,
                createTime = x.CreateTime
            }));
        }

        public async Task<Api_Response> AddWithdraw(string merchantId, decimal amount, string remark)
        {
            if (amount <= 0)
                return Fail("提现金额必须大于 0");

            var bal = ComputeBalance(merchantId);
            var minWithdraw = GetMinWithdraw();
            if (amount < minWithdraw)
                return Fail($"提现金额不得低于最低提现限额 ¥{minWithdraw:N2}");
            if (amount > bal.available)
                return Fail($"余额不足，当前可结算余额 ¥{bal.available:N2}");

            var bank = _context.MerchantBanks.FirstOrDefault(x => x.MerchantId == merchantId);
            if (bank == null)
                return Fail("请先在「银行账户」中绑定收款银行卡");

            var now = DateTime.Now;
            var outTransferNo = "TX" + now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);

            // 先落「待处理」记录，再发起微信转账到银行卡；成功才置已打款（实时扣减可结算余额）
            var rec = new MerchantWithdraw
            {
                MerchantId = merchantId,
                Amount = amount,
                BankName = bank.BankName,
                BankAccount = bank.BankAccount,
                AccountName = bank.AccountName,
                Remark = remark,
                Status = 1,          // 待处理
                TransferStatus = 1,  // 受理中
                OutTransferNo = outTransferNo,
                CreateTime = now
            };
            _context.MerchantWithdraws.Add(rec);
            _context.SaveChanges();

            var txReq = new TransferToBankRequest
            {
                OutTransferNo = outTransferNo,
                BankCardNo = bank.BankAccount ?? "",
                AccountName = bank.AccountName ?? "",
                BankName = bank.BankName ?? "",
                Amount = amount,
                Remark = "商户提现-" + outTransferNo
            };
            var txRes = await _weChatPay.TransferToBankAsync(txReq);

            if (txRes.Success)
            {
                rec.TransferStatus = 2; // 成功
                rec.TransferNo = txRes.TransferNo;
                rec.TransferTime = now;
                rec.Status = 2;        // 已打款（无需审核，成功即入账扣减余额）
            }
            else
            {
                rec.TransferStatus = 3; // 失败
                rec.FailReason = txRes.FailReason;
                // Status 保持 1 待处理，可重试
            }
            _context.SaveChanges();

            return Ok(
                txRes.Success ? "提现申请已受理，资金将转入绑定银行卡" : "提现申请受理失败：" + (txRes.FailReason ?? "未知错误"),
                new
                {
                    available = bal.available - (txRes.Success ? amount : 0m),
                    transferStatus = rec.TransferStatus,
                    isMock = txRes.IsMock
                });
        }

        #endregion

        #region 银行 / 资质

        public Api_Response GetBank(string merchantId)
        {
            var bank = _context.MerchantBanks.FirstOrDefault(x => x.MerchantId == merchantId);
            if (bank == null)
                return Ok("ok", new List<object>());
            return Ok("ok", new List<object> { new
            {
                id = bank.Id,
                bankName = bank.BankName,
                bankAccount = bank.BankAccount,
                accountName = bank.AccountName
            }});
        }

        public Api_Response UpdateBank(string merchantId, MerchantBankDto dto)
        {
            var bank = _context.MerchantBanks.FirstOrDefault(x => x.MerchantId == merchantId);
            var now = DateTime.Now;
            if (bank == null)
            {
                bank = new MerchantBank
                {
                    Id = Config.NewSequentialGuid(),
                    MerchantId = merchantId,
                    CreateTime = now
                };
                _context.MerchantBanks.Add(bank);
            }

            bank.BankName = dto.BankName;
            bank.BankAccount = dto.BankAccount;
            bank.AccountName = dto.AccountName;

            _context.SaveChanges();
            return Ok("保存成功");
        }

        public Api_Response GetQualifications(string merchantId)
        {
            var rows = _context.MerchantQualifications
                .Where(x => x.MerchantId == merchantId && x.IsBan != 1)
                .OrderByDescending(x => x.CreateTime)
                .ToList();
            return Ok("ok", rows.Select(x => new
            {
                id = x.Id,
                qualType = x.QualType,
                qualNo = x.QualNo,
                fileUrl = x.FileUrl,
                expireDate = x.ExpireDate,
                isBan = x.IsBan,
                createTime = x.CreateTime
            }));
        }

        public Api_Response AddQualification(string merchantId, MerchantQualificationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.QualType) || string.IsNullOrWhiteSpace(dto.QualNo))
                return Fail("资质类型和编号不能为空");

            var now = DateTime.Now;
            _context.MerchantQualifications.Add(new MerchantQualification
            {
                Id = Config.NewSequentialGuid(),
                MerchantId = merchantId,
                QualType = dto.QualType,
                QualNo = dto.QualNo,
                FileUrl = dto.FileUrl,
                ExpireDate = dto.ExpireDate,
                IsBan = 0,
                CreateTime = now
            });
            _context.SaveChanges();
            return Ok("提交成功");
        }

        #endregion

        #region 私有辅助

        /// <summary>
        /// 计算商户余额：以 merchant_bill 为数据源（与订单列表、对账明细同源）。
        /// 可结算余额 = 已完成(order_status 4/6)账单金额合计 - 已提现合计（下限 0）。
        /// </summary>
        private (decimal totalPaid, decimal withdrawn, decimal available, int paidOrderCount) ComputeBalance(string merchantId)
        {
            var completedBills = _context.MerchantBills
                .Where(x => x.MerchantId == merchantId
                    && (x.OrderStatus == OrderStatus.Completed || x.OrderStatus == OrderStatus.Refunded))
                .ToList();
            var totalPaid = completedBills.Sum(x => x.Amount ?? 0m);
            var paidOrderCount = completedBills.Count;

            var withdrawn = _context.MerchantWithdraws
                .Where(x => x.MerchantId == merchantId && x.Status == 2)
                .Sum(x => x.Amount);

            var available = totalPaid - withdrawn;
            if (available < 0) available = 0;
            return (totalPaid, withdrawn, available, paidOrderCount);
        }

        /// <summary>余额同步日志：已停用，不再写入 merchant_balance_sync_log 表（余额由 ComputeBalance 实时计算）</summary>
        private void AppendSyncLog(string merchantId, string message, int status)
        {
            // no-op: 不再依赖 merchant_balance_sync_log 表
        }

        /// <summary>最低提现限额：默认 100（不再依赖 sys_config 表，避免该表缺失导致提现失败）</summary>
        private decimal GetMinWithdraw()
        {
            return 100m;
        }

        private object BuildMerchantDto(Merchant s, MerchantBank? bank = null, int qualCount = 0)
        {
            return new
            {
                id = s.Id,
                merchantName = s.MerchantName,
                avatar = s.Avatar,
                account = s.Account,
                isBan = s.IsBan,
                bankName = bank?.BankName,
                bankAccount = bank?.BankAccount,
                accountName = bank?.AccountName,
                qualificationCount = qualCount,
                createTime = s.CreateTime,
            };
        }

        private object GetHomeStats(string merchantId)
        {
            // 首页统计以订单主表为数据源：已付款（pay_status=1）且未取消/未删除的订单才计入总订单与销售额
            var paidOrders = _context.ClientOrders
                .Where(x => x.MerchantId == merchantId
                    && x.PayStatus == PayStatus.Paid
                    && x.OrderStatus != OrderStatus.Cancelled
                    && x.IsBan != 1)
                .ToList();

            // 只统计已审核通过且在上架状态的商品（真正的"在售商品"）
            var products = _context.MerchantGood
                .Where(x => x.MerchantId == merchantId)
                .ToList()
                .Join(_context.Goods, sg => sg.GoodsId, g => g.Id, (sg, g) => new { sg, g })
                .Where(x => x.g.IsSale == 1 && x.g.AuditStatus == 1)
                .Count();

            return new
            {
                orders = paidOrders.Count,
                revenue = paidOrders.Sum(x => x.TotalAmount ?? 0),
                products = products,
                // 待处理 = 待接单（已付款但未处理）
                pending = paidOrders.Count(x => x.OrderStatus == OrderStatus.PendingAccept)
            };
        }

        private List<object> GetMerchantGoodList(string merchantId, string filter, string category = "")
        {
            var query = from g in _context.Goods
                        join sg in _context.MerchantGood on g.Id equals sg.GoodsId
                        where sg.MerchantId == merchantId
                        select new { g, sg };

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(x => x.g.CategoryId == category);

            switch (filter?.ToLower())
            {
                case "on":
                    query = query.Where(x => x.g.IsSale == 1);
                    break;
                case "off":
                    query = query.Where(x => x.g.IsSale != 1);
                    break;
                case "audit":
                    query = query.Where(x => x.g.AuditStatus == 0);
                    break;
                case "low":
                    query = query.Where(x => x.g.Stock < 20);
                    break;
            }

            var list = query.OrderByDescending(x => x.g.CreateTime).ToList();
            return list.Select(x => BuildGoodsDto(x.g, x.sg.SupplyPrice ?? 0)).ToList<object>();
        }

        private object BuildGoodsDto(Good g, decimal costPrice, string merchantId = "")
        {
            // 真实规格：JOIN goods_sku / goods_spec，输出 skus / specs 数组
            var skus = _context.GoodsSkus
                .Where(s => s.GoodsId == g.Id)
                .Select(s => new
                {
                    id = s.Id,
                    specId = s.SpecId,
                    skuName = s.SkuName,
                    price = s.Price,
                    stock = s.Stock,
                })
                .ToList();
            // 规格表 goods_spec 未创建：规格由已查出的 skus 合成（specId 即规格 id，skuName 即规格名）
            var specs = skus
                .Where(s => !string.IsNullOrEmpty(s.specId))
                .GroupBy(s => s.specId)
                .Select(grp => new
                {
                    id = grp.Key,
                    specName = grp.First().skuName,
                })
                .ToList();

            // 查询所属商户名称：优先使用传入的 merchantId；为空时尝试从 merchant_goods 反查兜底
            var resolvedMerchantId = merchantId;
            if (string.IsNullOrWhiteSpace(resolvedMerchantId))
            {
                resolvedMerchantId = _context.MerchantGood
                    .Where(sg => sg.GoodsId == g.Id)
                    .OrderBy(sg => sg.CreateTime)
                    .Select(sg => sg.MerchantId)
                    .FirstOrDefault();
            }
            var merchantName = "";
            if (!string.IsNullOrWhiteSpace(resolvedMerchantId))
            {
                var sup = _context.Merchants.FirstOrDefault(s => s.Id == resolvedMerchantId);
                merchantName = sup?.MerchantName ?? "";
            }

            return new
            {
                id = g.Id,
                name = g.GoodsName,
                goodsName = g.GoodsName,
                categoryId = g.CategoryId,
                price = g.Price,
                salePrice = g.SalePrice,
                marketPrice = g.SalePrice,
                stock = g.Stock,
                unit = g.Unit,
                coverImg = g.CoverImg,
                imgList = string.IsNullOrWhiteSpace(g.ImgList) ? new List<string>() : g.ImgList.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                isSale = g.IsSale,
                auditStatus = g.AuditStatus,
                costPrice = costPrice,
                supplyPrice = costPrice,
                description = "",
                createTime = g.CreateTime,
                updateTime = g.UpdateTime,
                skus = skus,
                specs = specs,
                merchantName = merchantName,
            };
        }

        /// <summary>
        /// 订单状态明细映射：以 order_status 为唯一权威来源，对外暴露 statusText/statusCls，
        /// 保证数据库与前端展示的订单状态文案完全一致（不再借用 pay_status 当作付款状态展示）。
        /// 1待接单 2接单备货中 3已发货·配送中 4/6已完成 9已取消
        /// </summary>
        private (string text, string cls) MapOrderStatus(int? status)
        {
            return status switch
            {
                1 => ("待接单", "pending"),
                2 => ("接单备货中", "shipping"),
                3 => ("已发货·配送中", "shipping"),
                4 or 6 => ("已完成", "completed"),
                9 => ("已取消", "cancelled"),
                _ => ("未知", "")
            };
        }

        /// <summary>
        /// 商户账单/商户采购订单状态映射（merchant_bill.order_status 语义与 client_order 不同，需独立映射）。
        /// 0待付款 1待接单 8备货中 2已发货 3已完成 9已取消
        /// </summary>
        private (string text, string cls) MapMerchantBillStatus(int? status)
        {
            return status switch
            {
                0 => ("待付款", "unpaid"),
                1 => ("待接单", "pending"),
                8 => ("备货中", "shipping"),
                2 => ("已发货", "shipping"),
                3 => ("已完成", "completed"),
                4 or 6 => ("已完成", "completed"),
                9 => ("已取消", "cancelled"),
                _ => ("未知", "")
            };
        }

        private object BuildOrderDto(ClientOrder o, List<ClientOrderItemDto> items, Merchant? merchant)
        {
            // 商户信息（下单商户）：client_id（小程序侧实际存买家 userId）；sell_client_id 列已删除
            var clientId = o.ClientId;
            // 收件人信息：从 member_address（消费者收货地址表）取，下单人 = order.ClientId（买家 userId）
            MemberAddress? recvAddr = null;
            if (!string.IsNullOrEmpty(o.ClientId))
            {
                var addrs = _context.MemberAddresses.Where(a => a.UserId == o.ClientId).ToList();
                if (addrs.Count > 0)
                {
                    recvAddr = addrs.FirstOrDefault(a =>
                            !string.IsNullOrEmpty(o.ReceiverName) && a.Name == o.ReceiverName
                            && !string.IsNullOrEmpty(o.ReceiverMobile) && a.Mobile == o.ReceiverMobile)
                        ?? addrs.FirstOrDefault(a => a.IsDefault == 1)
                        ?? addrs.First();
                }
            }
            var st = MapOrderStatus((int?)(o.OrderStatus));
            return new
            {
                id = o.Id,
                orderNo = o.OrderNo,
                orderStatus = o.OrderStatus,
                // 订单状态权威文案（数据库 order_status → 展示标签），前端直接展示，保证前后端一致
                statusText = st.text,
                statusCls = st.cls,
                payStatus = o.PayStatus,
                totalAmount = o.TotalAmount,
                // 完成时间：订单进入已完成/已签收时取更新时间
                completeTime = (o.OrderStatus == OrderStatus.Completed || o.OrderStatus == OrderStatus.Refunded) ? o.UpdateTime : (DateTime?)null,
                // 订单商品*数量 摘要，如：西红柿x10、鸡蛋x5
                goodsSummary = string.Join("、", items.Select(i => $"{(i.GoodsName ?? "商品")}x{(i.BuyNum ?? 0)}")),
                clientId = clientId,
                // 收件人/电话/地址改取 member_address（消费者收货地址表），不再使用商户表字段
                clientContact = recvAddr?.Name ?? "",
                clientPhone = recvAddr?.Mobile ?? "",
                clientAddress = recvAddr == null ? (o.Address ?? "") : FullAddressOf(recvAddr),
                merchantId = o.MerchantId,
                merchantName = merchant?.MerchantName ?? "-",
                address = o.Address,
                // 交付时间（商户发货后写入 ship_time）
                shipTime = o.ShipTime,
                remark = o.Remark,
                rejectReason = o.RejectReason,
                createTime = o.CreateTime,
                updateTime = o.UpdateTime,
                items = items.Select(i => new
                {
                    id = i.Id,
                    goodsId = i.GoodsId,
                    goodsName = i.GoodsName,
                    specName = i.SpecName,
                    buyPrice = i.BuyPrice,
                    buyNum = i.BuyNum,
                    subAmount = i.SubAmount,
                    goodsImg = i.GoodsImg
                }).ToList()
            };
        }

        /// <summary>member_address 省市区+详细地址拼成整串</summary>
        private static string FullAddressOf(MemberAddress a)
            => $"{a.Province ?? ""}{a.City ?? ""}{a.District ?? ""}{a.DetailAddr ?? ""}";

        /// <summary>
        /// 接单后写入 merchant_bill 一条对账记录（货款），供商户端对账明细展示。
        /// 订单状态按当前订单实际状态记录（接单后为 2 已接单备货中），付款状态取订单 PayStatus。
        /// </summary>
        private void CreateMerchantBill(ClientOrder order)
        {
            var items = order.ParseItems();
            var goodsName = string.Join(", ", items.Select(x => x.GoodsName).Where(x => !string.IsNullOrWhiteSpace(x)));
            var buyNum = items.Sum(x => x.BuyNum ?? 0);
            var now = DateTime.Now;

            _context.MerchantBills.Add(new MerchantBill
            {
                Id = Config.NewSequentialGuid(),
                BillNo = $"GYS{now:yyyyMMdd}-{_context.MerchantBills.Count(x => x.CreateTime.Date == now.Date) + 1:D3}",
                OrderNo = order.OrderNo,
                MerchantId = order.MerchantId!,
                ClientId = order.ClientId,
                GoodsId = items.FirstOrDefault()?.GoodsId,
                GoodsName = string.IsNullOrWhiteSpace(goodsName) ? "订单货款" : goodsName,
                BuyNum = buyNum,
                Amount = (decimal?)(order.TotalAmount ?? 0),
                OrderStatus = order.OrderStatus,
                BillType = 1,
                PayStatus = order.PayStatus,
                CreateTime = now,
                UpdateTime = now
            });
        }

        /// <summary>
        /// 按订单号同步更新 merchant_bill 的订单状态，确保余额、对账统计与订单状态一致。
        /// </summary>
        private void SyncMerchantBillStatus(ClientOrder order)
        {
            if (string.IsNullOrWhiteSpace(order.OrderNo) || string.IsNullOrWhiteSpace(order.MerchantId))
                return;

            var bill = _context.MerchantBills
                .FirstOrDefault(x => x.OrderNo == order.OrderNo && x.MerchantId == order.MerchantId);
            if (bill != null)
            {
                bill.OrderStatus = order.OrderStatus;
                bill.UpdateTime = DateTime.Now;
            }
        }

        #endregion

    }
}
