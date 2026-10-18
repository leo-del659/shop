import Cookies from 'js-cookie'

const admin_token = "admin_token";

/**
 * 保存token
 * @param token 
 */
export const setToken = (token: string) => {
    Cookies.set(admin_token, token);
}

/**
 * 获取token
 * @returns 
 */
export const getToken = () => {
    return Cookies.get(admin_token);
}

export const removeToken = () => {
    Cookies.remove(admin_token);
}

export const setCookie = (key: string, val: string) => {
    Cookies.set(key, val);
}

/**
 * 移除cookie
 * @param key 
 */
export const removeCookie = (key: string) => {
    Cookies.remove(key);
}