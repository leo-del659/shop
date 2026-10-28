-- ============================================================
-- 商户端「商品多规格」所需表结构
--   goods_spec : 商品规格组（一个商品一组，如「规格」「重量」）
--   goods_sku  : 商品 SKU（每个规格值一行：售价/库存/编码）
-- 说明：goods_sku 表已存在（后台管理端创建商品时写入），
--       本脚本补建 goods_spec 并为 goods_sku 增加 sku_code / sort 列。
-- ============================================================

CREATE TABLE IF NOT EXISTS `goods_spec` (
  `id`         varchar(50)  NOT NULL,
  `goods_id`   varchar(50)  NOT NULL COMMENT '商品ID(good.id)',
  `spec_name`  varchar(128) NOT NULL COMMENT '规格组名称 如：规格/重量/包装',
  `sort`       int          NOT NULL DEFAULT 0 COMMENT '排序',
  `create_time` datetime    DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_gs_goods` (`goods_id`),
  CONSTRAINT `fk_goods_spec_goods_id` FOREIGN KEY (`goods_id`) REFERENCES `good` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='商品规格组表';

-- goods_sku 增加 SKU 编码与排序（可重复执行）
SET @c1 := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'goods_sku' AND COLUMN_NAME = 'sku_code');
SET @s1 := IF(@c1 = 0, 'ALTER TABLE `goods_sku` ADD COLUMN `sku_code` varchar(64) DEFAULT NULL COMMENT ''SKU编码'' AFTER `sku_name`', 'SELECT 1');
PREPARE stmt FROM @s1; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @c2 := (SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'goods_sku' AND COLUMN_NAME = 'sort');
SET @s2 := IF(@c2 = 0, 'ALTER TABLE `goods_sku` ADD COLUMN `sort` int NOT NULL DEFAULT 0 COMMENT ''排序'' AFTER `stock`', 'SELECT 1');
PREPARE stmt FROM @s2; EXECUTE stmt; DEALLOCATE PREPARE stmt;
