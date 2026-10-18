import type { Login_Request_Dto } from '@/dto/request/base'
import type { Api_Reseponse_Dto } from '@/dto/response/base';
import request from '@/utils/request'

/**
 * 管理员登陆
 * @param data 
 */
export const logins = (data: Login_Request_Dto): Promise<Api_Reseponse_Dto> => {
    return request({
        url: "/api/login/Login_Admin",
        method: "post",
        data
    });
}

/**
 * 检查登陆
 * @returns 
 */
export const check_login = (): Promise<Api_Reseponse_Dto> => {
    return request({
        url: "/api/login/Check_Login",
        method: "get",
    });
}

/**
 * 获取用户菜单（复杂 RBAC 权限树）
 * @returns 
 */
export const get_menus = (): Promise<Api_Reseponse_Dto> => {
    return request({
        url: "/api/login/Get_Menus",
        method: "get",
    });
}
