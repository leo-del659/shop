/*
 Navicat Premium Data Transfer

 Source Server         : localhost_3306
 Source Server Type    : MySQL
 Source Server Version : 80046 (8.0.46)
 Source Host           : localhost:3306
 Source Schema         : 111

 Target Server Type    : MySQL
 Target Server Version : 80046 (8.0.46)
 File Encoding         : 65001

 Date: 18/10/2026 18:06:18
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for audit_record
-- ----------------------------
DROP TABLE IF EXISTS `audit_record`;
CREATE TABLE `audit_record`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `biz_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '业务类型 商品/用户/商户',
  `biz_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '业务主键',
  `audit_status` int NULL DEFAULT NULL COMMENT '1通过 2驳回',
  `audit_remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '审核意见',
  `auditor` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '审核人',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_audit_biz`(`biz_type` ASC, `biz_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '审核记录表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of audit_record
-- ----------------------------
INSERT INTO `audit_record` VALUES ('00887e4a71ba40ef9e6aa3ef473ddee3', 'good', 'b01767771fcf43e4b62f7d03ba072d83', 1, NULL, 'admin', '2026-08-13 08:57:02');
INSERT INTO `audit_record` VALUES ('01a7cddd38fb4b4e930cd0020590e6de', 'good', 'g007', 1, NULL, 'admin', '2026-08-05 14:24:58');
INSERT INTO `audit_record` VALUES ('1ae461bccc284739866b64acd349570e', 'good', 'g006', 1, NULL, 'admin', '2026-08-05 14:24:57');
INSERT INTO `audit_record` VALUES ('24cf6dedab124044a36026bbe2b335e5', 'good', 'g002', 2, '', 'admin', '2026-08-04 11:03:43');
INSERT INTO `audit_record` VALUES ('2a10033810974b42836b9e02cc713991', 'good', 'g003', 2, NULL, 'admin', '2026-08-04 11:23:25');
INSERT INTO `audit_record` VALUES ('300577e71e994d839eaa0695853d25fe', 'good', 'g001', 1, NULL, 'admin', '2026-08-04 11:09:38');
INSERT INTO `audit_record` VALUES ('35ce2bbd88404a4790c4d89d97bd770e', 'good', 'g010', 1, NULL, 'admin', '2026-08-04 11:03:22');
INSERT INTO `audit_record` VALUES ('35ffb0095a1c47aca2bfc104978dff8b', 'merchant', 'shop002', 2, '', 'admin', '2026-08-07 22:28:44');
INSERT INTO `audit_record` VALUES ('3678b9fa45e240d3922c813a964db437', 'good', 'd6ffca2b2738481cb7b2fe47d638be7d', 2, '1', 'admin', '2026-08-11 15:53:19');
INSERT INTO `audit_record` VALUES ('38617e37eca948789b68c6b145d6b324', 'good', '18ed3938649242ff8518f155953d498f', 1, NULL, 'admin', '2026-08-07 20:11:54');
INSERT INTO `audit_record` VALUES ('3d61b0046daa48c186754797b11a70a5', 'good', 'g001', 1, NULL, 'admin', '2026-08-04 11:16:19');
INSERT INTO `audit_record` VALUES ('553e41edcf6b4ed9a3eacb417ed48e2c', 'merchant', 'shop004', 1, NULL, 'admin', '2026-08-07 22:28:23');
INSERT INTO `audit_record` VALUES ('5ec56bc930fc4400802753554f9ea35b', 'good', 'g004', 1, NULL, 'admin', '2026-08-04 11:08:23');
INSERT INTO `audit_record` VALUES ('6badd9273548401bb1e339d2294e38f3', 'good', 'g005', 1, NULL, 'admin', '2026-08-05 14:24:57');
INSERT INTO `audit_record` VALUES ('78eb4fa95ba348de9e9a389d03b3a194', 'merchant', '6d5a47f51baf41e3addd4de4686a824d', 1, NULL, 'admin', '2026-08-13 09:03:17');
INSERT INTO `audit_record` VALUES ('7ac194e0e8ff40b7bc973b27d15366ba', 'good', 'g003', 2, NULL, 'admin', '2026-08-04 11:16:19');
INSERT INTO `audit_record` VALUES ('7b960b44f2694a4797097161287610c0', 'good', '57f61fb00a0c4124bbf423c16dd89ca6', 2, '1', 'admin', '2026-08-07 22:49:28');
INSERT INTO `audit_record` VALUES ('7d5716164e1c473cb6f9e3960b8ad415', 'good', 'g003', 1, NULL, 'admin', '2026-08-04 11:23:24');
INSERT INTO `audit_record` VALUES ('80ef95996abb4533a267df9034e45f6b', 'good', '24d9556f13fe4e80a7bd7c88edbc205a', 1, NULL, 'admin', '2026-09-14 11:46:36');
INSERT INTO `audit_record` VALUES ('876bffe026e44eeb959958153791e846', 'good', 'd6ffca2b2738481cb7b2fe47d638be7d', 2, '1', 'admin', '2026-08-11 15:54:41');
INSERT INTO `audit_record` VALUES ('8d01d0857f724ac9b5218d120c6e11f4', 'good', 'g009', 1, NULL, 'admin', '2026-08-05 14:24:59');
INSERT INTO `audit_record` VALUES ('8d0613840bb54534bc77a159653d046e', 'merchant', 'e7e3bde360104a2a9d27bfb569dec802', 1, NULL, 'admin', '2026-08-04 18:48:57');
INSERT INTO `audit_record` VALUES ('923000fcd3654b329976b280b3a4bd4c', 'merchant', 'shop002', 1, NULL, 'admin', '2026-08-11 15:21:45');
INSERT INTO `audit_record` VALUES ('9bf02f17222d465bacbc40d05af1ab72', 'merchant', 'shop003', 1, NULL, 'admin', '2026-08-11 15:29:16');
INSERT INTO `audit_record` VALUES ('a57b31fb526a440b93c5dd68b4e1e91c', 'good', 'g009', 1, NULL, 'admin', '2026-08-04 11:03:23');
INSERT INTO `audit_record` VALUES ('a7087cef381e4cb3a8a754a736aa0296', 'good', 'g037', 2, '1', 'admin', '2026-08-13 15:25:19');
INSERT INTO `audit_record` VALUES ('ac31cdf8c5084f30ba103c8fe9a77675', 'good', 'g004', 1, NULL, 'admin', '2026-08-04 11:17:18');
INSERT INTO `audit_record` VALUES ('ar_0001', '商品', 'gd_0001', 1, '符合上架标准', 'admin', '2026-07-30 10:10:21');
INSERT INTO `audit_record` VALUES ('ar_0002', '商品', 'gd_0002', 1, '通过', 'admin', '2026-07-30 10:10:21');
INSERT INTO `audit_record` VALUES ('b048fae0776a4b9d8dc459c635a181bc', 'merchant', 'shop001', 1, NULL, 'admin', '2026-08-04 19:03:05');
INSERT INTO `audit_record` VALUES ('b32cd4d669d947aa8161e441639d78ee', 'good', '148fb3b453d34c84ab33080d91438a4b', 2, '', 'admin', '2026-08-04 11:35:11');
INSERT INTO `audit_record` VALUES ('b445b8130c914bfb93b4b0eb4e764aab', 'good', 'g008', 1, NULL, 'admin', '2026-08-05 14:24:58');
INSERT INTO `audit_record` VALUES ('c1b62d49681543b5ace35cee19ef787a', 'good', 'g001', 1, NULL, 'admin', '2026-08-05 14:24:56');
INSERT INTO `audit_record` VALUES ('d565f5aa1f0f49918a96229b772f9aa5', 'good', 'g010', 1, NULL, 'admin', '2026-08-04 11:23:03');
INSERT INTO `audit_record` VALUES ('d576edb0bf5244fb816110bc54366a44', 'good', 'g005', 2, NULL, 'admin', '2026-08-04 11:17:19');
INSERT INTO `audit_record` VALUES ('d7e6d06301994fdab0f1f8d961bd90ca', 'good', 'g001', 1, NULL, 'admin', '2026-08-04 11:23:25');
INSERT INTO `audit_record` VALUES ('db8bb47362a540ce8b33917f0c0aa034', 'good', '46e98c3c259b405f8e5400ee464e225a', 2, '', 'admin', '2026-08-06 08:38:25');
INSERT INTO `audit_record` VALUES ('e24c1ed3b37540beb511ae2e2e60d788', 'merchant', 'shop002', 2, '', 'admin', '2026-08-04 19:03:20');
INSERT INTO `audit_record` VALUES ('e31f16719475497385b7141a22cb5d87', 'good', 'g003', 1, NULL, 'admin', '2026-08-04 11:24:05');
INSERT INTO `audit_record` VALUES ('e3949ef2cf1644cba0cf1afaffb13e18', 'merchant', 'shop003', 2, '1', 'admin', '2026-08-05 18:54:17');
INSERT INTO `audit_record` VALUES ('e9ead94f066e49ccaeb518d0ae95540f', 'good', 'g003', 2, 'test reject', 'admin', '2026-08-04 11:09:38');
INSERT INTO `audit_record` VALUES ('ec17f37b669d431bb79b1d33e04c0b8c', 'good', 'g001', 1, NULL, 'admin', '2026-08-04 11:05:32');
INSERT INTO `audit_record` VALUES ('eeb9910fbbcd44f7b7bec98ddacb880e', 'good', 'c53d2e9b1a044ae0a6b314f7f866af6f', 1, NULL, 'admin', '2026-08-13 08:56:25');
INSERT INTO `audit_record` VALUES ('f0f593df91cf453a8ee12f9b497c44a1', 'good', 'd6ffca2b2738481cb7b2fe47d638be7d', 2, '1', 'admin', '2026-08-11 15:53:32');

-- ----------------------------
-- Table structure for client_login
-- ----------------------------
DROP TABLE IF EXISTS `client_login`;
CREATE TABLE `client_login`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `client_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '消费者用户ID（sys_users.u_id）',
  `account` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '登录账号',
  `name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '用户名称',
  `avatar` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '头像地址',
  `create_time` datetime NULL DEFAULT NULL COMMENT '创建时间',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uk_client_login_account`(`account` ASC) USING BTREE,
  INDEX `idx_client_login_client`(`client_id` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 9 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '用户端登录账号（消费者手机号验证码登录）' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of client_login
-- ----------------------------
INSERT INTO `client_login` VALUES (1, '84f6a9f12f4a41e5b3701426ead5647a', '13800002222', '13800002222', NULL, '2026-10-17 09:02:13');
INSERT INTO `client_login` VALUES (2, '0fb5a2f052614b42afb3732cee812919', '13800001111', '13800001111', NULL, '2026-10-17 09:03:37');
INSERT INTO `client_login` VALUES (3, '4ae99d2f4d194ffca171be252f68e8f9', '19332405533', '19332405533', NULL, '2026-10-17 09:12:56');
INSERT INTO `client_login` VALUES (4, '59fcb930d17a4b4b919ef2ca5fc0df58', '13800003333', '张三', '/api/file/617ca11645b042ef9e1ba35e5b94eb32.png', '2026-10-17 09:39:53');
INSERT INTO `client_login` VALUES (5, 'd4436e1517404f72b0ccc95a3f09fae5', '13583395459', '13583395459', NULL, '2026-10-17 09:44:55');
INSERT INTO `client_login` VALUES (7, '9432473ed28b4126b2e274e373d53a0c', '13800009999', '冒烟买家', '', '2026-10-17 14:02:25');
INSERT INTO `client_login` VALUES (8, '479b927d83d14267bc4b8840297d638b', '13800000099', '13800000099', NULL, '2026-10-17 15:16:31');

-- ----------------------------
-- Table structure for clientorders
-- ----------------------------
DROP TABLE IF EXISTS `clientorders`;
CREATE TABLE `clientorders`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `client_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '买家用户ID（sys_users.u_id）',
  `order_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `total_amount` double NULL DEFAULT NULL,
  `order_status` int NULL DEFAULT 1,
  `pay_status` int NULL DEFAULT 0,
  `goods_info` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `reject_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `address` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `is_ban` int NULL DEFAULT 0,
  `create_time` datetime NULL DEFAULT NULL,
  `update_time` datetime NULL DEFAULT NULL,
  `items_json` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL COMMENT '订单商品明细 JSON 数组',
  `merchant_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '供货商户ID',
  `receiver_name` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '收货人姓名',
  `receiver_mobile` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '收货人手机号',
  `pay_time` datetime NULL DEFAULT NULL COMMENT '付款时间',
  `ship_time` datetime NULL DEFAULT NULL COMMENT '发货时间',
  `complete_time` datetime NULL DEFAULT NULL COMMENT '成交/完成时间',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uk_order_no`(`order_no` ASC) USING BTREE,
  INDEX `idx_client`(`client_id` ASC) USING BTREE,
  INDEX `idx_status`(`order_status` ASC) USING BTREE,
  INDEX `idx_merchant`(`merchant_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '用户采购订单主表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of clientorders
-- ----------------------------
INSERT INTO `clientorders` VALUES ('0c3ae3c7a2b747389a603daefa817847', 'm_0001', 'ORD2609160002', 299, 4, 1, '泰国金枕头榴莲×10', NULL, '', '广东省 深圳市 南山区 科技园南路1号', 0, '2026-09-16 08:45:19', '2026-09-16 09:28:54', '[{\"id\":\"daab70f7cb7d4516b2916796f49bd254\",\"goodsId\":\"g003\",\"specId\":\"6048a94b442a41d2b662ab2806384148\",\"goodsName\":\"\\u6CF0\\u56FD\\u91D1\\u6795\\u5934\\u69B4\\u83B2\",\"specName\":\"\\u6574\\u4E2A\",\"goodsImg\":\"/api/file/9681bad1404545daa14b2a931a9a5a7d.webp\",\"buyPrice\":29.9,\"buyNum\":10,\"subAmount\":299,\"isBan\":0,\"createTime\":\"2026-09-16T08:45:18\",\"updateTime\":\"2026-09-16T08:45:18\"}]', 'sp_0001', '张三', '13800000001', '2026-09-16 08:45:19', '2026-09-16 09:28:52', '2026-09-16 09:28:54');
INSERT INTO `clientorders` VALUES ('30a374d1fe9f4c618c7366b4cab4398c', 'm_0001', 'ORD2610140006', 9.9, 9, 0, '海南金煌芒果×1', NULL, '超时未支付，系统自动取消', NULL, 0, '2026-10-14 11:12:59', '2026-10-14 11:13:59', '[{\"id\":\"2fd5d259e04a4628a2852e1d2806b3e4\",\"goodsId\":\"g005\",\"specId\":null,\"goodsName\":\"\\u6D77\\u5357\\u91D1\\u714C\\u8292\\u679C\",\"specName\":null,\"goodsImg\":\"/api/file/9e28bd418f844cca8fc8882d76177cc5.webp\",\"buyPrice\":9.9,\"buyNum\":1,\"subAmount\":9.9,\"isBan\":0,\"createTime\":\"2026-10-14T11:12:58\",\"updateTime\":\"2026-10-14T11:12:58\"}]', 'sp_0001', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `clientorders` VALUES ('3bc5dc22b5584d6987f1de03ecdf7ca9', 'm_0001', 'ORD2610140007', 98.9, 4, 1, '挪威三文鱼刺身×1', NULL, 'fallback-test-1791947971', NULL, 0, '2026-10-14 11:19:36', '2026-10-14 11:25:07', '[{\"id\":\"b94f82a9f8f24c2c95b26791be4b82b6\",\"goodsId\":\"g015\",\"specId\":null,\"goodsName\":\"\\u632A\\u5A01\\u4E09\\u6587\\u9C7C\\u523A\\u8EAB\",\"specName\":null,\"goodsImg\":\"/api/file/2212c656d5b440cd921e730f7722e418.webp\",\"buyPrice\":98.9,\"buyNum\":1,\"subAmount\":98.9,\"isBan\":0,\"createTime\":\"2026-10-14T11:19:36\",\"updateTime\":\"2026-10-14T11:19:36\"}]', 'sp_0001', NULL, NULL, '2026-10-14 11:20:20', '2026-10-14 11:25:07', '2026-10-14 11:25:07');
INSERT INTO `clientorders` VALUES ('45fe1edffc1e4af7b98273d67844105a', 'm_0001', 'ORD2610140003', 98.9, 2, 1, '挪威三文鱼刺身×1', NULL, NULL, NULL, 0, '2026-10-14 11:03:47', '2026-10-17 10:49:14', '[{\"id\":\"82aebcbf4825457a9fd0d2ab30335258\",\"goodsId\":\"g015\",\"specId\":null,\"goodsName\":\"\\u632A\\u5A01\\u4E09\\u6587\\u9C7C\\u523A\\u8EAB\",\"specName\":null,\"goodsImg\":\"/api/file/2212c656d5b440cd921e730f7722e418.webp\",\"buyPrice\":98.9,\"buyNum\":1,\"subAmount\":98.9,\"isBan\":0,\"createTime\":\"2026-10-14T11:03:47\",\"updateTime\":\"2026-10-14T11:03:47\"}]', 'sp_0001', NULL, NULL, '2026-10-17 10:49:14', NULL, NULL);
INSERT INTO `clientorders` VALUES ('5c3733cd5efe46508aa2a22305887514', 'm_0001', 'ORD2610140004', 69.9, 1, 0, '智利进口车厘子 JJ级×1', NULL, NULL, NULL, 0, '2026-10-14 11:07:57', '2026-10-14 11:07:57', '[{\"id\":\"920260dca4014d5f8a3e5f2312608314\",\"goodsId\":\"g004\",\"specId\":null,\"goodsName\":\"\\u667A\\u5229\\u8FDB\\u53E3\\u8F66\\u5398\\u5B50 JJ\\u7EA7\",\"specName\":null,\"goodsImg\":\"/api/file/28afffcc6ae74fc18ddc086a1ff47efd.webp\",\"buyPrice\":69.9,\"buyNum\":1,\"subAmount\":69.9,\"isBan\":0,\"createTime\":\"2026-10-14T11:07:56\",\"updateTime\":\"2026-10-14T11:07:56\"}]', 'sp_0001', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `clientorders` VALUES ('69f8800f693348dc8da50667017a4ca9', 'm_0001', 'ORD2610140005', 9.9, 1, 0, '海南金煌芒果×1', NULL, NULL, NULL, 0, '2026-10-14 11:09:09', '2026-10-14 16:42:36', '[{\"id\":\"de90403e7efe429ba0ea471854dd1d9f\",\"goodsId\":\"g005\",\"specId\":null,\"goodsName\":\"\\u6D77\\u5357\\u91D1\\u714C\\u8292\\u679C\",\"specName\":null,\"goodsImg\":\"/api/file/9e28bd418f844cca8fc8882d76177cc5.webp\",\"buyPrice\":9.9,\"buyNum\":1,\"subAmount\":9.9,\"isBan\":0,\"createTime\":\"2026-10-14T11:09:09\",\"updateTime\":\"2026-10-14T11:09:09\"}]', 'sp_0001', NULL, NULL, NULL, NULL, NULL);
INSERT INTO `clientorders` VALUES ('743e4810a92845c98de8aad67a42bd7b', 'm_0001', 'ORD2610140002', 12.9, 2, 1, '云南高原红富士苹果×1', NULL, NULL, NULL, 0, '2026-10-14 11:03:47', '2026-10-17 14:02:26', '[{\"id\":\"232bc11b6f6f4783a36105a176f76c38\",\"goodsId\":\"g001\",\"specId\":null,\"goodsName\":\"\\u4E91\\u5357\\u9AD8\\u539F\\u7EA2\\u5BCC\\u58EB\\u82F9\\u679C\",\"specName\":null,\"goodsImg\":\"/api/file/11843cbeafb341d58ec6effd478038ee.webp\",\"buyPrice\":12.9,\"buyNum\":1,\"subAmount\":12.9,\"isBan\":0,\"createTime\":\"2026-10-14T11:03:47\",\"updateTime\":\"2026-10-14T11:03:47\"}]', 'sp_0001', NULL, NULL, '2026-10-17 14:02:26', NULL, NULL);
INSERT INTO `clientorders` VALUES ('79fda0d10c28424db318863fb41bbdac', 'm_0001', 'ORD2610140001', 25.8, 4, 1, '云南高原红富士苹果×2', NULL, NULL, '测试地址', 0, '2026-10-14 11:03:37', '2026-10-14 11:03:44', '[{\"id\":\"db1dddd42196497084125c6e94b56444\",\"goodsId\":\"g001\",\"specId\":null,\"goodsName\":\"\\u4E91\\u5357\\u9AD8\\u539F\\u7EA2\\u5BCC\\u58EB\\u82F9\\u679C\",\"specName\":null,\"goodsImg\":\"/api/file/11843cbeafb341d58ec6effd478038ee.webp\",\"buyPrice\":12.9,\"buyNum\":2,\"subAmount\":25.8,\"isBan\":0,\"createTime\":\"2026-10-14T11:03:36\",\"updateTime\":\"2026-10-14T11:03:36\"}]', 'sp_0001', '冒烟测试', '13800138000', '2026-10-14 11:03:42', '2026-10-14 11:03:44', '2026-10-14 11:03:44');
INSERT INTO `clientorders` VALUES ('a92484fae614486991750697e3142cd8', 'm_0001', 'ORD2609160005', 25.8, 2, 1, '云南高原红富士苹果×2', NULL, '运费联调', '广东省 深圳市 南山区 科技园南路1号', 0, '2026-09-16 11:39:03', '2026-10-17 10:47:55', '[{\"id\":\"bedae293e52049febdf5c945d6850c49\",\"goodsId\":\"g001\",\"specId\":null,\"goodsName\":\"\\u4E91\\u5357\\u9AD8\\u539F\\u7EA2\\u5BCC\\u58EB\\u82F9\\u679C\",\"specName\":null,\"goodsImg\":\"/api/file/11843cbeafb341d58ec6effd478038ee.webp\",\"buyPrice\":12.9,\"buyNum\":2,\"subAmount\":25.8,\"isBan\":0,\"createTime\":\"2026-09-16T11:39:02\",\"updateTime\":\"2026-09-16T11:39:02\"}]', 'sp_0001', '张三', '13800000001', '2026-10-17 10:46:55', NULL, NULL);
INSERT INTO `clientorders` VALUES ('ad5ec527f72e4eb9a95fd2d66271aa6e', 'm_0001', 'ORD2609150003', 9.9, 4, 1, '海南金煌芒果×1', NULL, '', '广东省 深圳市 南山区 科技园南路1号', 0, '2026-09-15 18:40:10', '2026-09-16 08:40:22', '[{\"id\":\"03d1d86edea2470bbb22943739c913fb\",\"goodsId\":\"g005\",\"specId\":\"1d91e28bd2a24ff58821e26f87885b88\",\"goodsName\":\"\\u6D77\\u5357\\u91D1\\u714C\\u8292\\u679C\",\"specName\":\"500g/\\u65A4\",\"goodsImg\":\"/api/file/9e28bd418f844cca8fc8882d76177cc5.webp\",\"buyPrice\":9.9,\"buyNum\":1,\"subAmount\":9.9,\"isBan\":0,\"createTime\":\"2026-09-15T18:40:09\",\"updateTime\":\"2026-09-15T18:40:09\"}]', 'sp_0001', '张三', '13800000001', '2026-09-15 18:40:10', '2026-09-16 08:40:20', '2026-09-16 08:40:22');
INSERT INTO `clientorders` VALUES ('c34c684fda284ef5913fde432bbb4ff0', 'm_0001', 'ORD2609160003', 345, 9, 1, '智利进口车厘子 JJ级×1', NULL, '', '广东省 深圳市 南山区 科技园南路1号', 0, '2026-09-16 11:02:40', '2026-09-16 11:20:15', '[{\"id\":\"eebc24bc09204ec4ab063a1225120754\",\"goodsId\":\"g004\",\"specId\":\"2d2282cc55ab4dc484d93e8dd128168f\",\"goodsName\":\"\\u667A\\u5229\\u8FDB\\u53E3\\u8F66\\u5398\\u5B50 JJ\\u7EA7\",\"specName\":\"5\\u65A4\\u88C5(J\\u7EA7)\",\"goodsImg\":\"/api/file/28afffcc6ae74fc18ddc086a1ff47efd.webp\",\"buyPrice\":345,\"buyNum\":1,\"subAmount\":345,\"isBan\":0,\"createTime\":\"2026-09-16T11:02:40\",\"updateTime\":\"2026-09-16T11:02:40\"}]', 'sp_0001', '张三', '13800000001', '2026-09-16 11:02:40', NULL, NULL);
INSERT INTO `clientorders` VALUES ('db146285c4474093ad311b140dfc8cac', 'm_0001', 'ORD2609150002', 12.9, 4, 1, '云南高原红富士苹果×1', NULL, '', '广东省 深圳市 南山区 科技园南路1号', 0, '2026-09-15 11:21:39', '2026-09-15 18:36:32', '[{\"id\":\"1b976c1e16e74679885554e82168d88a\",\"goodsId\":\"g001\",\"specId\":\"8957151f6fde45dea240caecb42b7b1f\",\"goodsName\":\"\\u4E91\\u5357\\u9AD8\\u539F\\u7EA2\\u5BCC\\u58EB\\u82F9\\u679C\",\"specName\":\"500g/\\u4EFD\",\"goodsImg\":\"/api/file/11843cbeafb341d58ec6effd478038ee.webp\",\"buyPrice\":12.9,\"buyNum\":1,\"subAmount\":12.9,\"isBan\":0,\"createTime\":\"2026-09-15T11:21:38\",\"updateTime\":\"2026-09-15T11:21:38\"}]', 'sp_0001', NULL, NULL, NULL, NULL, '2026-09-15 18:36:32');
INSERT INTO `clientorders` VALUES ('f1d226eb5ed744049bd3848df2444575', 'm_0001', 'ORD2609160004', 345, 9, 1, '智利进口车厘子 JJ级×1', NULL, '', '广东省 深圳市 南山区 科技园南路1号', 0, '2026-09-16 11:22:14', '2026-09-16 11:22:18', '[{\"id\":\"bd36914fb0d5477d9db4f2ffb6c90ae4\",\"goodsId\":\"g004\",\"specId\":\"2d2282cc55ab4dc484d93e8dd128168f\",\"goodsName\":\"\\u667A\\u5229\\u8FDB\\u53E3\\u8F66\\u5398\\u5B50 JJ\\u7EA7\",\"specName\":\"5\\u65A4\\u88C5(J\\u7EA7)\",\"goodsImg\":\"/api/file/28afffcc6ae74fc18ddc086a1ff47efd.webp\",\"buyPrice\":345,\"buyNum\":1,\"subAmount\":345,\"isBan\":0,\"createTime\":\"2026-09-16T11:22:14\",\"updateTime\":\"2026-09-16T11:22:14\"}]', 'sp_0001', '张三', '13800000001', '2026-09-16 11:22:14', NULL, NULL);

-- ----------------------------
-- Table structure for good
-- ----------------------------
DROP TABLE IF EXISTS `good`;
CREATE TABLE `good`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `category_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '所属分类ID',
  `goods_name` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '商品名称',
  `price` decimal(18, 2) NOT NULL COMMENT '原价',
  `sale_price` decimal(18, 2) NOT NULL COMMENT '售卖价/团购价',
  `stock` int NOT NULL COMMENT '库存',
  `cover_img` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '封面图',
  `img_list` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '多图逗号分隔',
  `unit` varchar(16) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '单位',
  `audit_status` int NULL DEFAULT NULL COMMENT '0待审核 1通过 2驳回',
  `audit_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '关联审核记录audit_record.id',
  `is_sale` int NULL DEFAULT 0 COMMENT '1上架 0下架',
  `create_time` datetime NULL DEFAULT NULL,
  `update_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_good_audit`(`audit_status` ASC) USING BTREE,
  INDEX `fk_good_category_id`(`category_id` ASC) USING BTREE,
  CONSTRAINT `fk_good_category_id` FOREIGN KEY (`category_id`) REFERENCES `goods_category` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商品审核主表(后台)' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of good
-- ----------------------------
INSERT INTO `good` VALUES ('g001', 'cat00101', '云南高原红富士苹果', 18.80, 12.90, 356, '/api/file/11843cbeafb341d58ec6effd478038ee.webp', '/api/file/11843cbeafb341d58ec6effd478038ee.webp', '份', 1, 'c1b62d49681543b5ace35cee19ef787a', 1, '2026-07-30 10:46:42', '2026-10-16 11:19:39');
INSERT INTO `good` VALUES ('g003', 'cat00101', '泰国金枕头榴莲', 39.90, 29.90, 257, '/api/file/9681bad1404545daa14b2a931a9a5a7d.webp', NULL, '份', 1, 'e31f16719475497385b7141a22cb5d87', 1, '2026-07-30 10:46:42', '2026-09-08 19:06:01');
INSERT INTO `good` VALUES ('g004', 'cat00101', '智利进口车厘子 JJ级', 89.90, 69.90, 148, '/api/file/28afffcc6ae74fc18ddc086a1ff47efd.webp', NULL, '份', 1, 'ac31cdf8c5084f30ba103c8fe9a77675', 1, '2026-07-30 10:46:42', '2026-09-16 11:22:18');
INSERT INTO `good` VALUES ('g005', 'cat00101', '海南金煌芒果', 15.80, 9.90, 295, '/api/file/9e28bd418f844cca8fc8882d76177cc5.webp', NULL, '份', 1, '6badd9273548401bb1e339d2294e38f3', 1, '2026-07-30 10:46:42', '2026-10-14 11:13:59');
INSERT INTO `good` VALUES ('g006', 'cat00102', '有机小白菜', 6.80, 4.90, 199, '/api/file/7c7cd943d34b46838bb1e8ec7725ad2e.webp', NULL, '份', 1, '1ae461bccc284739866b64acd349570e', 1, '2026-07-30 10:46:42', '2026-09-08 19:06:38');
INSERT INTO `good` VALUES ('g007', 'cat00102', '农家自种西红柿', 8.80, 5.90, 350, '/api/file/b0cb21948cd0417c82f4b405aae672b8.webp', NULL, '份', 1, '01a7cddd38fb4b4e930cd0020590e6de', 1, '2026-07-30 10:46:42', '2026-09-08 19:06:46');
INSERT INTO `good` VALUES ('g008', 'cat00102', '山东铁棍山药', 12.80, 8.90, 180, '/api/file/a68afe4c746e4ff0a6d680d95820829d.webp', NULL, '份', 1, 'b445b8130c914bfb93b4b0eb4e764aab', 1, '2026-07-30 10:46:42', '2026-09-08 19:06:55');
INSERT INTO `good` VALUES ('g009', 'cat00103', '新西兰佳沛金果奇异果', 10.80, 7.90, 260, '/api/file/94d8d79cac0a4d62b140e4b0bd81bbc5.webp', NULL, '份', 1, '8d01d0857f724ac9b5218d120c6e11f4', 1, '2026-07-30 10:46:42', '2026-09-08 19:07:11');
INSERT INTO `good` VALUES ('g010', 'cat00103', '越南红心火龙果', 16.80, 11.90, 220, '/api/file/57f4c3fadbcd4d6b8485441b2e41d28c.webp', NULL, '份', 1, 'd565f5aa1f0f49918a96229b772f9aa5', 1, '2026-07-30 10:46:42', '2026-09-08 19:07:23');
INSERT INTO `good` VALUES ('g011', 'cat00201', '黑猪五花肉', 38.80, 29.90, 120, '/api/file/423d623fd7cb44dc881285bf02e9da6d.webp', NULL, '份', 1, NULL, 1, '2026-07-30 10:46:42', '2026-09-08 19:07:33');
INSERT INTO `good` VALUES ('g012', 'cat00201', '澳洲谷饲牛腩块', 68.80, 49.90, 80, '/api/file/56468fc7ea3f4607832119bfed67e923.webp', NULL, '个', 1, NULL, 1, '2026-07-30 10:46:42', '2026-09-08 19:07:50');
INSERT INTO `good` VALUES ('g013', 'cat00202', '三黄鸡 整只', 48.80, 35.90, 60, '/api/file/9bfd94c804ff4542b3e3e945eff0c2d8.webp', NULL, '个', 1, NULL, 1, '2026-07-30 10:46:42', '2026-09-08 19:07:59');
INSERT INTO `good` VALUES ('g014', 'cat00203', '野生大虾', 88.80, 68.90, 40, '/api/file/d1c387dbd329494c9b3fae63d8bf4e69.webp', NULL, '个', 1, NULL, 1, '2026-07-30 10:46:42', '2026-09-08 19:08:09');
INSERT INTO `good` VALUES ('g015', 'cat00203', '挪威三文鱼刺身', 128.00, 98.90, 3, '/api/file/2212c656d5b440cd921e730f7722e418.webp', NULL, '元', 1, NULL, 1, '2026-07-30 10:46:42', '2026-09-08 19:08:20');
INSERT INTO `good` VALUES ('g016', 'cat00204', '农家土鸡蛋', 22.80, 16.90, 300, '/api/file/91cdb9277d6945059d36777009b9ae25.webp', NULL, '盒', 1, NULL, 1, '2026-07-30 10:46:42', '2026-09-08 19:08:28');
INSERT INTO `good` VALUES ('g017', 'cat00204', '老豆腐', 4.50, 3.50, 150, '/api/file/2a5b5b5ce8bf45f1b31bf2874146f8a4.webp', NULL, '块', 1, NULL, 1, '2026-07-30 10:46:42', '2026-09-08 19:08:42');
INSERT INTO `good` VALUES ('g018', 'cat00205', '蒙牛纯牛奶 全脂250ml', 69.90, 49.90, 500, '/api/file/db424a3f74684740bbb412c4d6217a5b.webp', NULL, '箱', 1, NULL, 1, '2026-07-30 10:46:42', '2026-09-08 19:09:13');
INSERT INTO `good` VALUES ('g019', 'cat00301', '洽洽香瓜子 五香味', 14.80, 9.90, 600, '/images/products/097c0e187f1246359b07171b09798c9b.webp', NULL, '包', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:09:17');
INSERT INTO `good` VALUES ('g020', 'cat00301', '三只松鼠每日坚果', 39.80, 29.90, 200, '/images/products/aa4979119150476192ea7c4cab267018.webp,/images/products/e56b22dc8e284ef6a721a17f77477d10.webp', NULL, '箱', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:09:38');
INSERT INTO `good` VALUES ('g021', 'cat00302', '奥利奥夹心饼干', 15.80, 11.90, 400, '/images/products/cdfc7cbb1cee4fc288833c2a376be909.webp,/images/products/540d110e6b1f4be28ededb38c843fe13.webp', NULL, '包', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:09:53');
INSERT INTO `good` VALUES ('g022', 'cat00303', '德芙丝滑牛奶巧克力', 32.80, 24.90, 180, '/images/products/33b51978facc46f382eb4c224b4d6be3.webp', NULL, '盒', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:10:22');
INSERT INTO `good` VALUES ('g023', 'cat00304', '可口可乐 330ml*24罐', 58.80, 39.90, 350, '/images/products/e0c89335c95d4ae3821e0bd1cc28759d.webp', NULL, '箱', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:10:37');
INSERT INTO `good` VALUES ('g024', 'cat00304', '农夫山泉茶π 500ml', 4.00, 2.90, 800, '/images/products/7ba977e4338642f690a756e2dbb38136.webp', NULL, '瓶', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:10:58');
INSERT INTO `good` VALUES ('g025', 'cat00401', '五常大米 稻花香 5kg', 69.80, 49.90, 200, '/images/products/4b9dd24ffedd4aa387af0fbf2537db1d.webp', NULL, '袋', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:11:20');
INSERT INTO `good` VALUES ('g026', 'cat00401', '金龙鱼面粉 高筋 5kg', 32.80, 24.90, 250, '/images/products/127af5974dbe4da284037b7c2bb0bcfc.webp', NULL, '袋', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-13 10:05:28');
INSERT INTO `good` VALUES ('g027', 'cat00402', '鲁花花生油 5L', 159.00, 129.00, 78, '/images/products/46c2d4e9b6094a81b257107f11fbc3ac.webp', NULL, '桶', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:11:47');
INSERT INTO `good` VALUES ('g028', 'cat00403', '海天酱油 生抽 500ml', 12.80, 8.90, 400, '/images/products/1e010f82d1864169a06be2f45d345bd5.webp', NULL, '瓶', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:12:01');
INSERT INTO `good` VALUES ('g029', 'cat00403', '李锦记蚝油 520g', 18.80, 13.90, 300, '/images/products/addc79396e664a8ab930795258ef4965.webp', NULL, '瓶', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:12:14');
INSERT INTO `good` VALUES ('g030', 'cat00501', '蓝月亮洗衣液 3kg', 49.80, 34.90, 150, '/images/products/378bfefb32e0415e8964631688f59871.webp', NULL, '瓶', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:12:28');
INSERT INTO `good` VALUES ('g031', 'cat00501', '立白洗洁精 1.5kg', 19.80, 13.90, 280, '/images/products/b9e1b4b3215a459a9531e385febb6e52.webp', NULL, '瓶', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:12:43');
INSERT INTO `good` VALUES ('g032', 'cat00502', '维达抽纸 3层100抽*24包', 49.80, 36.90, 180, '/images/products/54ef696c10b64d03a052b542c5eb6be7.webp', NULL, '提', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:13:58');
INSERT INTO `good` VALUES ('g033', 'cat00502', '清风卷纸 4层140克*10卷', 29.80, 19.90, 220, '/images/products/3fa41c71be324c8ea240f81346938b2e.webp', NULL, '提', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:14:21');
INSERT INTO `good` VALUES ('g034', 'cat00601', '酱卤猪蹄', 58.80, 45.90, 50, '/images/products/62bd89c75d4f41a1bc968681188b5819.webp', NULL, '只', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:14:32');
INSERT INTO `good` VALUES ('g035', 'cat00601', '四川麻辣兔头', 12.80, 8.90, 100, '/images/products/3e40a3d00d084ddea7833885006dd550.webp', NULL, '元', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:14:51');
INSERT INTO `good` VALUES ('g036', 'cat00602', '手工鲜肉大包', 3.00, 2.50, 200, '/images/products/54900881e05248c8953bbcc6b68d5210.webp', NULL, '元', 1, NULL, 1, '2026-07-30 10:46:42', '2026-08-05 14:15:06');

-- ----------------------------
-- Table structure for goods_category
-- ----------------------------
DROP TABLE IF EXISTS `goods_category`;
CREATE TABLE `goods_category`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `parent_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '父分类ID，一级分类为空',
  `category_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '分类名称',
  `sort` int NULL DEFAULT 0 COMMENT '排序',
  `is_ban` int NULL DEFAULT 0 COMMENT '0启用 1禁用',
  `icon` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '分类图标',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商品多级分类表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of goods_category
-- ----------------------------
INSERT INTO `goods_category` VALUES ('cat001', NULL, '新鲜果蔬', 1, 0, NULL, '2026-08-11 14:51:00');
INSERT INTO `goods_category` VALUES ('cat00101', 'cat001', '时令水果', 1, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00102', 'cat001', '新鲜蔬菜', 2, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00103', 'cat001', '进口水果', 3, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat002', NULL, '肉禽蛋奶', 2, 0, NULL, '2026-08-11 14:51:00');
INSERT INTO `goods_category` VALUES ('cat00201', 'cat002', '猪肉牛肉', 1, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00202', 'cat002', '鸡鸭禽肉', 2, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00203', 'cat002', '海鲜水产', 3, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00204', 'cat002', '禽蛋豆制品', 4, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00205', 'cat002', '鲜奶乳品', 5, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat003', NULL, '零食饮料', 3, 0, '', '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00301', 'cat003', '坚果炒货', 1, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00302', 'cat003', '饼干糕点', 2, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00303', 'cat003', '糖果巧克力', 3, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00304', 'cat003', '饮品果汁', 4, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat004', NULL, '粮油调味', 4, 0, '', '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00401', 'cat004', '米面杂粮', 1, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00402', 'cat004', '食用油脂', 2, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00403', 'cat004', '调味调料', 3, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat005', NULL, '日用百货', 5, 0, '', '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00501', 'cat005', '清洁洗护', 1, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00502', 'cat005', '纸品日用', 2, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat006', NULL, '熟食面点', 6, 0, '', '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00601', 'cat006', '中式熟食', 1, 0, NULL, '2026-07-30 10:42:23');
INSERT INTO `goods_category` VALUES ('cat00602', 'cat006', '烘焙面点', 2, 0, NULL, '2026-07-30 10:42:23');

-- ----------------------------
-- Table structure for goods_review
-- ----------------------------
DROP TABLE IF EXISTS `goods_review`;
CREATE TABLE `goods_review`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '主键',
  `goods_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商品ID(good.id)',
  `goods_name` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商品名称快照',
  `sku_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '规格ID(goods_sku.id)',
  `sku_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '规格名称快照',
  `member_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '评价人标识',
  `member_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '评价人昵称',
  `avatar` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '评价人头像快照',
  `order_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '关联订单ID',
  `rating` int NOT NULL DEFAULT 5 COMMENT '评分 1-5',
  `content` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '评价内容',
  `images` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '评价图，逗号分隔',
  `tags` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '标签，逗号分隔，如：口感好,包装精美',
  `merchant_review` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '对商户评价(独立文本)',
  `is_anonymous` tinyint(1) NOT NULL DEFAULT 0 COMMENT '是否匿名:0否 1是',
  `status` int NOT NULL DEFAULT 0 COMMENT '0待审核 1通过 2驳回',
  `reply` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商户回复',
  `reply_time` datetime NULL DEFAULT NULL COMMENT '回复时间',
  `create_time` datetime NULL DEFAULT NULL COMMENT '创建时间',
  `reject_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '驳回原因',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_gr_goods`(`goods_id` ASC) USING BTREE,
  INDEX `idx_gr_member`(`member_id` ASC) USING BTREE,
  INDEX `idx_gr_order`(`order_id` ASC) USING BTREE,
  CONSTRAINT `fk_goods_review_goods_id` FOREIGN KEY (`goods_id`) REFERENCES `good` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商品用户评价表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of goods_review
-- ----------------------------
INSERT INTO `goods_review` VALUES ('0765b9e1-91fd-11f1-bbda-28f52bf063a0', 'g001', '云南高原红富士苹果', NULL, NULL, 'seed_254990', '社区居民', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说云南高原红富士苹果好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-22 08:26:14', NULL);
INSERT INTO `goods_review` VALUES ('07661625-91fd-11f1-bbda-28f52bf063a0', 'g001', '云南高原红富士苹果', NULL, NULL, 'seed_729019', '微信用户', '/api/file/default-avatar.png', NULL, 5, '云南高原红富士苹果收到了，包装很严实，质量超出预期', NULL, '包装精美,物流快', NULL, 0, 1, NULL, NULL, '2026-07-20 16:44:07', NULL);
INSERT INTO `goods_review` VALUES ('0766dfd9-91fd-11f1-bbda-28f52bf063a0', 'g001', '云南高原红富士苹果', NULL, NULL, 'seed_934122', '精打细算', '/api/file/default-avatar.png', NULL, 4, '收到云南高原红富士苹果很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-08-05 21:54:50', NULL);
INSERT INTO `goods_review` VALUES ('07674e5c-91fd-11f1-bbda-28f52bf063a0', 'g003', '泰国金枕头榴莲', NULL, NULL, 'seed_312926', '美食控', '/api/file/default-avatar.png', NULL, 5, '拼团买的泰国金枕头榴莲，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-16 18:33:50', NULL);
INSERT INTO `goods_review` VALUES ('0767c5ec-91fd-11f1-bbda-28f52bf063a0', 'g003', '泰国金枕头榴莲', NULL, NULL, 'seed_555010', '退休陈姨', '/api/file/default-avatar.png', NULL, 4, '买了泰国金枕头榴莲，确实不错，会回购的', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-21 04:58:53', NULL);
INSERT INTO `goods_review` VALUES ('076827d8-91fd-11f1-bbda-28f52bf063a0', 'g003', '泰国金枕头榴莲', NULL, NULL, 'seed_745718', '小李', '/api/file/default-avatar.png', NULL, 5, '收到泰国金枕头榴莲很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-08-06 14:07:25', NULL);
INSERT INTO `goods_review` VALUES ('07687130-91fd-11f1-bbda-28f52bf063a0', 'g004', '智利进口车厘子 JJ级', NULL, NULL, 'seed_270083', '王阿姨', '/api/file/default-avatar.png', NULL, 5, '收到智利进口车厘子 JJ级很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-09 10:21:53', NULL);
INSERT INTO `goods_review` VALUES ('0768cec0-91fd-11f1-bbda-28f52bf063a0', 'g004', '智利进口车厘子 JJ级', NULL, NULL, 'seed_21808', '阳光大叔', '/api/file/default-avatar.png', NULL, 4, '智利进口车厘子 JJ级味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-13 09:28:16', NULL);
INSERT INTO `goods_review` VALUES ('0769430e-91fd-11f1-bbda-28f52bf063a0', 'g004', '智利进口车厘子 JJ级', NULL, NULL, 'seed_774807', '小李', '/api/file/default-avatar.png', NULL, 5, '整体满意，智利进口车厘子 JJ级发货速度快，推荐购买', NULL, '物流快,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-13 16:03:45', NULL);
INSERT INTO `goods_review` VALUES ('07699333-91fd-11f1-bbda-28f52bf063a0', 'g004', '智利进口车厘子 JJ级', NULL, NULL, 'seed_713604', '阳光大叔', '/api/file/default-avatar.png', NULL, 5, '收到智利进口车厘子 JJ级很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-15 18:50:17', NULL);
INSERT INTO `goods_review` VALUES ('0769e7d5-91fd-11f1-bbda-28f52bf063a0', 'g004', '智利进口车厘子 JJ级', NULL, NULL, 'seed_304241', '小李', '/api/file/default-avatar.png', NULL, 3, '口感很好，家人都说智利进口车厘子 JJ级好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-24 23:35:25', NULL);
INSERT INTO `goods_review` VALUES ('076a3769-91fd-11f1-bbda-28f52bf063a0', 'g005', '海南金煌芒果', NULL, NULL, 'seed_983628', '小李', '/api/file/default-avatar.png', NULL, 4, '海南金煌芒果物流速度很快，第二天就到了，很新鲜', NULL, '物流快,新鲜', NULL, 0, 1, NULL, NULL, '2026-07-11 21:09:10', NULL);
INSERT INTO `goods_review` VALUES ('076a85d1-91fd-11f1-bbda-28f52bf063a0', 'g005', '海南金煌芒果', NULL, NULL, 'seed_428287', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 4, '拼团买的海南金煌芒果，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-25 12:40:45', NULL);
INSERT INTO `goods_review` VALUES ('076ad498-91fd-11f1-bbda-28f52bf063a0', 'g005', '海南金煌芒果', NULL, NULL, 'seed_322707', '社区居民', '/api/file/default-avatar.png', NULL, 4, '拼团买的海南金煌芒果，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-08-03 17:33:20', NULL);
INSERT INTO `goods_review` VALUES ('076b221d-91fd-11f1-bbda-28f52bf063a0', 'g005', '海南金煌芒果', NULL, NULL, 'seed_900318', '精打细算', '/api/file/default-avatar.png', NULL, 4, '海南金煌芒果物流速度很快，第二天就到了，很新鲜', NULL, '物流快,新鲜', NULL, 0, 1, NULL, NULL, '2026-07-10 10:35:44', NULL);
INSERT INTO `goods_review` VALUES ('076bb139-91fd-11f1-bbda-28f52bf063a0', 'g006', '有机小白菜', NULL, NULL, 'seed_651153', '美食控', '/api/file/default-avatar.png', NULL, 4, '有机小白菜品质稳定，已经回购好几次了', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-20 05:55:31', NULL);
INSERT INTO `goods_review` VALUES ('076c18d8-91fd-11f1-bbda-28f52bf063a0', 'g006', '有机小白菜', NULL, NULL, 'seed_518504', '小李', '/api/file/default-avatar.png', NULL, 4, '整体满意，有机小白菜发货速度快，推荐购买', NULL, '物流快,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-27 01:31:30', NULL);
INSERT INTO `goods_review` VALUES ('076c7aa8-91fd-11f1-bbda-28f52bf063a0', 'g006', '有机小白菜', NULL, NULL, 'seed_650787', '精打细算', '/api/file/default-avatar.png', NULL, 5, '有机小白菜味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-26 10:26:20', NULL);
INSERT INTO `goods_review` VALUES ('076cc95a-91fd-11f1-bbda-28f52bf063a0', 'g006', '有机小白菜', NULL, NULL, 'seed_644117', '王阿姨', '/api/file/default-avatar.png', NULL, 5, '有机小白菜收到了，包装很严实，质量超出预期', NULL, '包装精美,物流快', NULL, 0, 1, NULL, NULL, '2026-07-27 14:18:54', NULL);
INSERT INTO `goods_review` VALUES ('076d14c7-91fd-11f1-bbda-28f52bf063a0', 'g006', '有机小白菜', NULL, NULL, 'seed_968447', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 4, '买了有机小白菜，确实不错，会回购的', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-26 09:55:05', NULL);
INSERT INTO `goods_review` VALUES ('076d596b-91fd-11f1-bbda-28f52bf063a0', 'g007', '农家自种西红柿', NULL, NULL, 'seed_57610', '小李', '/api/file/default-avatar.png', NULL, 4, '农家自种西红柿品质稳定，已经回购好几次了', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-25 11:48:51', NULL);
INSERT INTO `goods_review` VALUES ('076d9a31-91fd-11f1-bbda-28f52bf063a0', 'g007', '农家自种西红柿', NULL, NULL, 'seed_20006', '美食控', '/api/file/default-avatar.png', NULL, 5, '农家自种西红柿味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-16 21:10:42', NULL);
INSERT INTO `goods_review` VALUES ('076defb9-91fd-11f1-bbda-28f52bf063a0', 'g007', '农家自种西红柿', NULL, NULL, 'seed_55835', '退休陈姨', '/api/file/default-avatar.png', NULL, 5, '农家自种西红柿物流速度很快，第二天就到了，很新鲜', NULL, '物流快,新鲜', NULL, 0, 1, NULL, NULL, '2026-07-10 22:29:41', NULL);
INSERT INTO `goods_review` VALUES ('076e6ba3-91fd-11f1-bbda-28f52bf063a0', 'g007', '农家自种西红柿', NULL, NULL, 'seed_641993', '邻里小张', '/api/file/default-avatar.png', NULL, 5, '整体满意，农家自种西红柿发货速度快，推荐购买', NULL, '物流快,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-27 08:53:24', NULL);
INSERT INTO `goods_review` VALUES ('076ed254-91fd-11f1-bbda-28f52bf063a0', 'g007', '农家自种西红柿', NULL, NULL, 'seed_101346', '王阿姨', '/api/file/default-avatar.png', NULL, 4, '农家自种西红柿品质稳定，已经回购好几次了', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-10 00:19:44', NULL);
INSERT INTO `goods_review` VALUES ('076f22e5-91fd-11f1-bbda-28f52bf063a0', 'g008', '山东铁棍山药', NULL, NULL, 'seed_440746', '王阿姨', '/api/file/default-avatar.png', NULL, 5, '山东铁棍山药物流速度很快，第二天就到了，很新鲜', NULL, '物流快,新鲜', NULL, 0, 1, NULL, NULL, '2026-07-27 22:05:34', NULL);
INSERT INTO `goods_review` VALUES ('076f6d47-91fd-11f1-bbda-28f52bf063a0', 'g008', '山东铁棍山药', NULL, NULL, 'seed_90104', '阳光大叔', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说山东铁棍山药好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-31 13:47:21', NULL);
INSERT INTO `goods_review` VALUES ('076fbbf8-91fd-11f1-bbda-28f52bf063a0', 'g008', '山东铁棍山药', NULL, NULL, 'seed_834906', '邻里小张', '/api/file/default-avatar.png', NULL, 5, '山东铁棍山药味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-29 06:42:13', NULL);
INSERT INTO `goods_review` VALUES ('077008cf-91fd-11f1-bbda-28f52bf063a0', 'g009', '新西兰佳沛金果奇异果', NULL, NULL, 'seed_328078', '精打细算', '/api/file/default-avatar.png', NULL, 5, '新西兰佳沛金果奇异果物流速度很快，第二天就到了，很新鲜', NULL, '物流快,新鲜', NULL, 0, 1, NULL, NULL, '2026-07-11 23:15:23', NULL);
INSERT INTO `goods_review` VALUES ('077070e2-91fd-11f1-bbda-28f52bf063a0', 'g009', '新西兰佳沛金果奇异果', NULL, NULL, 'seed_521906', '精打细算', '/api/file/default-avatar.png', NULL, 5, '整体满意，新西兰佳沛金果奇异果发货速度快，推荐购买', NULL, '物流快,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-28 06:45:55', NULL);
INSERT INTO `goods_review` VALUES ('0770dcfe-91fd-11f1-bbda-28f52bf063a0', 'g009', '新西兰佳沛金果奇异果', NULL, NULL, 'seed_241039', '小李', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说新西兰佳沛金果奇异果好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-31 23:47:13', NULL);
INSERT INTO `goods_review` VALUES ('07712f24-91fd-11f1-bbda-28f52bf063a0', 'g009', '新西兰佳沛金果奇异果', NULL, NULL, 'seed_840065', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 5, '买了新西兰佳沛金果奇异果，确实不错，会回购的', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-08-03 04:04:17', NULL);
INSERT INTO `goods_review` VALUES ('07718dc4-91fd-11f1-bbda-28f52bf063a0', 'g010', '越南红心火龙果', NULL, NULL, 'seed_551037', '王阿姨', '/api/file/default-avatar.png', NULL, 5, '收到越南红心火龙果很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-30 19:04:25', NULL);
INSERT INTO `goods_review` VALUES ('0771e998-91fd-11f1-bbda-28f52bf063a0', 'g010', '越南红心火龙果', NULL, NULL, 'seed_169421', '精打细算', '/api/file/default-avatar.png', NULL, 5, '拼团买的越南红心火龙果，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-24 16:25:54', NULL);
INSERT INTO `goods_review` VALUES ('077241f5-91fd-11f1-bbda-28f52bf063a0', 'g010', '越南红心火龙果', NULL, NULL, 'seed_657996', '微信用户', '/api/file/default-avatar.png', NULL, 5, '拼团买的越南红心火龙果，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-29 22:13:55', NULL);
INSERT INTO `goods_review` VALUES ('07729cb8-91fd-11f1-bbda-28f52bf063a0', 'g010', '越南红心火龙果', NULL, NULL, 'seed_414107', '王阿姨', '/api/file/default-avatar.png', NULL, 5, '越南红心火龙果质量一般，但价格摆在这里，还算可以', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-14 15:47:36', NULL);
INSERT INTO `goods_review` VALUES ('07732cc3-91fd-11f1-bbda-28f52bf063a0', 'g011', '黑猪五花肉', NULL, NULL, 'seed_732642', '微信用户', '/api/file/default-avatar.png', NULL, 5, '拼团买的黑猪五花肉，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-19 11:18:11', NULL);
INSERT INTO `goods_review` VALUES ('077399f1-91fd-11f1-bbda-28f52bf063a0', 'g011', '黑猪五花肉', NULL, NULL, 'seed_524501', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说黑猪五花肉好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-22 06:11:50', NULL);
INSERT INTO `goods_review` VALUES ('0773edcf-91fd-11f1-bbda-28f52bf063a0', 'g011', '黑猪五花肉', NULL, NULL, 'seed_102429', '邻里小张', '/api/file/default-avatar.png', NULL, 5, '收到黑猪五花肉很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-15 23:55:32', NULL);
INSERT INTO `goods_review` VALUES ('07743e3a-91fd-11f1-bbda-28f52bf063a0', 'g012', '澳洲谷饲牛腩块', NULL, NULL, 'seed_630273', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 5, '澳洲谷饲牛腩块品质稳定，已经回购好几次了', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-08-04 19:04:12', NULL);
INSERT INTO `goods_review` VALUES ('07748d5e-91fd-11f1-bbda-28f52bf063a0', 'g012', '澳洲谷饲牛腩块', NULL, NULL, 'seed_198357', '邻里小张', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说澳洲谷饲牛腩块好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-29 15:19:52', NULL);
INSERT INTO `goods_review` VALUES ('0774dc0a-91fd-11f1-bbda-28f52bf063a0', 'g012', '澳洲谷饲牛腩块', NULL, NULL, 'seed_229274', '邻里小张', '/api/file/default-avatar.png', NULL, 4, '澳洲谷饲牛腩块味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-21 03:45:16', NULL);
INSERT INTO `goods_review` VALUES ('07752883-91fd-11f1-bbda-28f52bf063a0', 'g012', '澳洲谷饲牛腩块', NULL, NULL, 'seed_849084', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 5, '澳洲谷饲牛腩块收到了，包装很严实，质量超出预期', NULL, '包装精美,物流快', NULL, 0, 1, NULL, NULL, '2026-07-13 20:48:54', NULL);
INSERT INTO `goods_review` VALUES ('0775ac1b-91fd-11f1-bbda-28f52bf063a0', 'g012', '澳洲谷饲牛腩块', NULL, NULL, 'seed_400967', '阳光大叔', '/api/file/default-avatar.png', NULL, 5, '收到澳洲谷饲牛腩块很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-08-01 16:57:01', NULL);
INSERT INTO `goods_review` VALUES ('07762bad-91fd-11f1-bbda-28f52bf063a0', 'g013', '三黄鸡 整只', NULL, NULL, 'seed_887109', '精打细算', '/api/file/default-avatar.png', NULL, 5, '收到三黄鸡 整只很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-22 07:13:38', NULL);
INSERT INTO `goods_review` VALUES ('07768ad0-91fd-11f1-bbda-28f52bf063a0', 'g013', '三黄鸡 整只', NULL, NULL, 'seed_730125', '社区居民', '/api/file/default-avatar.png', NULL, 4, '三黄鸡 整只收到了，包装很严实，质量超出预期', NULL, '包装精美,物流快', NULL, 0, 1, NULL, NULL, '2026-08-02 18:31:14', NULL);
INSERT INTO `goods_review` VALUES ('0776dba5-91fd-11f1-bbda-28f52bf063a0', 'g013', '三黄鸡 整只', NULL, NULL, 'seed_673547', '美食控', '/api/file/default-avatar.png', NULL, 5, '买了三黄鸡 整只，确实不错，会回购的', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-19 05:01:04', NULL);
INSERT INTO `goods_review` VALUES ('077727bf-91fd-11f1-bbda-28f52bf063a0', 'g013', '三黄鸡 整只', NULL, NULL, 'seed_718299', '邻里小张', '/api/file/default-avatar.png', NULL, 4, '三黄鸡 整只物流速度很快，第二天就到了，很新鲜', NULL, '物流快,新鲜', NULL, 0, 1, NULL, NULL, '2026-08-06 09:52:43', NULL);
INSERT INTO `goods_review` VALUES ('0777754b-91fd-11f1-bbda-28f52bf063a0', 'g013', '三黄鸡 整只', NULL, NULL, 'seed_19200', '退休陈姨', '/api/file/default-avatar.png', NULL, 4, '三黄鸡 整只味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-29 23:05:14', NULL);
INSERT INTO `goods_review` VALUES ('0777c4a4-91fd-11f1-bbda-28f52bf063a0', 'g014', '野生大虾', NULL, NULL, 'seed_549274', '邻里小张', '/api/file/default-avatar.png', NULL, 4, '买了野生大虾，确实不错，会回购的', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-29 08:53:21', NULL);
INSERT INTO `goods_review` VALUES ('07785105-91fd-11f1-bbda-28f52bf063a0', 'g014', '野生大虾', NULL, NULL, 'seed_379435', '精打细算', '/api/file/default-avatar.png', NULL, 5, '整体满意，野生大虾发货速度快，推荐购买', NULL, '物流快,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-24 01:05:30', NULL);
INSERT INTO `goods_review` VALUES ('0778f5cd-91fd-11f1-bbda-28f52bf063a0', 'g014', '野生大虾', NULL, NULL, 'seed_74872', '小李', '/api/file/default-avatar.png', NULL, 5, '买了野生大虾，确实不错，会回购的', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-08-06 12:11:48', NULL);
INSERT INTO `goods_review` VALUES ('07797f3c-91fd-11f1-bbda-28f52bf063a0', 'g015', '挪威三文鱼刺身', NULL, NULL, 'seed_239235', '王阿姨', '/api/file/default-avatar.png', NULL, 4, '挪威三文鱼刺身收到了，包装很严实，质量超出预期', NULL, '包装精美,物流快', NULL, 0, 1, NULL, NULL, '2026-07-18 20:29:19', NULL);
INSERT INTO `goods_review` VALUES ('0779cfab-91fd-11f1-bbda-28f52bf063a0', 'g015', '挪威三文鱼刺身', NULL, NULL, 'seed_371079', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 3, '挪威三文鱼刺身味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-12 00:51:15', NULL);
INSERT INTO `goods_review` VALUES ('077a1ed6-91fd-11f1-bbda-28f52bf063a0', 'g015', '挪威三文鱼刺身', NULL, NULL, 'seed_615483', '王阿姨', '/api/file/default-avatar.png', NULL, 5, '整体满意，挪威三文鱼刺身发货速度快，推荐购买', NULL, '物流快,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-17 21:25:23', NULL);
INSERT INTO `goods_review` VALUES ('077a6db0-91fd-11f1-bbda-28f52bf063a0', 'g016', '农家土鸡蛋', NULL, NULL, 'seed_727335', '阳光大叔', '/api/file/default-avatar.png', NULL, 4, '农家土鸡蛋收到了，包装很严实，质量超出预期', NULL, '包装精美,物流快', NULL, 0, 1, NULL, NULL, '2026-07-09 19:21:35', NULL);
INSERT INTO `goods_review` VALUES ('077b39d8-91fd-11f1-bbda-28f52bf063a0', 'g016', '农家土鸡蛋', NULL, NULL, 'seed_420093', '邻里小张', '/api/file/default-avatar.png', NULL, 5, '农家土鸡蛋质量一般，但价格摆在这里，还算可以', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-17 01:31:30', NULL);
INSERT INTO `goods_review` VALUES ('077bdf7b-91fd-11f1-bbda-28f52bf063a0', 'g016', '农家土鸡蛋', NULL, NULL, 'seed_954393', '小李', '/api/file/default-avatar.png', NULL, 5, '农家土鸡蛋质量一般，但价格摆在这里，还算可以', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-12 23:15:38', NULL);
INSERT INTO `goods_review` VALUES ('077c3ef5-91fd-11f1-bbda-28f52bf063a0', 'g016', '农家土鸡蛋', NULL, NULL, 'seed_651296', '小李', '/api/file/default-avatar.png', NULL, 5, '买了农家土鸡蛋，确实不错，会回购的', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-21 12:19:15', NULL);
INSERT INTO `goods_review` VALUES ('077c8bd8-91fd-11f1-bbda-28f52bf063a0', 'g017', '老豆腐', NULL, NULL, 'seed_2107', '阳光大叔', '/api/file/default-avatar.png', NULL, 5, '老豆腐味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-31 02:20:11', NULL);
INSERT INTO `goods_review` VALUES ('077cdf08-91fd-11f1-bbda-28f52bf063a0', 'g017', '老豆腐', NULL, NULL, 'seed_223610', '退休陈姨', '/api/file/default-avatar.png', NULL, 5, '老豆腐品质稳定，已经回购好几次了', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-28 13:02:48', NULL);
INSERT INTO `goods_review` VALUES ('077d9218-91fd-11f1-bbda-28f52bf063a0', 'g017', '老豆腐', NULL, NULL, 'seed_76018', '退休陈姨', '/api/file/default-avatar.png', NULL, 5, '整体满意，老豆腐发货速度快，推荐购买', NULL, '物流快,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-12 10:24:21', NULL);
INSERT INTO `goods_review` VALUES ('077e0cbe-91fd-11f1-bbda-28f52bf063a0', 'g017', '老豆腐', NULL, NULL, 'seed_427962', '退休陈姨', '/api/file/default-avatar.png', NULL, 5, '收到老豆腐很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-27 20:38:31', NULL);
INSERT INTO `goods_review` VALUES ('077e5fd1-91fd-11f1-bbda-28f52bf063a0', 'g018', '蒙牛纯牛奶 全脂250ml', NULL, NULL, 'seed_496615', '精打细算', '/api/file/default-avatar.png', NULL, 5, '蒙牛纯牛奶 全脂250ml收到了，包装很严实，质量超出预期', NULL, '包装精美,物流快', NULL, 0, 1, NULL, NULL, '2026-07-10 01:05:25', NULL);
INSERT INTO `goods_review` VALUES ('077eb957-91fd-11f1-bbda-28f52bf063a0', 'g018', '蒙牛纯牛奶 全脂250ml', NULL, NULL, 'seed_982815', '王阿姨', '/api/file/default-avatar.png', NULL, 3, '口感很好，家人都说蒙牛纯牛奶 全脂250ml好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-19 00:19:25', NULL);
INSERT INTO `goods_review` VALUES ('077f091c-91fd-11f1-bbda-28f52bf063a0', 'g018', '蒙牛纯牛奶 全脂250ml', NULL, NULL, 'seed_753023', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 4, '口感很好，家人都说蒙牛纯牛奶 全脂250ml好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-18 07:00:43', NULL);
INSERT INTO `goods_review` VALUES ('077f696e-91fd-11f1-bbda-28f52bf063a0', 'g018', '蒙牛纯牛奶 全脂250ml', NULL, NULL, 'seed_831443', '美食控', '/api/file/default-avatar.png', NULL, 5, '蒙牛纯牛奶 全脂250ml质量一般，但价格摆在这里，还算可以', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-11 08:28:51', NULL);
INSERT INTO `goods_review` VALUES ('077fea59-91fd-11f1-bbda-28f52bf063a0', 'g019', '洽洽香瓜子 五香味', NULL, NULL, 'seed_487112', '小李', '/api/file/default-avatar.png', NULL, 5, '洽洽香瓜子 五香味质量一般，但价格摆在这里，还算可以', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-17 06:08:07', NULL);
INSERT INTO `goods_review` VALUES ('07809239-91fd-11f1-bbda-28f52bf063a0', 'g019', '洽洽香瓜子 五香味', NULL, NULL, 'seed_446327', '精打细算', '/api/file/default-avatar.png', NULL, 5, '收到洽洽香瓜子 五香味很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-13 15:22:19', NULL);
INSERT INTO `goods_review` VALUES ('0780ff52-91fd-11f1-bbda-28f52bf063a0', 'g019', '洽洽香瓜子 五香味', NULL, NULL, 'seed_733024', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 5, '洽洽香瓜子 五香味物流速度很快，第二天就到了，很新鲜', NULL, '物流快,新鲜', NULL, 0, 1, NULL, NULL, '2026-07-24 03:35:26', NULL);
INSERT INTO `goods_review` VALUES ('07815c29-91fd-11f1-bbda-28f52bf063a0', 'g019', '洽洽香瓜子 五香味', NULL, NULL, 'seed_62627', '王阿姨', '/api/file/default-avatar.png', NULL, 4, '收到洽洽香瓜子 五香味很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-25 10:06:37', NULL);
INSERT INTO `goods_review` VALUES ('0781b1bf-91fd-11f1-bbda-28f52bf063a0', 'g020', '三只松鼠每日坚果', NULL, NULL, 'seed_87801', '阳光大叔', '/api/file/default-avatar.png', NULL, 4, '收到三只松鼠每日坚果很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-08-04 03:19:52', NULL);
INSERT INTO `goods_review` VALUES ('078202a9-91fd-11f1-bbda-28f52bf063a0', 'g020', '三只松鼠每日坚果', NULL, NULL, 'seed_112281', '美食控', '/api/file/default-avatar.png', NULL, 4, '拼团买的三只松鼠每日坚果，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-28 00:40:54', NULL);
INSERT INTO `goods_review` VALUES ('07829954-91fd-11f1-bbda-28f52bf063a0', 'g020', '三只松鼠每日坚果', NULL, NULL, 'seed_935332', '精打细算', '/api/file/default-avatar.png', NULL, 4, '三只松鼠每日坚果品质稳定，已经回购好几次了', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-23 18:05:00', NULL);
INSERT INTO `goods_review` VALUES ('078317be-91fd-11f1-bbda-28f52bf063a0', 'g020', '三只松鼠每日坚果', NULL, NULL, 'seed_805978', '精打细算', '/api/file/default-avatar.png', NULL, 5, '三只松鼠每日坚果物流速度很快，第二天就到了，很新鲜', NULL, '物流快,新鲜', NULL, 0, 1, NULL, NULL, '2026-08-04 09:55:31', NULL);
INSERT INTO `goods_review` VALUES ('0783ae62-91fd-11f1-bbda-28f52bf063a0', 'g021', '奥利奥夹心饼干', NULL, NULL, 'seed_758367', '邻里小张', '/api/file/default-avatar.png', NULL, 5, '买了奥利奥夹心饼干，确实不错，会回购的', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-29 03:23:37', NULL);
INSERT INTO `goods_review` VALUES ('07840477-91fd-11f1-bbda-28f52bf063a0', 'g021', '奥利奥夹心饼干', NULL, NULL, 'seed_691259', '精打细算', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说奥利奥夹心饼干好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-28 19:47:09', NULL);
INSERT INTO `goods_review` VALUES ('078450d9-91fd-11f1-bbda-28f52bf063a0', 'g021', '奥利奥夹心饼干', NULL, NULL, 'seed_772744', '小李', '/api/file/default-avatar.png', NULL, 4, '奥利奥夹心饼干品质稳定，已经回购好几次了', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-31 15:56:27', NULL);
INSERT INTO `goods_review` VALUES ('07849f75-91fd-11f1-bbda-28f52bf063a0', 'g021', '奥利奥夹心饼干', NULL, NULL, 'seed_832978', '邻里小张', '/api/file/default-avatar.png', NULL, 4, '奥利奥夹心饼干收到了，包装很严实，质量超出预期', NULL, '包装精美,物流快', NULL, 0, 1, NULL, NULL, '2026-07-10 03:30:53', NULL);
INSERT INTO `goods_review` VALUES ('07857931-91fd-11f1-bbda-28f52bf063a0', 'g022', '德芙丝滑牛奶巧克力', NULL, NULL, 'seed_904717', '小李', '/api/file/default-avatar.png', NULL, 5, '买了德芙丝滑牛奶巧克力，确实不错，会回购的', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-22 10:46:10', NULL);
INSERT INTO `goods_review` VALUES ('0785e0b5-91fd-11f1-bbda-28f52bf063a0', 'g022', '德芙丝滑牛奶巧克力', NULL, NULL, 'seed_293883', '美食控', '/api/file/default-avatar.png', NULL, 5, '德芙丝滑牛奶巧克力品质稳定，已经回购好几次了', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-19 02:13:05', NULL);
INSERT INTO `goods_review` VALUES ('07864823-91fd-11f1-bbda-28f52bf063a0', 'g022', '德芙丝滑牛奶巧克力', NULL, NULL, 'seed_358709', '美食控', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说德芙丝滑牛奶巧克力好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-20 13:35:10', NULL);
INSERT INTO `goods_review` VALUES ('078696af-91fd-11f1-bbda-28f52bf063a0', 'g022', '德芙丝滑牛奶巧克力', NULL, NULL, 'seed_385609', '王阿姨', '/api/file/default-avatar.png', NULL, 5, '德芙丝滑牛奶巧克力质量一般，但价格摆在这里，还算可以', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-25 07:28:26', NULL);
INSERT INTO `goods_review` VALUES ('0786e5d8-91fd-11f1-bbda-28f52bf063a0', 'g023', '可口可乐 330ml*24罐', NULL, NULL, 'seed_285410', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 4, '口感很好，家人都说可口可乐 330ml*24罐好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-09 10:53:57', NULL);
INSERT INTO `goods_review` VALUES ('07873c29-91fd-11f1-bbda-28f52bf063a0', 'g023', '可口可乐 330ml*24罐', NULL, NULL, 'seed_316733', '美食控', '/api/file/default-avatar.png', NULL, 4, '可口可乐 330ml*24罐质量一般，但价格摆在这里，还算可以', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-28 17:07:23', NULL);
INSERT INTO `goods_review` VALUES ('07880530-91fd-11f1-bbda-28f52bf063a0', 'g023', '可口可乐 330ml*24罐', NULL, NULL, 'seed_901103', '美食控', '/api/file/default-avatar.png', NULL, 5, '收到可口可乐 330ml*24罐很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-17 14:47:15', NULL);
INSERT INTO `goods_review` VALUES ('07886300-91fd-11f1-bbda-28f52bf063a0', 'g024', '农夫山泉茶π 500ml', NULL, NULL, 'seed_188503', '小李', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说农夫山泉茶π 500ml好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-24 16:18:52', NULL);
INSERT INTO `goods_review` VALUES ('0788bb74-91fd-11f1-bbda-28f52bf063a0', 'g024', '农夫山泉茶π 500ml', NULL, NULL, 'seed_873974', '阳光大叔', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说农夫山泉茶π 500ml好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-24 18:52:21', NULL);
INSERT INTO `goods_review` VALUES ('07891099-91fd-11f1-bbda-28f52bf063a0', 'g024', '农夫山泉茶π 500ml', NULL, NULL, 'seed_863095', '美食控', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说农夫山泉茶π 500ml好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-28 05:29:41', NULL);
INSERT INTO `goods_review` VALUES ('07896267-91fd-11f1-bbda-28f52bf063a0', 'g024', '农夫山泉茶π 500ml', NULL, NULL, 'seed_117499', '退休陈姨', '/api/file/default-avatar.png', NULL, 4, '农夫山泉茶π 500ml物流速度很快，第二天就到了，很新鲜', NULL, '物流快,新鲜', NULL, 0, 1, NULL, NULL, '2026-07-09 23:15:36', NULL);
INSERT INTO `goods_review` VALUES ('0789b370-91fd-11f1-bbda-28f52bf063a0', 'g024', '农夫山泉茶π 500ml', NULL, NULL, 'seed_918731', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 5, '农夫山泉茶π 500ml味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-26 02:34:16', NULL);
INSERT INTO `goods_review` VALUES ('078a80a1-91fd-11f1-bbda-28f52bf063a0', 'g025', '五常大米 稻花香 5kg', NULL, NULL, 'seed_220749', '阳光大叔', '/api/file/default-avatar.png', NULL, 4, '收到五常大米 稻花香 5kg很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-19 22:08:46', NULL);
INSERT INTO `goods_review` VALUES ('078aed42-91fd-11f1-bbda-28f52bf063a0', 'g025', '五常大米 稻花香 5kg', NULL, NULL, 'seed_545546', '微信用户', '/api/file/default-avatar.png', NULL, 5, '五常大米 稻花香 5kg质量一般，但价格摆在这里，还算可以', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-17 12:41:24', NULL);
INSERT INTO `goods_review` VALUES ('078b576f-91fd-11f1-bbda-28f52bf063a0', 'g025', '五常大米 稻花香 5kg', NULL, NULL, 'seed_784107', '美食控', '/api/file/default-avatar.png', NULL, 5, '五常大米 稻花香 5kg收到了，包装很严实，质量超出预期', NULL, '包装精美,物流快', NULL, 0, 1, NULL, NULL, '2026-07-16 00:32:37', NULL);
INSERT INTO `goods_review` VALUES ('078bb253-91fd-11f1-bbda-28f52bf063a0', 'g026', '金龙鱼面粉 高筋 5kg', NULL, NULL, 'seed_349412', '退休陈姨', '/api/file/default-avatar.png', NULL, 4, '金龙鱼面粉 高筋 5kg物流速度很快，第二天就到了，很新鲜', NULL, '物流快,新鲜', NULL, 0, 1, NULL, NULL, '2026-07-26 08:44:02', NULL);
INSERT INTO `goods_review` VALUES ('078c04d1-91fd-11f1-bbda-28f52bf063a0', 'g026', '金龙鱼面粉 高筋 5kg', NULL, NULL, 'seed_194414', '阳光大叔', '/api/file/default-avatar.png', NULL, 4, '拼团买的金龙鱼面粉 高筋 5kg，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-23 14:01:03', NULL);
INSERT INTO `goods_review` VALUES ('078cdcd6-91fd-11f1-bbda-28f52bf063a0', 'g026', '金龙鱼面粉 高筋 5kg', NULL, NULL, 'seed_599904', '邻里小张', '/api/file/default-avatar.png', NULL, 5, '整体满意，金龙鱼面粉 高筋 5kg发货速度快，推荐购买', NULL, '物流快,性价比高', NULL, 0, 1, NULL, NULL, '2026-08-02 10:24:57', NULL);
INSERT INTO `goods_review` VALUES ('078d6be8-91fd-11f1-bbda-28f52bf063a0', 'g026', '金龙鱼面粉 高筋 5kg', NULL, NULL, 'seed_387300', '小李', '/api/file/default-avatar.png', NULL, 5, '金龙鱼面粉 高筋 5kg品质稳定，已经回购好几次了', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-31 08:24:32', NULL);
INSERT INTO `goods_review` VALUES ('078dd0ce-91fd-11f1-bbda-28f52bf063a0', 'g027', '鲁花花生油 5L', NULL, NULL, 'seed_686040', '小李', '/api/file/default-avatar.png', NULL, 5, '拼团买的鲁花花生油 5L，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-08-05 00:34:15', NULL);
INSERT INTO `goods_review` VALUES ('078e3527-91fd-11f1-bbda-28f52bf063a0', 'g027', '鲁花花生油 5L', NULL, NULL, 'seed_790173', '阳光大叔', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说鲁花花生油 5L好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-16 00:20:42', NULL);
INSERT INTO `goods_review` VALUES ('078e88ca-91fd-11f1-bbda-28f52bf063a0', 'g027', '鲁花花生油 5L', NULL, NULL, 'seed_894062', '社区居民', '/api/file/default-avatar.png', NULL, 5, '整体满意，鲁花花生油 5L发货速度快，推荐购买', NULL, '物流快,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-09 09:20:10', NULL);
INSERT INTO `goods_review` VALUES ('078ed0e9-91fd-11f1-bbda-28f52bf063a0', 'g027', '鲁花花生油 5L', NULL, NULL, 'seed_248564', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 5, '拼团买的鲁花花生油 5L，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-22 11:57:30', NULL);
INSERT INTO `goods_review` VALUES ('078fe154-91fd-11f1-bbda-28f52bf063a0', 'g028', '海天酱油 生抽 500ml', NULL, NULL, 'seed_818632', '邻里小张', '/api/file/default-avatar.png', NULL, 5, '收到海天酱油 生抽 500ml很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-18 14:39:55', NULL);
INSERT INTO `goods_review` VALUES ('07906caa-91fd-11f1-bbda-28f52bf063a0', 'g028', '海天酱油 生抽 500ml', NULL, NULL, 'seed_599590', '精打细算', '/api/file/default-avatar.png', NULL, 4, '海天酱油 生抽 500ml质量一般，但价格摆在这里，还算可以', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-11 17:43:25', NULL);
INSERT INTO `goods_review` VALUES ('0790c611-91fd-11f1-bbda-28f52bf063a0', 'g028', '海天酱油 生抽 500ml', NULL, NULL, 'seed_260237', '退休陈姨', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说海天酱油 生抽 500ml好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-11 18:25:01', NULL);
INSERT INTO `goods_review` VALUES ('07912714-91fd-11f1-bbda-28f52bf063a0', 'g028', '海天酱油 生抽 500ml', NULL, NULL, 'seed_675384', '退休陈姨', '/api/file/default-avatar.png', NULL, 5, '买了海天酱油 生抽 500ml，确实不错，会回购的', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-22 20:48:44', NULL);
INSERT INTO `goods_review` VALUES ('0791e018-91fd-11f1-bbda-28f52bf063a0', 'g028', '海天酱油 生抽 500ml', NULL, NULL, 'seed_12829', '小李', '/api/file/default-avatar.png', NULL, 5, '拼团买的海天酱油 生抽 500ml，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-25 05:15:33', NULL);
INSERT INTO `goods_review` VALUES ('07924fea-91fd-11f1-bbda-28f52bf063a0', 'g029', '李锦记蚝油 520g', NULL, NULL, 'seed_175419', '精打细算', '/api/file/default-avatar.png', NULL, 5, '收到李锦记蚝油 520g很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-11 08:58:48', NULL);
INSERT INTO `goods_review` VALUES ('0792c34c-91fd-11f1-bbda-28f52bf063a0', 'g029', '李锦记蚝油 520g', NULL, NULL, 'seed_401124', '退休陈姨', '/api/file/default-avatar.png', NULL, 5, '收到李锦记蚝油 520g很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-08-06 07:22:02', NULL);
INSERT INTO `goods_review` VALUES ('07932447-91fd-11f1-bbda-28f52bf063a0', 'g029', '李锦记蚝油 520g', NULL, NULL, 'seed_245158', '阳光大叔', '/api/file/default-avatar.png', NULL, 5, '李锦记蚝油 520g物流速度很快，第二天就到了，很新鲜', NULL, '物流快,新鲜', NULL, 0, 1, NULL, NULL, '2026-07-16 08:22:29', NULL);
INSERT INTO `goods_review` VALUES ('07938033-91fd-11f1-bbda-28f52bf063a0', 'g029', '李锦记蚝油 520g', NULL, NULL, 'seed_524813', '阳光大叔', '/api/file/default-avatar.png', NULL, 4, '李锦记蚝油 520g收到了，包装很严实，质量超出预期', NULL, '包装精美,物流快', NULL, 0, 1, NULL, NULL, '2026-07-18 17:28:56', NULL);
INSERT INTO `goods_review` VALUES ('07946d3e-91fd-11f1-bbda-28f52bf063a0', 'g030', '蓝月亮洗衣液 3kg', NULL, NULL, 'seed_869719', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 4, '蓝月亮洗衣液 3kg味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-16 07:42:23', NULL);
INSERT INTO `goods_review` VALUES ('0794e5d0-91fd-11f1-bbda-28f52bf063a0', 'g030', '蓝月亮洗衣液 3kg', NULL, NULL, 'seed_999607', '美食控', '/api/file/default-avatar.png', NULL, 5, '蓝月亮洗衣液 3kg质量一般，但价格摆在这里，还算可以', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-08-01 10:58:02', NULL);
INSERT INTO `goods_review` VALUES ('07956a6a-91fd-11f1-bbda-28f52bf063a0', 'g030', '蓝月亮洗衣液 3kg', NULL, NULL, 'seed_804217', '阳光大叔', '/api/file/default-avatar.png', NULL, 5, '收到蓝月亮洗衣液 3kg很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-10 02:45:49', NULL);
INSERT INTO `goods_review` VALUES ('0795cb86-91fd-11f1-bbda-28f52bf063a0', 'g030', '蓝月亮洗衣液 3kg', NULL, NULL, 'seed_183554', '退休陈姨', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说蓝月亮洗衣液 3kg好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-08-05 15:18:15', NULL);
INSERT INTO `goods_review` VALUES ('079627ea-91fd-11f1-bbda-28f52bf063a0', 'g031', '立白洗洁精 1.5kg', NULL, NULL, 'seed_656894', '社区居民', '/api/file/default-avatar.png', NULL, 5, '立白洗洁精 1.5kg收到了，包装很严实，质量超出预期', NULL, '包装精美,物流快', NULL, 0, 1, NULL, NULL, '2026-07-16 19:44:37', NULL);
INSERT INTO `goods_review` VALUES ('07968e18-91fd-11f1-bbda-28f52bf063a0', 'g031', '立白洗洁精 1.5kg', NULL, NULL, 'seed_924181', '社区居民', '/api/file/default-avatar.png', NULL, 5, '拼团买的立白洗洁精 1.5kg，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-20 01:59:53', NULL);
INSERT INTO `goods_review` VALUES ('0797057d-91fd-11f1-bbda-28f52bf063a0', 'g031', '立白洗洁精 1.5kg', NULL, NULL, 'seed_25673', '阳光大叔', '/api/file/default-avatar.png', NULL, 5, '口感很好，家人都说立白洗洁精 1.5kg好吃，份量也足', NULL, '口感好,份量足', NULL, 0, 1, NULL, NULL, '2026-07-25 10:06:22', NULL);
INSERT INTO `goods_review` VALUES ('079754d5-91fd-11f1-bbda-28f52bf063a0', 'g031', '立白洗洁精 1.5kg', NULL, NULL, 'seed_823064', '小李', '/api/file/default-avatar.png', NULL, 5, '买了立白洗洁精 1.5kg，确实不错，会回购的', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-19 19:40:31', NULL);
INSERT INTO `goods_review` VALUES ('07979d54-91fd-11f1-bbda-28f52bf063a0', 'g032', '维达抽纸 3层100抽*24包', NULL, NULL, 'seed_601606', '王阿姨', '/api/file/default-avatar.png', NULL, 5, '维达抽纸 3层100抽*24包味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-08-01 01:31:40', NULL);
INSERT INTO `goods_review` VALUES ('0797e32b-91fd-11f1-bbda-28f52bf063a0', 'g032', '维达抽纸 3层100抽*24包', NULL, NULL, 'seed_720167', '王阿姨', '/api/file/default-avatar.png', NULL, 4, '收到维达抽纸 3层100抽*24包很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-21 18:27:53', NULL);
INSERT INTO `goods_review` VALUES ('0798289b-91fd-11f1-bbda-28f52bf063a0', 'g032', '维达抽纸 3层100抽*24包', NULL, NULL, 'seed_745813', '美食控', '/api/file/default-avatar.png', NULL, 5, '维达抽纸 3层100抽*24包物流速度很快，第二天就到了，很新鲜', NULL, '物流快,新鲜', NULL, 0, 1, NULL, NULL, '2026-07-30 10:46:20', NULL);
INSERT INTO `goods_review` VALUES ('07986a9a-91fd-11f1-bbda-28f52bf063a0', 'g032', '维达抽纸 3层100抽*24包', NULL, NULL, 'seed_827836', '邻里小张', '/api/file/default-avatar.png', NULL, 4, '收到维达抽纸 3层100抽*24包很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-22 01:49:28', NULL);
INSERT INTO `goods_review` VALUES ('0798a9a0-91fd-11f1-bbda-28f52bf063a0', 'g032', '维达抽纸 3层100抽*24包', NULL, NULL, 'seed_449644', '邻里小张', '/api/file/default-avatar.png', NULL, 4, '整体满意，维达抽纸 3层100抽*24包发货速度快，推荐购买', NULL, '物流快,性价比高', NULL, 0, 1, NULL, NULL, '2026-08-01 19:05:19', NULL);
INSERT INTO `goods_review` VALUES ('0798ec3d-91fd-11f1-bbda-28f52bf063a0', 'g033', '清风卷纸 4层140克*10卷', NULL, NULL, 'seed_396122', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 5, '清风卷纸 4层140克*10卷品质稳定，已经回购好几次了', NULL, '性价比高,口感好', NULL, 0, 1, NULL, NULL, '2026-07-08 14:59:46', NULL);
INSERT INTO `goods_review` VALUES ('0799a1ff-91fd-11f1-bbda-28f52bf063a0', 'g033', '清风卷纸 4层140克*10卷', NULL, NULL, 'seed_205264', '退休陈姨', '/api/file/default-avatar.png', NULL, 4, '拼团买的清风卷纸 4层140克*10卷，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-24 18:24:20', NULL);
INSERT INTO `goods_review` VALUES ('079a1c0f-91fd-11f1-bbda-28f52bf063a0', 'g033', '清风卷纸 4层140克*10卷', NULL, NULL, 'seed_107594', '邻里小张', '/api/file/default-avatar.png', NULL, 4, '清风卷纸 4层140克*10卷味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-08-01 18:49:11', NULL);
INSERT INTO `goods_review` VALUES ('079a8dec-91fd-11f1-bbda-28f52bf063a0', 'g033', '清风卷纸 4层140克*10卷', NULL, NULL, 'seed_669346', '退休陈姨', '/api/file/default-avatar.png', NULL, 5, '收到清风卷纸 4层140克*10卷很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-08-03 18:27:32', NULL);
INSERT INTO `goods_review` VALUES ('079adf48-91fd-11f1-bbda-28f52bf063a0', 'g034', '酱卤猪蹄', NULL, NULL, 'seed_41286', '宝妈莉莉', '/api/file/default-avatar.png', NULL, 5, '酱卤猪蹄质量一般，但价格摆在这里，还算可以', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-30 00:37:32', NULL);
INSERT INTO `goods_review` VALUES ('079b2b4a-91fd-11f1-bbda-28f52bf063a0', 'g034', '酱卤猪蹄', NULL, NULL, 'seed_73892', '王阿姨', '/api/file/default-avatar.png', NULL, 4, '酱卤猪蹄质量一般，但价格摆在这里，还算可以', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-28 23:33:10', NULL);
INSERT INTO `goods_review` VALUES ('079b8679-91fd-11f1-bbda-28f52bf063a0', 'g034', '酱卤猪蹄', NULL, NULL, 'seed_577247', '微信用户', '/api/file/default-avatar.png', NULL, 5, '收到酱卤猪蹄很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-31 23:25:46', NULL);
INSERT INTO `goods_review` VALUES ('079c1b69-91fd-11f1-bbda-28f52bf063a0', 'g034', '酱卤猪蹄', NULL, NULL, 'seed_523763', '退休陈姨', '/api/file/default-avatar.png', NULL, 5, '整体满意，酱卤猪蹄发货速度快，推荐购买', NULL, '物流快,性价比高', NULL, 0, 1, NULL, NULL, '2026-08-01 23:53:12', NULL);
INSERT INTO `goods_review` VALUES ('079c75c5-91fd-11f1-bbda-28f52bf063a0', 'g034', '酱卤猪蹄', NULL, NULL, 'seed_897832', '美食控', '/api/file/default-avatar.png', NULL, 5, '收到酱卤猪蹄很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-08-01 00:33:02', NULL);
INSERT INTO `goods_review` VALUES ('079cceb9-91fd-11f1-bbda-28f52bf063a0', 'g035', '四川麻辣兔头', NULL, NULL, 'seed_895310', '退休陈姨', '/api/file/default-avatar.png', NULL, 4, '收到四川麻辣兔头很新鲜，客服态度也很好', NULL, '新鲜,物流快', NULL, 0, 1, NULL, NULL, '2026-07-21 01:54:22', NULL);
INSERT INTO `goods_review` VALUES ('079d2d5a-91fd-11f1-bbda-28f52bf063a0', 'g035', '四川麻辣兔头', NULL, NULL, 'seed_10187', '阳光大叔', '/api/file/default-avatar.png', NULL, 4, '整体满意，四川麻辣兔头发货速度快，推荐购买', NULL, '物流快,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-20 08:18:10', NULL);
INSERT INTO `goods_review` VALUES ('079d8514-91fd-11f1-bbda-28f52bf063a0', 'g035', '四川麻辣兔头', NULL, NULL, 'seed_532804', '微信用户', '/api/file/default-avatar.png', NULL, 5, '四川麻辣兔头收到了，包装很严实，质量超出预期', NULL, '包装精美,物流快', NULL, 0, 1, NULL, NULL, '2026-08-03 05:55:37', NULL);
INSERT INTO `goods_review` VALUES ('079dd5ad-91fd-11f1-bbda-28f52bf063a0', 'g036', '手工鲜肉大包', NULL, NULL, 'seed_628455', '退休陈姨', '/api/file/default-avatar.png', NULL, 4, '拼团买的手工鲜肉大包，价格实惠很划算', NULL, '性价比高', NULL, 0, 1, NULL, NULL, '2026-07-27 11:06:23', NULL);
INSERT INTO `goods_review` VALUES ('079e409a-91fd-11f1-bbda-28f52bf063a0', 'g036', '手工鲜肉大包', NULL, NULL, 'seed_127863', '王阿姨', '/api/file/default-avatar.png', NULL, 5, '手工鲜肉大包味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-08-02 23:25:49', NULL);
INSERT INTO `goods_review` VALUES ('079eb554-91fd-11f1-bbda-28f52bf063a0', 'g036', '手工鲜肉大包', NULL, NULL, 'seed_635957', '社区居民', '/api/file/default-avatar.png', NULL, 5, '手工鲜肉大包味道正宗，和朋友一起拼团买的很满意', NULL, '口感好,性价比高', NULL, 0, 1, NULL, NULL, '2026-07-08 10:14:40', NULL);
INSERT INTO `goods_review` VALUES ('1297c2b9ad8f4b7cb70bcae4d2d0789a', 'g001', '云南高原红富士苹果', '91cb259cfc6047ffb7bd5cf26090ec84', '500g/份', '2531e0020fe9407c86fe0182168a907d', '张三', '/api/file/default-avatar.png', 'c7284e86e6944cdf932a3568cb4d5dcf', 5, '很好吃', NULL, '品质好', '很好吃', 0, 1, NULL, NULL, '2026-08-08 17:12:21', NULL);
INSERT INTO `goods_review` VALUES ('555acf362aa6485f945c3fd32e86c62a', 'g001', '云南高原红富士苹果', '91cb259cfc6047ffb7bd5cf26090ec84', '500g/份', '2531e0020fe9407c86fe0182168a907d', '张三', '/api/file/default-avatar.png', 'd32973e87d7a454d986ed0f0169ce436', 4, '好', NULL, '品质好', '好', 0, 0, NULL, NULL, '2026-08-08 15:56:29', NULL);
INSERT INTO `goods_review` VALUES ('7f09be72d0954ae3a11235e05783a417', 'g005', '海南金煌芒果', '1d91e28bd2a24ff58821e26f87885b88', '500g/斤', 'aa8aa123f1fd41f1a9c9942cdeabcbf0', '张三', '/api/file/ab671241e6454995bf482cf761b11a87.png', '48475d37708b47538aad303a8a9638d8', 5, NULL, NULL, '品质好', NULL, 0, 1, NULL, NULL, '2026-08-15 17:52:15', NULL);
INSERT INTO `goods_review` VALUES ('ac6883caa7d9425ea1a5ca7dea056ab5', 'g001', '云南高原红富士苹果', '91cb259cfc6047ffb7bd5cf26090ec84', '500g/份', '43209d02200c43aa814304185518ef5c', '张三', '/api/file/default-avatar.png', '390b72c633c94d66a5a37813e3ba4f63', 5, '好', NULL, '品质好', '好', 0, 0, NULL, NULL, '2026-08-07 23:11:49', NULL);
INSERT INTO `goods_review` VALUES ('adbfe6b529eb404dafe7f33becc7ecba', 'g001', '云南高原红富士苹果', '91cb259cfc6047ffb7bd5cf26090ec84', '500g/份', '2531e0020fe9407c86fe0182168a907d', '张三', '/api/file/default-avatar.png', 'a32538a9909f4e47ad484bc147f8b666', 5, NULL, NULL, '发货快', NULL, 0, 1, NULL, NULL, '2026-08-08 17:20:23', NULL);
INSERT INTO `goods_review` VALUES ('rev_g001_01', 'g001', '云南高原红富士苹果', NULL, NULL, 'u_demo_01', '李阿姨', '/api/file/default-avatar.png', NULL, 5, '苹果很新鲜，脆甜多汁，孩子特别喜欢吃！', '', '品质好', NULL, 0, 1, NULL, NULL, '2026-08-05 10:23:00', NULL);
INSERT INTO `goods_review` VALUES ('rev_g001_02', 'g001', '云南高原红富士苹果', NULL, NULL, 'u_demo_02', '王先生', '/api/file/default-avatar.png', NULL, 5, '早上下的单，下午就送到了，发货真快。', '', '发货快', NULL, 0, 1, '感谢支持，欢迎再次光临！', '2026-08-05 14:00:00', '2026-08-05 11:10:00', NULL);
INSERT INTO `goods_review` VALUES ('rev_g001_03', 'g001', '云南高原红富士苹果', NULL, NULL, 'u_demo_03', '张女士', '/api/file/default-avatar.png', NULL, 4, '味道不错，客服态度也很好，就是包装可以再严实一点。', '', '品质好,服务好', NULL, 0, 1, NULL, NULL, '2026-08-06 09:45:00', NULL);
INSERT INTO `goods_review` VALUES ('rev_g001_04', 'g001', '云南高原红富士苹果', NULL, NULL, 'u_demo_04', '匿名用户', '/api/file/default-avatar.png', NULL, 3, '个头有点小，味道还行，性价比一般。', '', '品质一般', NULL, 1, 1, NULL, NULL, '2026-08-06 16:20:00', NULL);
INSERT INTO `goods_review` VALUES ('rev_g001_05', 'g001', '云南高原红富士苹果', NULL, NULL, 'u_demo_05', '陈小姐', '/api/file/default-avatar.png', NULL, 5, '回购很多次了，家人都喜欢吃，推荐！', '', '', '', 0, 1, NULL, NULL, '2026-08-07 08:15:00', NULL);

-- ----------------------------
-- Table structure for goods_sku
-- ----------------------------
DROP TABLE IF EXISTS `goods_sku`;
CREATE TABLE `goods_sku`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `goods_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '商品ID(good.id)',
  `spec_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '规格ID(goods_spec.id)',
  `sku_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT 'SKU名称',
  `price` decimal(18, 2) NOT NULL COMMENT 'SKU价格',
  `stock` int NOT NULL COMMENT 'SKU库存',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_sku_goods`(`goods_id` ASC) USING BTREE,
  CONSTRAINT `fk_goods_sku_goods_id` FOREIGN KEY (`goods_id`) REFERENCES `good` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商品SKU规格表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of goods_sku
-- ----------------------------
INSERT INTO `goods_sku` VALUES ('06b33c4573f7498d99e9c7f140233e32', 'g013', '55c7284584ad4635ad390e730e4eaf55', '整只', 35.90, 60, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('13f8c9a8bdd74e9b94c914eb7b7c1f7c', 'g035', 'e98477eefa54469fb8cb5a05b469a86b', '个', 8.90, 100, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('199ab09e4f384431a8e73435c6a3d17e', 'g027', '0579ef3b3b604fe1a231e64e99233a2d', '5L/桶', 129.00, 78, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('1d91e28bd2a24ff58821e26f87885b88', 'g005', '5c61bade22144a2080343e1b5a61fde4', '500g/斤', 9.90, 298, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('1ffa675f398642c091c8cd4f58dd642c', 'g016', '867728ac52c542d99ffbbdf4f5df8826', '10枚装', 16.90, 120, '2026-08-05 14:08:22');
INSERT INTO `goods_sku` VALUES ('2c8be24b3008466f8b9508cb6ff34f7c', 'g009', '4f217fd870c540c2a60a8106f37b12d5', '4个/盒', 7.90, 260, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('2d015bdad7cf425da8caec5f1500da29', 'g029', 'ccb813fff7784577be360fdfad87d608', '520g', 13.90, 300, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('2d2282cc55ab4dc484d93e8dd128168f', 'g004', '1b3a5725557049ef8c932ecfa79e3e76', '5斤装(J级)', 345.00, 4, '2026-08-06 09:34:05');
INSERT INTO `goods_sku` VALUES ('3a57a01cf8d94478a74f5ffea4fd7df8', 'g023', '705655dd2240456ba7f8bd2f911ce740', '瓶装 500ml×12瓶', 39.90, 100, '2026-08-05 14:10:37');
INSERT INTO `goods_sku` VALUES ('3af3e4513a224cb5b4cc45655cc3fa45', 'g030', 'b17423fe016b4026911240e359aac034', '3kg/瓶', 34.90, 150, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('3b52081bd3cb410a9dd0bfea9cf4f505', 'g008', '1dd1a910c12346c9bab6742efbc1b946', '500g/份', 8.90, 180, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('3bfa3524ca684949a18eac291fe43076', 'g016', '29761165bd4d49d696840b44fb812171', '60枚装', 18.00, 80, '2026-08-05 14:08:22');
INSERT INTO `goods_sku` VALUES ('544a297605d14f9aa481d6df5d93060e', 'g010', 'efb6bdef90754b9ab5efdd520d59046f', '500g/份', 11.90, 220, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('554963271e7b459abbd5a4f47d37213a', 'g022', '7e58203f61104d7994595a37fa6dfbbe', '98g/盒', 24.90, 180, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('5a9c90fd02a143138e07c0efe8776ae9', 'g016', 'cad2590605814f55ad9e2b57f8e88712', '30枚装', 8.00, 100, '2026-08-05 14:08:22');
INSERT INTO `goods_sku` VALUES ('5b713929da2c42ecbc2e35edd72b7760', 'g011', '53ef0353d11b4cc5b01e42279ccfb199', '五花肉（肥瘦相间）', 29.90, 50, '2026-08-05 14:06:23');
INSERT INTO `goods_sku` VALUES ('5fc175a0c6094a9e8fd8de7525e03b5e', 'g020', 'b29884855df54892a69160ab7b600640', '家庭款 1350g（54包）', 20.00, 20, '2026-08-05 14:09:38');
INSERT INTO `goods_sku` VALUES ('6048a94b442a41d2b662ab2806384148', 'g003', 'ad6e96a25bd44af1bd1e69a8269056d1', '整个', 29.90, 257, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('69e4266e15644d668cdf5b4827960cc1', 'g031', '2983afd2e6544a76a8b4c848512704e1', '1.5kg', 13.90, 280, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('6fceed5ca1c7423482b23b5939fae823', 'g020', '016763fbdf804973ab9988257d7888ea', '经典款 750g（30包）', 29.90, 120, '2026-08-05 14:09:38');
INSERT INTO `goods_sku` VALUES ('812cdab052894ec39a8db68a261c90a5', 'g032', '09064954542f418889e5c3429775faf8', '24包/提', 36.90, 180, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('844ea6dcadf44a7c838bd3fedaf5540c', 'g028', 'eb908fbd7295409db83a3ab26dd8d82e', '1L 家庭装', 5.00, 130, '2026-08-05 14:12:01');
INSERT INTO `goods_sku` VALUES ('8501de8f61f74b3a9697400cf64092b7', 'g021', '1278dad8bdeb4c8d8ea1fa8eb44d4e07', '97g/包', 11.90, 400, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('873cc221327a4fe0a64c6a7f1b8a8c68', 'g020', '0589f9592114473badd79f6ff5008b27', '升级款 900g（36包）', 8.00, 60, '2026-08-05 14:09:38');
INSERT INTO `goods_sku` VALUES ('8c800e7404d2451abbe206e863e4498e', 'g001', '86bf72b8ad644a02aed41b7fd27ca92a', '1', 12.90, 0, '2026-10-16 11:19:39');
INSERT INTO `goods_sku` VALUES ('9a15759dca1b4db5b06bcec1ff78fd21', 'g026', 'b097792ee52440ae928d7218c3412112', '5kg/袋', 24.90, 250, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('9ecb9f6964634e73a9e8422a9891e1f8', 'g004', '80f615b92b5543fcbfe3cecae556b2e2', '1斤装(J级)', 69.90, 95, '2026-08-06 09:34:05');
INSERT INTO `goods_sku` VALUES ('aecc08eaa93a4ff0a0a8607a736611ae', 'g028', '5530bcaa5993439681cd91f24afd1b2b', '500ml 小瓶装', 8.90, 200, '2026-08-05 14:12:01');
INSERT INTO `goods_sku` VALUES ('b64c59cf9e914c489d3af0024ce5e665', 'g023', '7cdc194bebb847d582971eea689b6fec', '迷你罐 200ml×48罐', 8.00, 50, '2026-08-05 14:10:37');
INSERT INTO `goods_sku` VALUES ('ba293f2c92a94c2db4aed0ce5993e934', 'g015', '39b11d3cfa434bc1b5ec7c036331c451', '250g/份', 98.90, 30, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('ba7a66b616c2463595f0a4d9276dc85f', 'g033', '3fc2f160169e4ad6a7c0b6b5f851dc45', '10卷/提', 19.90, 220, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('be8063dafd8b441b86afbb47045f60b3', 'g014', '257d0df11b52432d98f9e70743cf1ef5', '500g/份', 68.90, 40, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('c3f91133b958493bbbddc1738b6028d2', 'g004', '1ccb2e333a334195a8c793c930573481', '2斤装(J级)', 138.00, 50, '2026-08-06 09:34:05');
INSERT INTO `goods_sku` VALUES ('c457d1edd69a46d2a76b40bcd71ea49a', 'g019', '911a710f54204d03b33ca36e1eb38fcb', '308g/包', 9.90, 600, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('cb486a7ded804b0da46dd10d735ea8c7', 'g025', 'e8e6485b023e41b8957247f44f15144b', '5kg/袋', 49.90, 200, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('cba56f1bcca54f31b7f87d86f6da2b3e', 'g028', '6146925848e54f5a85100d8fd841f75d', '1.9L 大桶装', 10.00, 70, '2026-08-05 14:12:01');
INSERT INTO `goods_sku` VALUES ('cef018b1fddc4db3b8fdfa5eb63e9b5c', 'g011', '9f7aae7cc0ac4d4c9ced9306058fdc21', '后腿肉（炖煮首选）', 1.00, 30, '2026-08-05 14:06:23');
INSERT INTO `goods_sku` VALUES ('d50e82840987498dbe8ddd307f7caf0d', 'g017', '8b79d5f5597a47ba9f2be52aefb98d4b', '块', 3.50, 150, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('d5788dee268d4e0b84ab1a4e8177f5b7', 'g036', '0b8dfc15340a48bfb3b98305de82bd99', '个', 2.50, 200, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('db09882520ba425988ff2f03db0da7f0', 'g034', '6f2a2a34c46f4622bc8f7e74ed4fb444', '只', 45.90, 50, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('dc327ddc284f408ca69b09013b76e536', 'g018', '734a0bb2bd114839b04dc348e75bf866', '24盒/箱', 49.90, 500, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('dcf43e675c7b4ae79401265df333e423', 'g023', 'fe46f36997924241aaa1e162b5f3898e', '听装 330ml×24罐', 39.90, 200, '2026-08-05 14:10:37');
INSERT INTO `goods_sku` VALUES ('dfd4238a7b48437d9adf347727f9d4dd', 'g011', 'f5f29aac73184315af1564ef6dd8967e', '前腿肉（瘦肉多）', 2.00, 40, '2026-08-05 14:06:23');
INSERT INTO `goods_sku` VALUES ('e30ce301962e41068c74f0189d5ce130', 'g012', 'c71830fa25064188b97d52302fe5e740', '500g/份', 49.90, 80, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('e655f0768f7c4473b7d471970a453c7b', 'g001', '571981d853494a2fbaa3fc6bb8935d7f', '500g/份', 12.90, 356, '2026-10-16 11:19:39');
INSERT INTO `goods_sku` VALUES ('f54ecaf2f44e4675a9df4f12580cec03', 'g007', 'be21fdf9610b4c4ba87fa93e11d2b436', '500g/份', 5.90, 350, '2026-08-06 09:03:16');
INSERT INTO `goods_sku` VALUES ('f6b7e9da0c5840dea4665593dd3f1d60', 'g006', '462b4d200f9e4a24b06480c6f5d466ae', '300g/份', 4.90, 199, '2026-08-11 09:32:55');
INSERT INTO `goods_sku` VALUES ('f8438924baca4e4b9a91be3ceb52d1ba', 'g024', 'cf0d239dc53448d3b9f80b21c65fe1e2', '500ml', 2.90, 800, '2026-08-06 09:03:16');

-- ----------------------------
-- Table structure for logistics_company
-- ----------------------------
DROP TABLE IF EXISTS `logistics_company`;
CREATE TABLE `logistics_company`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `company_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '物流公司名称',
  `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '物流编码',
  `phone` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '联系电话',
  `is_ban` int NULL DEFAULT 0 COMMENT '是否禁用',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '物流公司' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of logistics_company
-- ----------------------------
INSERT INTO `logistics_company` VALUES ('lc_001', '顺丰速运', 'WL0001', '95338', 0, '2026-07-30 10:10:21');
INSERT INTO `logistics_company` VALUES ('lc_002', '圆通速递', 'WL0002', '95554', 0, '2026-07-30 10:10:21');

-- ----------------------------
-- Table structure for member_address
-- ----------------------------
DROP TABLE IF EXISTS `member_address`;
CREATE TABLE `member_address`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `user_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '所属消费者标识（登录凭据code，与订单merchant_id一致）',
  `name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '收件人',
  `mobile` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '收件手机号',
  `province` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '省份',
  `city` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '城市',
  `district` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '区/县',
  `detail_addr` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '详细地址',
  `is_default` int NULL DEFAULT 0 COMMENT '0普通 1默认地址',
  `create_time` datetime NULL DEFAULT NULL,
  `update_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_member_user_id`(`user_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '消费者收货地址表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of member_address
-- ----------------------------
INSERT INTO `member_address` VALUES ('679deb5631cc44e7bb1cf8b3ac72b675', 'm_0001', '张三', '13800000001', '广东省', '深圳市', '南山区', '科技园南路1号', 1, '2026-09-15 18:40:05', '2026-09-15 18:40:05');

-- ----------------------------
-- Table structure for merchant_bank
-- ----------------------------
DROP TABLE IF EXISTS `merchant_bank`;
CREATE TABLE `merchant_bank`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `merchant_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商户ID',
  `bank_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '开户行',
  `bank_account` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '银行账号',
  `account_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '开户名',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_sb_merchant`(`merchant_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商户银行账户' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of merchant_bank
-- ----------------------------
INSERT INTO `merchant_bank` VALUES ('sb_001', 'sp_0001', '中国工商银行', '6222021234567890123', '孙瑾', '2026-07-30 10:10:21');
INSERT INTO `merchant_bank` VALUES ('sb_002', 'sp_0002', '建设银行', '6217xxxxxxxx102', '金谷粮油供应', '2026-07-30 10:10:21');

-- ----------------------------
-- Table structure for merchant_bill
-- ----------------------------
DROP TABLE IF EXISTS `merchant_bill`;
CREATE TABLE `merchant_bill`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `bill_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `order_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '关联订单编号',
  `merchant_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '供货商户ID',
  `goods_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT '' COMMENT '商品名称',
  `buy_num` int NULL DEFAULT 0 COMMENT '购入数量',
  `bill_type` int NULL DEFAULT 1 COMMENT '1货款 2服务费 3运费 4其他',
  `period_start` datetime NULL DEFAULT NULL COMMENT '计费起期',
  `period_end` datetime NULL DEFAULT NULL COMMENT '计费止期',
  `amount` decimal(18, 2) NULL DEFAULT 0.00 COMMENT '支付金额',
  `order_status` int NULL DEFAULT 0 COMMENT '0待付款 1已付款 2已发货 3已完成 4已取消 5退款中 6已退款 7已驳回',
  `paid_amount` decimal(18, 2) NULL DEFAULT NULL COMMENT '已付金额',
  `pay_status` int NULL DEFAULT 0 COMMENT '0待付 1已付 2部分付',
  `pay_type` int NULL DEFAULT NULL COMMENT '1微信 2支付宝 3银行卡 4线下',
  `pay_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '支付流水号',
  `pay_time` datetime NULL DEFAULT NULL COMMENT '付款时间',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `create_time` datetime NULL DEFAULT NULL,
  `update_time` datetime NULL DEFAULT NULL,
  `client_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '买家用户ID（sys_users.u_id）',
  `goods_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商品ID',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uk_merchant_bill_no`(`bill_no` ASC) USING BTREE,
  INDEX `idx_merchant_bill_merchant`(`merchant_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商户账单（应付）' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of merchant_bill
-- ----------------------------
INSERT INTO `merchant_bill` VALUES ('1381105c347e4d96a159c9eba9e3b383', 'GYS20260916-002', 'ORD2609160002', 'sp_0001', '泰国金枕头榴莲', 10, 1, NULL, NULL, 299.00, 4, NULL, 1, NULL, NULL, NULL, NULL, '2026-09-16 08:46:10', '2026-09-16 09:28:54', 'm_0001', 'g003');
INSERT INTO `merchant_bill` VALUES ('4762a1c982524086b2bd160307cad9f5', 'GYS20260916-001', 'ORD2609150003', 'sp_0001', '海南金煌芒果', 1, 1, NULL, NULL, 9.90, 4, NULL, 1, NULL, NULL, NULL, NULL, '2026-09-16 08:40:15', '2026-09-16 08:58:09', 'm_0001', 'g005');
INSERT INTO `merchant_bill` VALUES ('5eed11bf66f94f299f2d2bb69329d84e', 'GYS20260915-001', 'ORD2609150002', 'sp_0001', '云南高原红富士苹果', 1, 1, NULL, NULL, 12.90, 4, NULL, 1, NULL, NULL, NULL, NULL, '2026-09-15 18:36:32', '2026-09-15 18:36:32', 'm_0001', 'g001');
INSERT INTO `merchant_bill` VALUES ('7acd60ca3de349f6860c00ca84d389d7', 'GYS20261014-002', 'ORD2610140007', 'sp_0001', '挪威三文鱼刺身', 1, 1, NULL, NULL, 98.90, 4, NULL, 1, NULL, NULL, NULL, NULL, '2026-10-14 11:25:07', '2026-10-14 11:25:07', 'm_0001', 'g015');
INSERT INTO `merchant_bill` VALUES ('94bea3540f614e6cbecca36b7c0706c3', 'GYS20261017-002', 'ORD2610140003', 'sp_0001', '挪威三文鱼刺身', 1, 1, NULL, NULL, 98.90, 2, NULL, 1, NULL, NULL, NULL, NULL, '2026-10-17 10:49:14', '2026-10-17 10:49:14', 'm_0001', 'g015');
INSERT INTO `merchant_bill` VALUES ('b80624d721e543f589a908dc9ea1852b', 'GYS20261017-003', 'ORD2610140002', 'sp_0001', '云南高原红富士苹果', 1, 1, NULL, NULL, 12.90, 2, NULL, 1, NULL, NULL, NULL, NULL, '2026-10-17 14:02:26', '2026-10-17 14:02:26', 'm_0001', 'g001');
INSERT INTO `merchant_bill` VALUES ('c37262f5260942ff84da9a5748c36a57', 'GYS20261014-001', 'ORD2610140001', 'sp_0001', '云南高原红富士苹果', 2, 1, NULL, NULL, 25.80, 4, NULL, 1, NULL, NULL, NULL, NULL, '2026-10-14 11:03:44', '2026-10-14 11:03:44', 'm_0001', 'g001');
INSERT INTO `merchant_bill` VALUES ('c8a4eb4c3d4442b49db5a3a351d79562', 'GYS20261017-001', 'ORD2609160005', 'sp_0001', '云南高原红富士苹果', 2, 1, NULL, NULL, 25.80, 2, NULL, 1, NULL, NULL, NULL, NULL, '2026-10-17 10:47:55', '2026-10-17 10:47:55', 'm_0001', 'g001');

-- ----------------------------
-- Table structure for merchant_good
-- ----------------------------
DROP TABLE IF EXISTS `merchant_good`;
CREATE TABLE `merchant_good`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `merchant_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商户ID',
  `goods_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商品ID(goods_info.id)',
  `supply_price` decimal(18, 2) NULL DEFAULT NULL COMMENT '供货价',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_sg_merchant`(`merchant_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商户商品' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of merchant_good
-- ----------------------------
INSERT INTO `merchant_good` VALUES ('535f5f35877f412c9c7beb86bc46e5c9', 'sp_0001', '24d9556f13fe4e80a7bd7c88edbc205a', NULL, '2026-09-14 11:46:26');
INSERT INTO `merchant_good` VALUES ('cce7cbf077404bd587917d5c82593919', 'sp_0001', '296b8d78ceef4b1ca99c3a001a66e6cf', 0.00, '2026-09-16 09:40:27');
INSERT INTO `merchant_good` VALUES ('sg_gen_1001', 'sp_0001', 'g001', 9.03, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1002', 'sp_0001', 'g003', 20.93, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1003', 'sp_0001', 'g004', 48.93, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1004', 'sp_0001', 'g005', 6.93, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1005', 'sp_0001', 'g006', 3.43, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1006', 'sp_0001', 'g007', 4.13, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1007', 'sp_0001', 'g008', 6.23, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1008', 'sp_0001', 'g009', 5.53, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1009', 'sp_0001', 'g010', 8.33, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1010', 'sp_0001', 'g011', 20.93, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1011', 'sp_0001', 'g012', 34.93, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1012', 'sp_0001', 'g013', 25.13, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1013', 'sp_0001', 'g014', 48.23, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1014', 'sp_0001', 'g015', 69.23, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1015', 'sp_0001', 'g016', 11.83, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1016', 'sp_0001', 'g017', 2.45, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1017', 'sp_0001', 'g018', 34.93, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1018', 'sp_0002', 'g019', 6.93, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1019', 'sp_0002', 'g020', 20.93, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1020', 'sp_0002', 'g021', 8.33, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1021', 'sp_0002', 'g022', 17.43, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1022', 'sp_0002', 'g023', 27.93, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1023', 'sp_0002', 'g024', 2.03, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1024', 'sp_0002', 'g025', 34.93, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1025', 'sp_0002', 'g026', 17.43, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1026', 'sp_0002', 'g027', 90.30, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1027', 'sp_0002', 'g028', 6.23, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1028', 'sp_0002', 'g029', 9.73, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1029', 'sp_0002', 'g030', 24.43, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1030', 'sp_0002', 'g031', 9.73, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1031', 'sp_0002', 'g032', 25.83, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sg_gen_1032', 'sp_0002', 'g033', 13.93, '2026-08-08 22:11:00');
INSERT INTO `merchant_good` VALUES ('sgf6bbc651855c', 'sp_0001', 'g037', 1.00, '2026-08-13 16:26:41');

-- ----------------------------
-- Table structure for merchant_level
-- ----------------------------
DROP TABLE IF EXISTS `merchant_level`;
CREATE TABLE `merchant_level`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `level_name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '等级名称',
  `sort` int NULL DEFAULT 0 COMMENT '排序',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商户等级' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of merchant_level
-- ----------------------------
INSERT INTO `merchant_level` VALUES ('sl_001', '战略供应商', 1, '核心供应商', '2026-07-30 10:10:21');
INSERT INTO `merchant_level` VALUES ('sl_002', '普通供应商', 2, '一般合作', '2026-07-30 10:10:21');

-- ----------------------------
-- Table structure for merchant_login
-- ----------------------------
DROP TABLE IF EXISTS `merchant_login`;
CREATE TABLE `merchant_login`  (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `merchant_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '商户ID',
  `account` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `avatar` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商户头像图片地址',
  `password` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `salt` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '密码盐值',
  `create_time` datetime NULL DEFAULT CURRENT_TIMESTAMP,
  `merchant_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商户名称',
  `is_ban` int NOT NULL DEFAULT 0 COMMENT '是否禁用 0-正常 1-禁用',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uk_merchant`(`merchant_id` ASC) USING BTREE,
  UNIQUE INDEX `uk_merchant_account`(`account` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 8 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商户登录账号' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of merchant_login
-- ----------------------------
INSERT INTO `merchant_login` VALUES (1, 'sp_0001', '12500000001', NULL, '$argon2id$v=19$m=19456,t=2,p=1$vmV47tDPxovrvlqgFM9ajg$vH+Eq9o1/ZkXTb1fcc8WGZZIxRbJaiNUvYKYj6dGBlc', 'cad7dac1', '2026-08-08 22:22:20', '绿野果蔬', 0);
INSERT INTO `merchant_login` VALUES (2, 'sp_0002', '14700000002', NULL, '$argon2id$v=19$m=19456,t=2,p=1$vmV47tDPxovrvlqgFM9ajg$vH+Eq9o1/ZkXTb1fcc8WGZZIxRbJaiNUvYKYj6dGBlc', 'cad7f637', '2026-08-08 22:22:20', '金谷粮油', 0);

-- ----------------------------
-- Table structure for merchant_qualification
-- ----------------------------
DROP TABLE IF EXISTS `merchant_qualification`;
CREATE TABLE `merchant_qualification`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `merchant_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商户ID',
  `qual_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '资质类型',
  `qual_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '资质编号',
  `file_url` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '资质文件',
  `expire_date` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '到期日期',
  `is_ban` int NULL DEFAULT 0 COMMENT '是否作废',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_sq_merchant`(`merchant_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商户资质' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of merchant_qualification
-- ----------------------------
INSERT INTO `merchant_qualification` VALUES ('sq_001', 'sp_0001', '食品经营许可证', 'SP2026-001', '/api/file/4704383e29354a10b3c543444c9b9f91.jpg', '2027-12-31', 0, '2026-08-04 15:20:44');
INSERT INTO `merchant_qualification` VALUES ('sq_002', 'sp_0002', '食品生产许可证', 'SC2026-002', '/uploads/qualifications/d9a8def7037c40a0bef13d0e95120fc0.jpg', '2027-06-01', 0, '2026-07-30 10:10:21');
INSERT INTO `merchant_qualification` VALUES ('sq_003', 'sp_0001', '有机产品认证', 'ORG2026-003', '/api/file/e7dee3a0d8314f47b23b99011f944e92.webp', '2028-03-15', 0, '2026-08-04 14:25:39');

-- ----------------------------
-- Table structure for merchant_withdraw
-- ----------------------------
DROP TABLE IF EXISTS `merchant_withdraw`;
CREATE TABLE `merchant_withdraw`  (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `merchant_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '商户ID',
  `amount` decimal(18, 2) NOT NULL DEFAULT 0.00,
  `bank_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `bank_account` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `account_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `status` int NOT NULL DEFAULT 1,
  `create_time` datetime NULL DEFAULT CURRENT_TIMESTAMP,
  `out_transfer_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商户转账单号（本地生成，全局唯一）',
  `transfer_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '微信转账单号',
  `transfer_status` int NOT NULL DEFAULT 0 COMMENT '0未发起 1受理中 2成功 3失败',
  `fail_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '转账失败原因',
  `transfer_time` datetime NULL DEFAULT NULL COMMENT '转账完成时间',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_sw_out_transfer_no`(`out_transfer_no` ASC) USING BTREE,
  INDEX `idx_sw_merchant`(`merchant_id` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 26 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '商户提现记录' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of merchant_withdraw
-- ----------------------------
INSERT INTO `merchant_withdraw` VALUES (22, 'sp_0001', 100.00, '中国工商银行', '6222021234567890123', '孙瑾', '', 2, '2026-09-16 09:50:25', NULL, NULL, 0, NULL, NULL);

-- ----------------------------
-- Table structure for order_delivery
-- ----------------------------
DROP TABLE IF EXISTS `order_delivery`;
CREATE TABLE `order_delivery`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `order_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '订单ID',
  `logistics_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '物流公司ID',
  `tracking_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '运单号',
  `delivery_status` int NULL DEFAULT 0 COMMENT '0待发 1已发 2已签收',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_od_order`(`order_id` ASC) USING BTREE,
  INDEX `fk_order_delivery_logistics_id`(`logistics_id` ASC) USING BTREE,
  CONSTRAINT `fk_order_delivery_logistics_id` FOREIGN KEY (`logistics_id`) REFERENCES `logistics_company` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_order_delivery_order_id` FOREIGN KEY (`order_id`) REFERENCES `clientorders` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '订单配送' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of order_delivery
-- ----------------------------

-- ----------------------------
-- Table structure for order_event_log
-- ----------------------------
DROP TABLE IF EXISTS `order_event_log`;
CREATE TABLE `order_event_log`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `event_id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '消息事件ID，与RabbitMQ消息对账',
  `event_type` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '事件类型',
  `order_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `order_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `biz_key` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '业务幂等键，如 settle:{orderId}',
  `status` int NULL DEFAULT 0 COMMENT '0处理中 1已处理 2失败',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `create_time` datetime NULL DEFAULT NULL,
  `update_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uk_biz_key`(`biz_key` ASC) USING BTREE,
  INDEX `idx_event_order`(`order_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '订单消息事件日志（MQ消费幂等与审计）' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of order_event_log
-- ----------------------------
INSERT INTO `order_event_log` VALUES ('468476bdffe04851add6f0c116eb60a0', '6f8784530cd14f29a44557f0dcb4053f', 'order.completed', '79fda0d10c28424db318863fb41bbdac', 'ORD2610140001', 'settle:79fda0d10c28424db318863fb41bbdac', 1, '结算单 JS20261014-01', '2026-10-14 11:03:44', '2026-10-14 11:03:44');
INSERT INTO `order_event_log` VALUES ('56761afd87ed4ad8a8f8228aa9dd8bb0', 'f951a5ca649b4b6a94786a318dcaa0d9', 'order.created', '5c3733cd5efe46508aa2a22305887514', 'ORD2610140004', 'stock-alert:5c3733cd5efe46508aa2a22305887514', 1, '库存充足，无预警', '2026-10-14 11:07:59', '2026-10-14 11:07:59');
INSERT INTO `order_event_log` VALUES ('5e205e4c0e2a42ef8b8fcfdd0a50d4a4', '6c9295179c3d4ab7b259b15cef3fbc83', 'order.created', '79fda0d10c28424db318863fb41bbdac', 'ORD2610140001', 'stock-alert:79fda0d10c28424db318863fb41bbdac', 1, '库存充足，无预警', '2026-10-14 11:03:39', '2026-10-14 11:03:39');
INSERT INTO `order_event_log` VALUES ('7004e5744d1249d3ab657466645dbb5f', '9d225df479024cdd84c1583d0b3a1093', 'order.created', '69f8800f693348dc8da50667017a4ca9', 'ORD2610140005', 'stock-alert:69f8800f693348dc8da50667017a4ca9', 1, '库存充足，无预警', '2026-10-14 11:09:09', '2026-10-14 11:09:09');
INSERT INTO `order_event_log` VALUES ('7321685b31044ef68dd67a6d51bc2c5c', 'a677929ef5f84e2aacd84e7fc21faa20', 'order.created', '45fe1edffc1e4af7b98273d67844105a', 'ORD2610140003', 'stock-alert:45fe1edffc1e4af7b98273d67844105a', 1, '1 个商品低库存', '2026-10-14 11:03:47', '2026-10-14 11:03:47');
INSERT INTO `order_event_log` VALUES ('799928932ec142feaa1513e8f6ed2d5c', '7c4ec1efa1f84f0ea99629b694430e56', 'order.created', '30a374d1fe9f4c618c7366b4cab4398c', 'ORD2610140006', 'stock-alert:30a374d1fe9f4c618c7366b4cab4398c', 1, '库存充足，无预警', '2026-10-14 11:12:59', '2026-10-14 11:12:59');
INSERT INTO `order_event_log` VALUES ('8746da28d4c4450791101de4c99248fe', '7acc7d7629044d65a01e94cdb1c117cb', 'order.created', '743e4810a92845c98de8aad67a42bd7b', 'ORD2610140002', 'stock-alert:743e4810a92845c98de8aad67a42bd7b', 1, '库存充足，无预警', '2026-10-14 11:03:47', '2026-10-14 11:03:47');
INSERT INTO `order_event_log` VALUES ('cd2df3aa0f1f45bbbf08fd55a86d9819', '344cb7ad543f4ce8a2ad2b2f38cfb930', 'order.completed', '3bc5dc22b5584d6987f1de03ecdf7ca9', 'ORD2610140007', 'settle:3bc5dc22b5584d6987f1de03ecdf7ca9', 1, '结算单 JS20261014-02', '2026-10-14 11:25:07', '2026-10-14 11:25:07');

-- ----------------------------
-- Table structure for order_freight
-- ----------------------------
DROP TABLE IF EXISTS `order_freight`;
CREATE TABLE `order_freight`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `freight_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '运费单号 FR+yyMMddHHmmss+随机',
  `order_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '关联订单ID',
  `order_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '关联订单编号',
  `user_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '下单用户ID',
  `merchant_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商户ID',
  `goods_amount` decimal(18, 2) NULL DEFAULT 0.00 COMMENT '商品金额（运费计费基数）',
  `item_count` int NULL DEFAULT 0 COMMENT '计费件数',
  `freight_amount` decimal(18, 2) NULL DEFAULT 0.00 COMMENT '运费金额',
  `charge_type` int NULL DEFAULT 1 COMMENT '计费方式 1按件计费 2满额包邮',
  `charge_type_text` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '计费方式文案',
  `charge_rule` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '计费规则说明',
  `free_shipping` int NULL DEFAULT 0 COMMENT '是否包邮 0否 1是',
  `total_amount` decimal(18, 2) NULL DEFAULT 0.00 COMMENT '应付合计=商品金额+运费金额',
  `pay_status` int NULL DEFAULT 0 COMMENT '运费支付状态 0未支付 1已支付',
  `pay_time` datetime NULL DEFAULT NULL COMMENT '运费支付时间',
  `effective_time` datetime NULL DEFAULT NULL COMMENT '生效时间',
  `is_ban` int NULL DEFAULT 0 COMMENT '是否作废 0正常 1作废',
  `remark` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `create_time` datetime NULL DEFAULT NULL,
  `update_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uk_of_no`(`freight_no` ASC) USING BTREE,
  INDEX `idx_of_order`(`order_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '订单运费单' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of order_freight
-- ----------------------------
INSERT INTO `order_freight` VALUES ('f94a2a4df66d4c1e8abf3280ffbed369', 'FR20260916113905188', 'a92484fae614486991750697e3142cd8', 'ORD2609160005', 'm_0001', 'sp_0001', 25.80, 2, 10.00, 1, '按件计费', '首件 ¥8.00，续件 ¥2.00/件，共 2 件', 0, 35.80, 1, '2026-09-16 11:39:07', '2026-09-16 11:39:07', 0, NULL, '2026-09-16 11:39:05', '2026-09-16 11:39:07');

-- ----------------------------
-- Table structure for order_pay
-- ----------------------------
DROP TABLE IF EXISTS `order_pay`;
CREATE TABLE `order_pay`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `order_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '订单ID',
  `pay_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '支付流水号',
  `pay_amount` decimal(18, 2) NULL DEFAULT NULL COMMENT '支付金额',
  `pay_type` int NULL DEFAULT NULL COMMENT '支付方式',
  `pay_status` int NULL DEFAULT 0 COMMENT '0未付 1已付',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_op_order`(`order_id` ASC) USING BTREE,
  CONSTRAINT `fk_order_pay_order_id` FOREIGN KEY (`order_id`) REFERENCES `clientorders` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '订单支付' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of order_pay
-- ----------------------------
INSERT INTO `order_pay` VALUES ('3c000e3736d64d0bb482c42d9d4bfb24', '3bc5dc22b5584d6987f1de03ecdf7ca9', 'PAY202610141120200593902', 98.90, 11, 1, '2026-10-14 11:20:20');
INSERT INTO `order_pay` VALUES ('ca0c2368e13f46e383cd217e6350c034', 'f1d226eb5ed744049bd3848df2444575', 'PAY202609161122140726306', 345.00, 11, 1, '2026-09-16 11:22:14');
INSERT INTO `order_pay` VALUES ('d0924384e1a8473d99d02ae352864aae', '79fda0d10c28424db318863fb41bbdac', 'PAY202610141103417262617', 25.80, 11, 1, '2026-10-14 11:03:42');
INSERT INTO `order_pay` VALUES ('d4d095480b9447588e736c442aea61d6', 'c34c684fda284ef5913fde432bbb4ff0', 'PAY202609161102404642263', 345.00, 11, 1, '2026-09-16 11:02:40');

-- ----------------------------
-- Table structure for order_refund
-- ----------------------------
DROP TABLE IF EXISTS `order_refund`;
CREATE TABLE `order_refund`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `order_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '订单ID',
  `refund_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '退款单号',
  `refund_amount` decimal(18, 2) NULL DEFAULT NULL COMMENT '退款金额',
  `refund_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '退款原因',
  `refund_status` int NULL DEFAULT 0 COMMENT '0待退款 1已退款 2驳回',
  `create_time` datetime NULL DEFAULT NULL COMMENT '申请时间',
  `user_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '申请人用户ID',
  `refund_type` int NULL DEFAULT 2 COMMENT '1仅退款 2退货退款',
  `refund_num` int NULL DEFAULT NULL COMMENT '退货数量',
  `goods_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '关联商品ID',
  `goods_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '关联商品名称',
  `goods_img` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '关联商品图片',
  `handle_remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '审核备注',
  `handle_time` datetime NULL DEFAULT NULL COMMENT '审核时间',
  `handle_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '审核人ID',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_orf_order`(`order_id` ASC) USING BTREE,
  CONSTRAINT `fk_order_refund_order_id` FOREIGN KEY (`order_id`) REFERENCES `clientorders` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '订单退款' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of order_refund
-- ----------------------------
INSERT INTO `order_refund` VALUES ('4ba8121b01b54fe39d16df2b17905c1e', 'c34c684fda284ef5913fde432bbb4ff0', 'RF20260916112015274871', 345.00, '待发货直接退款', 1, '2026-09-16 11:20:15', 'm_0001', 1, NULL, 'g004', '智利进口车厘子 JJ级', '/api/file/28afffcc6ae74fc18ddc086a1ff47efd.webp', NULL, '2026-09-16 11:20:15', NULL);
INSERT INTO `order_refund` VALUES ('c3149d0eeaf546c7a35243b07a490e4c', 'f1d226eb5ed744049bd3848df2444575', 'RF20260916112218173350', 345.00, '待发货直接退款', 1, '2026-09-16 11:22:18', 'm_0001', 1, NULL, 'g004', '智利进口车厘子 JJ级', '/api/file/28afffcc6ae74fc18ddc086a1ff47efd.webp', NULL, '2026-09-16 11:22:18', NULL);

-- ----------------------------
-- Table structure for settle_record
-- ----------------------------
DROP TABLE IF EXISTS `settle_record`;
CREATE TABLE `settle_record`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `client_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '买家用户ID（sys_users.u_id）',
  `settle_no` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '结算单号',
  `settle_amount` decimal(18, 2) NULL DEFAULT NULL COMMENT '结算金额',
  `settle_type` int NULL DEFAULT NULL COMMENT '1线上 2线下',
  `status` int NULL DEFAULT 0 COMMENT '0待结算 1已结算 2已退款',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uk_settle_no`(`settle_no` ASC) USING BTREE,
  INDEX `idx_sr_client`(`client_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '用户结算记录' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of settle_record
-- ----------------------------
INSERT INTO `settle_record` VALUES ('3f7eab2a22ea489db075963dd5a9653a', 'm_0001', 'JS20261014-01', 25.80, 1, 0, '2026-10-14 11:03:44');
INSERT INTO `settle_record` VALUES ('dfe798c4d32d496891677c2c6b9d5806', 'm_0001', 'JS20261014-02', 98.90, 1, 0, '2026-10-14 11:25:07');

-- ----------------------------
-- Table structure for sms_code
-- ----------------------------
DROP TABLE IF EXISTS `sms_code`;
CREATE TABLE `sms_code`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '短信验证码表',
  `mobile` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '手机号码',
  `code` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '验证码',
  `type` int NULL DEFAULT 3 COMMENT '验证码类型 1-商户 2-商户 3-管理员',
  `is_used` int NULL DEFAULT 0 COMMENT '是否已使用 0-否 1-是',
  `expire_time` datetime NULL DEFAULT NULL COMMENT '过期时间',
  `create_time` datetime NULL DEFAULT NULL COMMENT '创建时间',
  `ip` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '发送IP',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '短信验证码表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of sms_code
-- ----------------------------
INSERT INTO `sms_code` VALUES ('1498564b4a2d4528b8ba24492738a885', '13800009999', '466353', 3, 1, '2026-10-17 14:07:24', '2026-10-17 14:02:24', '::1');
INSERT INTO `sms_code` VALUES ('1abf12a9d824447f933298ac0fe35474', '13800001111', '573072', 3, 1, '2026-10-17 09:08:36', '2026-10-17 09:03:36', '::1');
INSERT INTO `sms_code` VALUES ('254ce91c82ed44e5aa46e2b5e5e38aaf', '11111111111', '493540', 1, 1, '2026-10-17 17:11:01', '2026-10-17 17:06:01', '::1');
INSERT INTO `sms_code` VALUES ('255908c54d004badbd9fe188213b54c6', '13800002222', '544093', 3, 1, '2026-10-17 09:08:35', '2026-10-17 09:03:35', '::1');
INSERT INTO `sms_code` VALUES ('305ced36a0b44004bd9bd8d8038c43b9', '13800002222', '361550', 3, 1, '2026-10-17 09:07:12', '2026-10-17 09:02:12', '::1');
INSERT INTO `sms_code` VALUES ('33ebb67a02974955a16cc667b733f891', '13800003333', '361489', 3, 1, '2026-10-17 09:44:51', '2026-10-17 09:39:51', '::1');
INSERT INTO `sms_code` VALUES ('3d0e2d36edbc452cbf4f875f73c3626b', '12232435445', '158976', 3, 0, '2026-10-17 15:34:08', '2026-10-17 15:29:08', '::1');
INSERT INTO `sms_code` VALUES ('7ae6e9f997f340de95a59c8855a5d346', '13800009999', '155290', 3, 1, '2026-10-17 10:52:54', '2026-10-17 10:47:54', '::1');
INSERT INTO `sms_code` VALUES ('81cb942a6f9e430594906d0fef18d5d6', '13800009999', '129486', 3, 1, '2026-10-17 10:47:00', '2026-10-17 10:42:00', '::1');
INSERT INTO `sms_code` VALUES ('82150b4c62a242f58f2bb5c8418096bf', '19332405533', '453381', 3, 1, '2026-10-17 09:15:08', '2026-10-17 09:10:08', '::1');
INSERT INTO `sms_code` VALUES ('88c9924abf4c4aa8bac53b180ccf885d', '13900000000', '743920', 3, 1, '2026-10-17 08:42:46', '2026-10-17 08:37:46', '::1');
INSERT INTO `sms_code` VALUES ('926b1193e564438cb23fabf6ce48db76', '13900001234', '761023', 3, 1, '2026-10-16 17:20:06', '2026-10-16 17:15:06', '::1');
INSERT INTO `sms_code` VALUES ('ac05d34dc9084fec93506d3e36144b0c', '13900001234', '497445', 3, 1, '2026-10-16 17:18:06', '2026-10-16 17:13:06', '::1');
INSERT INTO `sms_code` VALUES ('b0802ad6cad24c399fa53244965ed57a', '13800001111', '687293', 3, 1, '2026-10-17 09:05:26', '2026-10-17 09:00:26', '::1');
INSERT INTO `sms_code` VALUES ('bc9b3297e4794f5c8e1407341cbf1a6e', '13800009999', '567022', 3, 1, '2026-10-17 10:48:22', '2026-10-17 10:43:22', '::1');
INSERT INTO `sms_code` VALUES ('c5bee09eefa34a928d2720670648948d', '13900005678', '463676', 3, 1, '2026-10-16 17:18:37', '2026-10-16 17:13:37', '::1');
INSERT INTO `sms_code` VALUES ('ca90f0d74d0a4a458b89e2e572d7e777', '13583395459', '819150', 3, 1, '2026-10-17 09:49:46', '2026-10-17 09:44:46', '::1');

-- ----------------------------
-- Table structure for stock
-- ----------------------------
DROP TABLE IF EXISTS `stock`;
CREATE TABLE `stock`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `goods_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商品ID',
  `warehouse` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '仓库',
  `total_stock` int NULL DEFAULT 0 COMMENT '总库存',
  `lock_stock` int NULL DEFAULT 0 COMMENT '锁定库存',
  `create_time` datetime NULL DEFAULT NULL,
  `update_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_stock_goods`(`goods_id` ASC) USING BTREE,
  CONSTRAINT `fk_stock_goods_id` FOREIGN KEY (`goods_id`) REFERENCES `good` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '库存表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of stock
-- ----------------------------
INSERT INTO `stock` VALUES ('88608b1789994d14ac4dd7a029404653', 'g001', '北门仓', 362, 6, '2026-08-13 09:46:38', '2026-10-16 11:19:39');
INSERT INTO `stock` VALUES ('af51414150bb46399fa7bd3c55693721', 'g003', '北门仓', 267, 10, '2026-08-13 09:46:22', '2026-09-16 08:45:19');
INSERT INTO `stock` VALUES ('st_5e7a7009945711f199af28f52bf063a0', 'g004', NULL, 165, 17, '2026-08-10 09:03:59', '2026-10-14 11:07:57');
INSERT INTO `stock` VALUES ('st_5e7a7327945711f199af28f52bf063a0', 'g005', NULL, 399, 2, '2026-08-10 09:03:59', '2026-10-14 11:13:59');
INSERT INTO `stock` VALUES ('st_5e7a73a7945711f199af28f52bf063a0', 'g006', NULL, 199, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7407945711f199af28f52bf063a0', 'g007', NULL, 350, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7464945711f199af28f52bf063a0', 'g008', NULL, 180, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a74bc945711f199af28f52bf063a0', 'g009', NULL, 260, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7517945711f199af28f52bf063a0', 'g010', NULL, 220, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a756c945711f199af28f52bf063a0', 'g011', NULL, 120, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a75c1945711f199af28f52bf063a0', 'g012', NULL, 80, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7616945711f199af28f52bf063a0', 'g013', NULL, 60, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a766d945711f199af28f52bf063a0', 'g014', NULL, 40, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a76cb945711f199af28f52bf063a0', 'g015', NULL, 30, 2, '2026-08-10 09:03:59', '2026-10-14 11:19:36');
INSERT INTO `stock` VALUES ('st_5e7a7721945711f199af28f52bf063a0', 'g016', NULL, 300, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a77a3945711f199af28f52bf063a0', 'g017', NULL, 150, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a77e6945711f199af28f52bf063a0', 'g018', NULL, 500, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7829945711f199af28f52bf063a0', 'g019', NULL, 600, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7871945711f199af28f52bf063a0', 'g020', NULL, 200, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a78b7945711f199af28f52bf063a0', 'g021', NULL, 400, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a78fd945711f199af28f52bf063a0', 'g022', NULL, 180, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7940945711f199af28f52bf063a0', 'g023', NULL, 350, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7988945711f199af28f52bf063a0', 'g024', NULL, 800, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a79c9945711f199af28f52bf063a0', 'g025', NULL, 200, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7a0e945711f199af28f52bf063a0', 'g026', NULL, 250, 0, '2026-08-10 09:03:59', '2026-08-13 09:04:54');
INSERT INTO `stock` VALUES ('st_5e7a7a54945711f199af28f52bf063a0', 'g027', NULL, 78, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7a98945711f199af28f52bf063a0', 'g028', NULL, 400, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7add945711f199af28f52bf063a0', 'g029', NULL, 300, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7b20945711f199af28f52bf063a0', 'g030', NULL, 150, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7b62945711f199af28f52bf063a0', 'g031', NULL, 280, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7ba8945711f199af28f52bf063a0', 'g032', NULL, 180, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7beb945711f199af28f52bf063a0', 'g033', NULL, 220, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7c2e945711f199af28f52bf063a0', 'g034', NULL, 50, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7c72945711f199af28f52bf063a0', 'g035', NULL, 100, 0, '2026-08-10 09:03:59', NULL);
INSERT INTO `stock` VALUES ('st_5e7a7cb8945711f199af28f52bf063a0', 'g036', NULL, 200, 0, '2026-08-10 09:03:59', NULL);

-- ----------------------------
-- Table structure for stock_in
-- ----------------------------
DROP TABLE IF EXISTS `stock_in`;
CREATE TABLE `stock_in`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `goods_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商品ID',
  `in_num` int NULL DEFAULT 0 COMMENT '入库数量',
  `batch_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '批次号',
  `operator` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '操作人',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_si_goods`(`goods_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '入库记录' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of stock_in
-- ----------------------------

-- ----------------------------
-- Table structure for stock_out
-- ----------------------------
DROP TABLE IF EXISTS `stock_out`;
CREATE TABLE `stock_out`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `goods_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '商品ID',
  `out_num` int NULL DEFAULT 0 COMMENT '出库数量',
  `batch_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '批次号',
  `operator` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '操作人',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_so_goods`(`goods_id` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '出库记录' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of stock_out
-- ----------------------------

-- ----------------------------
-- Table structure for sys_department
-- ----------------------------
DROP TABLE IF EXISTS `sys_department`;
CREATE TABLE `sys_department`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `parent_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '上级部门',
  `dept_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '部门名称',
  `sort` int NULL DEFAULT 0 COMMENT '排序',
  `is_ban` int NULL DEFAULT 0 COMMENT '是否禁用',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '系统部门' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of sys_department
-- ----------------------------
INSERT INTO `sys_department` VALUES ('dept_001', NULL, '总部', 1, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_department` VALUES ('dept_002', 'dept_001', '运营部', 1, 0, '2026-07-30 10:10:21');

-- ----------------------------
-- Table structure for sys_dept_role
-- ----------------------------
DROP TABLE IF EXISTS `sys_dept_role`;
CREATE TABLE `sys_dept_role`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `department_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '部门ID',
  `role_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '角色ID',
  `is_ban` int NULL DEFAULT 0 COMMENT '是否禁用',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `fk_sys_dept_role_role_id`(`role_id` ASC) USING BTREE,
  CONSTRAINT `fk_sys_dept_role_role_id` FOREIGN KEY (`role_id`) REFERENCES `sys_role` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '部门角色绑定' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of sys_dept_role
-- ----------------------------
INSERT INTO `sys_dept_role` VALUES ('dr_001', 'dept_001', 'role_001', 0, '2026-07-30 10:10:21');
INSERT INTO `sys_dept_role` VALUES ('dr_staff', 'dept_002', 'role_staff', 0, '2026-08-06 14:20:41');

-- ----------------------------
-- Table structure for sys_dept_role_menu
-- ----------------------------
DROP TABLE IF EXISTS `sys_dept_role_menu`;
CREATE TABLE `sys_dept_role_menu`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `dept_role_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '部门角色ID',
  `menu_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '菜单ID',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_drm_dr`(`dept_role_id` ASC) USING BTREE,
  INDEX `fk_sys_dept_role_menu_menu_id`(`menu_id` ASC) USING BTREE,
  CONSTRAINT `fk_sys_dept_role_menu_dept_role_id` FOREIGN KEY (`dept_role_id`) REFERENCES `sys_dept_role` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_sys_dept_role_menu_menu_id` FOREIGN KEY (`menu_id`) REFERENCES `sys_menu` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '部门角色菜单权限' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of sys_dept_role_menu
-- ----------------------------
INSERT INTO `sys_dept_role_menu` VALUES ('01632ace7aa74c0bb11e12fd1d566a9c', 'dr_001', 'menu_604', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('119faa0447b145bbb30cf965baa9a6fd', 'dr_001', 'fb0087a7d8704bd6842d006dd3306b7f', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('16251f955e364a9a984307bcccf7b2f9', 'dr_001', 'menu_421', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('222ab45de327463bb08d3e42bc3ced34', 'dr_staff', 'fb0087a7d8704bd6842d006dd3306b7f', '2026-08-13 09:51:10');
INSERT INTO `sys_dept_role_menu` VALUES ('22c083e4bde941ccae6dac83ca4d940d', 'dr_staff', 'menu_607', '2026-08-13 09:51:10');
INSERT INTO `sys_dept_role_menu` VALUES ('23ab8d2538504ad9882eeba73b100b57', 'dr_001', 'menu_701', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('272b603e8fff49f3a634a833de5fd3e3', 'dr_001', 'menu_602', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('2a38d5c3e9d84d4ba26af503e1620446', 'dr_staff', 'menu_102', '2026-08-13 09:51:10');
INSERT INTO `sys_dept_role_menu` VALUES ('37280a99dd104e4f89eb1437e2b5fefe', 'dr_staff', 'menu_604', '2026-08-13 09:51:10');
INSERT INTO `sys_dept_role_menu` VALUES ('4311702f27c04af387a50b3a18687b69', 'dr_staff', 'menu_602', '2026-08-13 09:51:10');
INSERT INTO `sys_dept_role_menu` VALUES ('53b84aecdd5a44ac98eb8b71e0094664', 'dr_001', 'menu_603', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('5b3c681535ff4da185601f0e0e280d9f', 'dr_001', 'menu_704', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('682f7417f54546f481d1b555d5bf4912', 'dr_staff', 'menu_601', '2026-08-13 09:51:10');
INSERT INTO `sys_dept_role_menu` VALUES ('6fe707187ddb4db2b25dafd7123faeba', 'dr_001', 'menu_703', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('7ee866da61034e6f9fb81335e7fa9948', 'dr_staff', 'menu_802', '2026-08-13 09:51:10');
INSERT INTO `sys_dept_role_menu` VALUES ('81aebed7e4f944c7a1f21d5180817356', 'dr_staff', 'menu_103', '2026-08-13 09:51:10');
INSERT INTO `sys_dept_role_menu` VALUES ('8e44227953784705ab88a2f713a55ca7', 'dr_staff', 'menu_421', '2026-08-13 09:51:10');
INSERT INTO `sys_dept_role_menu` VALUES ('a6ce9890c9f24174a15a812cd5d7a611', 'dr_001', 'menu_srank', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('b0a2df2b7c3a4ee2b08aed0630d3c980', 'dr_001', 'menu_702', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('bc9b014a86bb43278d9a1be7372cbd2d', 'dr_001', 'menu_607a', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('c1894e3d39b84ef6af2ce99560d72863', 'dr_staff', 'menu_srank', '2026-08-13 09:51:10');
INSERT INTO `sys_dept_role_menu` VALUES ('c827200f709e425cb68036ca1fab732e', 'dr_001', 'menu_003b', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('db6654251c3144638d4fed116d05832d', 'dr_staff', 'menu_603', '2026-08-13 09:51:10');
INSERT INTO `sys_dept_role_menu` VALUES ('df4ed1b39ac94bd4a4a7d0cf02187c71', 'dr_001', 'menu_705', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('e15cd5d6d40e48d481540938966255d6', 'dr_001', 'menu_601', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('e207179611ec4f6387225e3d1a6f46c1', 'dr_001', 'menu_802', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('ea9facc43c6943d89051eb4e67e4d42f', 'dr_001', 'menu_103', '2026-08-18 16:03:37');
INSERT INTO `sys_dept_role_menu` VALUES ('f03703d86fd14c59b1e02279ffca7435', 'dr_001', 'menu_102', '2026-08-18 16:03:37');

-- ----------------------------
-- Table structure for sys_login_log
-- ----------------------------
DROP TABLE IF EXISTS `sys_login_log`;
CREATE TABLE `sys_login_log`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `user_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '关联用户ID(sys_users.u_id)',
  `account` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '登录账号(冗余)',
  `login_ip` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '登录IP地址',
  `login_address` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '登录地点',
  `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '登陆的凭据',
  `login_time` datetime NOT NULL COMMENT '登录时间',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '系统登录日志表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of sys_login_log
-- ----------------------------
INSERT INTO `sys_login_log` VALUES ('066899abeaca403ea96ad395c1bcd194', '9432473ed28b4126b2e274e373d53a0c', '13800009999', '也没有', '没有', 'a910b488892b4ce5928355e5b73694d0', '2026-10-17 14:02:25');
INSERT INTO `sys_login_log` VALUES ('1da1a7b373ca4f64a0650d6543473e9c', '84f6a9f12f4a41e5b3701426ead5647a', '13800002222', '也没有', '没有', 'e20bb30e96c84d8da16197711c840a7c', '2026-10-17 09:02:13');
INSERT INTO `sys_login_log` VALUES ('1ffbae996d77459a80af2e9bf5e31406', 'u_0001', '13700000001', '也没有', '没有', '2aa893d6da3a42028e6adcf4befc3c3f', '2026-10-18 13:22:43');
INSERT INTO `sys_login_log` VALUES ('26242e4f31e44d4d89c2c3e74c5aa24a', 'u_0001', '13700000001', '也没有', '没有', 'ec8af647c6e34d24b40bb9b5cc9dc0c4', '2026-10-17 16:40:36');
INSERT INTO `sys_login_log` VALUES ('2e99aacaa81546e6bcc96a161ab74f59', '42b4547c5cb746959f5cf0355e4e9903', '13900005678', '也没有', '没有', '612318a63a8c440f803228da9dbdc68a', '2026-10-16 17:13:37');
INSERT INTO `sys_login_log` VALUES ('398a586617fc4db8802c459f4becb888', 'u_0001', '13700000001', '也没有', '没有', 'fc1c1182e0c04853beeb28554c8ffbe7', '2026-10-16 16:20:40');
INSERT INTO `sys_login_log` VALUES ('3a36e3ccfda74b299d50afe126878bd2', 'u_0001', '13700000001', '也没有', '没有', '6c539f2137794a31aa961f8244018800', '2026-10-18 13:39:15');
INSERT INTO `sys_login_log` VALUES ('3dee28fa811f423083d6c1c07b601323', 'u_0001', '13700000001', '也没有', '没有', '59796ced82084ab1843a36760c0881da', '2026-10-16 14:59:10');
INSERT INTO `sys_login_log` VALUES ('451cfad2bddc4ef8a51526d1d6db215c', 'u_0001', '13700000001', '也没有', '没有', '4a24789f6f39493c80ae846055118414', '2026-09-14 14:04:42');
INSERT INTO `sys_login_log` VALUES ('46dd3485d2064d62831c9e315aec755d', 'u_0001', '13700000001', '也没有', '没有', '799c698cffe243a7867d78472b10e6aa', '2026-10-17 14:07:59');
INSERT INTO `sys_login_log` VALUES ('5dda16b5002d406980d98a7727ca866c', '479b927d83d14267bc4b8840297d638b', '13800000099', '也没有', '没有', '29f4764acf4149e7a11600089818bf78', '2026-10-17 15:16:31');
INSERT INTO `sys_login_log` VALUES ('68e8c1babf154d05b8d2f907d047186d', 'u_0001', '13700000001', '也没有', '没有', '01a955d1fc9a48bab4a5b56b0f614365', '2026-09-14 16:30:27');
INSERT INTO `sys_login_log` VALUES ('734e7b41b0594127992e88f6a14cc822', '9432473ed28b4126b2e274e373d53a0c', '13800009999', '也没有', '没有', 'f92d38b5d8064f3a9a51190519a91176', '2026-10-17 10:48:47');
INSERT INTO `sys_login_log` VALUES ('78416a667d3f48a9b3a0b168be60f9a5', 'u_0001', '13700000001', '也没有', '没有', '3c86eb9026e64dc5bf81497657d39ef7', '2026-09-14 11:44:45');
INSERT INTO `sys_login_log` VALUES ('86e55a9b7345484bac11492a00fc6fda', 'u_0001', '13700000001', '也没有', '没有', 'e80e08516e7648cc89099c94199a4230', '2026-10-16 10:28:22');
INSERT INTO `sys_login_log` VALUES ('89bcfa69b529456a9220e4da80873cfd', 'u_0001', '13700000001', '也没有', '没有', '9a8f2f2f17df449a9e392b0580a63a5a', '2026-10-16 10:27:44');
INSERT INTO `sys_login_log` VALUES ('94b99ba170c14e7280db10100b7c4024', 'u_0001', '13700000001', '也没有', '没有', '79c67e39bf4b4375a9c4f3ea369435c9', '2026-10-17 15:52:10');
INSERT INTO `sys_login_log` VALUES ('98adb280e58f4edeaf8991f9e2e0b2c1', 'd4436e1517404f72b0ccc95a3f09fae5', '13583395459', '也没有', '没有', 'c1261948dbfd45028fcd53acb15e9a38', '2026-10-17 09:44:55');
INSERT INTO `sys_login_log` VALUES ('9db3126ad62140f5adf90d19e407268c', '84f6a9f12f4a41e5b3701426ead5647a', '13800002222', '也没有', '没有', 'd293205ca71e44b687b41021ef6aa476', '2026-10-17 09:03:36');
INSERT INTO `sys_login_log` VALUES ('a4593c8c344549ea8a4ed16251db660b', 'u_0001', '13700000001', '也没有', '没有', '1a6fb1dbc4c04558aef1227c0c1f3dd6', '2026-10-16 11:17:52');
INSERT INTO `sys_login_log` VALUES ('a8b0ec4ccb5b4deab3af4a38e38edbf6', 'u_0001', '13700000001', '也没有', '没有', '0c8e93a648c248ef981669ca0c94fb83', '2026-09-14 14:05:28');
INSERT INTO `sys_login_log` VALUES ('afcf804b8c244c71b567d1a022dafe7b', '9432473ed28b4126b2e274e373d53a0c', '13800009999', '也没有', '没有', '3a884bd4af9b4fadb0681f97e0884738', '2026-10-17 10:42:01');
INSERT INTO `sys_login_log` VALUES ('b4ac09ce6c8f4ed687190ab7620824ea', 'u_0001', '13700000001', '也没有', '没有', 'eeba34f602164d11b73e3ca78cf18ace', '2026-09-16 14:01:07');
INSERT INTO `sys_login_log` VALUES ('b6241c03f11544c594f41fc86805cd53', 'b872bd7493954fd1aadceeaca53dc9a7', '13900001234', '也没有', '没有', 'a2675589278841958d76f09619a43c2b', '2026-10-16 17:13:21');
INSERT INTO `sys_login_log` VALUES ('b650cd5da73249818ccde8d140306375', '59fcb930d17a4b4b919ef2ca5fc0df58', '13800003333', '也没有', '没有', 'b06078e705464ce1b92fe4a35c898424', '2026-10-17 09:39:53');
INSERT INTO `sys_login_log` VALUES ('bfdeadfdd13949e7b6b01a577e0bde02', 'u_0001', '13700000001', '也没有', '没有', '3e3a8cfc0874492883da6ca1028eccd0', '2026-10-16 15:58:15');
INSERT INTO `sys_login_log` VALUES ('c3f3f5ac10034473963139b003b35fdf', 'u_0001', '13700000001', '也没有', '没有', 'a977369238584fb094cd810e8a1006ed', '2026-10-16 14:45:04');
INSERT INTO `sys_login_log` VALUES ('cb7199301ca64c89950e5bed29a35545', 'u_0001', '13700000001', '也没有', '没有', '4e242a79dcee41a9b410248a5a9b83e5', '2026-10-16 10:27:39');
INSERT INTO `sys_login_log` VALUES ('cdf95c3a109c46cdb4c185c19c781d7d', 'u_0001', '13700000001', '也没有', '没有', '21b1ea047fa640e29c7ec6951d969618', '2026-10-17 17:39:13');
INSERT INTO `sys_login_log` VALUES ('d506250f83f147b8892172a8a47d2d67', 'u_0001', '13700000001', '也没有', '没有', '6a4cc368ca2548f7bc5e977b6b9ba1a6', '2026-09-14 11:05:05');
INSERT INTO `sys_login_log` VALUES ('d68583ecd7e64e3f947e2cc7ec6079c3', 'u_0001', '13700000001', '也没有', '没有', 'ee0dea636ed249939da382ab42241aad', '2026-09-14 14:04:36');
INSERT INTO `sys_login_log` VALUES ('d88da08159804367af9e5a1157d856b3', 'u_0001', '13700000001', '也没有', '没有', '1d723b538f844397b343dae67d5c1f94', '2026-09-14 14:04:41');
INSERT INTO `sys_login_log` VALUES ('dddf3610859947119a294783af9c826f', 'u_0001', '13700000001', '也没有', '没有', 'f2f769c631de45ed89538449bea4581a', '2026-09-14 14:04:43');
INSERT INTO `sys_login_log` VALUES ('e19c5cb102b84e5782577d9a7d9287b4', '9eff5d29efc24c3998e9b07a315a7692', '13900000000', '也没有', '没有', '0e912204b6a642ab95db611ee42ad8d2', '2026-10-17 08:38:00');
INSERT INTO `sys_login_log` VALUES ('e24a5cfe84ca4ee8b33e0dcba25f4aea', '0fb5a2f052614b42afb3732cee812919', '13800001111', '也没有', '没有', '6b9fdab8cf9c418ba67029aa8c653293', '2026-10-17 09:03:37');
INSERT INTO `sys_login_log` VALUES ('e2a2c67b4ee842f1bbfaea88cb5abdfc', '4ae99d2f4d194ffca171be252f68e8f9', '19332405533', '也没有', '没有', 'd50b1df9fc2641c684725e0f8e0add6b', '2026-10-17 09:12:56');
INSERT INTO `sys_login_log` VALUES ('e47efec61bbc4ad690b0e7ceee82f959', '0fb5a2f052614b42afb3732cee812919', '13800001111', '也没有', '没有', 'ea1d037a7d9a450aafc2e8c418a8056b', '2026-10-17 09:00:27');
INSERT INTO `sys_login_log` VALUES ('e7af804390964df4a6e9dd5514e5623f', 'u_0001', '13700000001', '也没有', '没有', 'ba78cfd0406e4c2d9fbaeaae19267d75', '2026-10-16 16:35:42');
INSERT INTO `sys_login_log` VALUES ('e9c8efdb458b4c7baf9325b82db1ff8a', 'u_0001', '13700000001', '也没有', '没有', 'a732d588058d4b3daa1c4f29126c70e9', '2026-10-17 18:24:35');
INSERT INTO `sys_login_log` VALUES ('ecf8aadc24fd440d8715c84ea662143d', 'u_0001', '13700000001', '也没有', '没有', '80136e46165e4e7cb71d4e9920f107f3', '2026-09-14 14:04:43');
INSERT INTO `sys_login_log` VALUES ('ef93c94b187b403ba90302b7ceef3e21', '9432473ed28b4126b2e274e373d53a0c', '13800009999', '也没有', '没有', 'f0f36b2e890c43e1930dddc93f05733e', '2026-10-17 10:43:22');
INSERT INTO `sys_login_log` VALUES ('f861b15a45104844863621f7b5d0ac65', 'u_0001', '13700000001', '也没有', '没有', 'bc03febd9da442c7b486b7930a6b86a4', '2026-10-16 14:39:45');
INSERT INTO `sys_login_log` VALUES ('fb0c28d41c28498e8eaa379dbde9c132', 'b872bd7493954fd1aadceeaca53dc9a7', '13900001234', '也没有', '没有', 'eac0e4bcb25045d3b21ac59e3295660d', '2026-10-16 17:15:06');
INSERT INTO `sys_login_log` VALUES ('fda82434e95f48f4ad665daae97c0346', 'u_0001', '13700000001', '也没有', '没有', '5086a0b3f8b24e75bff6b821271166c6', '2026-10-16 16:25:30');

-- ----------------------------
-- Table structure for sys_menu
-- ----------------------------
DROP TABLE IF EXISTS `sys_menu`;
CREATE TABLE `sys_menu`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `parent_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '父菜单',
  `menu_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '菜单名称',
  `path` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '路由',
  `icon` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL,
  `is_menu` int NULL DEFAULT 1 COMMENT '1菜单 0按钮',
  `sort` int NULL DEFAULT 0 COMMENT '排序',
  `is_ban` int NULL DEFAULT 0 COMMENT '是否禁用',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '系统菜单' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of sys_menu
-- ----------------------------
INSERT INTO `sys_menu` VALUES ('fb0087a7d8704bd6842d006dd3306b7f', 'menu_001', '商品列表', '/goods/list', '', 1, 1, 0, '2026-08-05 19:03:58');
INSERT INTO `sys_menu` VALUES ('menu_001', NULL, '商品管理', '/goods', NULL, 1, 1, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_003', NULL, '资质管理', '/qualification', '', 1, 4, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_003b', 'menu_003', '商户资质', '/qualification/merchant', NULL, 1, 2, 0, '2026-08-18 15:26:55');
INSERT INTO `sys_menu` VALUES ('menu_005', NULL, '结算管理', '/settle', '', 1, 7, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_006', NULL, '订单管理', '/order', NULL, 1, 5, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_007', NULL, '系统管理', '/sys', '', 1, 8, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_102', 'menu_001', '商品审核', '/goods/audit', NULL, 1, 2, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_103', 'menu_001', 'SKU规格', '/goods/sku', NULL, 1, 3, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_104', NULL, '商户管理', '/merchant', NULL, 1, 2, 0, '2026-08-05 15:04:47');
INSERT INTO `sys_menu` VALUES ('menu_421', 'menu_104', '商户列表', '/merchant/list', 'el-icon-s-shop', 1, 1, 0, '2026-08-06 19:39:09');
INSERT INTO `sys_menu` VALUES ('menu_601', 'menu_006', '订单列表', '/order/list', NULL, 1, 1, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_602', 'menu_607', '退款审核', '/order/review', '', 1, 2, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_603', 'menu_006', '物流管理', '/logistics', '', 1, 2, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_604', 'menu_006', '库存管理', '/stock', '', 1, 3, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_607', NULL, '退款管理', '/order/refund', 'el-icon-money', 1, 6, 0, '2026-08-05 14:58:10');
INSERT INTO `sys_menu` VALUES ('menu_607a', 'menu_607', '订单退款', '/order/refund/list', NULL, 1, 1, 0, '2026-08-18 15:26:55');
INSERT INTO `sys_menu` VALUES ('menu_701', 'menu_007', '组织架构', '/sys/org', NULL, 1, 1, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_702', 'menu_007', '菜单管理', '/sys/menu', '', 1, 2, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_703', 'menu_007', '数据字典', '/sys/dict', NULL, 1, 3, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_704', 'menu_007', '系统通知', '/sys/notice', NULL, 1, 4, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_705', 'menu_007', '权限分配', '/sys/right', NULL, 1, 5, 0, '2026-07-30 10:10:21');
INSERT INTO `sys_menu` VALUES ('menu_802', 'menu_005', '商户账单', '/settle/merchantbill', NULL, 1, 2, 0, '2026-08-05 14:42:03');
INSERT INTO `sys_menu` VALUES ('menu_srank', 'menu_104', '商户排行榜', '/merchant/rank', 'el-icon-trophy', 1, 2, 0, '2026-08-06 19:07:35');

-- ----------------------------
-- Table structure for sys_notice
-- ----------------------------
DROP TABLE IF EXISTS `sys_notice`;
CREATE TABLE `sys_notice`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '标题',
  `content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL COMMENT '内容',
  `type` int NULL DEFAULT 0 COMMENT '类型',
  `is_ban` int NULL DEFAULT 0 COMMENT '是否禁用',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '系统通知公告' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of sys_notice
-- ----------------------------
INSERT INTO `sys_notice` VALUES ('notice_001', '系统上线公告', '社区小店后台系统已上线，欢迎使用。', 1, 0, '2026-07-30 10:10:21');

-- ----------------------------
-- Table structure for sys_oper_log
-- ----------------------------
DROP TABLE IF EXISTS `sys_oper_log`;
CREATE TABLE `sys_oper_log`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '主键',
  `oper_time` datetime NULL DEFAULT NULL COMMENT '操作时间',
  `request_url` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '请求URL',
  `business_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '业务类型',
  `oper_action` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '操作行为描述',
  `request_method` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '请求方式',
  `request_params` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL COMMENT '请求参数',
  `create_at` datetime NULL DEFAULT NULL COMMENT '记录入库时间',
  `result` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '结果',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `idx_business_type`(`business_type` ASC) USING BTREE,
  INDEX `idx_oper_time`(`oper_time` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '系统操作日志表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of sys_oper_log
-- ----------------------------
INSERT INTO `sys_oper_log` VALUES ('08d819e9d9424e269d2d69dc56781154', '2026-09-14 11:05:06', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-09-14 11:05:06', 'ok');
INSERT INTO `sys_oper_log` VALUES ('108447b91aab439d884d52b2fefcba30', '2026-10-16 10:28:22', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-16 10:28:22', 'ok');
INSERT INTO `sys_oper_log` VALUES ('2a52a8ae11dc4a1b8c29ec9b11097bed', '2026-10-16 14:59:10', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-16 14:59:10', 'ok');
INSERT INTO `sys_oper_log` VALUES ('2b000c5bc5484ff8990b553ff42556be', '2026-10-17 14:08:00', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-17 14:08:00', 'ok');
INSERT INTO `sys_oper_log` VALUES ('35013582fe8f49b0be46c47978d92304', '2026-09-14 11:44:46', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-09-14 11:44:46', 'ok');
INSERT INTO `sys_oper_log` VALUES ('3a79679b3cb4459e9398edff21977e41', '2026-10-16 15:58:15', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-16 15:58:15', 'ok');
INSERT INTO `sys_oper_log` VALUES ('572c4f66c990474bbd02934afb8b9034', '2026-10-16 10:27:44', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-16 10:27:44', 'ok');
INSERT INTO `sys_oper_log` VALUES ('603e5773ad144003911c163c07f717d3', '2026-10-18 13:39:16', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-18 13:39:16', 'ok');
INSERT INTO `sys_oper_log` VALUES ('63c52faee7f2464f8657e2e019b8f752', '2026-10-18 13:22:43', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-18 13:22:43', 'ok');
INSERT INTO `sys_oper_log` VALUES ('63eaf543859646608ac0cb0e550d47b2', '2026-10-16 16:25:30', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-16 16:25:30', 'ok');
INSERT INTO `sys_oper_log` VALUES ('6654c44004c545b49d90bc36f48481f2', '2026-09-14 16:30:27', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-09-14 16:30:27', 'ok');
INSERT INTO `sys_oper_log` VALUES ('6ecc690f296a4d13847cfe89813f8004', '2026-10-16 11:17:52', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-16 11:17:52', 'ok');
INSERT INTO `sys_oper_log` VALUES ('7f90f84cf1154fabb8f69ad683b84e29', '2026-10-17 15:52:10', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-17 15:52:10', 'ok');
INSERT INTO `sys_oper_log` VALUES ('828e8935150b439291061fdb3da6aa5c', '2026-10-17 18:24:36', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-17 18:24:36', 'ok');
INSERT INTO `sys_oper_log` VALUES ('90aa6bc9d6b042588b52ea7a22ca1998', '2026-10-16 09:31:36', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-16 09:31:36', 'fail');
INSERT INTO `sys_oper_log` VALUES ('9fa23c201f4d496cbe253104a0cbab56', '2026-10-16 09:31:15', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-16 09:31:15', 'fail');
INSERT INTO `sys_oper_log` VALUES ('a3f4b7ad265d4382b3838d68650df71f', '2026-10-16 16:35:42', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-16 16:35:42', 'ok');
INSERT INTO `sys_oper_log` VALUES ('a6baf3764f014277b0dc1258bfa504be', '2026-09-16 14:01:07', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-09-16 14:01:07', 'ok');
INSERT INTO `sys_oper_log` VALUES ('b658bd01164b408290dd6171334c64e0', '2026-10-16 14:39:45', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-16 14:39:45', 'ok');
INSERT INTO `sys_oper_log` VALUES ('c83d3b8b68be4e0cb46edc0d145a0d1c', '2026-10-16 16:20:40', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-16 16:20:40', 'ok');
INSERT INTO `sys_oper_log` VALUES ('cfaf51e93c96427ba7af28a6e530c071', '2026-10-16 14:45:04', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-16 14:45:04', 'ok');
INSERT INTO `sys_oper_log` VALUES ('e794094fd8ae48ac8073e9a8904d774a', '2026-10-16 09:31:22', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-16 09:31:22', 'fail');
INSERT INTO `sys_oper_log` VALUES ('edc00b752117499784cab5d0cb7a0901', '2026-10-17 16:40:37', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-17 16:40:37', 'ok');
INSERT INTO `sys_oper_log` VALUES ('fdabb8b62d3d4c8bbf965d712037469b', '2026-10-17 17:39:14', '/api/login/Login_Admin', '用户管理', '管理员登陆', 'POST', '{\"account\":\"13700000001\",\"password\":\"1******\"}', '2026-10-17 17:39:14', 'ok');

-- ----------------------------
-- Table structure for sys_role
-- ----------------------------
DROP TABLE IF EXISTS `sys_role`;
CREATE TABLE `sys_role`  (
  `id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `role_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '角色名称',
  `role_key` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '角色权限标识',
  `sort` int NULL DEFAULT 0 COMMENT '排序',
  `is_ban` int NULL DEFAULT 0 COMMENT '是否禁用',
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '备注',
  `create_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '系统角色' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of sys_role
-- ----------------------------
INSERT INTO `sys_role` VALUES ('role_001', '超级管理员', 'admin', 1, 0, '全部权限', '2026-07-30 10:10:21');
INSERT INTO `sys_role` VALUES ('role_staff', '员工', 'staff', 3, 0, '基础员工权限', '2026-08-06 14:20:41');

-- ----------------------------
-- Table structure for sys_users
-- ----------------------------
DROP TABLE IF EXISTS `sys_users`;
CREATE TABLE `sys_users`  (
  `u_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '主键ID',
  `u_account` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '登录账号，唯一',
  `u_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '名字',
  `u_password` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '加密后的密码',
  `u_salt` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '加密盐值',
  `u_create_time` datetime NULL DEFAULT NULL COMMENT '创建时间',
  `u_isBan` int NULL DEFAULT 0 COMMENT '是否禁用 0正常 1禁用',
  `dept` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '部门ID',
  `role` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '角色ID',
  `avatar` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '头像地址',
  PRIMARY KEY (`u_id`) USING BTREE,
  UNIQUE INDEX `uk_account`(`u_account` ASC) USING BTREE,
  INDEX `fk_user_dept_id`(`dept` ASC) USING BTREE,
  INDEX `fk_user_role_id`(`role` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci COMMENT = '系统用户表' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Records of sys_users
-- ----------------------------
INSERT INTO `sys_users` VALUES ('0fb5a2f052614b42afb3732cee812919', '13800001111', '13800001111', '$argon2id$v=19$m=19456,t=2,p=1$ZVKFnfs9E8A7rLpId/zDbA$rtR0wDS521SWVuCTJVB9DkbMe7BEsoCUf9uKqIYQhTk', '28110d2d', '2026-10-17 09:00:27', 0, NULL, NULL, NULL);
INSERT INTO `sys_users` VALUES ('42b4547c5cb746959f5cf0355e4e9903', '13900005678', '13900005678', '$argon2id$v=19$m=19456,t=2,p=1$NVRyY70u4vT7AntmCX8OEg$JzTNPt9/rD/XhK+06yyoT+jHp+QX6lfS7f7xX/xfZwo', '36c05a82', '2026-10-16 17:13:37', 0, NULL, NULL, NULL);
INSERT INTO `sys_users` VALUES ('479b927d83d14267bc4b8840297d638b', '13800000099', '13800000099', '$argon2id$v=19$m=19456,t=2,p=1$wzUyaISrDKLhfh3tXABRRA$rj6tJj3GHA/5f4+iIc2Df8ZFVaJCc9R2yfEcRKeBzB4', 'eb591d1b', '2026-10-17 15:16:31', 0, NULL, NULL, NULL);
INSERT INTO `sys_users` VALUES ('4ae99d2f4d194ffca171be252f68e8f9', '19332405533', '19332405533', '$argon2id$v=19$m=19456,t=2,p=1$VyymYWEmB93tkKXEsYBL1A$EpIpogiNEW6BK3U8dgCyujHa2t5Q/p+L6NGbZ57rwwI', '8c935e7c', '2026-10-17 09:12:55', 0, NULL, NULL, NULL);
INSERT INTO `sys_users` VALUES ('59fcb930d17a4b4b919ef2ca5fc0df58', '13800003333', '13800003333', '$argon2id$v=19$m=19456,t=2,p=1$KZqunbs2DM3ljXwM2ApQpw$lYGJXJO/k62YUmc290eyNOkKEbpG7v6Ob0CqouSID08', '159dbe1a', '2026-10-17 09:39:53', 0, NULL, NULL, NULL);
INSERT INTO `sys_users` VALUES ('84f6a9f12f4a41e5b3701426ead5647a', '13800002222', '13800002222', '$argon2id$v=19$m=19456,t=2,p=1$2i3js8yJsCz/Tj3yFT1fSw$7BH/6uOgf6dd7f3HFpRhXIqhGYOeSmRV1Yffnqw/lLg', '92958e6f', '2026-10-17 09:02:13', 0, NULL, NULL, NULL);
INSERT INTO `sys_users` VALUES ('9432473ed28b4126b2e274e373d53a0c', '13800009999', '13800009999', '$argon2id$v=19$m=19456,t=2,p=1$jYtlUZgmqD9B5NRV6hNxHA$3yw6eINZkaopenvntXv5n1YBCOoQ48zTVCtNdwDWtLo', '9da6f989', '2026-10-17 10:42:01', 0, NULL, NULL, NULL);
INSERT INTO `sys_users` VALUES ('9eff5d29efc24c3998e9b07a315a7692', '13900000000', '13900000000', '$argon2id$v=19$m=19456,t=2,p=1$qnR2LwwErUmpCrTo5Mvd4Q$eb/8n63gLmkOf0ZES1ksXG9I8MoUYZ2Y1SFIKMGuRNw', 'a6639d9b', '2026-10-17 08:38:00', 0, NULL, NULL, NULL);
INSERT INTO `sys_users` VALUES ('aa8aa123f1fd41f1a9c9942cdeabcbf0', '13800000001', '张三', '$argon2id$v=19$m=19456,t=2,p=1$+a6trogcmHNCstSAaazsKg$iDbxQRFPy3NhsAQ0rfdnnXFm8a81jCXQv7ffta1bEVM', '9fb072ca', '2026-07-31 08:43:26', 0, 'dept_002', 'role_staff', '/api/file/ab671241e6454995bf482cf761b11a87.png');
INSERT INTO `sys_users` VALUES ('b872bd7493954fd1aadceeaca53dc9a7', '13900001234', '13900001234', '$argon2id$v=19$m=19456,t=2,p=1$/ngL96aoJKqgKNhu/mp8dg$KsSM9GtK5yzvpJfcUu5+Lm69bJyAK3QTvB3MCGm02AA', '0a073c5c', '2026-10-16 17:13:21', 0, NULL, NULL, NULL);
INSERT INTO `sys_users` VALUES ('ca868096c36d476794c574dc8879eb2e', '12232435445', '12232435445', '$argon2id$v=19$m=19456,t=2,p=1$qiVrPh4ZXYU4vy3reexIqw$YMJyWkpdO4ZdfEfsjo0sXrRRXwqcQRtla1OzzD8wPDg', '2b33c20c', '2026-10-17 15:29:14', 0, NULL, NULL, NULL);
INSERT INTO `sys_users` VALUES ('d4436e1517404f72b0ccc95a3f09fae5', '13583395459', '13583395459', '$argon2id$v=19$m=19456,t=2,p=1$UoemE+cT1ctSfBMPf7WIOQ$E779hbOtbE37/5SGYwwBFEHmcySl8GqJ2vy2zh4XqAo', '898ae07b', '2026-10-17 09:44:55', 0, NULL, NULL, NULL);
INSERT INTO `sys_users` VALUES ('u_0001', '13700000001', '超级管理员', '$argon2id$v=19$m=19456,t=2,p=1$7N6K7POLcz3OCCIAtuL44w$Mzf9k1lyL85/PZGGAslVQlkRVS9f0jzrYcO9C8FcaIA', '04c477fe', '2026-07-30 10:10:21', 0, 'dept_001', 'role_001', NULL);

SET FOREIGN_KEY_CHECKS = 1;
