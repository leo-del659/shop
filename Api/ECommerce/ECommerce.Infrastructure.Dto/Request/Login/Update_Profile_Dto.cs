using System;

namespace ECommerce.Infrastructure.Dto.Request.Login
{
    /// <summary>
    /// 消费者（C端）个人信息更新请求
    /// 昵称与头像地址均可单独更新；不传或留空则该字段保持不变
    /// </summary>
    [Serializable]
    public class Update_Profile_Dto
    {
        /// <summary>
        /// 昵称（留空则不修改）
        /// </summary>
        public string? name { get; set; }

        /// <summary>
        /// 头像地址（可为空字符串表示清除头像）
        /// </summary>
        public string? avatar { get; set; }
    }
}
