SET @result_column_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'work_orders'
      AND COLUMN_NAME = 'result'
);

SET @add_result_sql := IF(
    @result_column_exists = 0,
    'ALTER TABLE work_orders ADD COLUMN result TEXT NULL AFTER action_taken',
    'SELECT 1'
);

PREPARE add_result_stmt FROM @add_result_sql;
EXECUTE add_result_stmt;
DEALLOCATE PREPARE add_result_stmt;

CREATE TABLE IF NOT EXISTS work_order_photos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    work_order_id INT NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    content_type VARCHAR(120) NOT NULL,
    size_bytes BIGINT NOT NULL DEFAULT 0,
    file_data LONGBLOB NULL,
    uploaded_by VARCHAR(150) NULL,
    uploaded_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    KEY ix_work_order_photos_work_order_id (work_order_id),
    CONSTRAINT fk_work_order_photos_work_orders FOREIGN KEY (work_order_id) REFERENCES work_orders(id) ON DELETE CASCADE
);

UPDATE work_orders
SET result = CASE
    WHEN result IS NOT NULL AND result <> '' THEN result
    WHEN status IN ('COMPLETED', 'CLOSED') THEN 'Machine returned to normal operating condition after verification.'
    WHEN status = 'PENDING' THEN 'Waiting for follow-up verification or sparepart availability.'
    ELSE result
END
WHERE reported_at >= '2026-05-01'
  AND reported_at < '2026-06-01';
