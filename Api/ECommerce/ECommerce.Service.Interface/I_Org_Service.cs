using ECommerce.Infrastructure.Attributes;
using ECommerce.Infrastructure.Dto.Response;
using System.Threading.Tasks;

namespace ECommerce.Service.Interface
{
    [Provider_]
    public interface I_Org_Service
    {
        /// <summary>
        /// 部门树
        /// </summary>
        Task<Api_Response> GetDeptTree();
        /// <summary>
        /// 角色列表
        /// </summary>
        Task<Api_Response> GetRoles();
        /// <summary>
        /// 菜单树
        /// </summary>
        Task<Api_Response> GetMenuTree();
        /// <summary>
        /// 部门角色绑定关系
        /// </summary>
        Task<Api_Response> GetDeptRoles();
    }
}
