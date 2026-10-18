using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class GoodsSku_EFCore : Base_EFCore<GoodsSku>
    {
        public GoodsSku_EFCore(I_Repository<GoodsSku> repository) : base(repository)
        {
        }
    }
}
