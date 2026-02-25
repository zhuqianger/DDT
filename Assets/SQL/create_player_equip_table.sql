-- 玩家当前穿戴装备表（示例：MySQL 语法）
-- 一行记录表示一个玩家当前身上的装备槽位

CREATE TABLE IF NOT EXISTS `t_player_equip` (
    `player_id`   BIGINT UNSIGNED NOT NULL COMMENT '玩家ID，对应 t_player.id',

    -- 装备槽位（存放道具配置ID或装备实例ID，具体由业务决定）
    `weapon_id`   BIGINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '武器ID',
    `clothes_id`  BIGINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '衣服ID',
    `pants_id`    BIGINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '裤子ID',
    `hat_id`      BIGINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '帽子ID',
    `necklace_id` BIGINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '项链ID',

    `update_time` DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '最后修改时间',

    PRIMARY KEY (`player_id`),
    CONSTRAINT `fk_player_equip_player`
        FOREIGN KEY (`player_id`) REFERENCES `t_player` (`id`)
        ON DELETE CASCADE
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='玩家当前穿戴装备表';

