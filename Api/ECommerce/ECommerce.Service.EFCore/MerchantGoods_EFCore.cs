using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class MerchantGood_EFCore : Base_EFCore<MerchantGood>
    {
        public MerchantGood_EFCore(I_Repository<MerchantGood> repository) : base(repository)
        {
        }
    }
}
