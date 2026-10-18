using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.EFCore;
using ECommerce.Repository.Interface;

namespace ECommerce.Service.EFCore
{
    [Provider_,Inject_]
    public class AfterSchedule_EFCore : Base_EFCore<AfterSchedule>
    {
        public AfterSchedule_EFCore(I_Repository<AfterSchedule> repository) : base(repository)
        {
        }
    }
}
