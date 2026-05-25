CREATE TABLE IF NOT EXISTS telegram_settings (
    id INT NOT NULL PRIMARY KEY,
    bot_token VARCHAR(255) NOT NULL,
    chat_id VARCHAR(120) NULL,
    is_enabled TINYINT(1) NOT NULL DEFAULT 0,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO telegram_settings (id, bot_token, chat_id, is_enabled, updated_at)
VALUES (1, '', NULL, 0, CURRENT_TIMESTAMP)
ON DUPLICATE KEY UPDATE id = id;
