-- 商户通知表（库存预警等系统消息）
-- 与 chat_message（买卖家聊天消息）分离：通知由系统产生，不需要买家参与。
CREATE TABLE IF NOT EXISTS `merchant_notice` (
  `id` varchar(50) NOT NULL,
  `merchant_id` varchar(50) DEFAULT NULL COMMENT '商户ID（merchant.merchant_id，如 sp_0001）',
  `notice_type` int NOT NULL DEFAULT '1' COMMENT '通知类型：1库存预警 2订单提醒 3其他',
  `title` varchar(128) DEFAULT NULL COMMENT '通知标题',
  `content` varchar(500) DEFAULT NULL COMMENT '通知内容',
  `related_id` varchar(50) DEFAULT NULL COMMENT '关联业务ID（库存预警为商品ID）',
  `stock_snapshot` int NOT NULL DEFAULT '0' COMMENT '预警时的库存快照（用于去重：库存没变不重复预警）',
  `is_read` int NOT NULL DEFAULT '0' COMMENT '是否已读：0未读 1已读',
  `create_time` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_notice_merchant` (`merchant_id`),
  KEY `idx_notice_type_time` (`merchant_id`,`notice_type`,`create_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
