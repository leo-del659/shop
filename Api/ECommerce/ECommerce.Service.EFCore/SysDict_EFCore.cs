using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class SysDict_EFCore : Base_EFCore<SysDict>
    {
        public SysDict_EFCore(I_Repository<SysDict> repository) : base(repository)
        {
        }
    }
}
