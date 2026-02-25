-- 玩家基础信息表（示例：MySQL 语法）
-- 根据实际情况调整库名、ENGINE、字符集等

CREATE TABLE IF NOT EXISTS `t_player` (
    `id`           BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '玩家唯一ID',

    -- 最基础的账号字段
    `account`      VARCHAR(64)     NOT NULL COMMENT '账号名（登录用）',
    `password_hash`VARCHAR(255)    NOT NULL COMMENT '密码哈希（不存明文）',

    -- 基础展示与成长字段
    `nickname`     VARCHAR(64)     NOT NULL COMMENT '玩家昵称',
    `level`        INT             NOT NULL DEFAULT 1 COMMENT '玩家等级',
    `exp`          BIGINT          NOT NULL DEFAULT 0 COMMENT '当前经验值',
    `gold`         BIGINT          NOT NULL DEFAULT 0 COMMENT '金币',
    `diamond`      BIGINT          NOT NULL DEFAULT 0 COMMENT '钻石',

    -- 基础时间字段
    `create_time`  DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    `update_time`  DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '最后修改时间',

    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_account` (`account`),
    KEY `idx_nickname` (`nickname`)
) ENGINE=InnoDB
  DEFAULT CHARSET=utf8mb4
  COLLATE=utf8mb4_unicode_ci
  COMMENT='玩家基础信息表';

