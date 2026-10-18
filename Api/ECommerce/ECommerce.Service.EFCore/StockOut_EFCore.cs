using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class StockOut_EFCore : Base_EFCore<StockOut>
    {
        public StockOut_EFCore(I_Repository<StockOut> repository) : base(repository)
        {
        }
    }
}
