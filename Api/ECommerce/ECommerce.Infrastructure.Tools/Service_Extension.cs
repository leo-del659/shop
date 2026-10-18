using ECommerce.Infrastructure.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace ECommerce.Infrastructure.Tools
{
    public static class Service_Extension
    {
        /// <summary>
        /// 批量注入AddScoped
        /// </summary>
        /// <param name="service">原始类</param>
        /// <param name="providers">提供者程序集</param>
        /// <param name="injects">注入者程序集</param>
        public static void AddScoped_Extension(this IServiceCollection service,Assembly providers,Assembly injects)
        {
           
            var pros = providers.GetTypes().Where(d => d.GetCustomAttribute(typeof(Provider_Attribute)) != null
            ).ToList();
            var injs = injects.GetTypes().Where(d => d.GetCustomAttribute(typeof(Inject_Attribute)) != null
           ).ToList();
            foreach (var item in pros)
            {
               var bfs= injs.Where(d => item.IsAssignableFrom(d)).ToList();//找出injs中所有是item 的子类的类型

                bfs.ForEach(e =>
                {
                    service.AddScoped(item, e);
                });
            }
        }


        /// <summary>
        /// 批量注入AddTransient
        /// </summary>
        /// <param name="service">原始类</param>
        /// <param name="providers">提供者程序集</param>
        /// <param name="injects">注入者程序集</param>
        public static void AddTransient_Extension(this IServiceCollection service, Assembly providers, Assembly injects)
        {

            var pros = providers.GetTypes().Where(d => d.GetCustomAttribute(typeof(Provider_Attribute)) != null
            ).ToList();
            var injs = injects.GetTypes().Where(d => d.GetCustomAttribute(typeof(Inject_Attribute)) != null
           ).ToList();
            foreach (var item in pros)
            {
                var bfs = injs.Where(d => item.IsAssignableFrom(d)).ToList();//找出injs中所有是item 的子类的类型

                bfs.ForEach(e =>
                {
                    service.AddTransient(item, e);
                });
            }
        }



        /// <summary>
        /// 批量注入AddSingleton
        /// </summary>
        /// <param name="service">原始类</param>
        /// <param name="providers">提供者程序集</param>
        /// <param name="injects">注入者程序集</param>
        public static void AddSingleton_Extension(this IServiceCollection service, Assembly providers, Assembly injects)
        {

            var pros = providers.GetTypes().Where(d => d.GetCustomAttribute(typeof(Provider_Attribute)) != null
            ).ToList();
            var injs = injects.GetTypes().Where(d => d.GetCustomAttribute(typeof(Inject_Attribute)) != null
           ).ToList();
            foreach (var item in pros)
            {
                var bfs = injs.Where(d => item.IsAssignableFrom(d)).ToList();//找出injs中所有是item 的子类的类型

                bfs.ForEach(e =>
                {
                    service.AddSingleton(item, e);
                });
            }
        }

    }
}
