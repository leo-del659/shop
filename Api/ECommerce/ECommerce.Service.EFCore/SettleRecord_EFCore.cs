using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class SettleRecord_EFCore : Base_EFCore<SettleRecord>
    {
        public SettleRecord_EFCore(I_Repository<SettleRecord> repository) : base(repository)
        {
        }
    }
}
