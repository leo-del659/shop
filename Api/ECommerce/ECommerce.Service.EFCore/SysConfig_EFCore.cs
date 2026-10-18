using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class SysConfig_EFCore : Base_EFCore<SysConfig>
    {
        public SysConfig_EFCore(I_Repository<SysConfig> repository) : base(repository)
        {
        }
    }
}
