import { defineStore } from 'pinia';
import { get_notices } from '@/api/sys';
import { Api_Code } from '@/dto/response/base';

const READ_KEY = 'admin_notice_read_ids';

const loadReadIds = (): Set<string> => {
    try {
        const raw = localStorage.getItem(READ_KEY);
        if (raw) return new Set(JSON.parse(raw));
    } catch { }
    return new Set<string>();
};

const saveReadIds = (ids: Set<string>) => {
    localStorage.setItem(READ_KEY, JSON.stringify(Array.from(ids)));
};

export const useNoticeStore = defineStore('notice', {
    state: () => ({
        notices: [] as any[],
        readIds: loadReadIds(),
    }),
    getters: {
        unreadCount(state): number {
            const readSet = state.readIds;
            return state.notices.filter((n: any) => n && n.id != null && !readSet.has(String(n.id))).length;
        },
    },
    actions: {
        async fetchNotices() {
            const res = await get_notices();
            if (res.code === Api_Code.ok) this.notices = res.data || [];
            // 同步其他页面可能写入的 localStorage 变更
            this.readIds = loadReadIds();
        },
        markRead(id: string | number) {
            const key = String(id);
            if (this.readIds.has(key)) return;
            this.readIds.add(key);
            saveReadIds(this.readIds);
        },
        /**
         * 弹出通知下拉时调用：取最早一条未读标记为已读，让红点数字 -1
         * - count >= 2 时减 1（例如 2 → 1）
         * - count == 1 时减到 0，el-badge 自动隐藏（红色数字消失）
         * - count == 0 时无副作用
         */
        peek() {
            const firstUnread = this.notices.find((n: any) => n && n.id != null && !this.readIds.has(String(n.id)));
            if (firstUnread) this.markRead(firstUnread.id);
        },
        markAllRead() {
            let changed = false;
            this.notices.forEach((n: any) => {
                if (n && n.id != null) {
                    const key = String(n.id);
                    if (!this.readIds.has(key)) {
                        this.readIds.add(key);
                        changed = true;
                    }
                }
            });
            if (changed) saveReadIds(this.readIds);
        },
    },
});
