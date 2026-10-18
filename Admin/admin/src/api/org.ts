import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

/**
 * 部门树
 */
export const get_dept_tree = (): Promise<Api_Reseponse_Dto> => {
    return request({
        url: "/api/Org/GetDeptTree",
        method: "get",
    });
}

/**
 * 角色列表
 */
export const get_roles = (): Promise<Api_Reseponse_Dto> => {
    return request({
        url: "/api/Org/GetRoles",
        method: "get",
    });
}

/**
 * 菜单树
 */
export const get_menu_tree = (): Promise<Api_Reseponse_Dto> => {
    return request({
        url: "/api/Org/GetMenuTree",
        method: "get",
    });
}

/**
 * 部门角色绑定列表
 */
export const get_dept_roles = (): Promise<Api_Reseponse_Dto> => {
    return request({
        url: "/api/Org/GetDeptRoles",
        method: "get",
    });
}
