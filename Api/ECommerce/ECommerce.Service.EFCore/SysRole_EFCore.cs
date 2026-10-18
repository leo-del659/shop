using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class SysRole_EFCore : Base_EFCore<SysRole>
    {
        public SysRole_EFCore(I_Repository<SysRole> repository) : base(repository)
        {
        }
    }
}
