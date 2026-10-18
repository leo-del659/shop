import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

/* ============ 部门 ============ */
export const get_depts = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/GetDepts", method: "get" });
}
export const save_dept = (data: any): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/SaveDept", method: "post", data });
}
export const remove_dept = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/RemoveDept", method: "get", params: { id } });
}

/* ============ 角色 ============ */
export const get_roles = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/GetRoles", method: "get" });
}
export const save_role = (data: any): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/SaveRole", method: "post", data });
}
export const remove_role = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/RemoveRole", method: "get", params: { id } });
}

/* ============ 部门树（读取自 OrgController） ============ */
export const get_dept_tree = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Org/GetDeptTree", method: "get" });
}

/* ============ 部门角色绑定（读取自 OrgController） ============ */
export const get_dept_roles = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/Org/GetDeptRoles", method: "get" });
}

/* ============ 菜单 ============ */
export const get_menus_all = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/GetMenus", method: "get" });
}
export const save_menu = (data: any): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/SaveMenu", method: "post", data });
}
export const remove_menu = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/RemoveMenu", method: "get", params: { id } });
}

/* ============ 部门角色绑定 ============ */
export const save_dept_role = (data: any): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/SaveDeptRole", method: "post", data });
}
export const bind_dept_role_menus = (data: { deptrole_ids: string[]; menu_ids: string[] }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/BindDeptRoleMenus", method: "post", data });
}
export const get_dept_role_menus = (deptRoleId: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/GetDeptRoleMenus", method: "get", params: { deptRoleId } });
}

/* ============ 系统配置 ============ */
export const get_configs = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/GetConfigs", method: "get" });
}
export const save_config = (data: any): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/SaveConfig", method: "post", data });
}

/* ============ 系统通知 ============ */
export const get_notices = (): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/GetNotices", method: "get" });
}
export const save_notice = (data: any): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/SaveNotice", method: "post", data });
}
export const remove_notice = (id: string): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/AdminSys/RemoveNotice", method: "get", params: { id } });
}
