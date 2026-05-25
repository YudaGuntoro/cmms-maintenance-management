SET FOREIGN_KEY_CHECKS = 0;

TRUNCATE TABLE maintenance_types;
TRUNCATE TABLE work_order_priorities;
TRUNCATE TABLE work_order_statuses;
TRUNCATE TABLE downtime_categories;
TRUNCATE TABLE problem_report_categories;
TRUNCATE TABLE preventive_schedule_types;
TRUNCATE TABLE frequency_types;

SET FOREIGN_KEY_CHECKS = 1;

INSERT INTO maintenance_types (id, code, name, is_active, created_at) VALUES
(1, 'PREVENTIVE', 'Preventive', 1, NOW()),
(2, 'CORRECTIVE', 'Corrective', 1, NOW()),
(3, 'BREAKDOWN', 'Breakdown', 1, NOW()),
(4, 'PREDICTIVE', 'Predictive', 1, NOW()),
(5, 'INSPECTION', 'Inspection', 1, NOW()),
(6, 'CONTRACTOR_SUPERVISION', 'Contractor Supervision', 1, NOW());

INSERT INTO work_order_priorities (id, code, name, level, is_active, created_at) VALUES
(1, 'LOW', 'Low', 1, 1, NOW()),
(2, 'MEDIUM', 'Medium', 2, 1, NOW()),
(3, 'HIGH', 'High', 3, 1, NOW()),
(4, 'URGENT', 'Urgent', 4, 1, NOW());

INSERT INTO work_order_statuses (id, code, name, sequence, is_active, created_at) VALUES
(1, 'OPEN', 'Open', 1, 1, NOW()),
(2, 'ASSIGNED', 'Assigned', 2, 1, NOW()),
(3, 'IN_PROGRESS', 'In Progress', 3, 1, NOW()),
(4, 'PENDING', 'Pending', 4, 1, NOW()),
(5, 'DRAFT', 'Draft', 5, 1, NOW()),
(6, 'COMPLETED', 'Completed', 6, 1, NOW()),
(7, 'CLOSED', 'Closed', 7, 1, NOW()),
(8, 'CANCELLED', 'Cancelled', 8, 1, NOW());

INSERT INTO downtime_categories (id, code, name, is_active, created_at) VALUES
(1, 'MECHANICAL', 'Mechanical', 1, NOW()),
(2, 'ELECTRICAL', 'Electrical', 1, NOW()),
(3, 'UTILITY', 'Utility', 1, NOW()),
(4, 'OPERATIONAL', 'Operational', 1, NOW()),
(5, 'PROCESS', 'Process', 1, NOW()),
(6, 'MATERIAL', 'Material', 1, NOW()),
(7, 'PLANNED_STOP', 'Planned Stop', 1, NOW()),
(8, 'OTHER', 'Other', 1, NOW());

INSERT INTO problem_report_categories (id, code, name, is_active, created_at) VALUES
(1, 'DOWNTIME', 'Downtime', 1, NOW()),
(2, 'BREAKDOWN', 'Breakdown', 1, NOW()),
(3, 'QUALITY', 'Quality', 1, NOW()),
(4, 'SAFETY', 'Safety', 1, NOW()),
(5, 'OTHER', 'Other', 1, NOW());

INSERT INTO preventive_schedule_types (id, code, name, is_active, created_at) VALUES
(1, 'DAILY', 'Daily', 1, NOW()),
(2, 'WEEKLY', 'Weekly', 1, NOW()),
(3, 'MONTHLY', 'Monthly', 1, NOW()),
(4, 'YEARLY', 'Yearly', 1, NOW());

INSERT INTO frequency_types (id, code, name, interval_days, is_active, created_at) VALUES
(1, 'DAILY', 'Daily', 1, 1, NOW()),
(2, 'WEEKLY', 'Weekly', 7, 1, NOW()),
(3, 'MONTHLY', 'Monthly', 30, 1, NOW()),
(4, 'YEARLY', 'Yearly', 365, 1, NOW()),
(5, 'RUNNING_HOURS', 'Running Hours', 1, 1, NOW());
