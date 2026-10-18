using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class LogisticsCompany_EFCore : Base_EFCore<LogisticsCompany>
    {
        public LogisticsCompany_EFCore(I_Repository<LogisticsCompany> repository) : base(repository)
        {
        }
    }
}
