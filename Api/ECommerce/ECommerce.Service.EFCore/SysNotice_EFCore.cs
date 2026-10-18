using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class SysNotice_EFCore : Base_EFCore<SysNotice>
    {
        public SysNotice_EFCore(I_Repository<SysNotice> repository) : base(repository)
        {
        }
    }
}
