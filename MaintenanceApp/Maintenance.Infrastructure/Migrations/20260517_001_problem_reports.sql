CREATE TABLE IF NOT EXISTS problem_reports (
    id INT AUTO_INCREMENT PRIMARY KEY,
    report_number VARCHAR(80) NOT NULL,
    asset_id INT NOT NULL,
    title VARCHAR(200) NOT NULL,
    description TEXT NULL,
    category VARCHAR(30) NOT NULL,
    priority VARCHAR(30) NOT NULL,
    status VARCHAR(30) NOT NULL,
    reported_by VARCHAR(150) NULL,
    reported_at DATETIME NOT NULL,
    downtime_start DATETIME NULL,
    downtime_end DATETIME NULL,
    downtime_minutes INT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_problem_reports_report_number (report_number),
    KEY ix_problem_reports_asset_id (asset_id),
    KEY ix_problem_reports_status (status),
    KEY ix_problem_reports_category (category),
    CONSTRAINT fk_problem_reports_assets FOREIGN KEY (asset_id) REFERENCES assets(id)
);

SET @work_orders_report_column_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'work_orders'
      AND COLUMN_NAME = 'problem_report_id'
);

SET @add_work_orders_report_sql := IF(
    @work_orders_report_column_exists = 0,
    'ALTER TABLE work_orders ADD COLUMN problem_report_id INT NULL AFTER asset_id',
    'SELECT 1'
);

PREPARE add_work_orders_report_stmt FROM @add_work_orders_report_sql;
EXECUTE add_work_orders_report_stmt;
DEALLOCATE PREPARE add_work_orders_report_stmt;

SET @downtime_report_column_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'downtime_logs'
      AND COLUMN_NAME = 'problem_report_id'
);

SET @add_downtime_report_sql := IF(
    @downtime_report_column_exists = 0,
    'ALTER TABLE downtime_logs ADD COLUMN problem_report_id INT NULL AFTER work_order_id',
    'SELECT 1'
);

PREPARE add_downtime_report_stmt FROM @add_downtime_report_sql;
EXECUTE add_downtime_report_stmt;
DEALLOCATE PREPARE add_downtime_report_stmt;

SET @work_orders_report_index_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'work_orders'
      AND INDEX_NAME = 'ix_work_orders_problem_report_id'
);

SET @add_work_orders_report_index_sql := IF(
    @work_orders_report_index_exists = 0,
    'CREATE INDEX ix_work_orders_problem_report_id ON work_orders (problem_report_id)',
    'SELECT 1'
);

PREPARE add_work_orders_report_index_stmt FROM @add_work_orders_report_index_sql;
EXECUTE add_work_orders_report_index_stmt;
DEALLOCATE PREPARE add_work_orders_report_index_stmt;

SET @downtime_report_index_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'downtime_logs'
      AND INDEX_NAME = 'ix_downtime_logs_problem_report_id'
);

SET @add_downtime_report_index_sql := IF(
    @downtime_report_index_exists = 0,
    'CREATE UNIQUE INDEX ix_downtime_logs_problem_report_id ON downtime_logs (problem_report_id)',
    'SELECT 1'
);

PREPARE add_downtime_report_index_stmt FROM @add_downtime_report_index_sql;
EXECUTE add_downtime_report_index_stmt;
DEALLOCATE PREPARE add_downtime_report_index_stmt;

INSERT INTO problem_reports
(report_number, asset_id, title, description, category, priority, status, reported_by, reported_at, downtime_start, downtime_end, downtime_minutes, created_at, updated_at)
SELECT 'RPT-20260507-0001', a.id, 'Case conveyor jam at transfer point', 'Carton jam at transfer point before case packer. Work order was created from operator report.', 'DOWNTIME', 'HIGH', 'COMPLETED', 'Operator Packaging', '2026-05-07 16:20:00', '2026-05-07 16:20:00', '2026-05-07 17:20:00', 60, '2026-05-07 16:20:00', '2026-05-07 17:20:00'
FROM assets a
WHERE a.asset_code = 'DEMO-AST-004'
ON DUPLICATE KEY UPDATE
asset_id = VALUES(asset_id),
title = VALUES(title),
description = VALUES(description),
category = VALUES(category),
priority = VALUES(priority),
status = VALUES(status),
reported_by = VALUES(reported_by),
reported_at = VALUES(reported_at),
downtime_start = VALUES(downtime_start),
downtime_end = VALUES(downtime_end),
downtime_minutes = VALUES(downtime_minutes),
updated_at = VALUES(updated_at);

INSERT INTO problem_reports
(report_number, asset_id, title, description, category, priority, status, reported_by, reported_at, downtime_start, downtime_end, downtime_minutes, created_at, updated_at)
SELECT 'RPT-20260517-0001', a.id, 'Filler reject chute blocked', 'Reject chute blocked during start of shift. Production is stopped until maintenance checks the area.', 'DOWNTIME', 'URGENT', 'PENDING', 'Operator Packaging', '2026-05-17 08:45:00', '2026-05-17 08:45:00', NULL, NULL, '2026-05-17 08:45:00', '2026-05-17 08:45:00'
FROM assets a
WHERE a.asset_code = 'DEMO-AST-002'
ON DUPLICATE KEY UPDATE
asset_id = VALUES(asset_id),
title = VALUES(title),
description = VALUES(description),
category = VALUES(category),
priority = VALUES(priority),
status = VALUES(status),
reported_by = VALUES(reported_by),
reported_at = VALUES(reported_at),
downtime_start = VALUES(downtime_start),
downtime_end = VALUES(downtime_end),
downtime_minutes = VALUES(downtime_minutes),
updated_at = VALUES(updated_at);

INSERT INTO problem_reports
(report_number, asset_id, title, description, category, priority, status, reported_by, reported_at, downtime_start, downtime_end, downtime_minutes, created_at, updated_at)
SELECT 'RPT-20260517-0002', a.id, 'Boiler panel alarm intermittent', 'Alarm indicator flashes intermittently after panel reset. Production can continue but needs follow-up.', 'BREAKDOWN', 'MEDIUM', 'PENDING', 'Operator Utility', '2026-05-17 09:30:00', NULL, NULL, NULL, '2026-05-17 09:30:00', '2026-05-17 09:30:00'
FROM assets a
WHERE a.asset_code = 'DEMO-AST-005'
ON DUPLICATE KEY UPDATE
asset_id = VALUES(asset_id),
title = VALUES(title),
description = VALUES(description),
category = VALUES(category),
priority = VALUES(priority),
status = VALUES(status),
reported_by = VALUES(reported_by),
reported_at = VALUES(reported_at),
downtime_start = VALUES(downtime_start),
downtime_end = VALUES(downtime_end),
downtime_minutes = VALUES(downtime_minutes),
updated_at = VALUES(updated_at);

UPDATE work_orders wo
JOIN problem_reports pr ON pr.report_number = 'RPT-20260507-0001'
SET wo.problem_report_id = pr.id,
    wo.updated_at = '2026-05-17 10:00:00'
WHERE wo.wo_number = 'DEMO-WO-20260507-001';

UPDATE downtime_logs dl
JOIN work_orders wo ON dl.work_order_id = wo.id
JOIN problem_reports pr ON pr.report_number = 'RPT-20260507-0001'
SET dl.problem_report_id = pr.id
WHERE wo.wo_number = 'DEMO-WO-20260507-001';

INSERT INTO downtime_logs
(asset_id, work_order_id, problem_report_id, downtime_category, start_time, end_time, duration_minutes, description, created_at)
SELECT pr.asset_id, NULL, pr.id, 'OPERATIONAL', pr.downtime_start, pr.downtime_end, pr.downtime_minutes, CONCAT(pr.report_number, ' - ', pr.title), pr.created_at
FROM problem_reports pr
WHERE pr.report_number = 'RPT-20260517-0001'
  AND NOT EXISTS (SELECT 1 FROM downtime_logs dl WHERE dl.problem_report_id = pr.id);

UPDATE downtime_logs dl
JOIN problem_reports pr ON dl.problem_report_id = pr.id
SET dl.asset_id = pr.asset_id,
    dl.start_time = COALESCE(pr.downtime_start, pr.reported_at),
    dl.end_time = pr.downtime_end,
    dl.duration_minutes = pr.downtime_minutes,
    dl.description = CONCAT(pr.report_number, ' - ', pr.title)
WHERE pr.report_number = 'RPT-20260517-0001';
