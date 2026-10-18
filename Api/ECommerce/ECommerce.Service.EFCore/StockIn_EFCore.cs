using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class StockIn_EFCore : Base_EFCore<StockIn>
    {
        public StockIn_EFCore(I_Repository<StockIn> repository) : base(repository)
        {
        }
    }
}
