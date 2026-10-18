using System.Collections.Generic;
using System.Text.Json;

namespace ECommerce.Infrastructure.EFCore
{
    /// <summary>
    /// client_order_item 已合并进 client_order（见 items_json 列）。
    /// 本 DTO 与 items_json 数组中的元素一一对应；字段命名采用 PascalCase，
    /// 反序列化时开启大小写不敏感以兼容 SQL 写入的 camelCase key。
    /// </summary>
    public class ClientOrderItemDto
    {
        public string? Id { get; set; }
        public string? GoodsId { get; set; }
        public string? SpecId { get; set; }
        public string? GoodsName { get; set; }
        public string? SpecName { get; set; }
        public string? GoodsImg { get; set; }
        public double? BuyPrice { get; set; }
        public int? BuyNum { get; set; }
        public double? SubAmount { get; set; }
        public int? IsBan { get; set; }
        public string? CreateTime { get; set; }
        public string? UpdateTime { get; set; }
    }

    /// <summary>
    /// client_order.items_json 的解析与序列化辅助方法。
    /// 合并表后，订单明细统一内联在 client_order 一行里，避免跨表查询。
    /// </summary>
    public static class ClientOrderItemsHelper
    {
        private static readonly JsonSerializerOptions ReadOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly JsonSerializerOptions WriteOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// 解析订单的 items_json 为明细列表；空或非法 JSON 返回空列表（不抛异常）。
        /// </summary>
        public static List<ClientOrderItemDto> ParseItems(this ClientOrder order)
        {
            if (order == null || string.IsNullOrWhiteSpace(order.ItemsJson))
                return new List<ClientOrderItemDto>();
            return ParseItems(order.ItemsJson);
        }

        /// <summary>
        /// 解析 items_json 字符串为明细列表；空或非法 JSON 返回空列表（不抛异常）。
        /// 便于 Service / Controller 在只投影了 ItemsJson 列的场景下复用解析逻辑。
        /// </summary>
        public static List<ClientOrderItemDto> ParseItems(string itemsJson)
        {
            if (string.IsNullOrWhiteSpace(itemsJson))
                return new List<ClientOrderItemDto>();
            try
            {
                var list = JsonSerializer.Deserialize<List<ClientOrderItemDto>>(itemsJson, ReadOptions);
                return list ?? new List<ClientOrderItemDto>();
            }
            catch
            {
                return new List<ClientOrderItemDto>();
            }
        }

        /// <summary>
        /// 将明细列表序列化为 camelCase 的 items_json 字符串。
        /// </summary>
        public static string ToItemsJson(this List<ClientOrderItemDto> items)
        {
            return JsonSerializer.Serialize(items ?? new List<ClientOrderItemDto>(), WriteOptions);
        }
    }
}
