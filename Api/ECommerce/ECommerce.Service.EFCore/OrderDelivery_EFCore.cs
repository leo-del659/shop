using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class OrderDelivery_EFCore : Base_EFCore<OrderDelivery>
    {
        public OrderDelivery_EFCore(I_Repository<OrderDelivery> repository) : base(repository)
        {
        }
    }
}
