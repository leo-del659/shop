using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class OrderStatusLog_EFCore : Base_EFCore<OrderStatusLog>
    {
        public OrderStatusLog_EFCore(I_Repository<OrderStatusLog> repository) : base(repository)
        {
        }
    }
}
