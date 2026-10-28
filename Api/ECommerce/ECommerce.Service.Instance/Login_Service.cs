using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Login;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.Dto.Response.Org;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.EFCore;
using ECommerce.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ECommerce.Service.Instance
{
    [Inject_]
    public class Login_Service : Base_Service, I_Login_Service
    {
        private readonly Sys_User_EFCore _sys_User_EFCore;
        private readonly IConfiguration _configuration;
        private readonly Sys_Login_Log_EFCore _sys_Login_Log_EFCore;
        private readonly Sms_Code_EFCore _sms_Code_EFCore;
        private readonly SysDeptRole_EFCore _deptRole_EFCore;
        private readonly SysDeptRoleMenu_EFCore _deptRoleMenu_EFCore;
        private readonly SysMenu_EFCore _menu_EFCore;
        private readonly Client_Login_EFCore _client_Login_EFCore;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Login_Service> _logger;

        public Login_Service(
            IConfiguration configuration,
            Sys_User_EFCore sys_User_EFCore,
            Sys_Login_Log_EFCore sys_Login_Log_EFCore,
            Sms_Code_EFCore sms_Code_EFCore,
            SysDeptRole_EFCore deptRole_EFCore,
            SysDeptRoleMenu_EFCore deptRoleMenu_EFCore,
            SysMenu_EFCore menu_EFCore,
            Client_Login_EFCore client_Login_EFCore,
            IServiceProvider serviceProvider,
            ILogger<Login_Service> logger)
        {
            _sys_User_EFCore = sys_User_EFCore;
            _logger = logger;
            _configuration = configuration;
            _sys_Login_Log_EFCore = sys_Login_Log_EFCore;
            _sms_Code_EFCore = sms_Code_EFCore;
            _deptRole_EFCore = deptRole_EFCore;
            _deptRoleMenu_EFCore = deptRoleMenu_EFCore;
            _menu_EFCore = menu_EFCore;
            _client_Login_EFCore = client_Login_EFCore;
            _serviceProvider = serviceProvider;

        }

        /// <summary>
        /// 检查用户是否登陆
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public SysUser? Check_Login(string code)
        {
            // 1) 稳定身份优先：JWT 主题已是稳定的用户主键 UId（消费者登录/注册签发），
            //    直接按主键解析，保证「退出再登录」后订单/地址等数据仍归属同一账号。
            var byUid = _sys_User_EFCore.Query(d => d.UId == code).FirstOrDefault();
            if (byUid != null)
            {
                return byUid;
            }

            // 2) 兼容旧令牌 / 后台账号：JWT 主题仍为登录日志 Code，按登录日志反查用户，
            //    且要求该日志是用户「最后一条」登录日志（与原逻辑一致）。
            var iq = _sys_Login_Log_EFCore.Query(d => d.Code == code);
            if (!iq.Any())
            {
                return null;
            }

            var log = iq.OrderByDescending(d => d.LoginTime).First();

            //最后一条日志
            var last_log = _sys_Login_Log_EFCore.Query(false, d => d.LoginTime, d => d.UserId == log.UserId).First();

            if (log.Id == last_log.Id)
            {

                return _sys_User_EFCore.Find(last_log.UserId);
            }

            return null;


        }

        /// <summary>
        /// 获取当个用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Api_Response Get_User_ByID(string? id)
        {
            var user= _sys_User_EFCore.Find(id);
            return Ok("ok",user);
        }

        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<Api_Response> Login_Admin(Login_Request_Dto dto)
        {
            var iq = _sys_User_EFCore.Query(d => d.UAccount == dto.account);
            if (!iq.Any())
            {
                return Fail("账号或者密码错误_1");   //账号不存在 账号或者密码不正确
            }
            var user = await iq.SingleAsync();
            if (user.UIsBan == 1)
            {
                return Fail("账号已禁用");
            }

            var keys = _configuration["Login:Keys"];

            // 仅 Argon2id 校验（存量 MD5 / 明文已于 2026-10-16 迁移移除）
            var outcome = EncryptHelper.Verify(dto.password, user.UPassword, user.USalt, keys);
            if (outcome == EncryptHelper.PasswordVerifyOutcome.Failed)
            {
                return Fail("账号或者密码错误_2");
            }


            var log = new SysLoginLog
            {
                Id = Config.NewSequentialGuid(),
                Account = user.UAccount,
                LoginAddress = "没有",
                Code = dto.code,
                LoginIp = "也没有",
                LoginTime = DateTime.Now,
                UserId = user.UId
            };

            _sys_Login_Log_EFCore.Add(log);

            var result = _sys_Login_Log_EFCore.SaveChanges();

            return Get_Result(result);



        }


        /// <summary>
        /// 发送短信验证码
        /// 说明：开发环境未对接短信网关，直接把生成的 code 一并返回，便于前端联调；
        ///       生产环境请把 code 通过短信网关下发，并删除返回值中的 code。
        /// </summary>
        public Task<Api_Response> Send_Sms_Code(Send_Code_Request_Dto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.mobile))
            {
                return Task.FromResult(Fail("手机号不能为空"));
            }

            int type = dto.type ?? 3; // 默认管理员

            // 同一手机号 60 秒内不可重复发送
            var last = _sms_Code_EFCore.Query(false, d => d.CreateTime, d => d.Mobile == dto.mobile && d.Type == type)
                .FirstOrDefault();
            if (last != null && last.CreateTime.HasValue
                && (DateTime.Now - last.CreateTime.Value).TotalSeconds < 60)
            {
                return Task.FromResult(Fail("验证码发送过于频繁，请 60 秒后再试"));
            }

            // 同一 IP 1 分钟内最多发送 3 次（防止换手机号/switch type 轰炸）
            if (!string.IsNullOrWhiteSpace(dto.ip) && dto.ip != "unknown")
            {
                var since = DateTime.Now.AddMinutes(-1);
                var ipCount = _sms_Code_EFCore
                    .Query(d => d.Ip == dto.ip && d.CreateTime.HasValue && d.CreateTime.Value > since)
                    .Count();
                if (ipCount >= 3)
                {
                    return Task.FromResult(Fail("请求过于频繁，请 1 分钟后再试"));
                }
            }

            // 生成 6 位验证码
            var code = new Random().Next(100000, 1000000).ToString("D6");

            var entity = new SmsCode
            {
                Id = Config.NewSequentialGuid(),
                Mobile = dto.mobile,
                Code = code,
                Type = type,
                IsUsed = 0,
                CreateTime = DateTime.Now,
                ExpireTime = DateTime.Now.AddMinutes(5),
                Ip = dto.ip
            };

            _sms_Code_EFCore.Add(entity);
            var result = _sms_Code_EFCore.SaveChanges();

            // TODO: 调用短信网关下发 entity.Code 到 entity.Mobile

            // 开发环境直接返回 code 方便联调；生产请移除 data 中的 code
            return Task.FromResult(Get_Result(result, "验证码已发送", new { mobile = dto.mobile, code }));
        }


        /// <summary>
        /// 手机号 + 验证码 登录（免注册：首次登录自动创建账号，并同步写入 clientlogins）
        /// </summary>
        public Task<Api_Response> Login_By_Sms(Sms_Login_Request_Dto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.mobile) || string.IsNullOrWhiteSpace(dto.code))
            {
                return Task.FromResult(Fail("手机号或验证码不能为空"));
            }

            // 免注册：sys_users 中没有该手机号时自动创建消费者账号
            var user = _sys_User_EFCore.Query(d => d.UAccount == dto.mobile).FirstOrDefault();
            if (user == null)
            {
                user = EnsureConsumerAccount(dto.mobile);
                if (user == null)
                {
                    return Task.FromResult(Fail("账号创建失败，请稍后重试"));
                }
            }
            if (user.UIsBan == 1)
            {
                return Task.FromResult(Fail("账号已禁用"));
            }

            var codeEntity = _sms_Code_EFCore
                .Query(false, d => d.CreateTime, d => d.Mobile == dto.mobile && d.Code == dto.code && d.IsUsed == 0)
                .FirstOrDefault();

            if (codeEntity == null)
            {
                return Task.FromResult(Fail("验证码错误或已使用"));
            }
            if (codeEntity.ExpireTime.HasValue && codeEntity.ExpireTime.Value < DateTime.Now)
            {
                return Task.FromResult(Fail("验证码已过期，请重新获取"));
            }

            // 标记验证码已使用
            codeEntity.IsUsed = 1;
            _sms_Code_EFCore.Update(codeEntity);
            _sms_Code_EFCore.SaveChanges();

            // 登录成功后同步用户账号到 client_login（幂等：已存在则跳过）
            SyncClientLoginAccount(user.UId, dto.mobile);

            // 写入登录日志（与账号密码登录保持一致：每次生成新的登录凭据 code）
            var log = new SysLoginLog
            {
                Id = Config.NewSequentialGuid(),
                Account = user.UAccount,
                LoginAddress = "没有",
                Code = Config.NewSequentialGuid(),
                LoginIp = "也没有",
                LoginTime = DateTime.Now,
                UserId = user.UId
            };

            _sys_Login_Log_EFCore.Add(log);
            var result = _sys_Login_Log_EFCore.SaveChanges();

            // B 方案：登录即触发历史订单/地址 owner 迁移（把孤儿旧 UId 迁到当前规范 UId）。
            // 已注册用户每次登录都会走到这里，确保遗留数据被及时收敛；失败不影响登录主流程。
            TriggerLegacyConsolidation(dto.mobile);

            // 返回稳定用户主键 UId 作为登录凭据（由 Controller 生成 JWT 的主题标识），
            // 保证退出再登录后订单/地址等数据仍归属同一账号，而非每次登录都变的登录日志 Code。
            return Task.FromResult(Get_Result(result, "ok", user.UId));
        }


        /// <summary>
        /// 确保 sys_users 中存在该手机号的消费者账号，不存在则创建（手机号即账号）。
        /// 登录（免注册）与注册共用；创建成功返回用户，失败返回 null。
        /// </summary>
        private SysUser? EnsureConsumerAccount(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile)) return null;

            // USalt 继续写入随机盐：仅作历史标记（Argon2 的盐值已内嵌在哈希串中，不参与校验）
            var salt = Guid.NewGuid().ToString("N").Substring(0, 8);
            var keys = _configuration["Login:Keys"];
            var pwd = EncryptHelper.HashPassword("123456", keys);

            var user = new SysUser
            {
                UId = Config.NewSequentialGuid(),
                UAccount = mobile,
                UName = mobile,
                UPassword = pwd,
                USalt = salt,
                UCreateTime = DateTime.Now,
                UIsBan = 0,
                Dept = null,
                Role = null
            };

            _sys_User_EFCore.Add(user);
            var addResult = _sys_User_EFCore.SaveChanges();
            if (addResult <= 0)
            {
                return null;
            }
            return user;
        }


        /// <summary>
        /// 登录成功后把用户账号同步到 client_login 表（幂等：account 已存在则跳过）。
        /// client_login.client_id 列存放归属用户 UId；name 默认取手机号（可在设置页修改），
        /// avatar 初始为空。密码/盐字段已于 2026-10-17 从表结构移除（消费者不走密码登录）。
        /// </summary>
        private void SyncClientLoginAccount(string userId, string mobile)
        {
            try
            {
                var exists = _client_Login_EFCore
                    .Query(d => d.Account == mobile)
                    .Any();
                if (exists) return;

                var entity = new ClientLogin
                {
                    ClientId = userId,
                    Account = mobile,
                    Name = mobile,
                    CreateTime = DateTime.Now
                };

                _client_Login_EFCore.Add(entity);
                _client_Login_EFCore.SaveChanges();
            }
            catch (Exception ex)
            {
                // 同步 client_login 失败不阻断登录主流程，仅告警
                _logger.LogWarning(ex, "同步 client_login 失败（手机号 {Mobile}）", mobile);
            }
        }


        /// <summary>
        /// 获取当前登录消费者的用户资料（client_login：账号 / 用户名称 / 头像）。
        /// code 为 JWT 主题（消费者登录时为 sys_users.UId，与 client_login.client_id 对应）。
        /// </summary>
        public Api_Response Get_Client_Profile(string code, string? mobile)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Fail("登录已失效，请重新登录");
            }

            var login = _client_Login_EFCore.Query(d => d.ClientId == code).FirstOrDefault();
            if (login == null)
            {
                // 兜底：登录时会同步建行，正常不会走到；返回账号信息供前端展示
                return Ok("ok", new
                {
                    account = mobile ?? string.Empty,
                    name = string.Empty,
                    avatar = string.Empty
                });
            }

            return Ok("ok", new
            {
                account = login.Account,
                name = login.Name ?? string.Empty,
                avatar = login.Avatar ?? string.Empty
            });
        }


        /// <summary>
        /// 更新用户名称 / 头像（client_login，按 JWT 身份定位；行不存在时自动补建）。
        /// name 为 null/空白视为不修改；avatar 传 null 不修改，显式传 "" 可清除头像。
        /// </summary>
        public Api_Response Update_Client_Profile(string code, string? mobile, string? name, string? avatar)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Fail("登录已失效，请重新登录");
            }

            var login = _client_Login_EFCore.Query(d => d.ClientId == code).FirstOrDefault();
            if (login == null)
            {
                // 兜底补建（正常登录时已建行）
                if (string.IsNullOrWhiteSpace(mobile))
                {
                    return Fail("账号信息缺失，请重新登录");
                }
                login = new ClientLogin
                {
                    ClientId = code,
                    Account = mobile,
                    Name = mobile,
                    CreateTime = DateTime.Now
                };
                _client_Login_EFCore.Add(login);
            }
            else
            {
                _client_Login_EFCore.Update(login);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                login.Name = name.Trim();
            }
            if (avatar != null)
            {
                login.Avatar = avatar;
            }

            var result = _client_Login_EFCore.SaveChanges();

            return Get_Result(result, "保存成功", new
            {
                account = login.Account,
                name = login.Name ?? string.Empty,
                avatar = login.Avatar ?? string.Empty
            });
        }


        /// <summary>
        /// 消费者（C端）账号注册
        /// 校验短信验证码 → 查重 → 创建 sys_users 账号（手机号即账号）→ 写入登录日志并返回登录凭据 code
        /// </summary>
        public Task<Api_Response> Register(Register_Request_Dto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.mobile) || string.IsNullOrWhiteSpace(dto.code))
            {
                return Task.FromResult(Fail("手机号或验证码不能为空"));
            }

            // 验证码校验（与登录共用 sms_code 表，取最近一条未使用且未过期的记录）
            var codeEntity = _sms_Code_EFCore
                .Query(false, d => d.CreateTime, d => d.Mobile == dto.mobile && d.Code == dto.code && d.IsUsed == 0)
                .FirstOrDefault();

            if (codeEntity == null)
            {
                return Task.FromResult(Fail("验证码错误或已使用"));
            }
            if (codeEntity.ExpireTime.HasValue && codeEntity.ExpireTime.Value < DateTime.Now)
            {
                return Task.FromResult(Fail("验证码已过期，请重新获取"));
            }

            // 账号查重
            if (_sys_User_EFCore.Query(d => d.UAccount == dto.mobile).Any())
            {
                return Task.FromResult(Fail("该手机号已注册"));
            }

            // 标记验证码已使用
            codeEntity.IsUsed = 1;
            _sms_Code_EFCore.Update(codeEntity);
            _sms_Code_EFCore.SaveChanges();

            // 创建消费者账号（手机号即账号，默认密码 123456 采用 Argon2id 加盐哈希存储）
            // 与登录（免注册自动建号）共用 EnsureConsumerAccount，保证两条路径建号规则一致
            var user = EnsureConsumerAccount(dto.mobile);
            if (user == null)
            {
                return Task.FromResult(Fail("注册失败，请稍后重试"));
            }

            // 写入登录日志（与短信登录保持一致，生成新的登录凭据 code 以支持自动登录）
            var log = new SysLoginLog
            {
                Id = Config.NewSequentialGuid(),
                Account = user.UAccount,
                LoginAddress = "没有",
                Code = Config.NewSequentialGuid(),
                LoginIp = "也没有",
                LoginTime = DateTime.Now,
                UserId = user.UId
            };

            _sys_Login_Log_EFCore.Add(log);
            var logResult = _sys_Login_Log_EFCore.SaveChanges();

            // B 方案：新注册账号即把同手机号下的历史订单/地址 owner（旧 UId）迁到本账号。
            TriggerLegacyConsolidation(dto.mobile);

            // 返回稳定用户主键 UId 作为登录凭据（注册后自动登录，与短信登录保持一致），
            // 保证退出再登录后订单/地址等数据仍归属同一账号。
            return Task.FromResult(Get_Result(logResult, "注册成功", user.UId));
        }


        /// <summary>
        /// B 方案触发点：登录/注册成功后调用，把同手机号下的历史订单/地址 owner（旧 UId）迁移到当前规范 UId。
        /// 同步执行（在请求作用域内，DbContext 仍存活），并用 try/catch 包裹，迁移失败只告警，
        /// 绝不阻断登录/注册主流程。
        /// </summary>
        private void TriggerLegacyConsolidation(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile)) return;
            try
            {
                // 懒加载订单服务：认证关键路径（check_login / 登录 / 菜单）不应强依赖订单/实时通知服务，
                // 故改为按需解析，缺失时安全跳过，绝不阻断登录主流程。
                var orderService = _serviceProvider.GetService<IOrderService>();
                if (orderService == null) return;
                var migrated = orderService.ConsolidateLegacyOwnersByMobileAsync(mobile).GetAwaiter().GetResult();
                if (migrated > 0)
                {
                    _logger.LogInformation("历史 owner 迁移：手机号 {Mobile} 已收敛 {Count} 条孤儿记录到规范账号", mobile, migrated);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "历史 owner 迁移失败（手机号 {Mobile}）", mobile);
            }
        }

        /// <summary>
        /// 根据当前登录凭据，返回该用户基本信息（精简后不再返回菜单权限树）
        /// </summary>
        public Api_Response Get_Current_User(string code)
        {
            var user = Check_Login(code);
            if (user == null)
            {
                return Fail("登录已失效，请重新登录");
            }

            return Ok("ok", new
            {
                account = user.UAccount,
                name = user.UName,
                avatar = user.Avatar
            });
        }

        /// <summary>
        /// 更新消费者个人信息（昵称 / 头像）
        /// 通过当前登录凭据 code 解析出稳定用户，修改 sys_users 的 u_name / avatar
        /// </summary>
        public Api_Response Update_Profile(string code, string? name, string? avatar)
        {
            var user = Check_Login(code);
            if (user == null)
            {
                return Fail("登录已失效，请重新登录");
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                user.UName = name.Trim();
            }
            // avatar 可为空字符串（清除头像）或具体地址
            if (avatar != null)
            {
                user.Avatar = avatar;
            }

            _sys_User_EFCore.Update(user);
            var result = _sys_User_EFCore.SaveChanges();

            return Get_Result(result, "ok", new
            {
                name = user.UName,
                avatar = user.Avatar
            });
        }


        /// <summary>
        /// 根据当前登录凭据，返回该用户可见的菜单权限树（按 部门+角色 → 菜单 过滤）。
        /// </summary>
        public Api_Response Get_Current_Menus(string code)
        {
            var user = Check_Login(code);
            if (user == null)
            {
                return Fail("登录已失效，请重新登录");
            }

            // 部门 + 角色 → 部门角色
            var deptRole = _deptRole_EFCore
                .Query(d => d.DepartmentId == user.Dept && d.RoleId == user.Role && d.IsBan != 1)
                .FirstOrDefault();
            if (deptRole == null)
            {
                return Ok("ok", new List<Menu_Response_Dto>());
            }

            // 部门角色 → 菜单ID
            var menuIds = _deptRoleMenu_EFCore
                .Query(d => d.DeptRoleId == deptRole.Id)
                .Select(d => d.MenuId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToList();

            var menus = _menu_EFCore
                .Query(d => menuIds.Contains(d.Id) && d.IsBan != 1)
                .OrderBy(d => d.Sort)
                .ToList();

            // 补齐祖先菜单：用户常只绑定子菜单（如 /sys/menu）而未显式绑定父菜单
            // （如 /sys 系统管理）。若不补齐，Build_Menu_Tree 会把子菜单降级为顶级平铺，
            // 导致左侧"系统管理"分组丢失。这里递归向上补齐所有未包含的父链。
            var resolved = menus.ToDictionary(m => m.Id);
            var pending = new List<SysMenu>(menus);
            while (pending.Count > 0)
            {
                var next = new List<SysMenu>();
                foreach (var m in pending)
                {
                    if (string.IsNullOrWhiteSpace(m.ParentId) || resolved.ContainsKey(m.ParentId))
                        continue;

                    var parent = _menu_EFCore
                        .Query(d => d.Id == m.ParentId && d.IsBan != 1)
                        .FirstOrDefault();
                    if (parent != null)
                    {
                        resolved[parent.Id] = parent;
                        next.Add(parent);
                    }
                }
                pending = next;
            }

            var tree = Build_Menu_Tree(resolved.Values.OrderBy(d => d.Sort).ToList());
            return Ok("ok", tree);
        }


        /// <summary>
        /// 由扁平菜单列表构建多层级菜单树
        /// </summary>
        private List<Menu_Response_Dto> Build_Menu_Tree(List<SysMenu> all)
        {
            var dict = all.ToDictionary(m => m.Id);
            return all
                .Where(m => string.IsNullOrEmpty(m.ParentId) || !dict.ContainsKey(m.ParentId))
                .OrderBy(m => m.Sort)
                .Select(m => To_Menu_Node(m, all))
                .ToList();
        }

        /// <summary>
        /// 清洗菜单名称：去掉前导英文/数字/下划线前缀及后续空格
        /// </summary>
        private string Clean_Menu_Name(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name ?? string.Empty;
            return Regex.Replace(name.Trim(), @"^[a-zA-Z0-9_]+\s+", string.Empty).Trim();
        }

        /// <summary>
        /// 递归构造单个菜单节点（含子节点）
        /// </summary>
        private Menu_Response_Dto To_Menu_Node(SysMenu m, List<SysMenu> all)
        {
            return new Menu_Response_Dto
            {
                id = m.Id,
                parent_id = m.ParentId,
                name = Clean_Menu_Name(m.MenuName),
                path = m.Path,
                icon = m.Icon,
                isMenu = m.IsMenu,
                sort = m.Sort,
                children = all
                    .Where(c => c.ParentId == m.Id && c.IsBan != 1)
                    .OrderBy(c => c.Sort)
                    .Select(c => To_Menu_Node(c, all))
                    .ToList()
            };
        }
        /// <summary>
        /// 修改登录密码：校验原密码 → 生成 Argon2id 新哈希 → 加密存储新密码
        /// 与 Login_Admin 使用相同的 Argon2 规则保证一致性
        /// </summary>
        public Task<Api_Response> Change_Password(string code, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                return Task.FromResult(Fail("原密码和新密码不能为空"));
            }

            if (newPassword.Length < 6)
            {
                return Task.FromResult(Fail("新密码长度不能少于6位"));
            }

            var user = Check_Login(code);
            if (user == null)
            {
                return Task.FromResult(Fail("登录已失效，请重新登录"));
            }

            // 仅 Argon2id 校验
            var keys = _configuration["Login:Keys"];
            if (EncryptHelper.Verify(oldPassword, user.UPassword, user.USalt, keys)
                == EncryptHelper.PasswordVerifyOutcome.Failed)
            {
                return Task.FromResult(Fail("原密码错误"));
            }

            // 生成新密码 Argon2id 哈希（盐值与参数内嵌哈希串；USalt 继续写入随机盐仅作历史标记）
            var newSalt = Guid.NewGuid().ToString("N").Substring(0, 8);
            user.UPassword = EncryptHelper.HashPassword(newPassword, keys);
            user.USalt = newSalt;

            _sys_User_EFCore.Update(user);
            var result = _sys_User_EFCore.SaveChanges();

            return Task.FromResult(Get_Result(result, "密码修改成功"));
        }


    }
}
