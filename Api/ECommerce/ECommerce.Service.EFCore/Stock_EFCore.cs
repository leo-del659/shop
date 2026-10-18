using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class Stock_EFCore : Base_EFCore<Stock>
    {
        public Stock_EFCore(I_Repository<Stock> repository) : base(repository)
        {
        }
    }
}
