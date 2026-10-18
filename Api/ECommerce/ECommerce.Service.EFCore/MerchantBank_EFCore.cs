using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class MerchantBank_EFCore : Base_EFCore<MerchantBank>
    {
        public MerchantBank_EFCore(I_Repository<MerchantBank> repository) : base(repository)
        {
        }
    }
}
