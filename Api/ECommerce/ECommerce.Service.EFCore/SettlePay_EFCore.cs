using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class SettlePay_EFCore : Base_EFCore<SettlePay>
    {
        public SettlePay_EFCore(I_Repository<SettlePay> repository) : base(repository)
        {
        }
    }
}
