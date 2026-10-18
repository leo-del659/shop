using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class MerchantQualification_EFCore : Base_EFCore<MerchantQualification>
    {
        public MerchantQualification_EFCore(I_Repository<MerchantQualification> repository) : base(repository)
        {
        }
    }
}
