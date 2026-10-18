using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class MerchantLevel_EFCore : Base_EFCore<MerchantLevel>
    {
        public MerchantLevel_EFCore(I_Repository<MerchantLevel> repository) : base(repository)
        {
        }
    }
}
