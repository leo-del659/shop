using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using ECommerce.Infrastructure.Tools;
using ECommerce.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ECommerce.Api.Admin.Filters
{
    public class Action_Filter : IAsyncActionFilter
    {
        private readonly IConfiguration _configuration;
        private readonly I_Log_Service _log_Service;

        public Action_Filter(IConfiguration configuration, I_Log_Service log_Service)
        {
            _configuration = configuration;
            _log_Service = log_Service;

        }

        /// <summary>
        /// action 执行之后 会执行它
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="NotImplementedException"></exception>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {


            // 执行控制器接口
            var executedCtx = await next();
            var result = executedCtx.Result as ObjectResult;
            // 3. 读取返回结果


            // context 就是方法参数 ActionExecutingContext / ActionExecutedContext
            var descriptor = context.ActionDescriptor;

            // 1. 方式一：直接判断是否存在指定特性（推荐）
            var attr = descriptor.EndpointMetadata.Where(m => m is ActiconLog_Attribute).SingleOrDefault();

            if (attr != null)
            {

                //我这里是行为日志 不是异常日志 

                // GET 路由、查询参数、POST 实体对象全部在这里

                var paras = "";
                if (context.HttpContext.Request.Method == "GET")
                {
                    var keys = context.HttpContext.Request.Query.Keys;
                    foreach (var item in keys)
                    {
                        paras += $"{item}={context.HttpContext.Request.Query[item]};";
                    }
                }
                else
                {
                    var request = context.HttpContext.Request;
                    //启动倒带方式
                    request.EnableBuffering();

                    request.Body.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(request.Body, Encoding.UTF8);
                    paras = await reader.ReadToEndAsync();
                    //如果记录到数据库中 不要全部记录 
                    //如果记录到本地文件中 则可以全部记录
                    paras = paras.Length > 999 ? paras.Substring(0, 999) : paras;


                    paras = MaskPasswordJson(paras);


                }

                var res = result!.Value as Api_Response;
                var log_attr = attr as ActiconLog_Attribute;
                string message = "";
                var api = context.HttpContext.Request.Path;
                message += $"时间：{DateTime.Now.ToString()}\r\nURL:{api}\r\n" +
                    $"类型：{log_attr!.group}\r\n" +
                    $"行为：{log_attr!.action}\r\n" +
                    $"Method:{context.HttpContext.Request.Method}\r\n" +
                    $"参数：{paras}\r\n" +
                    $"结果：{res!.code}\r\n";

                var path = _configuration["Logs:Action_Log"];
                var fileName = DateTime.Now.ToString("yyyyMMdd") + ".txt";
                System.IO.File.AppendAllText(path + fileName, message);

                //存完记事本之后，还要存入 数据库 ，你们自己存就好了

                _log_Service.Add_Action(new Infrastructure.EFCore.SysOperLog
                {
                    Id = Config.NewSequentialGuid(),
                    CreateAt = DateTime.Now,
                    OperAction = log_attr!.action,
                    BusinessType = log_attr!.group,
                    OperTime = DateTime.Now,
                    RequestMethod = context.HttpContext.Request.Method,
                    RequestParams = paras,
                    RequestUrl = api,
                    Result = res.code.ToString(),
                });

            }
        }



        /// <summary>
        /// 纯字符串处理，替换password:"xxx"引号内内容为*****，不用正则
        /// </summary>
        private string MaskPasswordJson(string jsonStr)
        {

            var start_index = jsonStr.IndexOf("password");
            if (start_index > -1)
            {
                string s = jsonStr.Substring(start_index + 12);

                var stop_index = s.IndexOf("\"");
                int sss = start_index + 12 + stop_index;

                var stop_str = "******" + s.Substring(stop_index);

                jsonStr = jsonStr.Substring(0, start_index + 12) + stop_str;


            }


            return jsonStr;
        }


    }
}
