/**
 * 右滑删除控制逻辑（footprint / favorite 列表共用）
 *
 * 用法：
 *   import { createSwipeHandlers, SWIPE_OPEN_WIDTH } from '../../utils/swipe'
 *
 *   Page({
 *     data: { _swipeOffsets: {}, _isDragging: false, list: [], count: 0 },
 *     onLoad() { this._swipe = createSwipeHandlers(this) },
 *     onTouchStart(e) { this._swipe.start(e) },
 *     onTouchMove(e)  { this._swipe.move(e) },
 *     onTouchEnd(e)   { if (this._swipe.end()) { this._suppressClick = true; setTimeout(() => this._suppressClick = false, 250) } },
 *     goDetail(e) { if (this._suppressClick) return; ... }
 *   })
 *
 *   <!-- wxml -->
 *   <view class="swipe-wrap" data-id="{{item.goodsId}}"
 *         bindtouchstart="onTouchStart" bindtouchmove="onTouchMove" bindtouchend="onTouchEnd"
 *         bindtap="goDetail">
 *     <view class="swipe-content" style="transform: translateX({{_swipeOffsets[item.goodsId] || 0}}px); transition: {{_isDragging ? 'none' : 'transform 0.2s ease'}};">
 *       ... 卡片内容 ...
 *     </view>
 *     <view class="swipe-action" style="width: {{SWIPE_OPEN_WIDTH}}px;">
 *       <view class="del-btn" catchtap="onRemove" data-id="{{item.goodsId}}">删除</view>
 *     </view>
 *   </view>
 */

const SWIPE_THRESHOLD = 40   // 偏移 > 40px 视为展开
const SWIPE_OPEN_PX = 160    // 完全展开时的位移

export const SWIPE_OPEN_WIDTH = SWIPE_OPEN_PX

export interface SwipeState {
  id: string
  startX: number
  startY: number
  base: number
}

export function createSwipeHandlers(self: any) {
  let start: SwipeState | null = null

  return {
    start(e: any) {
      const t = e.touches[0]
      const id = e.currentTarget.dataset.id
      const offsets = self.data._swipeOffsets || {}
      start = { id, startX: t.clientX, startY: t.clientY, base: offsets[id] || 0 }
      self.setData({ _isDragging: true })
    },
    move(e: any) {
      if (!start) return
      const t = e.touches[0]
      const dx = t.clientX - start.startX
      const dy = t.clientY - start.startY
      // 纵滑主导时不响应（避免影响 scroll-view / 页面纵向滚动）
      if (Math.abs(dy) > Math.abs(dx) + 10) return
      let next = start.base + dx
      if (next > 0) next = 0
      if (next < -SWIPE_OPEN_PX) next = -SWIPE_OPEN_PX
      const offsets = { ...(self.data._swipeOffsets || {}), [start.id]: next }
      self.setData({ _swipeOffsets: offsets })
    },
    end() {
      if (!start) return false
      const offsets = { ...(self.data._swipeOffsets || {}) }
      const id = start.id
      const cur = offsets[id] || 0
      const target = cur < -SWIPE_THRESHOLD ? -SWIPE_OPEN_PX : 0
      // 标记本轮是否真的发生"显著滑动"（用来抑制 click 误跳详情）
      const moved = Math.abs(cur) > 5
      offsets[id] = target
      self.setData({ _swipeOffsets: offsets, _isDragging: false })
      start = null
      return moved
    }
  }
}
