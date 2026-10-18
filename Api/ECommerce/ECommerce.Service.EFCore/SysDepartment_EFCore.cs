using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class SysDepartment_EFCore : Base_EFCore<SysDepartment>
    {
        public SysDepartment_EFCore(I_Repository<SysDepartment> repository) : base(repository)
        {
        }
    }
}
