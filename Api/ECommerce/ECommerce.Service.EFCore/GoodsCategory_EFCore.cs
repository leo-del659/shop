using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class GoodsCategory_EFCore : Base_EFCore<GoodsCategory>
    {
        public GoodsCategory_EFCore(I_Repository<GoodsCategory> repository) : base(repository)
        {
        }
    }
}
