-- 玩家背包表（示例：MySQL 语法）
-- 一行记录表示玩家拥有的一种道具（可堆叠）

CREATE TABLE IF NOT EXISTS `t_player_bag` (
    `id`         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '自增主键（道具记录ID）',
    `player_id`  BIGINT UNSIGNED NOT NULL COMMENT '玩家ID，对应 t_player.id',

    `item_id`    BIGINT UNSIGNED NOT NULL COMMENT '道具ID（配置表ID）',
    `count`      INT             NOT NULL DEFAULT 0 COMMENT '道具数量（可堆叠）',

    `create_time`DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '获得时间',
    `update_time`DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '最后修改时间',

    PRIMARY KEY (`id`),
    KEY `idx_player` (`player_id`),
    KEY `idx_player_item` (`player_id`, `item_id`),
    CONSTRAINT `fk_player_bag_player`
        FOREIGN KEY (`player_id`) REFERENCES `t_player` (`id`)
        ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='玩家背包道具表';

