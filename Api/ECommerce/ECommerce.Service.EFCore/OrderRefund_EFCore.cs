using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class OrderRefund_EFCore : Base_EFCore<OrderRefund>
    {
        public OrderRefund_EFCore(I_Repository<OrderRefund> repository) : base(repository)
        {
        }
    }
}
