// 聊天消息接口（消费者 ↔ 商户）
import http from '../utils/request'

export interface ChatMessageItem {
  id: string
  clientId?: string
  merchantId?: string
  /** 发送方：1 买家 2 商户 */
  senderType: number
  content: string
  /** yyyy-MM-dd HH:mm:ss */
  createTime: string
}

export interface ChatPeer {
  peerId: string
  peerName: string
  peerAvatar: string
}

export interface ChatConversation {
  peerId: string
  peerName: string
  peerAvatar: string
  lastContent: string
  lastTime: string
  unread: number
}

/** 会话列表：聊过的商家（含最后一条消息与未读数） */
export function getChatConversations() {
  return http.get<ChatConversation[]>('/api/mini-program/chat/conversations')
}

/** 商家简要信息（名称/头像），用于从订单直接进入尚无历史消息的会话 */
export function getMerchantBrief(merchantId: string) {
  return http.get<ChatPeer>('/api/mini-program/chat/merchants/' + merchantId)
}

/**
 * 拉取与某商家的消息（最近 50 条，升序返回）
 * @param after 最后一条消息的 createTime，用于增量轮询
 */
export function getChatMessages(merchantId: string, after?: string, pageSize = 50) {
  const params: Record<string, any> = { pageSize }
  if (after) params.after = after
  return http.get<{ peer: ChatPeer; messages: ChatMessageItem[] }>(
    '/api/mini-program/chat/merchants/' + merchantId + '/messages',
    params
  )
}

/** 发送消息给商家 */
export function sendChatMessage(merchantId: string, content: string) {
  return http.post<ChatMessageItem>(
    '/api/mini-program/chat/merchants/' + merchantId + '/messages',
    { content }
  )
}
