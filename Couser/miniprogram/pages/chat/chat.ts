import { getMerchantBrief, getChatMessages, sendChatMessage, ChatMessageItem } from '../../api/chat'
import { requireLogin, getAppToken } from '../../app'
import { BASE_URL } from '../../config'

/** 断线重连最大尝试次数（仅 WebSocket 自身重试，不再降级轮询） */
const MAX_RECONNECT = 5
/** 重连退避基数（毫秒），实际延迟 = 基数 × 已尝试次数 */
const RECONNECT_BASE = 2000

/** 把 HTTP(S) 基址转换为 WebSocket 基址（ws / wss） */
function wsBaseUrl(): string {
  if (BASE_URL.startsWith('https')) return 'wss' + BASE_URL.slice(5)
  return 'ws' + BASE_URL.slice(4)
}

/** 格式化时间展示：今天显示 HH:mm，昨天显示「昨天 HH:mm」，更早显示 MM-dd HH:mm */
function bubbleTimeText(raw?: string): string {
  if (!raw) return ''
  const d = new Date(raw.replace(' ', 'T'))
  if (isNaN(d.getTime())) return raw
  const now = new Date()
  const p = (n: number) => (n < 10 ? '0' : '') + n
  const hm = `${p(d.getHours())}:${p(d.getMinutes())}`
  const startOfDay = (x: Date) => new Date(x.getFullYear(), x.getMonth(), x.getDate()).getTime()
  const diffDays = Math.round((startOfDay(now) - startOfDay(d)) / 86400000)
  if (diffDays === 0) return hm
  if (diffDays === 1) return '昨天 ' + hm
  return `${p(d.getMonth() + 1)}-${p(d.getDate())} ` + hm
}

/** 两条消息间隔超过 5 分钟时显示时间分隔 */
function gapOver5Min(prev?: string, next?: string): boolean {
  if (!prev || !next) return true
  const a = new Date(prev.replace(' ', 'T')).getTime()
  const b = new Date(next.replace(' ', 'T')).getTime()
  if (isNaN(a) || isNaN(b)) return false
  return b - a > 5 * 60 * 1000
}

interface ViewMessage extends ChatMessageItem {
  self: boolean
  timeText: string
  showTime: boolean
}

/** ===== 运行时连接状态（模块级，聊天页通常为单实例，安全） ===== */
let socketTask: any = null
let retryCount = 0
let closedByUser = false

Page({
  data: {
    merchantId: '',
    merchantName: '',
    merchantAvatar: '',
    avatarChar: '商',
    messages: [] as ViewMessage[],
    loading: true,
    input: '',
    canSend: false,
    scrollInto: '',
    scrollHeight: 600,
    safeBottom: 0,
    wsConnected: false
  },

  onLoad(options: any) {
    const merchantId = options.merchantId || ''
    const name = decodeURIComponent(options.name || '')
    const avatar = decodeURIComponent(options.avatar || '')
    this.setData({
      merchantId,
      merchantName: name,
      merchantAvatar: avatar,
      avatarChar: (name || '商')[0]
    })
    if (name) wx.setNavigationBarTitle({ title: name })

    // 可视区域高度：窗口高度 - 输入栏高度
    const info = wx.getSystemInfoSync()
    this.setData({ scrollHeight: info.windowHeight - 110 })

    if (!requireLogin()) return
    if (!merchantId) {
      wx.showToast({ title: '商家不存在', icon: 'none' })
      return
    }

    this.initChat()
  },

  onUnload() {
    // 用户主动离开：关闭 WebSocket 连接
    closedByUser = true
    if (socketTask) {
      try { socketTask.close({}) } catch (e) { /* 忽略 */ }
      socketTask = null
    }
  },

  /** 初始化：补齐商家信息 + 拉取历史消息 + 建立实时连接 */
  async initChat() {
    // 从消息列表进入时有 name；从订单「联系商家」直接进入时补拉商家信息
    if (!this.data.merchantName) {
      try {
        const brief = await getMerchantBrief(this.data.merchantId)
        this.setData({
          merchantName: brief.peerName || '商家',
          merchantAvatar: brief.peerAvatar || '',
          avatarChar: (brief.peerName || '商')[0]
        })
        wx.setNavigationBarTitle({ title: brief.peerName || '商家' })
      } catch (e) {
        // 拉取失败不影响聊天主流程
      }
    }

    await this.loadHistory()
    // 建立 WebSocket 实时通道（无轮询兜底）
    this.connectSocket()
  },

  /** 拉取最近历史消息（升序渲染） */
  async loadHistory() {
    try {
      const res = await getChatMessages(this.data.merchantId)
      this.applyMessages(res.messages || [])
      this.setData({ loading: false })
      this.scrollBottom()
    } catch (e) {
      this.setData({ loading: false })
    }
  },

  /** 建立 WebSocket 实时接收通道 */
  connectSocket() {
    const token = getAppToken()
    const peerId = this.data.merchantId
    if (!token || !peerId) {
      // 无登录态 / 无对端，无法建立连接（不再降级轮询）
      return
    }
    closedByUser = false
    const url =
      `${wsBaseUrl()}/ws/chat?token=${encodeURIComponent(token)}` +
      `&role=client&peerId=${encodeURIComponent(peerId)}`

    let task: any
    try {
      task = wx.connectSocket({ url })
    } catch (e) {
      // 连接建立失败：不再降级轮询，等待下次进入/重连
      return
    }
    socketTask = task

    task.onOpen(() => {
      retryCount = 0
      this.setData({ wsConnected: true })
      // 连接 / 重连成功：补拉断连期间可能漏掉的消息（一次性，非轮询）
      this.catchUp()
    })
    task.onMessage((res: any) => this.onSocketMessage(res))
    task.onClose(() => {
      this.setData({ wsConnected: false })
      if (!closedByUser) this.scheduleReconnect()
    })
    task.onError(() => {
      this.setData({ wsConnected: false })
      // 连接异常：仅尝试 WebSocket 重连，不降级轮询
      this.scheduleReconnect()
    })
  },

  /** 断线退避重连；超过上限则停止（无轮询兜底） */
  scheduleReconnect() {
    if (retryCount >= MAX_RECONNECT) return
    retryCount++
    const delay = RECONNECT_BASE * retryCount
    setTimeout(() => {
      if (closedByUser) return
      this.connectSocket()
    }, delay)
  },

  /** 解析 WS 推送：{type:'msg', message} → 增量追加 */
  onSocketMessage(res: any) {
    let payload: any
    try {
      payload = JSON.parse(res.data)
    } catch (e) {
      return
    }
    if (!payload || payload.type !== 'msg' || !payload.message) return
    this.appendMessages([payload.message as ChatMessageItem])
  },

  /** 连接 / 重连成功后一次性补拉：拉取最后一条之后的新消息（非轮询） */
  async catchUp() {
    const msgs = this.data.messages
    const last = msgs.length ? msgs[msgs.length - 1] : null
    try {
      const res = await getChatMessages(this.data.merchantId, last ? last.createTime : undefined)
      const fresh = res.messages || []
      if (fresh.length) this.appendMessages(fresh)
    } catch (e) {
      // 补拉失败静默，下次重连再尝试
    }
  },

  /** 合并新消息（按 id 去重）并渲染 */
  appendMessages(list: ChatMessageItem[]) {
    if (!list || !list.length) return
    const known = new Set(this.data.messages.map(m => m.id))
    const add = list.filter(m => !known.has(m.id))
    if (!add.length) return
    const merged = this.data.messages.concat(add as any)
    this.applyMessages(merged)
    this.scrollBottom()
  },

  /** 统一格式化消息（方向 / 时间文案 / 时间分隔）并 setData */
  applyMessages(list: ChatMessageItem[]) {
    const view: ViewMessage[] = list.map((m, i) => ({
      ...m,
      self: m.senderType === 1,
      timeText: bubbleTimeText(m.createTime),
      showTime: i === 0 || gapOver5Min(list[i - 1].createTime, m.createTime)
    }))
    this.setData({ messages: view })
  },

  /** 滚动到底部 */
  scrollBottom() {
    const msgs = this.data.messages
    if (!msgs.length) return
    this.setData({ scrollInto: 'msg-' + msgs[msgs.length - 1].id })
  },

  onInput(e: any) {
    const v = e.detail.value || ''
    this.setData({ input: v, canSend: v.trim().length > 0 })
  },

  /** 发送消息（走 HTTP，拿到含 id 的落库结果，避免乐观更新 id 缺失导致重复） */
  async onSend() {
    const content = this.data.input.trim()
    if (!content) return
    // 先本地清空输入框，提升手感；失败再提示
    this.setData({ input: '', canSend: false })
    try {
      const msg = await sendChatMessage(this.data.merchantId, content)
      if (msg && msg.id && !this.data.messages.some(m => m.id === msg.id)) {
        const merged = this.data.messages.concat([msg as any])
        this.applyMessages(merged)
        this.scrollBottom()
      }
    } catch (e) {
      wx.showToast({ title: '发送失败，请重试', icon: 'none' })
    }
  }
})
