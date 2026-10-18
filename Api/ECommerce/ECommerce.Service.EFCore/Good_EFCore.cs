using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class Good_EFCore : Base_EFCore<Good>
    {
        public Good_EFCore(I_Repository<Good> repository) : base(repository)
        {
        }
    }
}
