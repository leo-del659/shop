// 收货地址接口（消费者）
import http from '../utils/request'

export interface AddressItem {
  id: string
  name?: string
  mobile?: string
  province?: string
  city?: string
  district?: string
  detailAddr?: string
  /** 用户端根据省/市/区/详细地址拼接，便于展示与下单 */
  fullAddress?: string
  isDefault?: number
}

export interface AddressSaveDto {
  id?: string
  name?: string
  mobile?: string
  province?: string
  city?: string
  district?: string
  detailAddr?: string
  isDefault?: number
}

/** 地址列表（默认地址排前） */
export function getAddressList() {
  return http.get<AddressItem[]>('/api/address/list')
}

/** 新增/更新地址 */
export function saveAddress(data: AddressSaveDto) {
  return http.post<AddressItem>('/api/address/save', data)
}

/** 删除地址 */
export function removeAddress(id: string) {
  return http.post('/api/address/remove', { id })
}

/** 设为默认 */
export function setDefaultAddress(id: string) {
  return http.post('/api/address/setDefault', { id })
}
