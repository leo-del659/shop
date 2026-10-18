using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class SysDeptRoleMenu_EFCore : Base_EFCore<SysDeptRoleMenu>
    {
        public SysDeptRoleMenu_EFCore(I_Repository<SysDeptRoleMenu> repository) : base(repository)
        {
        }
    }
}
