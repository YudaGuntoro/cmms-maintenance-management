INSERT INTO assets
(asset_code, asset_name, asset_type, plant, area, production_line, location, manufacturer, model, serial_number, installation_date, criticality_level, status, created_at, updated_at)
VALUES
('DEMO-AST-001', 'Cooling Water Pump P-101', 'Pump', 'Jakarta Manufacturing Plant', 'Utility', 'Utility Line C', 'UT-C-11', 'Grundfos', 'CRN-45', 'DM-PUMP-101', '2022-02-14', 'HIGH', 'ACTIVE', '2026-05-01 07:30:00', '2026-05-16 10:00:00'),
('DEMO-AST-002', 'Filling Machine F-202', 'Filler', 'Jakarta Manufacturing Plant', 'Packaging', 'Packaging Line A', 'PK-A-11', 'Krones', 'FILL-X2', 'DM-FILL-202', '2021-09-08', 'CRITICAL', 'ACTIVE', '2026-05-01 07:30:00', '2026-05-16 10:00:00'),
('DEMO-AST-003', 'Air Compressor AC-303', 'Compressor', 'Cikarang Manufacturing Plant', 'Utility', 'Utility Line C', 'UT-C-12', 'Atlas Copco', 'GA90', 'DM-COMP-303', '2020-06-11', 'CRITICAL', 'UNDER_MAINTENANCE', '2026-05-01 07:30:00', '2026-05-16 10:00:00'),
('DEMO-AST-004', 'Case Conveyor CV-404', 'Conveyor', 'Jakarta Manufacturing Plant', 'Packaging', 'Packaging Line A', 'PK-A-12', 'Interroll', 'CVX-40', 'DM-CV-404', '2023-01-19', 'MEDIUM', 'ACTIVE', '2026-05-01 07:30:00', '2026-05-16 10:00:00'),
('DEMO-AST-005', 'Steam Boiler BL-505', 'Boiler', 'Cikarang Manufacturing Plant', 'Utility', 'Utility Line C', 'UT-C-13', 'Miura', 'LX-250', 'DM-BLR-505', '2019-12-03', 'CRITICAL', 'ACTIVE', '2026-05-01 07:30:00', '2026-05-16 10:00:00')
ON DUPLICATE KEY UPDATE
asset_name = VALUES(asset_name),
asset_type = VALUES(asset_type),
plant = VALUES(plant),
area = VALUES(area),
production_line = VALUES(production_line),
location = VALUES(location),
manufacturer = VALUES(manufacturer),
model = VALUES(model),
criticality_level = VALUES(criticality_level),
status = VALUES(status),
updated_at = VALUES(updated_at);

INSERT INTO technicians
(employee_no, name, email, phone, skill_type, shift, status, created_at, updated_at)
VALUES
('DEMO-TECH-001', 'Arif Pratama', 'arif.pratama@cmms.local', '0812300101', 'MECHANICAL', 'A', 'ACTIVE', '2026-05-01 07:30:00', '2026-05-16 10:00:00'),
('DEMO-TECH-002', 'Maya Lestari', 'maya.lestari@cmms.local', '0812300102', 'ELECTRICAL', 'B', 'ACTIVE', '2026-05-01 07:30:00', '2026-05-16 10:00:00'),
('DEMO-TECH-003', 'Yoga Saputra', 'yoga.saputra@cmms.local', '0812300103', 'UTILITY', 'C', 'ACTIVE', '2026-05-01 07:30:00', '2026-05-16 10:00:00'),
('DEMO-TECH-004', 'Nadia Paramita', 'nadia.paramita@cmms.local', '0812300104', 'GENERAL', 'A', 'ACTIVE', '2026-05-01 07:30:00', '2026-05-16 10:00:00')
ON DUPLICATE KEY UPDATE
name = VALUES(name),
email = VALUES(email),
phone = VALUES(phone),
skill_type = VALUES(skill_type),
shift = VALUES(shift),
status = VALUES(status),
updated_at = VALUES(updated_at);

INSERT INTO spareparts
(part_code, part_name, category, unit, stock_qty, minimum_stock, location, supplier, lead_time_days, price, is_critical, created_at, updated_at)
VALUES
('DEMO-SP-BRG-6306', 'Bearing 6306 ZZ', 'Bearing', 'PCS', 4, 8, 'WH-DM-A1', 'SKF Indonesia', 7, 185000, 1, '2026-05-01 07:30:00', '2026-05-16 10:00:00'),
('DEMO-SP-SEAL-25', 'Mechanical Seal 25mm', 'Seal', 'PCS', 3, 4, 'WH-DM-M1', 'Local Supplier', 5, 325000, 1, '2026-05-01 07:30:00', '2026-05-16 10:00:00'),
('DEMO-SP-SNS-PROX', 'Proximity Sensor M18', 'Sensor', 'PCS', 7, 6, 'WH-DM-E1', 'Omron', 14, 510000, 1, '2026-05-01 07:30:00', '2026-05-16 10:00:00'),
('DEMO-SP-BELT-B52', 'V-Belt B52', 'Belt', 'PCS', 12, 8, 'WH-DM-A2', 'Mitsuboshi', 6, 145000, 1, '2026-05-01 07:30:00', '2026-05-16 10:00:00'),
('DEMO-SP-FLT-OIL', 'Oil Filter Element', 'Filter', 'PCS', 5, 6, 'WH-DM-U1', 'Atlas Copco', 10, 390000, 1, '2026-05-01 07:30:00', '2026-05-16 10:00:00'),
('DEMO-SP-LUBE-68', 'Gear Oil ISO VG 68', 'Lubricant', 'L', 60, 40, 'WH-DM-L1', 'Shell', 3, 72000, 0, '2026-05-01 07:30:00', '2026-05-16 10:00:00')
ON DUPLICATE KEY UPDATE
part_name = VALUES(part_name),
category = VALUES(category),
unit = VALUES(unit),
stock_qty = VALUES(stock_qty),
minimum_stock = VALUES(minimum_stock),
location = VALUES(location),
supplier = VALUES(supplier),
lead_time_days = VALUES(lead_time_days),
price = VALUES(price),
is_critical = VALUES(is_critical),
updated_at = VALUES(updated_at);

INSERT INTO failure_codes (code, name, category, description)
VALUES
('DEMO-MECH-SEAL', 'Seal Leakage', 'MECHANICAL', 'Leakage from pump or mixer mechanical seal'),
('DEMO-ELEC-PLC', 'PLC I/O Fault', 'ELECTRICAL', 'PLC input output module intermittent or failed'),
('DEMO-UTIL-PRS', 'Utility Pressure Drop', 'UTILITY', 'Utility pressure below operating threshold'),
('DEMO-PROC-JAM', 'Product Flow Jam', 'OPERATIONAL', 'Product or packaging jam on conveyor or filler')
ON DUPLICATE KEY UPDATE
name = VALUES(name),
category = VALUES(category),
description = VALUES(description);

INSERT INTO root_causes (code, name, description)
VALUES
('DEMO-WEAR', 'Component Wear', 'Component reached expected wear life'),
('DEMO-DIRTY', 'Dirty Contact Surface', 'Sensor or contact surface contaminated by dust or oil'),
('DEMO-SETUP', 'Incorrect Machine Setup', 'Guide rail or machine parameter not set to standard'),
('DEMO-PM-MISS', 'Missed Preventive Maintenance', 'Preventive maintenance was delayed or incomplete')
ON DUPLICATE KEY UPDATE
name = VALUES(name),
description = VALUES(description);

INSERT INTO preventive_schedules
(asset_id, schedule_name, frequency_type, frequency_value, next_due_date, last_generated_at, estimated_duration_minutes, checklist_template, is_active, created_at, updated_at)
SELECT a.id, 'Demo weekly filler lubrication', 'WEEKLY', 1, '2026-05-17 08:00:00', '2026-05-10 08:55:00', 45, 'Inspect nozzle, lubricate bearing, check actuator movement', 1, '2026-05-01 07:30:00', '2026-05-16 10:00:00'
FROM assets a
WHERE a.asset_code = 'DEMO-AST-002'
  AND NOT EXISTS (SELECT 1 FROM preventive_schedules ps WHERE ps.asset_id = a.id AND ps.schedule_name = 'Demo weekly filler lubrication');

INSERT INTO preventive_schedules
(asset_id, schedule_name, frequency_type, frequency_value, next_due_date, last_generated_at, estimated_duration_minutes, checklist_template, is_active, created_at, updated_at)
SELECT a.id, 'Demo compressor monthly service', 'MONTHLY', 1, '2026-05-24 09:00:00', NULL, 120, 'Check oil level, clean filter, inspect pressure switch', 1, '2026-05-01 07:30:00', '2026-05-16 10:00:00'
FROM assets a
WHERE a.asset_code = 'DEMO-AST-003'
  AND NOT EXISTS (SELECT 1 FROM preventive_schedules ps WHERE ps.asset_id = a.id AND ps.schedule_name = 'Demo compressor monthly service');

INSERT INTO preventive_schedules
(asset_id, schedule_name, frequency_type, frequency_value, next_due_date, last_generated_at, estimated_duration_minutes, checklist_template, is_active, created_at, updated_at)
SELECT a.id, 'Demo boiler safety inspection', 'MONTHLY', 1, '2026-05-28 08:00:00', NULL, 150, 'Check burner, water level, safety valve, blowdown record', 1, '2026-05-01 07:30:00', '2026-05-16 10:00:00'
FROM assets a
WHERE a.asset_code = 'DEMO-AST-005'
  AND NOT EXISTS (SELECT 1 FROM preventive_schedules ps WHERE ps.asset_id = a.id AND ps.schedule_name = 'Demo boiler safety inspection');

INSERT INTO work_orders
(wo_number, asset_id, title, description, maintenance_type, priority, status, reported_by, assigned_to, reported_at, scheduled_at, started_at, completed_at, closed_at, downtime_start, downtime_end, downtime_minutes, repair_start, repair_end, repair_minutes, failure_code, root_cause, action_taken, created_at, updated_at, preventive_schedule_id, preventive_schedule_period_key)
VALUES
('DEMO-WO-20260501-001', (SELECT id FROM assets WHERE asset_code = 'DEMO-AST-001'), 'Pump seal leakage after startup', 'Cooling water pump seal area leaking during morning startup.', 'BREAKDOWN', 'HIGH', 'CLOSED', 'Operator Utility', (SELECT id FROM technicians WHERE employee_no = 'DEMO-TECH-001'), '2026-05-01 08:05:00', NULL, '2026-05-01 08:20:00', '2026-05-01 09:30:00', '2026-05-01 09:45:00', '2026-05-01 08:05:00', '2026-05-01 09:45:00', 100, '2026-05-01 08:20:00', '2026-05-01 09:30:00', 70, 'DEMO-MECH-SEAL', 'DEMO-WEAR', 'Replaced mechanical seal and verified leakage stopped.', '2026-05-01 08:05:00', '2026-05-01 09:45:00', NULL, NULL),
('DEMO-WO-20260503-001', (SELECT id FROM assets WHERE asset_code = 'DEMO-AST-002'), 'Filler proximity sensor unstable', 'Bottle detection sometimes missing at infeed area.', 'CORRECTIVE', 'MEDIUM', 'COMPLETED', 'Line Leader A', (SELECT id FROM technicians WHERE employee_no = 'DEMO-TECH-002'), '2026-05-03 10:10:00', '2026-05-03 11:00:00', '2026-05-03 11:05:00', '2026-05-03 11:50:00', NULL, NULL, NULL, NULL, '2026-05-03 11:05:00', '2026-05-03 11:50:00', 45, 'DEMO-ELEC-PLC', 'DEMO-DIRTY', 'Cleaned sensor lens and tightened connector.', '2026-05-03 10:10:00', '2026-05-03 11:50:00', NULL, NULL),
('DEMO-WO-20260505-001', (SELECT id FROM assets WHERE asset_code = 'DEMO-AST-003'), 'Compressor pressure drop', 'Plant air pressure dropped below process threshold.', 'BREAKDOWN', 'URGENT', 'CLOSED', 'Utility Operator', (SELECT id FROM technicians WHERE employee_no = 'DEMO-TECH-003'), '2026-05-05 13:00:00', NULL, '2026-05-05 13:12:00', '2026-05-05 15:00:00', '2026-05-05 15:15:00', '2026-05-05 13:00:00', '2026-05-05 15:15:00', 135, '2026-05-05 13:12:00', '2026-05-05 15:00:00', 108, 'DEMO-UTIL-PRS', 'DEMO-PM-MISS', 'Changed oil filter and adjusted pressure switch.', '2026-05-05 13:00:00', '2026-05-05 15:15:00', NULL, NULL),
('DEMO-WO-20260507-001', (SELECT id FROM assets WHERE asset_code = 'DEMO-AST-004'), 'Case conveyor jam', 'Carton jam at transfer point before case packer.', 'BREAKDOWN', 'HIGH', 'CLOSED', 'Operator Packaging', (SELECT id FROM technicians WHERE employee_no = 'DEMO-TECH-004'), '2026-05-07 16:20:00', NULL, '2026-05-07 16:28:00', '2026-05-07 17:05:00', '2026-05-07 17:20:00', '2026-05-07 16:20:00', '2026-05-07 17:20:00', 60, '2026-05-07 16:28:00', '2026-05-07 17:05:00', 37, 'DEMO-PROC-JAM', 'DEMO-SETUP', 'Cleared jam and reset guide rail width.', '2026-05-07 16:20:00', '2026-05-07 17:20:00', NULL, NULL),
('DEMO-WO-20260510-001', (SELECT id FROM assets WHERE asset_code = 'DEMO-AST-002'), 'Demo weekly filler lubrication', 'Generated preventive maintenance sample for weekly filler lubrication.', 'PREVENTIVE', 'MEDIUM', 'CLOSED', 'SYSTEM', (SELECT id FROM technicians WHERE employee_no = 'DEMO-TECH-001'), '2026-05-10 08:00:00', '2026-05-10 08:00:00', '2026-05-10 08:10:00', '2026-05-10 08:55:00', '2026-05-10 09:00:00', NULL, NULL, NULL, '2026-05-10 08:10:00', '2026-05-10 08:55:00', 45, NULL, NULL, 'Lubrication checklist completed.', '2026-05-10 08:00:00', '2026-05-10 09:00:00', (SELECT id FROM preventive_schedules WHERE schedule_name = 'Demo weekly filler lubrication' LIMIT 1), 'DEMO:WEEKLY:20260510'),
('DEMO-WO-20260512-001', (SELECT id FROM assets WHERE asset_code = 'DEMO-AST-005'), 'Boiler flame scanner inspection', 'Inspect flame scanner signal before planned production ramp up.', 'INSPECTION', 'LOW', 'ASSIGNED', 'Supervisor Utility', (SELECT id FROM technicians WHERE employee_no = 'DEMO-TECH-003'), '2026-05-12 09:30:00', '2026-05-16 14:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-12 09:30:00', '2026-05-12 09:30:00', NULL, NULL),
('DEMO-WO-20260514-001', (SELECT id FROM assets WHERE asset_code = 'DEMO-AST-003'), 'Compressor abnormal oil temperature', 'Oil temperature trend is increasing after lunch shift.', 'PREDICTIVE', 'HIGH', 'IN_PROGRESS', 'Planner', (SELECT id FROM technicians WHERE employee_no = 'DEMO-TECH-003'), '2026-05-14 13:15:00', '2026-05-14 15:00:00', '2026-05-14 15:05:00', NULL, NULL, NULL, NULL, NULL, '2026-05-14 15:05:00', NULL, NULL, NULL, NULL, NULL, '2026-05-14 13:15:00', '2026-05-14 15:05:00', NULL, NULL),
('DEMO-WO-20260515-001', (SELECT id FROM assets WHERE asset_code = 'DEMO-AST-001'), 'Pump coupling alignment check', 'Corrective action from vibration trend review.', 'CORRECTIVE', 'MEDIUM', 'OPEN', 'Reliability Engineer', NULL, '2026-05-15 08:40:00', '2026-05-18 09:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-15 08:40:00', '2026-05-15 08:40:00', NULL, NULL),
('DEMO-WO-20260516-001', (SELECT id FROM assets WHERE asset_code = 'DEMO-AST-004'), 'Conveyor belt tracking adjustment', 'Belt tracking slightly drifting to motor side.', 'CORRECTIVE', 'LOW', 'DRAFT', 'Planner', NULL, '2026-05-16 09:20:00', '2026-05-20 10:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-16 09:20:00', '2026-05-16 09:20:00', NULL, NULL)
ON DUPLICATE KEY UPDATE
asset_id = VALUES(asset_id),
title = VALUES(title),
description = VALUES(description),
maintenance_type = VALUES(maintenance_type),
priority = VALUES(priority),
status = VALUES(status),
reported_by = VALUES(reported_by),
assigned_to = VALUES(assigned_to),
reported_at = VALUES(reported_at),
scheduled_at = VALUES(scheduled_at),
started_at = VALUES(started_at),
completed_at = VALUES(completed_at),
closed_at = VALUES(closed_at),
downtime_start = VALUES(downtime_start),
downtime_end = VALUES(downtime_end),
downtime_minutes = VALUES(downtime_minutes),
repair_start = VALUES(repair_start),
repair_end = VALUES(repair_end),
repair_minutes = VALUES(repair_minutes),
failure_code = VALUES(failure_code),
root_cause = VALUES(root_cause),
action_taken = VALUES(action_taken),
updated_at = VALUES(updated_at),
preventive_schedule_id = VALUES(preventive_schedule_id),
preventive_schedule_period_key = VALUES(preventive_schedule_period_key);

INSERT INTO downtime_logs
(asset_id, work_order_id, downtime_category, start_time, end_time, duration_minutes, description, created_at)
VALUES
((SELECT asset_id FROM work_orders WHERE wo_number = 'DEMO-WO-20260501-001'), (SELECT id FROM work_orders WHERE wo_number = 'DEMO-WO-20260501-001'), 'MECHANICAL', '2026-05-01 08:05:00', '2026-05-01 09:45:00', 100, 'Demo pump seal leakage downtime', '2026-05-01 09:45:00'),
((SELECT asset_id FROM work_orders WHERE wo_number = 'DEMO-WO-20260505-001'), (SELECT id FROM work_orders WHERE wo_number = 'DEMO-WO-20260505-001'), 'UTILITY', '2026-05-05 13:00:00', '2026-05-05 15:15:00', 135, 'Demo compressor pressure drop downtime', '2026-05-05 15:15:00'),
((SELECT asset_id FROM work_orders WHERE wo_number = 'DEMO-WO-20260507-001'), (SELECT id FROM work_orders WHERE wo_number = 'DEMO-WO-20260507-001'), 'OPERATIONAL', '2026-05-07 16:20:00', '2026-05-07 17:20:00', 60, 'Demo conveyor jam downtime', '2026-05-07 17:20:00')
ON DUPLICATE KEY UPDATE
asset_id = VALUES(asset_id),
downtime_category = VALUES(downtime_category),
start_time = VALUES(start_time),
end_time = VALUES(end_time),
duration_minutes = VALUES(duration_minutes),
description = VALUES(description),
created_at = VALUES(created_at);

DELETE wosp
FROM work_order_spareparts wosp
JOIN work_orders wo ON wo.id = wosp.work_order_id
WHERE wo.wo_number LIKE 'DEMO-WO-202605%';

DELETE FROM inventory_transactions
WHERE remarks LIKE 'Demo May 2026%';

INSERT INTO work_order_spareparts (work_order_id, sparepart_id, qty_used, used_by, used_at)
VALUES
((SELECT id FROM work_orders WHERE wo_number = 'DEMO-WO-20260501-001'), (SELECT id FROM spareparts WHERE part_code = 'DEMO-SP-SEAL-25'), 1, 'Arif Pratama', '2026-05-01 09:05:00'),
((SELECT id FROM work_orders WHERE wo_number = 'DEMO-WO-20260503-001'), (SELECT id FROM spareparts WHERE part_code = 'DEMO-SP-SNS-PROX'), 1, 'Maya Lestari', '2026-05-03 11:35:00'),
((SELECT id FROM work_orders WHERE wo_number = 'DEMO-WO-20260505-001'), (SELECT id FROM spareparts WHERE part_code = 'DEMO-SP-FLT-OIL'), 1, 'Yoga Saputra', '2026-05-05 14:20:00'),
((SELECT id FROM work_orders WHERE wo_number = 'DEMO-WO-20260507-001'), (SELECT id FROM spareparts WHERE part_code = 'DEMO-SP-BELT-B52'), 1, 'Nadia Paramita', '2026-05-07 16:55:00');

INSERT INTO inventory_transactions (sparepart_id, transaction_type, quantity, balance_after, reference_type, reference_id, performed_by, transaction_at, remarks)
VALUES
((SELECT id FROM spareparts WHERE part_code = 'DEMO-SP-SEAL-25'), 'ISSUE', -1, 3, 'WORK_ORDER', (SELECT id FROM work_orders WHERE wo_number = 'DEMO-WO-20260501-001'), 'Arif Pratama', '2026-05-01 09:05:00', 'Demo May 2026 seal usage'),
((SELECT id FROM spareparts WHERE part_code = 'DEMO-SP-SNS-PROX'), 'ISSUE', -1, 7, 'WORK_ORDER', (SELECT id FROM work_orders WHERE wo_number = 'DEMO-WO-20260503-001'), 'Maya Lestari', '2026-05-03 11:35:00', 'Demo May 2026 sensor usage'),
((SELECT id FROM spareparts WHERE part_code = 'DEMO-SP-FLT-OIL'), 'ISSUE', -1, 5, 'WORK_ORDER', (SELECT id FROM work_orders WHERE wo_number = 'DEMO-WO-20260505-001'), 'Yoga Saputra', '2026-05-05 14:20:00', 'Demo May 2026 oil filter usage'),
((SELECT id FROM spareparts WHERE part_code = 'DEMO-SP-BELT-B52'), 'ISSUE', -1, 12, 'WORK_ORDER', (SELECT id FROM work_orders WHERE wo_number = 'DEMO-WO-20260507-001'), 'Nadia Paramita', '2026-05-07 16:55:00', 'Demo May 2026 belt usage');
