CREATE TABLE IF NOT EXISTS plants (
    id INT AUTO_INCREMENT PRIMARY KEY,
    plant_code VARCHAR(50) NOT NULL,
    plant_name VARCHAR(150) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_plants_plant_code (plant_code)
);

CREATE TABLE IF NOT EXISTS production_lines (
    id INT AUTO_INCREMENT PRIMARY KEY,
    plant_id INT NOT NULL,
    line_code VARCHAR(50) NOT NULL,
    line_name VARCHAR(150) NOT NULL,
    area VARCHAR(100) NOT NULL,
    UNIQUE KEY uq_production_lines_line_code (line_code),
    CONSTRAINT fk_production_lines_plants FOREIGN KEY (plant_id) REFERENCES plants(id)
);

CREATE TABLE IF NOT EXISTS assets (
    id INT AUTO_INCREMENT PRIMARY KEY,
    asset_code VARCHAR(80) NOT NULL,
    asset_name VARCHAR(200) NOT NULL,
    asset_type VARCHAR(100) NOT NULL,
    plant VARCHAR(100) NOT NULL,
    area VARCHAR(100) NOT NULL,
    production_line VARCHAR(100) NOT NULL,
    location VARCHAR(200) NOT NULL,
    manufacturer VARCHAR(150) NULL,
    model VARCHAR(150) NULL,
    serial_number VARCHAR(150) NULL,
    installation_date DATETIME NULL,
    criticality_level VARCHAR(30) NOT NULL,
    status VARCHAR(30) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_assets_asset_code (asset_code)
);

CREATE TABLE IF NOT EXISTS technicians (
    id INT AUTO_INCREMENT PRIMARY KEY,
    employee_no VARCHAR(80) NOT NULL,
    name VARCHAR(150) NOT NULL,
    email VARCHAR(150) NULL,
    phone VARCHAR(50) NULL,
    skill_type VARCHAR(30) NOT NULL,
    shift VARCHAR(50) NOT NULL,
    status VARCHAR(30) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_technicians_employee_no (employee_no)
);

CREATE TABLE IF NOT EXISTS preventive_schedules (
    id INT AUTO_INCREMENT PRIMARY KEY,
    asset_id INT NOT NULL,
    schedule_name VARCHAR(200) NOT NULL,
    frequency_type VARCHAR(30) NOT NULL,
    frequency_value INT NOT NULL,
    next_due_date DATETIME NOT NULL,
    last_generated_at DATETIME NULL,
    estimated_duration_minutes INT NULL,
    checklist_template TEXT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_pm_schedules_assets FOREIGN KEY (asset_id) REFERENCES assets(id)
);

CREATE TABLE IF NOT EXISTS work_orders (
    id INT AUTO_INCREMENT PRIMARY KEY,
    wo_number VARCHAR(80) NOT NULL,
    asset_id INT NOT NULL,
    title VARCHAR(200) NOT NULL,
    description TEXT NULL,
    maintenance_type VARCHAR(30) NOT NULL,
    priority VARCHAR(30) NOT NULL,
    status VARCHAR(30) NOT NULL,
    reported_by VARCHAR(150) NULL,
    assigned_to INT NULL,
    reported_at DATETIME NULL,
    scheduled_at DATETIME NULL,
    started_at DATETIME NULL,
    completed_at DATETIME NULL,
    closed_at DATETIME NULL,
    downtime_start DATETIME NULL,
    downtime_end DATETIME NULL,
    downtime_minutes INT NULL,
    repair_start DATETIME NULL,
    repair_end DATETIME NULL,
    repair_minutes INT NULL,
    failure_code VARCHAR(80) NULL,
    root_cause VARCHAR(80) NULL,
    action_taken TEXT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    preventive_schedule_id INT NULL,
    preventive_schedule_period_key VARCHAR(120) NULL,
    UNIQUE KEY uq_work_orders_wo_number (wo_number),
    UNIQUE KEY uq_work_orders_pm_period (preventive_schedule_id, preventive_schedule_period_key),
    KEY ix_work_orders_asset_id (asset_id),
    KEY ix_work_orders_status (status),
    CONSTRAINT fk_work_orders_assets FOREIGN KEY (asset_id) REFERENCES assets(id),
    CONSTRAINT fk_work_orders_technicians FOREIGN KEY (assigned_to) REFERENCES technicians(id) ON DELETE SET NULL,
    CONSTRAINT fk_work_orders_pm_schedules FOREIGN KEY (preventive_schedule_id) REFERENCES preventive_schedules(id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS downtime_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    asset_id INT NOT NULL,
    work_order_id INT NULL,
    downtime_category VARCHAR(30) NOT NULL,
    start_time DATETIME NOT NULL,
    end_time DATETIME NULL,
    duration_minutes INT NULL,
    description TEXT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_downtime_logs_work_order_id (work_order_id),
    KEY ix_downtime_logs_asset_id (asset_id),
    CONSTRAINT fk_downtime_logs_assets FOREIGN KEY (asset_id) REFERENCES assets(id),
    CONSTRAINT fk_downtime_logs_work_orders FOREIGN KEY (work_order_id) REFERENCES work_orders(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS spareparts (
    id INT AUTO_INCREMENT PRIMARY KEY,
    part_code VARCHAR(80) NOT NULL,
    part_name VARCHAR(200) NOT NULL,
    category VARCHAR(100) NULL,
    unit VARCHAR(30) NOT NULL,
    stock_qty DECIMAL(18,2) NOT NULL DEFAULT 0,
    minimum_stock DECIMAL(18,2) NOT NULL DEFAULT 0,
    location VARCHAR(150) NULL,
    supplier VARCHAR(150) NULL,
    lead_time_days INT NULL,
    price DECIMAL(18,2) NULL,
    is_critical TINYINT(1) NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_spareparts_part_code (part_code)
);

CREATE TABLE IF NOT EXISTS work_order_spareparts (
    id INT AUTO_INCREMENT PRIMARY KEY,
    work_order_id INT NOT NULL,
    sparepart_id INT NOT NULL,
    qty_used DECIMAL(18,2) NOT NULL,
    used_by VARCHAR(150) NULL,
    used_at DATETIME NOT NULL,
    CONSTRAINT fk_wo_spareparts_work_orders FOREIGN KEY (work_order_id) REFERENCES work_orders(id) ON DELETE CASCADE,
    CONSTRAINT fk_wo_spareparts_spareparts FOREIGN KEY (sparepart_id) REFERENCES spareparts(id)
);

CREATE TABLE IF NOT EXISTS inventory_transactions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    sparepart_id INT NOT NULL,
    transaction_type VARCHAR(50) NOT NULL,
    quantity DECIMAL(18,2) NOT NULL,
    balance_after DECIMAL(18,2) NOT NULL,
    reference_type VARCHAR(50) NULL,
    reference_id INT NULL,
    performed_by VARCHAR(150) NULL,
    transaction_at DATETIME NOT NULL,
    remarks TEXT NULL,
    CONSTRAINT fk_inventory_transactions_spareparts FOREIGN KEY (sparepart_id) REFERENCES spareparts(id)
);

CREATE TABLE IF NOT EXISTS failure_codes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(80) NOT NULL,
    name VARCHAR(150) NOT NULL,
    category VARCHAR(100) NULL,
    description TEXT NULL,
    UNIQUE KEY uq_failure_codes_code (code)
);

CREATE TABLE IF NOT EXISTS root_causes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(80) NOT NULL,
    name VARCHAR(150) NOT NULL,
    description TEXT NULL,
    UNIQUE KEY uq_root_causes_code (code)
);

CREATE TABLE IF NOT EXISTS preventivehistory (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Line VARCHAR(150) NULL,
    Machine VARCHAR(150) NULL,
    Technician VARCHAR(150) NULL,
    Action TEXT NULL,
    Image LONGBLOB NULL,
    TimeStamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS machinestatushistory (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    LineName VARCHAR(150) NULL,
    MachineName VARCHAR(150) NULL,
    Status VARCHAR(50) NULL,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NULL
);

INSERT IGNORE INTO plants (id, plant_code, plant_name, created_at) VALUES
(1, 'PLT-JKT', 'Jakarta Manufacturing Plant', '2026-05-01 00:00:00'),
(2, 'PLT-CKR', 'Cikarang Manufacturing Plant', '2026-05-01 00:00:00');

INSERT IGNORE INTO production_lines (id, plant_id, line_code, line_name, area) VALUES
(1, 1, 'LINE-A', 'Packaging Line A', 'Packaging'),
(2, 1, 'LINE-B', 'Processing Line B', 'Processing'),
(3, 2, 'LINE-C', 'Utility Line C', 'Utility');

INSERT IGNORE INTO assets (id, asset_code, asset_name, asset_type, plant, area, production_line, location, manufacturer, model, serial_number, installation_date, criticality_level, status, created_at, updated_at) VALUES
(1, 'AST-PK-001', 'Filler Machine 1', 'Filler', 'Jakarta Manufacturing Plant', 'Packaging', 'Packaging Line A', 'PK-A-01', 'Krones', 'FILL-900', 'KR900-001', '2021-01-10', 'CRITICAL', 'ACTIVE', '2026-05-01', '2026-05-01'),
(2, 'AST-PK-002', 'Capper Machine 1', 'Capper', 'Jakarta Manufacturing Plant', 'Packaging', 'Packaging Line A', 'PK-A-02', 'Krones', 'CAP-420', 'KR420-002', '2021-02-10', 'HIGH', 'ACTIVE', '2026-05-01', '2026-05-01'),
(3, 'AST-PK-003', 'Labeler Machine 1', 'Labeler', 'Jakarta Manufacturing Plant', 'Packaging', 'Packaging Line A', 'PK-A-03', 'Sidel', 'LBL-200', 'SD200-003', '2021-03-10', 'MEDIUM', 'ACTIVE', '2026-05-01', '2026-05-01'),
(4, 'AST-PR-001', 'Mixer Tank 1', 'Mixer', 'Jakarta Manufacturing Plant', 'Processing', 'Processing Line B', 'PR-B-01', 'GEA', 'MX-5000', 'GEA5000-004', '2020-05-15', 'CRITICAL', 'ACTIVE', '2026-05-01', '2026-05-01'),
(5, 'AST-PR-002', 'Homogenizer 1', 'Homogenizer', 'Jakarta Manufacturing Plant', 'Processing', 'Processing Line B', 'PR-B-02', 'Tetra Pak', 'HMG-18', 'TP18-005', '2020-08-18', 'CRITICAL', 'ACTIVE', '2026-05-01', '2026-05-01'),
(6, 'AST-PR-003', 'Pasteurizer 1', 'Pasteurizer', 'Jakarta Manufacturing Plant', 'Processing', 'Processing Line B', 'PR-B-03', 'Tetra Pak', 'PST-32', 'TP32-006', '2020-10-01', 'HIGH', 'ACTIVE', '2026-05-01', '2026-05-01'),
(7, 'AST-UT-001', 'Air Compressor 1', 'Compressor', 'Cikarang Manufacturing Plant', 'Utility', 'Utility Line C', 'UT-C-01', 'Atlas Copco', 'GA75', 'AC75-007', '2019-04-12', 'HIGH', 'ACTIVE', '2026-05-01', '2026-05-01'),
(8, 'AST-UT-002', 'Boiler 1', 'Boiler', 'Cikarang Manufacturing Plant', 'Utility', 'Utility Line C', 'UT-C-02', 'Miura', 'LX-200', 'MR200-008', '2019-07-20', 'CRITICAL', 'ACTIVE', '2026-05-01', '2026-05-01'),
(9, 'AST-CV-001', 'Conveyor A1', 'Conveyor', 'Jakarta Manufacturing Plant', 'Packaging', 'Packaging Line A', 'PK-A-04', 'Interroll', 'CV-80', 'IR80-009', '2022-01-20', 'MEDIUM', 'ACTIVE', '2026-05-01', '2026-05-01'),
(10, 'AST-WT-001', 'Cooling Tower 1', 'Cooling Tower', 'Cikarang Manufacturing Plant', 'Utility', 'Utility Line C', 'UT-C-03', 'BAC', 'CT-300', 'BAC300-010', '2018-11-11', 'HIGH', 'UNDER_MAINTENANCE', '2026-05-01', '2026-05-01');

INSERT IGNORE INTO technicians (id, employee_no, name, email, phone, skill_type, shift, status, created_at, updated_at) VALUES
(1, 'EMP-001', 'Budi Santoso', 'budi.santoso@example.com', '0811000001', 'MECHANICAL', 'A', 'ACTIVE', '2026-05-01', '2026-05-01'),
(2, 'EMP-002', 'Siti Rahma', 'siti.rahma@example.com', '0811000002', 'ELECTRICAL', 'A', 'ACTIVE', '2026-05-01', '2026-05-01'),
(3, 'EMP-003', 'Andi Wijaya', 'andi.wijaya@example.com', '0811000003', 'UTILITY', 'B', 'ACTIVE', '2026-05-01', '2026-05-01'),
(4, 'EMP-004', 'Rina Kurnia', 'rina.kurnia@example.com', '0811000004', 'GENERAL', 'B', 'ACTIVE', '2026-05-01', '2026-05-01'),
(5, 'EMP-005', 'Dedi Saputra', 'dedi.saputra@example.com', '0811000005', 'MECHANICAL', 'C', 'ACTIVE', '2026-05-01', '2026-05-01');

INSERT IGNORE INTO spareparts (id, part_code, part_name, category, unit, stock_qty, minimum_stock, location, supplier, lead_time_days, price, is_critical, created_at, updated_at) VALUES
(1, 'SP-BRG-6205', 'Bearing 6205', 'Bearing', 'PCS', 35, 10, 'WH-A1', 'SKF Indonesia', 7, 95000, 1, '2026-05-01', '2026-05-01'),
(2, 'SP-BELT-A45', 'V-Belt A45', 'Belt', 'PCS', 18, 8, 'WH-A2', 'Mitsuboshi', 5, 120000, 1, '2026-05-01', '2026-05-01'),
(3, 'SP-SNS-PX01', 'Photo Sensor PX01', 'Sensor', 'PCS', 6, 5, 'WH-E1', 'Omron', 14, 450000, 1, '2026-05-01', '2026-05-01'),
(4, 'SP-MTR-1HP', 'Motor 1 HP', 'Motor', 'PCS', 4, 2, 'WH-E2', 'Teco', 21, 2250000, 1, '2026-05-01', '2026-05-01'),
(5, 'SP-OL-68', 'Hydraulic Oil ISO 68', 'Lubricant', 'L', 120, 40, 'WH-L1', 'Shell', 3, 65000, 0, '2026-05-01', '2026-05-01'),
(6, 'SP-GSK-20', 'Gasket 20mm', 'Seal', 'PCS', 12, 15, 'WH-M1', 'Local Supplier', 4, 25000, 0, '2026-05-01', '2026-05-01'),
(7, 'SP-PLC-IO8', 'PLC I/O Module 8CH', 'Electrical', 'PCS', 2, 2, 'WH-E3', 'Siemens', 30, 3750000, 1, '2026-05-01', '2026-05-01'),
(8, 'SP-FLT-AIR', 'Air Filter Element', 'Filter', 'PCS', 9, 10, 'WH-U1', 'Atlas Copco', 10, 380000, 1, '2026-05-01', '2026-05-01'),
(9, 'SP-CHAIN-40', 'Roller Chain 40', 'Chain', 'M', 25, 10, 'WH-A3', 'Tsubaki', 7, 85000, 0, '2026-05-01', '2026-05-01'),
(10, 'SP-VALVE-SOL', 'Solenoid Valve 24V', 'Valve', 'PCS', 7, 4, 'WH-E4', 'SMC', 12, 725000, 1, '2026-05-01', '2026-05-01');

INSERT IGNORE INTO failure_codes (id, code, name, category, description) VALUES
(1, 'MECH-BRG', 'Bearing Failure', 'MECHANICAL', 'Bearing aus atau macet'),
(2, 'ELEC-MTR', 'Motor Failure', 'ELECTRICAL', 'Motor trip, terbakar, atau overload'),
(3, 'CTRL-SNS', 'Sensor Failure', 'ELECTRICAL', 'Sensor dirty, rusak, atau salah baca'),
(4, 'UTIL-AIR', 'Compressed Air Drop', 'UTILITY', 'Tekanan angin turun atau unstable'),
(5, 'PROC-JAM', 'Product Jam', 'OPERATIONAL', 'Material atau produk macet di mesin');

INSERT IGNORE INTO root_causes (id, code, name, description) VALUES
(1, 'LACK-LUBE', 'Insufficient Lubrication', 'Pelumasan kurang atau terlambat'),
(2, 'WEAR-TEAR', 'Normal Wear and Tear', 'Komponen habis masa pakai'),
(3, 'MISALIGN', 'Misalignment', 'Alignment komponen tidak sesuai'),
(4, 'DIRTY-SENSOR', 'Dirty Sensor', 'Sensor tertutup debu, oil, atau produk'),
(5, 'OPR-SETUP', 'Incorrect Setup', 'Setting operasi tidak sesuai standar');

INSERT IGNORE INTO preventive_schedules (id, asset_id, schedule_name, frequency_type, frequency_value, next_due_date, last_generated_at, estimated_duration_minutes, checklist_template, is_active, created_at, updated_at) VALUES
(1, 1, 'Weekly filler lubrication', 'WEEKLY', 1, '2026-05-10 08:00:00', NULL, 45, 'Check nozzle, grease bearing, inspect valve actuator', 1, '2026-05-01', '2026-05-01'),
(2, 4, 'Monthly mixer inspection', 'MONTHLY', 1, '2026-05-20 08:00:00', NULL, 90, 'Inspect agitator, coupling, seal, and vibration', 1, '2026-05-01', '2026-05-01'),
(3, 7, 'Compressor air filter check', 'WEEKLY', 1, '2026-05-12 08:00:00', NULL, 30, 'Inspect air filter, drain condensate, record pressure', 1, '2026-05-01', '2026-05-01'),
(4, 8, 'Boiler safety inspection', 'MONTHLY', 1, '2026-05-25 08:00:00', NULL, 120, 'Check safety valve, burner, water level, blowdown', 1, '2026-05-01', '2026-05-01'),
(5, 9, 'Conveyor chain tension check', 'WEEKLY', 1, '2026-05-08 08:00:00', NULL, 25, 'Inspect chain tension and lubricate sprocket', 1, '2026-05-01', '2026-05-01');

INSERT IGNORE INTO work_orders (id, wo_number, asset_id, title, description, maintenance_type, priority, status, reported_by, assigned_to, reported_at, scheduled_at, started_at, completed_at, closed_at, downtime_start, downtime_end, downtime_minutes, repair_start, repair_end, repair_minutes, failure_code, root_cause, action_taken, created_at, updated_at, preventive_schedule_id, preventive_schedule_period_key) VALUES
(1, 'WO-20260501-0001', 1, 'Filler abnormal vibration', 'Vibration above alarm threshold', 'BREAKDOWN', 'URGENT', 'CLOSED', 'Operator A', 1, '2026-05-01 08:10:00', NULL, '2026-05-01 08:20:00', '2026-05-01 09:20:00', '2026-05-01 09:35:00', '2026-05-01 08:12:00', '2026-05-01 09:32:00', 80, '2026-05-01 08:20:00', '2026-05-01 09:20:00', 60, 'MECH-BRG', 'LACK-LUBE', 'Replaced bearing and lubricated shaft', '2026-05-01 08:10:00', '2026-05-01 09:35:00', NULL, NULL),
(2, 'WO-20260502-0001', 2, 'Capper torque unstable', 'Torque result fluctuating', 'CORRECTIVE', 'HIGH', 'CLOSED', 'Operator B', 2, '2026-05-02 10:00:00', NULL, '2026-05-02 10:20:00', '2026-05-02 11:05:00', '2026-05-02 11:20:00', NULL, NULL, NULL, '2026-05-02 10:20:00', '2026-05-02 11:05:00', 45, NULL, NULL, 'Adjusted torque head and replaced worn belt', '2026-05-02 10:00:00', '2026-05-02 11:20:00', NULL, NULL),
(3, 'WO-20260503-0001', 7, 'Compressor low pressure', 'Air pressure drop causing line stop', 'BREAKDOWN', 'URGENT', 'CLOSED', 'Utility Operator', 3, '2026-05-03 13:00:00', NULL, '2026-05-03 13:15:00', '2026-05-03 14:45:00', '2026-05-03 15:00:00', '2026-05-03 13:00:00', '2026-05-03 15:00:00', 120, '2026-05-03 13:15:00', '2026-05-03 14:45:00', 90, 'UTIL-AIR', 'WEAR-TEAR', 'Replaced air filter and reset pressure switch', '2026-05-03 13:00:00', '2026-05-03 15:00:00', NULL, NULL),
(4, 'WO-20260504-0001', 4, 'Mixer seal leak', 'Minor product leakage around shaft seal', 'CORRECTIVE', 'HIGH', 'CLOSED', 'Operator C', 1, '2026-05-04 09:00:00', NULL, '2026-05-04 09:30:00', '2026-05-04 10:30:00', '2026-05-04 10:40:00', '2026-05-04 09:20:00', '2026-05-04 10:40:00', 80, '2026-05-04 09:30:00', '2026-05-04 10:30:00', 60, NULL, NULL, 'Replaced gasket and checked alignment', '2026-05-04 09:00:00', '2026-05-04 10:40:00', NULL, NULL),
(5, 'WO-20260505-0001', 3, 'Label sensor intermittent', 'Label sensor sometimes not detected', 'BREAKDOWN', 'HIGH', 'CLOSED', 'Operator A', 2, '2026-05-05 15:00:00', NULL, '2026-05-05 15:10:00', '2026-05-05 15:45:00', '2026-05-05 15:55:00', '2026-05-05 15:00:00', '2026-05-05 16:00:00', 60, '2026-05-05 15:10:00', '2026-05-05 15:45:00', 35, 'CTRL-SNS', 'DIRTY-SENSOR', 'Cleaned sensor lens and adjusted bracket', '2026-05-05 15:00:00', '2026-05-05 15:55:00', NULL, NULL),
(6, 'WO-20260506-0001', 9, 'Conveyor chain noise', 'Abnormal chain noise during operation', 'CORRECTIVE', 'MEDIUM', 'COMPLETED', 'Operator D', 5, '2026-05-06 08:00:00', NULL, '2026-05-06 08:30:00', '2026-05-06 09:00:00', NULL, NULL, NULL, NULL, '2026-05-06 08:30:00', '2026-05-06 09:00:00', 30, NULL, NULL, 'Lubricated chain and tightened tensioner', '2026-05-06 08:00:00', '2026-05-06 09:00:00', NULL, NULL),
(7, 'WO-20260507-0001', 5, 'Homogenizer pressure spike', 'Pressure spike during startup', 'PREDICTIVE', 'MEDIUM', 'OPEN', 'Supervisor A', NULL, '2026-05-07 08:00:00', '2026-05-16 08:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-07 08:00:00', '2026-05-07 08:00:00', NULL, NULL),
(8, 'WO-20260508-0001', 9, 'Conveyor weekly chain PM', 'Generated sample PM', 'PREVENTIVE', 'MEDIUM', 'CLOSED', 'SYSTEM', 5, '2026-05-08 08:00:00', '2026-05-08 08:00:00', '2026-05-08 08:05:00', '2026-05-08 08:35:00', '2026-05-08 08:45:00', NULL, NULL, NULL, '2026-05-08 08:05:00', '2026-05-08 08:35:00', 30, NULL, NULL, 'Checked chain tension and lubricated sprocket', '2026-05-08 08:00:00', '2026-05-08 08:45:00', 5, '5:WEEKLY:20260508'),
(9, 'WO-20260509-0001', 8, 'Boiler burner ignition fault', 'Burner failed to ignite', 'BREAKDOWN', 'URGENT', 'CLOSED', 'Utility Operator', 3, '2026-05-09 06:30:00', NULL, '2026-05-09 06:45:00', '2026-05-09 08:15:00', '2026-05-09 08:30:00', '2026-05-09 06:30:00', '2026-05-09 08:30:00', 120, '2026-05-09 06:45:00', '2026-05-09 08:15:00', 90, 'ELEC-MTR', 'WEAR-TEAR', 'Replaced ignition electrode and tested burner', '2026-05-09 06:30:00', '2026-05-09 08:30:00', NULL, NULL),
(10, 'WO-20260510-0001', 1, 'Weekly filler lubrication', 'Generated sample PM', 'PREVENTIVE', 'MEDIUM', 'CLOSED', 'SYSTEM', 1, '2026-05-10 08:00:00', '2026-05-10 08:00:00', '2026-05-10 08:10:00', '2026-05-10 08:55:00', '2026-05-10 09:00:00', NULL, NULL, NULL, '2026-05-10 08:10:00', '2026-05-10 08:55:00', 45, NULL, NULL, 'Completed weekly lubrication checklist', '2026-05-10 08:00:00', '2026-05-10 09:00:00', 1, '1:WEEKLY:20260510'),
(11, 'WO-20260511-0001', 6, 'Pasteurizer temperature drift', 'Outlet temperature drift above tolerance', 'CORRECTIVE', 'HIGH', 'ASSIGNED', 'QA', 2, '2026-05-11 10:00:00', '2026-05-15 13:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-11 10:00:00', '2026-05-11 10:00:00', NULL, NULL),
(12, 'WO-20260512-0001', 7, 'Compressor air filter check', 'Generated sample PM', 'PREVENTIVE', 'MEDIUM', 'CLOSED', 'SYSTEM', 3, '2026-05-12 08:00:00', '2026-05-12 08:00:00', '2026-05-12 08:00:00', '2026-05-12 08:30:00', '2026-05-12 08:40:00', NULL, NULL, NULL, '2026-05-12 08:00:00', '2026-05-12 08:30:00', 30, NULL, NULL, 'Replaced clogged air filter element', '2026-05-12 08:00:00', '2026-05-12 08:40:00', 3, '3:WEEKLY:20260512'),
(13, 'WO-20260513-0001', 4, 'Mixer abnormal noise', 'Noise from coupling area', 'BREAKDOWN', 'HIGH', 'CLOSED', 'Operator C', 1, '2026-05-13 11:00:00', NULL, '2026-05-13 11:10:00', '2026-05-13 12:00:00', '2026-05-13 12:20:00', '2026-05-13 11:00:00', '2026-05-13 12:30:00', 90, '2026-05-13 11:10:00', '2026-05-13 12:00:00', 50, 'MECH-BRG', 'MISALIGN', 'Realigned coupling and replaced bearing', '2026-05-13 11:00:00', '2026-05-13 12:20:00', NULL, NULL),
(14, 'WO-20260514-0001', 10, 'Cooling tower fan trip', 'Fan motor overload trip', 'BREAKDOWN', 'URGENT', 'IN_PROGRESS', 'Utility Operator', 3, '2026-05-14 14:00:00', NULL, '2026-05-14 14:20:00', NULL, NULL, '2026-05-14 14:00:00', NULL, NULL, '2026-05-14 14:20:00', NULL, NULL, NULL, NULL, NULL, '2026-05-14 14:00:00', '2026-05-14 14:20:00', NULL, NULL),
(15, 'WO-20260515-0001', 1, 'Filler nozzle inspection', 'Routine inspection request', 'INSPECTION', 'LOW', 'OPEN', 'Supervisor B', NULL, '2026-05-15 08:00:00', '2026-05-18 08:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-15 08:00:00', '2026-05-15 08:00:00', NULL, NULL),
(16, 'WO-20260516-0001', 2, 'Capper belt replacement', 'Scheduled corrective belt replacement', 'CORRECTIVE', 'MEDIUM', 'DRAFT', 'Planner', NULL, '2026-05-16 08:00:00', '2026-05-21 08:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-16 08:00:00', '2026-05-16 08:00:00', NULL, NULL),
(17, 'WO-20260517-0001', 5, 'Homogenizer oil leak', 'Oil seepage near hydraulic hose', 'CORRECTIVE', 'HIGH', 'PENDING', 'Operator E', 1, '2026-05-17 09:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Waiting spare hose', '2026-05-17 09:00:00', '2026-05-17 09:00:00', NULL, NULL),
(18, 'WO-20260518-0001', 3, 'Labeler product jam', 'Bottle jam at labeler infeed', 'BREAKDOWN', 'HIGH', 'CLOSED', 'Operator A', 4, '2026-05-18 16:00:00', NULL, '2026-05-18 16:10:00', '2026-05-18 16:40:00', '2026-05-18 16:50:00', '2026-05-18 16:00:00', '2026-05-18 17:00:00', 60, '2026-05-18 16:10:00', '2026-05-18 16:40:00', 30, 'PROC-JAM', 'OPR-SETUP', 'Cleared jam and corrected guide rail setup', '2026-05-18 16:00:00', '2026-05-18 16:50:00', NULL, NULL),
(19, 'WO-20260519-0001', 7, 'Compressor oil change', 'Planned oil change', 'PREVENTIVE', 'LOW', 'OPEN', 'Planner', NULL, '2026-05-19 08:00:00', '2026-05-22 08:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-19 08:00:00', '2026-05-19 08:00:00', NULL, NULL),
(20, 'WO-20260520-0001', 4, 'Monthly mixer inspection', 'Generated sample PM', 'PREVENTIVE', 'MEDIUM', 'OPEN', 'SYSTEM', NULL, '2026-05-20 08:00:00', '2026-05-20 08:00:00', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2026-05-20 08:00:00', '2026-05-20 08:00:00', 2, '2:MONTHLY:20260520');

INSERT IGNORE INTO downtime_logs (id, asset_id, work_order_id, downtime_category, start_time, end_time, duration_minutes, description, created_at) VALUES
(1, 1, 1, 'MECHANICAL', '2026-05-01 08:12:00', '2026-05-01 09:32:00', 80, 'Filler abnormal vibration', '2026-05-01 09:35:00'),
(2, 7, 3, 'UTILITY', '2026-05-03 13:00:00', '2026-05-03 15:00:00', 120, 'Compressor low pressure', '2026-05-03 15:00:00'),
(3, 4, 4, 'MECHANICAL', '2026-05-04 09:20:00', '2026-05-04 10:40:00', 80, 'Mixer seal leak', '2026-05-04 10:40:00'),
(4, 3, 5, 'ELECTRICAL', '2026-05-05 15:00:00', '2026-05-05 16:00:00', 60, 'Label sensor intermittent', '2026-05-05 15:55:00'),
(5, 8, 9, 'UTILITY', '2026-05-09 06:30:00', '2026-05-09 08:30:00', 120, 'Boiler burner ignition fault', '2026-05-09 08:30:00'),
(6, 4, 13, 'MECHANICAL', '2026-05-13 11:00:00', '2026-05-13 12:30:00', 90, 'Mixer abnormal noise', '2026-05-13 12:20:00'),
(7, 3, 18, 'OPERATIONAL', '2026-05-18 16:00:00', '2026-05-18 17:00:00', 60, 'Labeler product jam', '2026-05-18 16:50:00');

INSERT IGNORE INTO work_order_spareparts (id, work_order_id, sparepart_id, qty_used, used_by, used_at) VALUES
(1, 1, 1, 2, 'Budi Santoso', '2026-05-01 09:10:00'),
(2, 2, 2, 1, 'Siti Rahma', '2026-05-02 10:50:00'),
(3, 3, 8, 1, 'Andi Wijaya', '2026-05-03 14:30:00'),
(4, 4, 6, 2, 'Budi Santoso', '2026-05-04 10:00:00'),
(5, 5, 3, 1, 'Siti Rahma', '2026-05-05 15:30:00');

INSERT IGNORE INTO inventory_transactions (id, sparepart_id, transaction_type, quantity, balance_after, reference_type, reference_id, performed_by, transaction_at, remarks) VALUES
(1, 1, 'ISSUE', -2, 35, 'WORK_ORDER', 1, 'Budi Santoso', '2026-05-01 09:10:00', 'Seed usage for WO-20260501-0001'),
(2, 2, 'ISSUE', -1, 18, 'WORK_ORDER', 2, 'Siti Rahma', '2026-05-02 10:50:00', 'Seed usage for WO-20260502-0001'),
(3, 8, 'ISSUE', -1, 9, 'WORK_ORDER', 3, 'Andi Wijaya', '2026-05-03 14:30:00', 'Seed usage for WO-20260503-0001'),
(4, 6, 'ISSUE', -2, 12, 'WORK_ORDER', 4, 'Budi Santoso', '2026-05-04 10:00:00', 'Seed usage for WO-20260504-0001'),
(5, 3, 'ISSUE', -1, 6, 'WORK_ORDER', 5, 'Siti Rahma', '2026-05-05 15:30:00', 'Seed usage for WO-20260505-0001');
