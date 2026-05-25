SET FOREIGN_KEY_CHECKS = 0;

TRUNCATE TABLE contractor_work_audits;
TRUNCATE TABLE contractor_work_documents;
TRUNCATE TABLE contractor_work_plans;
TRUNCATE TABLE work_order_photos;
TRUNCATE TABLE work_order_spareparts;
TRUNCATE TABLE inventory_transactions;
TRUNCATE TABLE downtime_logs;
TRUNCATE TABLE work_orders;
TRUNCATE TABLE problem_reports;
TRUNCATE TABLE preventive_schedules;
TRUNCATE TABLE spareparts;
TRUNCATE TABLE technicians;
TRUNCATE TABLE assets;

SET FOREIGN_KEY_CHECKS = 1;

INSERT INTO assets
(asset_code, asset_name, asset_type, plant, area, production_line, location, manufacturer, model, serial_number, installation_date, criticality_level, status, created_at, updated_at) VALUES
('CMMS-AST-001', 'Filling Machine F-01', 'Filling Machine', 'Plant A', 'Production', 'Line 1', 'Production Hall A1', 'Krones', 'F1200', 'FILL-2026-001', '2022-03-12', 'CRITICAL', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-002', 'Labeling Machine L-02', 'Labeler', 'Plant A', 'Production', 'Line 1', 'Production Hall A1', 'Sidel', 'LBL-900', 'LBL-2026-002', '2021-08-20', 'HIGH', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-003', 'Case Packer CP-03', 'Packing Machine', 'Plant A', 'Packaging', 'Line 2', 'Packaging Area B1', 'Syntegon', 'CP300', 'CP-2026-003', '2020-11-05', 'HIGH', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-004', 'Conveyor CV-04', 'Conveyor', 'Plant A', 'Packaging', 'Line 2', 'Transfer Area B2', 'Interroll', 'CV500', 'CV-2026-004', '2019-05-17', 'MEDIUM', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-005', 'Air Compressor AC-05', 'Compressor', 'Plant A', 'Utility', 'Utility', 'Compressor Room', 'Atlas Copco', 'GA75', 'AC-2026-005', '2018-09-14', 'CRITICAL', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-006', 'Boiler BL-06', 'Boiler', 'Plant A', 'Utility', 'Utility', 'Boiler Room', 'Miura', 'LX-300', 'BL-2026-006', '2017-01-29', 'CRITICAL', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-007', 'Cooling Tower CT-07', 'Cooling Tower', 'Plant A', 'Utility', 'Utility', 'Outdoor Utility', 'BAC', 'CT-100', 'CT-2026-007', '2018-06-22', 'HIGH', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-008', 'Chiller CH-08', 'Chiller', 'Plant A', 'Utility', 'Utility', 'Chiller Room', 'York', 'YVAA', 'CH-2026-008', '2020-02-15', 'CRITICAL', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-009', 'Pump Transfer PT-09', 'Pump', 'Plant A', 'Process', 'Line 3', 'Process Area C1', 'Grundfos', 'CRN45', 'PT-2026-009', '2021-04-08', 'HIGH', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-010', 'Mixing Tank MX-10', 'Tank', 'Plant A', 'Process', 'Line 3', 'Mixing Area C2', 'Alfa Laval', 'MX-5000', 'MX-2026-010', '2021-10-19', 'HIGH', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-011', 'CIP System CIP-11', 'CIP System', 'Plant A', 'Process', 'CIP', 'CIP Room', 'GEA', 'CIP-800', 'CIP-2026-011', '2019-12-12', 'HIGH', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-012', 'Forklift FL-12', 'Material Handling', 'Plant A', 'Warehouse', 'Warehouse', 'Warehouse Dock', 'Toyota', '8FD25', 'FL-2026-012', '2022-07-01', 'MEDIUM', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-013', 'Generator GN-13', 'Generator', 'Plant A', 'Utility', 'Power House', 'Generator Room', 'Cummins', 'C500D5', 'GN-2026-013', '2018-03-25', 'CRITICAL', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-014', 'HVAC AHU-14', 'HVAC', 'Plant A', 'Facility', 'Facility', 'AHU Room', 'Daikin', 'AHU-60', 'AHU-2026-014', '2020-08-03', 'MEDIUM', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00'),
('CMMS-AST-015', 'Waste Water Pump WW-15', 'Pump', 'Plant A', 'WWTP', 'WWTP', 'WWTP Basin', 'Ebara', 'DWO', 'WW-2026-015', '2019-02-18', 'HIGH', 'ACTIVE', '2026-05-01 07:00:00', '2026-05-01 07:00:00');

INSERT INTO technicians
(employee_no, name, email, phone, skill_type, shift, status, created_at, updated_at) VALUES
('MTC-001', 'Andi Wijaya', 'andi.wijaya@cmms.local', '081200000001', 'MECHANICAL', 'A', 'ACTIVE', NOW(), NOW()),
('MTC-002', 'Budi Santoso', 'budi.santoso@cmms.local', '081200000002', 'ELECTRICAL', 'A', 'ACTIVE', NOW(), NOW()),
('MTC-003', 'Citra Lestari', 'citra.lestari@cmms.local', '081200000003', 'UTILITY', 'B', 'ACTIVE', NOW(), NOW()),
('MTC-004', 'Dedi Kurniawan', 'dedi.kurniawan@cmms.local', '081200000004', 'MECHANICAL', 'B', 'ACTIVE', NOW(), NOW()),
('MTC-005', 'Eka Prasetyo', 'eka.prasetyo@cmms.local', '081200000005', 'ELECTRICAL', 'C', 'ACTIVE', NOW(), NOW()),
('MTC-006', 'Fajar Nugroho', 'fajar.nugroho@cmms.local', '081200000006', 'GENERAL', 'C', 'ACTIVE', NOW(), NOW()),
('MTC-007', 'Gita Permata', 'gita.permata@cmms.local', '081200000007', 'UTILITY', 'A', 'ACTIVE', NOW(), NOW()),
('MTC-008', 'Hendra Saputra', 'hendra.saputra@cmms.local', '081200000008', 'MECHANICAL', 'B', 'ACTIVE', NOW(), NOW()),
('MTC-009', 'Intan Maharani', 'intan.maharani@cmms.local', '081200000009', 'ELECTRICAL', 'C', 'ACTIVE', NOW(), NOW()),
('MTC-010', 'Joko Firmansyah', 'joko.firmansyah@cmms.local', '081200000010', 'GENERAL', 'A', 'ACTIVE', NOW(), NOW());

INSERT INTO spareparts
(part_code, part_name, category, unit, stock_qty, minimum_stock, location, supplier, lead_time_days, price, is_critical, created_at, updated_at) VALUES
('CMMS-SP-001', 'Mechanical Seal 25mm', 'Seal', 'PCS', 18, 5, 'Rack A-01', 'PT Sealindo', 7, 450000, 1, NOW(), NOW()),
('CMMS-SP-002', 'Proximity Sensor M18', 'Sensor', 'PCS', 12, 4, 'Rack B-02', 'PT Sensorik', 5, 320000, 1, NOW(), NOW()),
('CMMS-SP-003', 'V-Belt B52', 'Belt', 'PCS', 25, 8, 'Rack C-03', 'PT Belt Nusantara', 3, 125000, 0, NOW(), NOW()),
('CMMS-SP-004', 'Oil Filter OF-10', 'Filter', 'PCS', 16, 6, 'Rack A-04', 'PT Filter Prima', 4, 210000, 0, NOW(), NOW()),
('CMMS-SP-005', 'Contactor 24VDC', 'Electrical', 'PCS', 10, 3, 'Rack E-01', 'PT Panelindo', 6, 380000, 1, NOW(), NOW()),
('CMMS-SP-006', 'Bearing 6205', 'Bearing', 'PCS', 30, 10, 'Rack M-05', 'PT Bearing Jaya', 2, 90000, 0, NOW(), NOW()),
('CMMS-SP-007', 'Solenoid Valve 1/2 Inch', 'Pneumatic', 'PCS', 9, 3, 'Rack P-02', 'PT Pneumatic', 7, 520000, 1, NOW(), NOW()),
('CMMS-SP-008', 'Gasket EPDM DN50', 'Gasket', 'PCS', 40, 12, 'Rack G-01', 'PT Gasketindo', 3, 45000, 0, NOW(), NOW());

INSERT INTO preventive_schedules
(asset_id, schedule_name, schedule_type_id, frequency_type_id, frequency_value, next_due_date, last_generated_at, estimated_duration_minutes, checklist_template, is_active, created_at, updated_at)
SELECT a.id, d.schedule_name, pst.id, ft.id, d.frequency_value, d.next_due_date, d.last_generated_at, d.estimated_duration_minutes, d.checklist_template, 1, NOW(), NOW()
FROM (
    SELECT 'CMMS-AST-001' asset_code, 'Daily filler sanitation inspection' schedule_name, 'DAILY' schedule_type, 'DAILY' frequency_type, 1 frequency_value, '2026-05-02 08:00:00' next_due_date, NULL last_generated_at, 45 estimated_duration_minutes, 'Check nozzle, guard, leakage, and sanitizer readiness.' checklist_template
    UNION ALL SELECT 'CMMS-AST-002', 'Weekly labeler sensor cleaning', 'WEEKLY', 'WEEKLY', 1, '2026-05-04 09:00:00', NULL, 60, 'Clean label sensor and verify label gap detection.'
    UNION ALL SELECT 'CMMS-AST-003', 'Monthly case packer PM', 'MONTHLY', 'MONTHLY', 1, '2026-05-05 10:00:00', NULL, 120, 'Inspect gripper, vacuum, chain tension, and guarding.'
    UNION ALL SELECT 'CMMS-AST-005', 'Weekly compressor filter inspection', 'WEEKLY', 'WEEKLY', 1, '2026-05-06 08:30:00', NULL, 90, 'Check oil level, filter differential pressure, and drain.'
    UNION ALL SELECT 'CMMS-AST-006', 'Monthly boiler safety check', 'MONTHLY', 'MONTHLY', 1, '2026-05-07 13:00:00', NULL, 150, 'Verify burner, safety valve, water level, and blowdown.'
    UNION ALL SELECT 'CMMS-AST-008', 'Monthly chiller condenser cleaning', 'MONTHLY', 'MONTHLY', 1, '2026-05-08 08:00:00', NULL, 180, 'Clean condenser, check refrigerant sight glass, and trend approach.'
    UNION ALL SELECT 'CMMS-AST-009', 'Weekly transfer pump inspection', 'WEEKLY', 'WEEKLY', 1, '2026-05-11 09:00:00', NULL, 60, 'Check vibration, seal leak, motor current, and coupling.'
    UNION ALL SELECT 'CMMS-AST-011', 'Daily CIP valve route inspection', 'DAILY', 'DAILY', 1, '2026-05-12 07:30:00', NULL, 45, 'Inspect valve feedback and air leaks.'
    UNION ALL SELECT 'CMMS-AST-013', 'Monthly generator test run', 'MONTHLY', 'MONTHLY', 1, '2026-05-14 15:00:00', NULL, 120, 'Run generator, verify ATS, battery, coolant, and fuel.'
    UNION ALL SELECT 'CMMS-AST-015', 'Weekly WWTP pump inspection', 'WEEKLY', 'WEEKLY', 1, '2026-05-15 10:30:00', NULL, 75, 'Check impeller, level switch, vibration, and panel alarm.'
) d
JOIN assets a ON a.asset_code = d.asset_code
JOIN preventive_schedule_types pst ON pst.code = d.schedule_type
JOIN frequency_types ft ON ft.code = d.frequency_type;

INSERT INTO problem_reports
(report_number, asset_id, title, description, category_id, priority_id, status_id, reported_by, reported_at, downtime_start, downtime_end, downtime_minutes, created_at, updated_at)
SELECT d.report_number, a.id, d.title, d.title, prc.id, wop.id, wos.id, d.reported_by, d.reported_at, d.downtime_start, d.downtime_end, d.downtime_minutes, d.reported_at, d.reported_at
FROM (
    SELECT 'RPT-20260501-0001' report_number, 'CMMS-AST-001' asset_code, 'Filler nozzle dripping' title, 'Operator saw dripping on nozzle head 4.', 'BREAKDOWN' category_code, 'HIGH' priority_code, 'COMPLETED' status_code, 'Operator Line 1' reported_by, '2026-05-01 08:10:00' reported_at, NULL downtime_start, NULL downtime_end, NULL downtime_minutes
    UNION ALL SELECT 'RPT-20260503-0001', 'CMMS-AST-004', 'Conveyor chain abnormal noise', 'Noise from transfer conveyor during shift A.', 'BREAKDOWN', 'MEDIUM', 'COMPLETED', 'Operator Packaging', '2026-05-03 10:20:00', NULL, NULL, NULL
    UNION ALL SELECT 'RPT-20260505-0001', 'CMMS-AST-005', 'Compressor pressure drop', 'Air pressure dropped below line requirement.', 'DOWNTIME', 'URGENT', 'COMPLETED', 'Utility Operator', '2026-05-05 13:05:00', '2026-05-05 13:05:00', '2026-05-05 14:10:00', 65
    UNION ALL SELECT 'RPT-20260507-0001', 'CMMS-AST-009', 'Transfer pump seal leak', 'Product trace found near pump seal.', 'BREAKDOWN', 'HIGH', 'IN_PROGRESS', 'Process Operator', '2026-05-07 09:40:00', NULL, NULL, NULL
    UNION ALL SELECT 'RPT-20260509-0001', 'CMMS-AST-002', 'Label skew issue', 'Labels skewed on random bottles.', 'QUALITY', 'MEDIUM', 'PENDING', 'QA Inspector', '2026-05-09 11:25:00', NULL, NULL, NULL
    UNION ALL SELECT 'RPT-20260511-0001', 'CMMS-AST-006', 'Boiler low water alarm', 'Low water alarm appeared during startup.', 'SAFETY', 'URGENT', 'COMPLETED', 'Utility Operator', '2026-05-11 07:50:00', NULL, NULL, NULL
    UNION ALL SELECT 'RPT-20260513-0001', 'CMMS-AST-003', 'Case packer mispick', 'Vacuum cup failed to pick carton.', 'BREAKDOWN', 'HIGH', 'COMPLETED', 'Packing Operator', '2026-05-13 15:30:00', NULL, NULL, NULL
    UNION ALL SELECT 'RPT-20260515-0001', 'CMMS-AST-008', 'Chiller high approach', 'Chiller approach temperature higher than normal.', 'DOWNTIME', 'HIGH', 'COMPLETED', 'Utility Operator', '2026-05-15 12:15:00', '2026-05-15 12:15:00', '2026-05-15 13:05:00', 50
    UNION ALL SELECT 'RPT-20260517-0001', 'CMMS-AST-010', 'Mixer vibration trend high', 'Vibration trend exceeded warning threshold.', 'OTHER', 'MEDIUM', 'PENDING', 'Process Supervisor', '2026-05-17 16:10:00', NULL, NULL, NULL
    UNION ALL SELECT 'RPT-20260519-0001', 'CMMS-AST-012', 'Forklift horn intermittent', 'Horn does not always respond.', 'SAFETY', 'MEDIUM', 'PENDING', 'Warehouse Lead', '2026-05-19 09:35:00', NULL, NULL, NULL
    UNION ALL SELECT 'RPT-20260521-0001', 'CMMS-AST-013', 'Generator battery weak', 'Battery voltage low before test run.', 'BREAKDOWN', 'HIGH', 'IN_PROGRESS', 'Utility Operator', '2026-05-21 14:00:00', NULL, NULL, NULL
    UNION ALL SELECT 'RPT-20260523-0001', 'CMMS-AST-014', 'AHU air flow low', 'Production room airflow below setpoint.', 'QUALITY', 'MEDIUM', 'PENDING', 'Facility User', '2026-05-23 10:05:00', NULL, NULL, NULL
    UNION ALL SELECT 'RPT-20260525-0001', 'CMMS-AST-015', 'WWTP pump overload trip', 'Pump tripped on overload during rain.', 'DOWNTIME', 'HIGH', 'COMPLETED', 'WWTP Operator', '2026-05-25 18:20:00', '2026-05-25 18:20:00', '2026-05-25 19:00:00', 40
    UNION ALL SELECT 'RPT-20260527-0001', 'CMMS-AST-007', 'Cooling tower fan belt slip', 'Fan belt slip sound during peak load.', 'BREAKDOWN', 'MEDIUM', 'COMPLETED', 'Utility Operator', '2026-05-27 13:45:00', NULL, NULL, NULL
    UNION ALL SELECT 'RPT-20260530-0001', 'CMMS-AST-011', 'CIP valve feedback mismatch', 'Valve feedback mismatch on route 2.', 'OTHER', 'LOW', 'CANCELLED', 'CIP Operator', '2026-05-30 08:15:00', NULL, NULL, NULL
) d
JOIN assets a ON a.asset_code = d.asset_code
JOIN problem_report_categories prc ON prc.code = d.category_code
JOIN work_order_priorities wop ON wop.code = d.priority_code
JOIN work_order_statuses wos ON wos.code = d.status_code;

INSERT INTO work_orders
(wo_number, asset_id, problem_report_id, title, description, maintenance_type_id, priority_id, status_id, reported_by, assigned_to, reported_at, scheduled_at, started_at, completed_at, closed_at, downtime_start, downtime_end, downtime_minutes, repair_start, repair_end, repair_minutes, failure_code, root_cause, action_taken, result, created_at, updated_at, preventive_schedule_id, preventive_schedule_period_key)
SELECT d.wo_number, a.id, pr.id, d.title, d.description, mt.id, wop.id, wos.id, d.reported_by, t.id, d.reported_at, d.scheduled_at, d.started_at, d.completed_at, d.closed_at, d.downtime_start, d.downtime_end, d.downtime_minutes, d.repair_start, d.repair_end, d.repair_minutes, d.failure_code, d.root_cause, d.action_taken, d.result_text, d.created_at, d.updated_at, ps.id, d.pm_period_key
FROM (
    SELECT 'WO-20260501-0001' wo_number, 'CMMS-AST-001' asset_code, 'RPT-20260501-0001' report_number, 'Repair filler nozzle dripping' title, 'Check and replace nozzle seal.' description, 'BREAKDOWN' maintenance_code, 'HIGH' priority_code, 'CLOSED' status_code, 'Operator Line 1' reported_by, 'Andi Wijaya' technician_name, '2026-05-01 08:15:00' reported_at, '2026-05-01 08:30:00' scheduled_at, '2026-05-01 08:35:00' started_at, '2026-05-01 09:25:00' completed_at, '2026-05-01 09:40:00' closed_at, '2026-05-01 08:20:00' downtime_start, '2026-05-01 09:25:00' downtime_end, 65 downtime_minutes, '2026-05-01 08:35:00' repair_start, '2026-05-01 09:25:00' repair_end, 50 repair_minutes, 'SEAL_LEAK' failure_code, 'Seal worn' root_cause, 'Replaced seal and verified nozzle.' action_taken, 'Normal after test run.' result_text, '2026-05-01 08:15:00' created_at, '2026-05-01 09:40:00' updated_at, NULL schedule_name, NULL pm_period_key
    UNION ALL SELECT 'WO-20260502-0001', 'CMMS-AST-001', NULL, 'Daily filler sanitation inspection', 'Generated PM from daily schedule.', 'PREVENTIVE', 'MEDIUM', 'COMPLETED', 'SYSTEM', 'Budi Santoso', '2026-05-02 07:45:00', '2026-05-02 08:00:00', '2026-05-02 08:00:00', '2026-05-02 08:35:00', NULL, NULL, NULL, NULL, '2026-05-02 08:00:00', '2026-05-02 08:35:00', 35, NULL, NULL, 'Inspection completed.', 'All checkpoints OK.', '2026-05-02 07:45:00', '2026-05-02 08:35:00', 'Daily filler sanitation inspection', '1:DAILY:20260502'
    UNION ALL SELECT 'WO-20260503-0001', 'CMMS-AST-004', 'RPT-20260503-0001', 'Inspect conveyor chain noise', 'Lubricate and check chain tension.', 'CORRECTIVE', 'MEDIUM', 'CLOSED', 'Operator Packaging', 'Dedi Kurniawan', '2026-05-03 10:25:00', '2026-05-03 10:45:00', '2026-05-03 10:50:00', '2026-05-03 11:35:00', '2026-05-03 11:50:00', NULL, NULL, NULL, '2026-05-03 10:50:00', '2026-05-03 11:35:00', 45, 'CHAIN_NOISE', 'Lubrication dry', 'Lubricated and adjusted tension.', 'Noise reduced to normal.', '2026-05-03 10:25:00', '2026-05-03 11:50:00', NULL, NULL
    UNION ALL SELECT 'WO-20260504-0001', 'CMMS-AST-002', NULL, 'Weekly labeler sensor cleaning', 'Generated weekly PM.', 'PREVENTIVE', 'LOW', 'COMPLETED', 'SYSTEM', 'Eka Prasetyo', '2026-05-04 08:40:00', '2026-05-04 09:00:00', '2026-05-04 09:05:00', '2026-05-04 10:00:00', NULL, NULL, NULL, NULL, '2026-05-04 09:05:00', '2026-05-04 10:00:00', 55, NULL, NULL, 'Sensor cleaned and calibrated.', 'Label gap stable.', '2026-05-04 08:40:00', '2026-05-04 10:00:00', 'Weekly labeler sensor cleaning', '2:WEEKLY:20260504'
    UNION ALL SELECT 'WO-20260505-0001', 'CMMS-AST-005', 'RPT-20260505-0001', 'Restore compressor pressure', 'Investigate pressure drop and air leak.', 'BREAKDOWN', 'URGENT', 'CLOSED', 'Utility Operator', 'Citra Lestari', '2026-05-05 13:10:00', '2026-05-05 13:15:00', '2026-05-05 13:18:00', '2026-05-05 14:10:00', '2026-05-05 14:25:00', '2026-05-05 13:05:00', '2026-05-05 14:10:00', 65, '2026-05-05 13:18:00', '2026-05-05 14:10:00', 52, 'AIR_LEAK', 'Drain valve stuck open', 'Repaired drain valve and restarted compressor.', 'Pressure stable.', '2026-05-05 13:10:00', '2026-05-05 14:25:00', NULL, NULL
    UNION ALL SELECT 'WO-20260506-0001', 'CMMS-AST-005', NULL, 'Weekly compressor filter inspection', 'Generated PM.', 'PREVENTIVE', 'MEDIUM', 'COMPLETED', 'SYSTEM', 'Gita Permata', '2026-05-06 08:10:00', '2026-05-06 08:30:00', '2026-05-06 08:35:00', '2026-05-06 09:35:00', NULL, NULL, NULL, NULL, '2026-05-06 08:35:00', '2026-05-06 09:35:00', 60, NULL, NULL, 'Filter checked and drain cleaned.', 'PM completed.', '2026-05-06 08:10:00', '2026-05-06 09:35:00', 'Weekly compressor filter inspection', '4:WEEKLY:20260506'
    UNION ALL SELECT 'WO-20260507-0001', 'CMMS-AST-009', 'RPT-20260507-0001', 'Repair transfer pump seal leak', 'Replace pump seal after leak finding.', 'BREAKDOWN', 'HIGH', 'IN_PROGRESS', 'Process Operator', 'Andi Wijaya', '2026-05-07 09:45:00', '2026-05-07 10:00:00', '2026-05-07 10:05:00', NULL, NULL, NULL, NULL, NULL, '2026-05-07 10:05:00', NULL, NULL, 'SEAL_LEAK', 'Under investigation', 'Seal replacement ongoing.', NULL, '2026-05-07 09:45:00', '2026-05-07 10:05:00', NULL, NULL
    UNION ALL SELECT 'WO-20260508-0001', 'CMMS-AST-008', NULL, 'Chiller condenser cleaning', 'Generated monthly PM.', 'PREVENTIVE', 'MEDIUM', 'COMPLETED', 'SYSTEM', 'Gita Permata', '2026-05-08 07:45:00', '2026-05-08 08:00:00', '2026-05-08 08:10:00', '2026-05-08 10:50:00', NULL, NULL, NULL, NULL, '2026-05-08 08:10:00', '2026-05-08 10:50:00', 160, NULL, NULL, 'Condenser cleaned.', 'Approach temperature improved.', '2026-05-08 07:45:00', '2026-05-08 10:50:00', 'Monthly chiller condenser cleaning', '6:MONTHLY:20260508'
    UNION ALL SELECT 'WO-20260509-0001', 'CMMS-AST-002', 'RPT-20260509-0001', 'Correct label skew issue', 'Check sensor alignment and label roll tension.', 'CORRECTIVE', 'MEDIUM', 'ASSIGNED', 'QA Inspector', 'Eka Prasetyo', '2026-05-09 11:40:00', '2026-05-09 13:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-09 11:40:00', '2026-05-09 12:00:00', NULL, NULL
    UNION ALL SELECT 'WO-20260510-0001', 'CMMS-AST-010', NULL, 'Inspect mixer gearbox oil', 'Predictive inspection based on vibration trend.', 'PREDICTIVE', 'MEDIUM', 'COMPLETED', 'Maintenance Planner', 'Hendra Saputra', '2026-05-10 09:00:00', '2026-05-10 10:00:00', '2026-05-10 10:05:00', '2026-05-10 10:45:00', NULL, NULL, NULL, NULL, '2026-05-10 10:05:00', '2026-05-10 10:45:00', 40, NULL, NULL, 'Oil condition checked.', 'No abnormal particle found.', '2026-05-10 09:00:00', '2026-05-10 10:45:00', NULL, NULL
    UNION ALL SELECT 'WO-20260511-0001', 'CMMS-AST-006', 'RPT-20260511-0001', 'Verify boiler low water alarm', 'Inspect level sensor and low water trip.', 'CORRECTIVE', 'URGENT', 'CLOSED', 'Utility Operator', 'Citra Lestari', '2026-05-11 08:00:00', '2026-05-11 08:10:00', '2026-05-11 08:15:00', '2026-05-11 09:20:00', '2026-05-11 09:40:00', NULL, NULL, NULL, '2026-05-11 08:15:00', '2026-05-11 09:20:00', 65, 'LEVEL_SENSOR', 'Sensor fouling', 'Cleaned sensor chamber and tested alarm.', 'Alarm test passed.', '2026-05-11 08:00:00', '2026-05-11 09:40:00', NULL, NULL
    UNION ALL SELECT 'WO-20260512-0001', 'CMMS-AST-011', NULL, 'Daily CIP valve route inspection', 'Generated PM.', 'PREVENTIVE', 'LOW', 'COMPLETED', 'SYSTEM', 'Fajar Nugroho', '2026-05-12 07:15:00', '2026-05-12 07:30:00', '2026-05-12 07:35:00', '2026-05-12 08:05:00', NULL, NULL, NULL, NULL, '2026-05-12 07:35:00', '2026-05-12 08:05:00', 30, NULL, NULL, 'Valve feedback checked.', 'All routes normal.', '2026-05-12 07:15:00', '2026-05-12 08:05:00', 'Daily CIP valve route inspection', '8:DAILY:20260512'
    UNION ALL SELECT 'WO-20260513-0001', 'CMMS-AST-003', 'RPT-20260513-0001', 'Repair case packer vacuum cup', 'Replace worn vacuum cup and check air line.', 'BREAKDOWN', 'HIGH', 'CLOSED', 'Packing Operator', 'Dedi Kurniawan', '2026-05-13 15:35:00', '2026-05-13 15:45:00', '2026-05-13 15:50:00', '2026-05-13 16:40:00', '2026-05-13 16:55:00', '2026-05-13 15:30:00', '2026-05-13 16:40:00', 70, '2026-05-13 15:50:00', '2026-05-13 16:40:00', 50, 'VACUUM_LOSS', 'Vacuum cup worn', 'Replaced cup and adjusted vacuum pressure.', 'Mispick resolved.', '2026-05-13 15:35:00', '2026-05-13 16:55:00', NULL, NULL
    UNION ALL SELECT 'WO-20260514-0001', 'CMMS-AST-013', NULL, 'Generator monthly test run', 'Generated PM.', 'PREVENTIVE', 'MEDIUM', 'COMPLETED', 'SYSTEM', 'Budi Santoso', '2026-05-14 14:45:00', '2026-05-14 15:00:00', '2026-05-14 15:05:00', '2026-05-14 16:20:00', NULL, NULL, NULL, NULL, '2026-05-14 15:05:00', '2026-05-14 16:20:00', 75, NULL, NULL, 'Test run completed.', 'Generator response normal.', '2026-05-14 14:45:00', '2026-05-14 16:20:00', 'Monthly generator test run', '9:MONTHLY:20260514'
    UNION ALL SELECT 'WO-20260515-0001', 'CMMS-AST-008', 'RPT-20260515-0001', 'Investigate chiller high approach', 'Check condenser water flow and cleaning condition.', 'CORRECTIVE', 'HIGH', 'CLOSED', 'Utility Operator', 'Gita Permata', '2026-05-15 12:20:00', '2026-05-15 12:30:00', '2026-05-15 12:35:00', '2026-05-15 13:05:00', '2026-05-15 13:25:00', '2026-05-15 12:15:00', '2026-05-15 13:05:00', 50, '2026-05-15 12:35:00', '2026-05-15 13:05:00', 30, 'FLOW_LOW', 'Strainer dirty', 'Cleaned strainer and verified flow.', 'Approach back to normal.', '2026-05-15 12:20:00', '2026-05-15 13:25:00', NULL, NULL
    UNION ALL SELECT 'WO-20260516-0001', 'CMMS-AST-006', NULL, 'Boiler stack inspection', 'Monthly inspection of stack and burner flame.', 'INSPECTION', 'MEDIUM', 'COMPLETED', 'Maintenance Planner', 'Citra Lestari', '2026-05-16 08:00:00', '2026-05-16 09:00:00', '2026-05-16 09:10:00', '2026-05-16 10:30:00', NULL, NULL, NULL, NULL, '2026-05-16 09:10:00', '2026-05-16 10:30:00', 80, NULL, NULL, 'Inspection completed.', 'Combustion normal.', '2026-05-16 08:00:00', '2026-05-16 10:30:00', NULL, NULL
    UNION ALL SELECT 'WO-20260517-0001', 'CMMS-AST-010', 'RPT-20260517-0001', 'Analyze mixer vibration trend', 'Check bearing and coupling condition.', 'PREDICTIVE', 'MEDIUM', 'OPEN', 'Process Supervisor', NULL, '2026-05-17 16:25:00', '2026-05-18 09:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-17 16:25:00', '2026-05-17 16:25:00', NULL, NULL
    UNION ALL SELECT 'WO-20260518-0001', 'CMMS-AST-004', NULL, 'Vendor supervision conveyor belt vulcanizing', 'Supervise vendor work and safety permit readiness.', 'CONTRACTOR_SUPERVISION', 'HIGH', 'ASSIGNED', 'CMMS Administrator', 'Joko Firmansyah', '2026-05-18 08:00:00', '2026-05-18 13:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Prepare area and standby supervision.', NULL, '2026-05-18 08:00:00', '2026-05-18 08:10:00', NULL, NULL
    UNION ALL SELECT 'WO-20260519-0001', 'CMMS-AST-012', 'RPT-20260519-0001', 'Repair forklift horn', 'Check horn wiring and switch.', 'CORRECTIVE', 'MEDIUM', 'ASSIGNED', 'Warehouse Lead', 'Fajar Nugroho', '2026-05-19 09:40:00', '2026-05-19 11:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-19 09:40:00', '2026-05-19 10:00:00', NULL, NULL
    UNION ALL SELECT 'WO-20260520-0001', 'CMMS-AST-009', NULL, 'Weekly transfer pump inspection', 'Generated PM.', 'PREVENTIVE', 'LOW', 'COMPLETED', 'SYSTEM', 'Andi Wijaya', '2026-05-20 08:30:00', '2026-05-20 09:00:00', '2026-05-20 09:05:00', '2026-05-20 09:50:00', NULL, NULL, NULL, NULL, '2026-05-20 09:05:00', '2026-05-20 09:50:00', 45, NULL, NULL, 'Pump checked.', 'No abnormal vibration.', '2026-05-20 08:30:00', '2026-05-20 09:50:00', 'Weekly transfer pump inspection', '7:WEEKLY:20260520'
    UNION ALL SELECT 'WO-20260521-0001', 'CMMS-AST-013', 'RPT-20260521-0001', 'Replace generator battery', 'Battery weak before test run.', 'CORRECTIVE', 'HIGH', 'IN_PROGRESS', 'Utility Operator', 'Budi Santoso', '2026-05-21 14:10:00', '2026-05-21 15:00:00', '2026-05-21 15:05:00', NULL, NULL, NULL, NULL, NULL, '2026-05-21 15:05:00', NULL, NULL, 'BATTERY_LOW', 'Battery aged', 'Battery replacement in progress.', NULL, '2026-05-21 14:10:00', '2026-05-21 15:05:00', NULL, NULL
    UNION ALL SELECT 'WO-20260522-0001', 'CMMS-AST-015', NULL, 'WWTP pump inspection', 'Generated weekly PM.', 'PREVENTIVE', 'MEDIUM', 'COMPLETED', 'SYSTEM', 'Hendra Saputra', '2026-05-22 10:00:00', '2026-05-22 10:30:00', '2026-05-22 10:35:00', '2026-05-22 11:20:00', NULL, NULL, NULL, NULL, '2026-05-22 10:35:00', '2026-05-22 11:20:00', 45, NULL, NULL, 'Pump inspection completed.', 'All clear.', '2026-05-22 10:00:00', '2026-05-22 11:20:00', 'Weekly WWTP pump inspection', '10:WEEKLY:20260522'
    UNION ALL SELECT 'WO-20260523-0001', 'CMMS-AST-014', 'RPT-20260523-0001', 'Inspect AHU airflow low', 'Check filter, damper, and fan belt.', 'CORRECTIVE', 'MEDIUM', 'OPEN', 'Facility User', NULL, '2026-05-23 10:20:00', '2026-05-23 14:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-23 10:20:00', '2026-05-23 10:20:00', NULL, NULL
    UNION ALL SELECT 'WO-20260524-0001', 'CMMS-AST-007', NULL, 'Cooling tower basin cleaning vendor supervision', 'Vendor cleaning supervision and LOTO coordination.', 'CONTRACTOR_SUPERVISION', 'HIGH', 'COMPLETED', 'CMMS Administrator', 'Joko Firmansyah', '2026-05-24 07:30:00', '2026-05-24 08:00:00', '2026-05-24 08:05:00', '2026-05-24 12:00:00', NULL, NULL, NULL, NULL, '2026-05-24 08:05:00', '2026-05-24 12:00:00', 235, NULL, NULL, 'Supervised vendor cleaning and startup.', 'Work finished safely.', '2026-05-24 07:30:00', '2026-05-24 12:00:00', NULL, NULL
    UNION ALL SELECT 'WO-20260525-0001', 'CMMS-AST-015', 'RPT-20260525-0001', 'Reset WWTP pump overload trip', 'Inspect overload setting and pump impeller.', 'BREAKDOWN', 'HIGH', 'CLOSED', 'WWTP Operator', 'Hendra Saputra', '2026-05-25 18:25:00', '2026-05-25 18:30:00', '2026-05-25 18:35:00', '2026-05-25 19:00:00', '2026-05-25 19:20:00', '2026-05-25 18:20:00', '2026-05-25 19:00:00', 40, '2026-05-25 18:35:00', '2026-05-25 19:00:00', 25, 'OVERLOAD', 'Debris on impeller', 'Removed debris and reset overload.', 'Pump normal.', '2026-05-25 18:25:00', '2026-05-25 19:20:00', NULL, NULL
    UNION ALL SELECT 'WO-20260526-0001', 'CMMS-AST-003', NULL, 'Case packer monthly PM', 'Generated PM.', 'PREVENTIVE', 'MEDIUM', 'COMPLETED', 'SYSTEM', 'Dedi Kurniawan', '2026-05-26 09:30:00', '2026-05-26 10:00:00', '2026-05-26 10:10:00', '2026-05-26 11:50:00', NULL, NULL, NULL, NULL, '2026-05-26 10:10:00', '2026-05-26 11:50:00', 100, NULL, NULL, 'PM completed.', 'No abnormal finding.', '2026-05-26 09:30:00', '2026-05-26 11:50:00', 'Monthly case packer PM', '3:MONTHLY:20260526'
    UNION ALL SELECT 'WO-20260527-0001', 'CMMS-AST-007', 'RPT-20260527-0001', 'Adjust cooling tower fan belt', 'Retension belt and check pulley alignment.', 'CORRECTIVE', 'MEDIUM', 'CLOSED', 'Utility Operator', 'Gita Permata', '2026-05-27 14:00:00', '2026-05-27 14:30:00', '2026-05-27 14:35:00', '2026-05-27 15:10:00', '2026-05-27 15:25:00', NULL, NULL, NULL, '2026-05-27 14:35:00', '2026-05-27 15:10:00', 35, 'BELT_SLIP', 'Low belt tension', 'Adjusted belt tension.', 'Fan running smoothly.', '2026-05-27 14:00:00', '2026-05-27 15:25:00', NULL, NULL
    UNION ALL SELECT 'WO-20260528-0001', 'CMMS-AST-006', NULL, 'Boiler annual statutory inspection support', 'Support statutory vendor inspection.', 'CONTRACTOR_SUPERVISION', 'URGENT', 'PENDING', 'CMMS Administrator', 'Citra Lestari', '2026-05-28 08:00:00', '2026-05-28 09:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Waiting permit completion.', NULL, '2026-05-28 08:00:00', '2026-05-28 08:00:00', NULL, NULL
    UNION ALL SELECT 'WO-20260529-0001', 'CMMS-AST-005', NULL, 'Compressor vibration inspection', 'Predictive inspection after trend alert.', 'PREDICTIVE', 'MEDIUM', 'COMPLETED', 'Maintenance Planner', 'Andi Wijaya', '2026-05-29 13:00:00', '2026-05-29 14:00:00', '2026-05-29 14:05:00', '2026-05-29 14:40:00', NULL, NULL, NULL, NULL, '2026-05-29 14:05:00', '2026-05-29 14:40:00', 35, NULL, NULL, 'Vibration checked.', 'Trend acceptable.', '2026-05-29 13:00:00', '2026-05-29 14:40:00', NULL, NULL
    UNION ALL SELECT 'WO-20260530-0001', 'CMMS-AST-011', 'RPT-20260530-0001', 'Review CIP valve feedback report', 'Report cancelled after operator retest.', 'CORRECTIVE', 'LOW', 'CANCELLED', 'CIP Operator', NULL, '2026-05-30 08:30:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'No action required after retest.', 'Cancelled after verification.', '2026-05-30 08:30:00', '2026-05-30 09:00:00', NULL, NULL
    UNION ALL SELECT 'WO-20260531-0001', 'CMMS-AST-014', NULL, 'Facility AHU monthly inspection', 'Monthly facility inspection.', 'INSPECTION', 'MEDIUM', 'OPEN', 'Maintenance Planner', NULL, '2026-05-31 09:00:00', '2026-05-31 13:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-31 09:00:00', '2026-05-31 09:00:00', NULL, NULL
) d
JOIN assets a ON a.asset_code = d.asset_code
LEFT JOIN problem_reports pr ON pr.report_number = d.report_number
JOIN maintenance_types mt ON mt.code = d.maintenance_code
JOIN work_order_priorities wop ON wop.code = d.priority_code
JOIN work_order_statuses wos ON wos.code = d.status_code
LEFT JOIN technicians t ON t.name = d.technician_name
LEFT JOIN preventive_schedules ps ON ps.schedule_name = d.schedule_name;

INSERT INTO downtime_logs
(asset_id, work_order_id, problem_report_id, downtime_category_id, start_time, end_time, duration_minutes, description, created_at)
SELECT a.id, wo.id, pr.id, dc.id, d.start_time, d.end_time, d.duration_minutes, d.description, d.created_at
FROM (
    SELECT 'CMMS-AST-001' asset_code, 'WO-20260501-0001' wo_number, 'RPT-20260501-0001' report_number, 'MECHANICAL' category_code, '2026-05-01 08:20:00' start_time, '2026-05-01 09:25:00' end_time, 65 duration_minutes, 'Filler nozzle leak downtime.' description, '2026-05-01 09:25:00' created_at
    UNION ALL SELECT 'CMMS-AST-005', 'WO-20260505-0001', 'RPT-20260505-0001', 'UTILITY', '2026-05-05 13:05:00', '2026-05-05 14:10:00', 65, 'Air pressure drop affected line operation.', '2026-05-05 14:10:00'
    UNION ALL SELECT 'CMMS-AST-003', 'WO-20260513-0001', 'RPT-20260513-0001', 'MECHANICAL', '2026-05-13 15:30:00', '2026-05-13 16:40:00', 70, 'Case packer vacuum issue downtime.', '2026-05-13 16:40:00'
    UNION ALL SELECT 'CMMS-AST-008', 'WO-20260515-0001', 'RPT-20260515-0001', 'UTILITY', '2026-05-15 12:15:00', '2026-05-15 13:05:00', 50, 'Chiller high approach downtime.', '2026-05-15 13:05:00'
    UNION ALL SELECT 'CMMS-AST-015', 'WO-20260525-0001', 'RPT-20260525-0001', 'MECHANICAL', '2026-05-25 18:20:00', '2026-05-25 19:00:00', 40, 'WWTP pump overload trip.', '2026-05-25 19:00:00'
    UNION ALL SELECT 'CMMS-AST-004', NULL, NULL, 'OPERATIONAL', '2026-05-02 12:00:00', '2026-05-02 12:20:00', 20, 'Planned line clearance for sanitation.', '2026-05-02 12:20:00'
    UNION ALL SELECT 'CMMS-AST-006', NULL, NULL, 'UTILITY', '2026-05-06 06:30:00', '2026-05-06 07:05:00', 35, 'Steam pressure stabilization.', '2026-05-06 07:05:00'
    UNION ALL SELECT 'CMMS-AST-010', NULL, NULL, 'PROCESS', '2026-05-08 15:00:00', '2026-05-08 15:45:00', 45, 'Process hold for mixing parameter adjustment.', '2026-05-08 15:45:00'
    UNION ALL SELECT 'CMMS-AST-007', NULL, NULL, 'UTILITY', '2026-05-12 13:10:00', '2026-05-12 13:50:00', 40, 'Cooling tower basin level correction.', '2026-05-12 13:50:00'
    UNION ALL SELECT 'CMMS-AST-002', NULL, NULL, 'MATERIAL', '2026-05-14 10:30:00', '2026-05-14 10:50:00', 20, 'Label roll material splice issue.', '2026-05-14 10:50:00'
    UNION ALL SELECT 'CMMS-AST-011', NULL, NULL, 'OPERATIONAL', '2026-05-18 07:30:00', '2026-05-18 08:10:00', 40, 'CIP route waiting time.', '2026-05-18 08:10:00'
    UNION ALL SELECT 'CMMS-AST-013', NULL, NULL, 'ELECTRICAL', '2026-05-21 15:00:00', '2026-05-21 15:30:00', 30, 'Generator battery change standby.', '2026-05-21 15:30:00'
    UNION ALL SELECT 'CMMS-AST-014', NULL, NULL, 'OPERATIONAL', '2026-05-23 11:10:00', '2026-05-23 11:40:00', 30, 'Facility air balancing wait.', '2026-05-23 11:40:00'
    UNION ALL SELECT 'CMMS-AST-009', NULL, NULL, 'PROCESS', '2026-05-26 16:00:00', '2026-05-26 16:25:00', 25, 'Pump route switchover delay.', '2026-05-26 16:25:00'
    UNION ALL SELECT 'CMMS-AST-005', NULL, NULL, 'UTILITY', '2026-05-29 08:00:00', '2026-05-29 08:30:00', 30, 'Compressed air header check.', '2026-05-29 08:30:00'
) d
JOIN assets a ON a.asset_code = d.asset_code
LEFT JOIN work_orders wo ON wo.wo_number = d.wo_number
LEFT JOIN problem_reports pr ON pr.report_number = d.report_number
JOIN downtime_categories dc ON dc.code = d.category_code;

INSERT INTO work_order_photos
(work_order_id, file_name, content_type, size_bytes, file_data, uploaded_by, uploaded_at)
SELECT wo.id, d.file_name, 'image/png', 8, UNHEX('89504E470D0A1A0A'), d.uploaded_by, d.uploaded_at
FROM (
    SELECT 'WO-20260501-0001' wo_number, 'filler-nozzle-after.png' file_name, 'Andi Wijaya' uploaded_by, '2026-05-01 09:30:00' uploaded_at
    UNION ALL SELECT 'WO-20260505-0001', 'compressor-drain-valve.png', 'Citra Lestari', '2026-05-05 14:12:00'
    UNION ALL SELECT 'WO-20260513-0001', 'case-packer-vacuum-cup.png', 'Dedi Kurniawan', '2026-05-13 16:42:00'
    UNION ALL SELECT 'WO-20260524-0001', 'cooling-tower-vendor-work.png', 'Joko Firmansyah', '2026-05-24 12:05:00'
    UNION ALL SELECT 'WO-20260525-0001', 'wwtp-pump-impeller.png', 'Hendra Saputra', '2026-05-25 19:05:00'
) d
JOIN work_orders wo ON wo.wo_number = d.wo_number;

INSERT INTO work_order_spareparts
(work_order_id, sparepart_id, qty_used, used_by, used_at)
SELECT wo.id, sp.id, d.qty_used, d.used_by, d.used_at
FROM (
    SELECT 'WO-20260501-0001' wo_number, 'CMMS-SP-001' part_code, 1 qty_used, 'Andi Wijaya' used_by, '2026-05-01 09:20:00' used_at
    UNION ALL SELECT 'WO-20260503-0001', 'CMMS-SP-003', 1, 'Dedi Kurniawan', '2026-05-03 11:20:00'
    UNION ALL SELECT 'WO-20260505-0001', 'CMMS-SP-004', 1, 'Citra Lestari', '2026-05-05 14:00:00'
    UNION ALL SELECT 'WO-20260509-0001', 'CMMS-SP-002', 1, 'Eka Prasetyo', '2026-05-09 13:00:00'
    UNION ALL SELECT 'WO-20260511-0001', 'CMMS-SP-005', 1, 'Citra Lestari', '2026-05-11 09:10:00'
    UNION ALL SELECT 'WO-20260513-0001', 'CMMS-SP-007', 1, 'Dedi Kurniawan', '2026-05-13 16:30:00'
    UNION ALL SELECT 'WO-20260515-0001', 'CMMS-SP-008', 2, 'Gita Permata', '2026-05-15 12:55:00'
    UNION ALL SELECT 'WO-20260521-0001', 'CMMS-SP-005', 1, 'Budi Santoso', '2026-05-21 15:20:00'
    UNION ALL SELECT 'WO-20260525-0001', 'CMMS-SP-006', 2, 'Hendra Saputra', '2026-05-25 18:55:00'
    UNION ALL SELECT 'WO-20260527-0001', 'CMMS-SP-003', 1, 'Gita Permata', '2026-05-27 15:00:00'
) d
JOIN work_orders wo ON wo.wo_number = d.wo_number
JOIN spareparts sp ON sp.part_code = d.part_code;

INSERT INTO inventory_transactions
(sparepart_id, transaction_type, quantity, balance_after, reference_type, reference_id, performed_by, transaction_at, remarks)
SELECT sp.id, 'ISSUE', -d.qty_used, sp.stock_qty - d.qty_used, 'WORK_ORDER', wo.id, d.used_by, d.used_at, CONCAT('Dummy May 2026 usage for ', d.wo_number)
FROM (
    SELECT 'WO-20260501-0001' wo_number, 'CMMS-SP-001' part_code, 1 qty_used, 'Andi Wijaya' used_by, '2026-05-01 09:20:00' used_at
    UNION ALL SELECT 'WO-20260503-0001', 'CMMS-SP-003', 1, 'Dedi Kurniawan', '2026-05-03 11:20:00'
    UNION ALL SELECT 'WO-20260505-0001', 'CMMS-SP-004', 1, 'Citra Lestari', '2026-05-05 14:00:00'
    UNION ALL SELECT 'WO-20260513-0001', 'CMMS-SP-007', 1, 'Dedi Kurniawan', '2026-05-13 16:30:00'
    UNION ALL SELECT 'WO-20260525-0001', 'CMMS-SP-006', 2, 'Hendra Saputra', '2026-05-25 18:55:00'
) d
JOIN work_orders wo ON wo.wo_number = d.wo_number
JOIN spareparts sp ON sp.part_code = d.part_code;

INSERT INTO contractor_work_plans
(vendor_name, vendor_pic_name, vendor_pic_phone, worker_count, internal_pic_name, department_area, work_title, work_description, work_area, work_location, asset_id, additional_notes, start_at, end_at, estimated_duration_minutes, status, permit_document_status, working_at_height, hot_work, welding, electrical_work, confined_space, heavy_equipment_activity, chemical_handling, shutdown_activity, loto_required, need_safety_standby, created_by, created_at, updated_by, updated_at, permit_uploaded_by, permit_uploaded_at, work_order_id)
SELECT d.vendor_name, d.vendor_pic_name, d.vendor_pic_phone, d.worker_count, d.internal_pic_name, d.department_area, d.work_title, d.work_description, d.work_area, d.work_location, a.id, d.additional_notes, d.start_at, d.end_at, TIMESTAMPDIFF(MINUTE, d.start_at, d.end_at), d.status, d.permit_status, d.working_at_height, d.hot_work, d.welding, d.electrical_work, d.confined_space, d.heavy_equipment_activity, d.chemical_handling, d.shutdown_activity, d.loto_required, d.need_safety_standby, 'CMMS Administrator', d.created_at, 'CMMS Administrator', d.created_at, d.permit_uploaded_by, d.permit_uploaded_at, wo.id
FROM (
    SELECT 'PT Beltindo Teknik' vendor_name, 'Rizky Pratama' vendor_pic_name, '081288800001' vendor_pic_phone, 5 worker_count, 'Joko Firmansyah' internal_pic_name, 'Packaging' department_area, 'Conveyor belt vulcanizing' work_title, 'Vendor performs belt joint vulcanizing on transfer conveyor.' work_description, 'Packaging' work_area, 'Transfer Area B2' work_location, 'CMMS-AST-004' asset_code, 'Area needs barricade and LOTO.' additional_notes, '2026-05-18 13:00:00' start_at, '2026-05-18 17:00:00' end_at, 'READY_TO_START' status, 'UPLOADED' permit_status, 0 working_at_height, 1 hot_work, 0 welding, 1 electrical_work, 0 confined_space, 0 heavy_equipment_activity, 0 chemical_handling, 1 shutdown_activity, 1 loto_required, 1 need_safety_standby, '2026-05-17 10:00:00' created_at, 'Safety Admin' permit_uploaded_by, '2026-05-17 14:00:00' permit_uploaded_at, 'WO-20260518-0001' wo_number
    UNION ALL SELECT 'PT Cooling Service', 'Maman Suryana', '081288800002', 4, 'Joko Firmansyah', 'Utility', 'Cooling tower basin cleaning', 'Cleaning basin and nozzle inspection.', 'Utility', 'Outdoor Utility', 'CMMS-AST-007', 'Confined area access control required.', '2026-05-24 08:00:00', '2026-05-24 12:00:00', 'FINISHED', 'UPLOADED', 1, 0, 0, 0, 1, 0, 1, 1, 1, 1, '2026-05-23 09:00:00', 'Safety Admin', '2026-05-23 13:00:00', 'WO-20260524-0001'
    UNION ALL SELECT 'PT Boiler Inspeksi', 'Slamet Hidayat', '081288800003', 3, 'Citra Lestari', 'Utility', 'Boiler statutory inspection', 'Support statutory inspection and NDT.', 'Utility', 'Boiler Room', 'CMMS-AST-006', 'Waiting final permit document.', '2026-05-28 09:00:00', '2026-05-28 15:00:00', 'WAITING_PERMIT_DOCUMENT', 'NOT_UPLOADED', 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, '2026-05-27 11:00:00', NULL, NULL, 'WO-20260528-0001'
    UNION ALL SELECT 'PT HVAC Prima', 'Lina Oktavia', '081288800004', 2, 'Fajar Nugroho', 'Facility', 'AHU duct cleaning', 'Duct cleaning and filter replacement.', 'Facility', 'AHU Room', 'CMMS-AST-014', 'Weekend work request.', '2026-05-30 09:00:00', '2026-05-30 13:00:00', 'PLANNED', 'NEED_REVISION', 1, 0, 0, 1, 0, 0, 1, 0, 1, 0, '2026-05-29 10:30:00', 'Safety Admin', '2026-05-29 15:00:00', NULL
    UNION ALL SELECT 'PT Pump Specialist', 'Agus Salim', '081288800005', 3, 'Hendra Saputra', 'WWTP', 'WWTP pump alignment', 'Pump alignment and coupling inspection.', 'WWTP', 'WWTP Basin', 'CMMS-AST-015', 'Prepare standby pump.', '2026-05-31 08:00:00', '2026-05-31 12:00:00', 'PLANNED', 'EXPIRED', 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, '2026-05-25 10:00:00', 'Safety Admin', '2026-05-25 14:00:00', NULL
) d
JOIN assets a ON a.asset_code = d.asset_code
LEFT JOIN work_orders wo ON wo.wo_number = d.wo_number;

INSERT INTO contractor_work_documents
(contractor_work_plan_id, document_type, document_status, file_name, content_type, size_bytes, file_data, uploaded_by, uploaded_at, expires_at, notes)
SELECT cwp.id, d.document_type, d.document_status, d.file_name, 'application/pdf', 12, UNHEX('255044462D312E340A'), d.uploaded_by, d.uploaded_at, d.expires_at, d.notes
FROM (
    SELECT 'PT Beltindo Teknik' vendor_name, 'PERMIT' document_type, 'UPLOADED' document_status, 'permit-beltindo-20260518.pdf' file_name, 'Safety Admin' uploaded_by, '2026-05-17 14:00:00' uploaded_at, '2026-05-18 23:59:59' expires_at, 'Permit approved for planned work.' notes
    UNION ALL SELECT 'PT Cooling Service', 'JSA', 'UPLOADED', 'jsa-cooling-service-20260524.pdf', 'Safety Admin', '2026-05-23 13:00:00', '2026-05-24 23:59:59', 'JSA attached.'
    UNION ALL SELECT 'PT HVAC Prima', 'PERMIT', 'NEED_REVISION', 'permit-hvac-prima-20260530.pdf', 'Safety Admin', '2026-05-29 15:00:00', '2026-05-30 23:59:59', 'Need supervisor signature.'
    UNION ALL SELECT 'PT Pump Specialist', 'PERMIT', 'EXPIRED', 'permit-pump-specialist-20260531.pdf', 'Safety Admin', '2026-05-25 14:00:00', '2026-05-28 23:59:59', 'Expired before work date.'
) d
JOIN contractor_work_plans cwp ON cwp.vendor_name = d.vendor_name;

INSERT INTO contractor_work_audits
(contractor_work_plan_id, action, field_name, old_value, new_value, performed_by, created_at)
SELECT cwp.id, d.action, d.field_name, d.old_value, d.new_value, d.performed_by, d.created_at
FROM (
    SELECT 'PT Beltindo Teknik' vendor_name, 'CREATE' action, NULL field_name, NULL old_value, 'READY_TO_START' new_value, 'CMMS Administrator' performed_by, '2026-05-17 10:00:00' created_at
    UNION ALL SELECT 'PT Cooling Service', 'STATUS_CHANGE', 'status', 'ONGOING', 'FINISHED', 'Joko Firmansyah', '2026-05-24 12:00:00'
    UNION ALL SELECT 'PT Boiler Inspeksi', 'CREATE', NULL, NULL, 'WAITING_PERMIT_DOCUMENT', 'CMMS Administrator', '2026-05-27 11:00:00'
    UNION ALL SELECT 'PT HVAC Prima', 'DOCUMENT_UPLOAD', 'permit_document_status', 'NOT_UPLOADED', 'NEED_REVISION', 'Safety Admin', '2026-05-29 15:00:00'
) d
JOIN contractor_work_plans cwp ON cwp.vendor_name = d.vendor_name;
