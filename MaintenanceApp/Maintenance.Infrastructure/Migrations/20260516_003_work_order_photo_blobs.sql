SET @file_data_column_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'work_order_photos'
      AND COLUMN_NAME = 'file_data'
);

SET @add_file_data_sql := IF(
    @file_data_column_exists = 0,
    'ALTER TABLE work_order_photos ADD COLUMN file_data LONGBLOB NULL AFTER content_type',
    'SELECT 1'
);

PREPARE add_file_data_stmt FROM @add_file_data_sql;
EXECUTE add_file_data_stmt;
DEALLOCATE PREPARE add_file_data_stmt;

SET @size_bytes_column_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'work_order_photos'
      AND COLUMN_NAME = 'size_bytes'
);

SET @add_size_bytes_sql := IF(
    @size_bytes_column_exists = 0,
    'ALTER TABLE work_order_photos ADD COLUMN size_bytes BIGINT NOT NULL DEFAULT 0 AFTER content_type',
    'SELECT 1'
);

PREPARE add_size_bytes_stmt FROM @add_size_bytes_sql;
EXECUTE add_size_bytes_stmt;
DEALLOCATE PREPARE add_size_bytes_stmt;

SET @file_url_column_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'work_order_photos'
      AND COLUMN_NAME = 'file_url'
);

SET @drop_file_url_sql := IF(
    @file_url_column_exists = 1,
    'ALTER TABLE work_order_photos DROP COLUMN file_url',
    'SELECT 1'
);

PREPARE drop_file_url_stmt FROM @drop_file_url_sql;
EXECUTE drop_file_url_stmt;
DEALLOCATE PREPARE drop_file_url_stmt;
