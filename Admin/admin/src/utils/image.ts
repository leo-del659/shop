/**
 * 把后端返回的相对图片地址（如 /api/file/xxx.png）拼接为完整可访问 URL。
 * 已经以 http/https 开头的地址直接返回。
 */
export const get_image_url = (url?: string | null): string => {
    if (!url) return ''
    if (/^https?:\/\//i.test(url)) return url
    const base = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/$/, '')
    const rel = url.startsWith('/') ? url : '/' + url
    return `${base}${rel}`.replace(/([^:])\/\//g, '$1/')
}
