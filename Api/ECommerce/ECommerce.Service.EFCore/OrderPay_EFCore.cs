using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class OrderPay_EFCore : Base_EFCore<OrderPay>
    {
        public OrderPay_EFCore(I_Repository<OrderPay> repository) : base(repository)
        {
        }
    }
}
