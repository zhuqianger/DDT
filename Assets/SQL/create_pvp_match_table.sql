-- PVP 对局总结表（示例：MySQL 语法）
-- 不记录过程操作，仅在一局结束时写入最终结果，方便统计与排查

CREATE TABLE IF NOT EXISTS `t_pvp_match` (
    `id`             BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '对局ID',

    `mode`           TINYINT         NOT NULL DEFAULT 1 COMMENT '模式类型（目前仅支持 1=1v1）',
    `map_id`         INT             NOT NULL DEFAULT 0 COMMENT '地图ID',

    -- 参与玩家（当前仅支持 1v1）
    `player_a_id`    BIGINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '玩家A ID',
    `player_b_id`    BIGINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '玩家B ID',

    `winner_player_id`BIGINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '获胜玩家ID（只能是 A 或 B）',

    `start_time`     DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '对局开始时间',
    `end_time`       DATETIME        NULL COMMENT '对局结束时间',

    `status`         TINYINT         NOT NULL DEFAULT 0 COMMENT '对局状态：0未结束 1正常结束',

    PRIMARY KEY (`id`),
    KEY `idx_start_time` (`start_time`),
    KEY `idx_player_a` (`player_a_id`),
    KEY `idx_player_b` (`player_b_id`)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='PVP 对局总结表（仅记录结果，不做回放）';

