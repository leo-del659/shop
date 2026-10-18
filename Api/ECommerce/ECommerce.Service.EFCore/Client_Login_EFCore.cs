using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    /// <summary>
    /// ClientLogin 表（clientlogins）的所有数据库操作
    /// </summary>
    [Provider_, Inject_]
    public class Client_Login_EFCore : Base_EFCore<ClientLogin>
    {
        public Client_Login_EFCore(I_Repository<ClientLogin> repository) : base(repository)
        {
        }
    }
}
