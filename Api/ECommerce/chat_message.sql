-- ============================================================
-- 买卖家聊天消息表（用户端小程序 ↔ 商户端小程序 消息互通）
-- 对应实体：ECommerce.Infrastructure.EFCore.ChatMessage
-- 在 MySQL 中执行一次即可；表已存在时跳过。
-- ============================================================

CREATE TABLE IF NOT EXISTS `chat_message` (
  `id`            varchar(50)  NOT NULL                COMMENT '主键（顺序GUID）',
  `client_id`     varchar(50)  DEFAULT NULL            COMMENT '买家用户ID（sys_users.u_id）',
  `merchant_id`   varchar(50)  DEFAULT NULL            COMMENT '商户ID（merchant.id）',
  `sender_type`   int          NOT NULL DEFAULT '1'    COMMENT '发送方类型：1买家 2商户',
  `content`       varchar(500) DEFAULT NULL            COMMENT '消息文本内容',
  `client_read`   int          NOT NULL DEFAULT '0'    COMMENT '买家是否已读：0未读 1已读',
  `merchant_read` int          NOT NULL DEFAULT '0'    COMMENT '商户是否已读：0未读 1已读',
  `create_time`   datetime     NOT NULL                COMMENT '创建时间',
  PRIMARY KEY (`id`),
  KEY `idx_chat_client` (`client_id`),
  KEY `idx_chat_merchant` (`merchant_id`),
  KEY `idx_chat_pair_time` (`client_id`,`merchant_id`,`create_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='买卖家聊天消息（用户端与商户端互通）';
