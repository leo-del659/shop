using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class Merchant_EFCore : Base_EFCore<Merchant>
    {
        public Merchant_EFCore(I_Repository<Merchant> repository) : base(repository)
        {
        }
    }
}
