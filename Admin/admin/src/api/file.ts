import request from '@/utils/request'
import type { Api_Reseponse_Dto } from '@/dto/response/base'

/**
 * 上传文件到后端
 * 后端接口：/api/File/upload
 * 返回的 data 形如 { url: "/api/file/xxx.png" }，可直接用于 el-image / img src
 *
 * 注意：用 FormData 提交时不能手动设 Content-Type，axios 会自动加 boundary
 */
export const upload_file = (file: File): Promise<Api_Reseponse_Dto> => {
    const fd = new FormData()
    fd.append('file', file)
    return request({
        url: '/api/File/upload',
        method: 'post',
        data: fd
    })
}
