/**
 * 通用格式化工具
 */

/**
 * 将后端返回的日期（可能是字符串、Date 或 ISO 格式）格式化为 yyyy-MM-dd HH:mm:ss
 * 无法解析时返回原值，避免页面出现 undefined
 */
export const format_datetime = (val: any): string => {
    if (val === null || val === undefined || val === "") return "-";
    const d = val instanceof Date ? val : new Date(val);
    if (isNaN(d.getTime())) return String(val);
    const p = (n: number) => (n < 10 ? "0" + n : "" + n);
    return (
        d.getFullYear() +
        "-" + p(d.getMonth() + 1) +
        "-" + p(d.getDate()) +
        " " + p(d.getHours()) +
        ":" + p(d.getMinutes()) +
        ":" + p(d.getSeconds())
    );
};

/**
 * 将后端返回的日期（可能是字符串、Date 或 ISO 格式）格式化为 yyyy-MM-dd（年月日）
 * 无法解析时返回原值，避免页面出现 undefined
 */
export const format_date = (val: any): string => {
    if (val === null || val === undefined || val === "") return "-";
    const d = val instanceof Date ? val : new Date(val);
    if (isNaN(d.getTime())) return String(val);
    const p = (n: number) => (n < 10 ? "0" + n : "" + n);
    return (
        d.getFullYear() +
        "-" + p(d.getMonth() + 1) +
        "-" + p(d.getDate())
    );
};

/**
 * 生成快递单号（与数据库同类型：字符串）。优先使用物流公司编码拼前缀。
 */
export const gen_tracking_no = (companyCode?: string): string => {
    const d = new Date();
    const p = (n: number) => (n < 10 ? "0" + n : "" + n);
    const ts =
        "" + (d.getFullYear() % 100) + p(d.getMonth() + 1) + p(d.getDate()) +
        p(d.getHours()) + p(d.getMinutes()) + p(d.getSeconds());
    const rand = Math.floor(100000 + Math.random() * 900000);
    const prefix = companyCode && companyCode.length ? companyCode.toUpperCase() : "KD";
    return prefix + ts + rand;
};
