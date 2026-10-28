import { getChatConversations, ChatConversation, getMerchantNotices, MerchantNoticeItem } from '../../utils/api'
import { resolveUrl } from '../../utils/request'

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

interface ConvView extends ChatConversation {
  avatar: string
  lastTimeText: string
}

/** 通知条目（带展示用的相对时间） */
interface NoticeView extends MerchantNoticeItem {
  timeText: string
}

Page({
  data: {
    conversations: [] as ConvView[],
    /** 库存预警等系统通知（显示在最顶部） */
    notices: [] as NoticeView[],
    /** 未读通知数 */
    noticeUnread: 0,
    /** 库存预警入口：最新一条内容预览 */
    noticePreview: '',
    /** 库存预警入口：最新一条时间 */
    noticeTimeText: '',
  },

  onShow() {
    this.loadConversations()
    this.loadNotices()
  },

  /** 加载会话列表（聊过的买家） */
  async loadConversations() {
    try {
      const list = await getChatConversations()
      this.setData({
        conversations: (list || []).map((c) => ({
          ...c,
          avatar: resolveUrl(c.peerAvatar || ''),
          lastTimeText: convTimeText(c.lastTime),
        })),
      })
    } catch (e) {
      // 拉取失败保留旧数据，下次 onShow 重试
    }
  },

  /**
   * 加载系统通知：后端在返回列表前会扫描库存，
   * 低库存商品自动生成库存预警（同一商品库存未变化前不重复生成）。
   */
  async loadNotices() {
    try {
      const res = await getMerchantNotices()
      const list = res && res.notices ? res.notices : []
      const notices = list.map((n) => ({ ...n, timeText: convTimeText(n.createTime) }))
      // 后端按时间倒序返回：首条即最新
      const latest = notices[0]
      this.setData({
        notices,
        noticeUnread: (res && res.unread) || 0,
        noticePreview: latest ? (latest.content || latest.title || '') : '',
        noticeTimeText: latest ? latest.timeText : '',
      })
    } catch (e) {
      // 通知拉取失败不影响会话列表
    }
  },

  /** 点击库存预警：进入聊天气泡界面查看全部预警 */
  onNoticeTap() {
    wx.navigateTo({ url: '/pages/chat/warning/warning' })
  },

  /** 进入与该买家的聊天界面 */
  onEnterChat(e: any) {
    const item = e.currentTarget.dataset.item as ConvView
    if (!item || !item.peerId) return
    const params = [
      'clientId=' + item.peerId,
      'name=' + encodeURIComponent(item.peerName || ''),
      'avatar=' + encodeURIComponent(item.avatar || ''),
    ].join('&')
    wx.navigateTo({ url: '/pages/chat/chat?' + params })
  },
})
