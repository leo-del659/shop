using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class SettleRefund_EFCore : Base_EFCore<SettleRefund>
    {
        public SettleRefund_EFCore(I_Repository<SettleRefund> repository) : base(repository)
        {
        }
    }
}
