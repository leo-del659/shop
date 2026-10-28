import { getChatConversations, ChatConversation } from '../../api/chat'
import { requireLogin } from '../../app'

/** 把 "yyyy-MM-dd HH:mm:ss" 裁剪成会话列表时间：今天只显示时分，更早显示月-日 */
function convTimeText(raw?: string): string {
  if (!raw) return ''
  const d = new Date(raw.replace(' ', 'T'))
  if (isNaN(d.getTime())) return raw
  const now = new Date()
  const p = (n: number) => (n < 10 ? '0' : '') + n
  const sameDay = d.getFullYear() === now.getFullYear() && d.getMonth() === now.getMonth() && d.getDate() === now.getDate()
  if (sameDay) return `${p(d.getHours())}:${p(d.getMinutes())}`
  return `${p(d.getMonth() + 1)}-${p(d.getDate())}`
}

Page({
  data: {
    loading: true,
    conversations: [] as (ChatConversation & { lastTimeText: string })[]
  },

  onShow() {
    if (!requireLogin()) return
    this.loadConversations()
  },

  /** 加载会话列表（历史商家） */
  async loadConversations() {
    try {
      const list = await getChatConversations()
      this.setData({
        loading: false,
        conversations: (list || []).map(c => ({ ...c, lastTimeText: convTimeText(c.lastTime) }))
      })
    } catch (e) {
      this.setData({ loading: false })
    }
  },

  /** 进入与该商家的聊天界面 */
  onEnterChat(e: any) {
    const item = e.currentTarget.dataset.item as ChatConversation
    if (!item || !item.peerId) return
    const params = [
      'merchantId=' + item.peerId,
      'name=' + encodeURIComponent(item.peerName || ''),
      'avatar=' + encodeURIComponent(item.peerAvatar || '')
    ].join('&')
    wx.navigateTo({ url: '/pages/chat/chat?' + params })
  }
})
