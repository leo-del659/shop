/**
 * 接口结果
 */
export enum Api_Code {
    ok = 0,
    fail = -1
}

/**
 * 接口返回格式
 */
export interface Api_Reseponse_Dto {
    /**
     * 响应结果
     */
    code: Api_Code;
    /**
     * 响应消息
     */
    message: string;
    /**
     * 响应数据
     */
    data?: any;
}

export interface User_Dto {
    account: string;
    name: string;
}