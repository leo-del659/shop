// 消费者个人信息接口
import http from '../utils/request'

export interface Profile {
  /** 登录手机号（账号） */
  account: string
  /** 昵称 */
  name: string
  /** 头像地址（可能为空，可能为相对地址 /api/file/...） */
  avatar: string
}

/** 获取当前登录用户个人信息 */
export function getProfile() {
  return http.get<Profile>('/api/login/Get_Current_User')
}

/** 更新昵称 / 头像，不传或留空的字段保持不变 */
export function updateProfile(data: { name?: string; avatar?: string }) {
  return http.post<Profile>('/api/login/Update_Profile', data)
}
