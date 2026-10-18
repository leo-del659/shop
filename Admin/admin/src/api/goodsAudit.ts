import type { AuditHandleDto } from '@/dto/request/goods'
import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base';

/**
 * 待审核列表（仅返回 AuditStatus=0 待审核商品）
 */
export const pending_list = (params: { page: number; size: number; auditStatus?: number }): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/GoodsAudit/PendingList", method: "get", params: { ...params, auditStatus: 0 } });
}

/**
 * 审核处理
 */
export const handle_audit = (data: AuditHandleDto): Promise<Api_Reseponse_Dto> => {
    return request({ url: "/api/GoodsAudit/Handle", method: "post", data });
}
