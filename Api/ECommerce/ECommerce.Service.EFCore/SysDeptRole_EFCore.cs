using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class SysDeptRole_EFCore : Base_EFCore<SysDeptRole>
    {
        public SysDeptRole_EFCore(I_Repository<SysDeptRole> repository) : base(repository)
        {
        }
    }
}
