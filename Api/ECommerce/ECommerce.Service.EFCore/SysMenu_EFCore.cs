using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class SysMenu_EFCore : Base_EFCore<SysMenu>
    {
        public SysMenu_EFCore(I_Repository<SysMenu> repository) : base(repository)
        {
        }
    }
}
