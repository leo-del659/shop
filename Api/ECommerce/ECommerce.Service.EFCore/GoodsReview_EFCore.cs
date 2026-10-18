using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_, Inject_]
    public class GoodsReview_EFCore : Base_EFCore<GoodsReview>
    {
        public GoodsReview_EFCore(I_Repository<GoodsReview> repository) : base(repository) { }
    }
}
