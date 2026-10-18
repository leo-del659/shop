using ECommerce.Infrastructure.Dto.Response;

namespace ECommerce.Service.Instance
{
    public class Base_Service
    {
        /// <summary>
        /// 获取结果
        /// </summary>
        /// <param name="result"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Api_Response Get_Result(int result, string? message="ok", object? data=null)
        {
            var res = new Api_Response
            {
                code = result > 0 ? Api_Code.ok : Api_Code.fail,
                message = result > 0 ? "操作成功" : "操作失败",
                data = data
            };
            if (!string.IsNullOrEmpty(message))
            {
                res.message = message;
            }
            return res;

        }


        /// <summary>
        /// 正确的操作
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Api_Response Ok(string message = "ok", object? data = null)
        {
            return new Api_Response
            {
                code = Api_Code.ok,
                message = message,
                data = data
            };
        }

        /// <summary>
        /// 错误的操作
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Api_Response Fail(string? message = "操作失败")
        {
            return new Api_Response
            {
                code = Api_Code.fail,
                message = message
            };
        }


    }
}
