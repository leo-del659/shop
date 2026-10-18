using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Request.Login;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{

    [Provider_]
    public interface I_Login_Service
    {

        /// <summary>
        /// 管理员登陆
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public Task<Api_Response> Login_Admin(Login_Request_Dto dto);
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Api_Response Get_User_ByID(string? id);

        /// <summary>
        /// 检查用户是否登陆
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public SysUser? Check_Login(string code);

        /// <summary>
        /// 发送短信验证码（存入 sms_code，开发环境直接返回 code 便于联调）
        /// </summary>
        public Task<Api_Response> Send_Sms_Code(Send_Code_Request_Dto dto);

        /// <summary>
        /// 手机号 + 验证码 登录，校验通过后返回本次登录凭据 code
        /// （免注册：sys_users 无该手机号时自动建号，并同步写入 client_login）
        /// </summary>
        public Task<Api_Response> Login_By_Sms(Sms_Login_Request_Dto dto);

        /// <summary>
        /// 获取当前登录消费者的用户资料（client_login：账号 / 用户名称 / 头像）
        /// </summary>
        public Api_Response Get_Client_Profile(string code, string? mobile);

        /// <summary>
        /// 更新用户名称 / 头像（client_login，按 JWT 身份定位；行不存在时自动补建）
        /// </summary>
        public Api_Response Update_Client_Profile(string code, string? mobile, string? name, string? avatar);

        /// <summary>
        /// 消费者（C端）账号注册，校验短信验证码通过后创建账号并写入登录日志，返回登录凭据 code
        /// </summary>
        public Task<Api_Response> Register(Register_Request_Dto dto);

        /// <summary>
        /// 获取当前登录用户基本信息（精简后不再返回菜单权限树）
        /// </summary>
        public Api_Response Get_Current_User(string code);

        /// <summary>
        /// 更新消费者个人信息（昵称 / 头像），从当前登录凭据解析用户后写入 sys_users
        /// </summary>
        public Api_Response Update_Profile(string code, string? name, string? avatar);

        /// <summary>
        /// 根据当前登录凭据，返回该用户可见的菜单权限树（按 部门+角色 → 菜单 过滤）
        /// </summary>
        public Api_Response Get_Current_Menus(string code);

        /// <summary>
        /// 修改登录密码：校验原密码后重新生成盐值并加密存储新密码
        /// 适用于管理员后台和商户端（JWT 识别用户身份后操作 sys_users 表）
        /// </summary>
        public Task<Api_Response> Change_Password(string code, string oldPassword, string newPassword);


    }
}
