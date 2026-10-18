using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class ClientOrder_EFCore : Base_EFCore<ClientOrder>
    {
        public ClientOrder_EFCore(I_Repository<ClientOrder> repository) : base(repository)
        {
        }
    }
}
