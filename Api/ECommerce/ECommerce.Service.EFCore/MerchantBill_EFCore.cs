using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class MerchantBill_EFCore : Base_EFCore<MerchantBill>
    {
        public MerchantBill_EFCore(I_Repository<MerchantBill> repository) : base(repository)
        {
        }
    }
}
