using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class GoodsSpec_EFCore : Base_EFCore<GoodsSpec>
    {
        public GoodsSpec_EFCore(I_Repository<GoodsSpec> repository) : base(repository)
        {
        }
    }
}
