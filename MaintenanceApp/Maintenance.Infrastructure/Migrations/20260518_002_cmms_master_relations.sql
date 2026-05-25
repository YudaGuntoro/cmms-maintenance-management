SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS contractor_work_audits;
DROP TABLE IF EXISTS contractor_work_documents;
DROP TABLE IF EXISTS contractor_work_plans;
DROP TABLE IF EXISTS work_order_photos;
DROP TABLE IF EXISTS work_order_spareparts;
DROP TABLE IF EXISTS inventory_transactions;
DROP TABLE IF EXISTS downtime_logs;
DROP TABLE IF EXISTS work_orders;
DROP TABLE IF EXISTS problem_reports;
DROP TABLE IF EXISTS preventive_schedules;

DROP TABLE IF EXISTS maintenance_types;
DROP TABLE IF EXISTS work_order_priorities;
DROP TABLE IF EXISTS work_order_statuses;
DROP TABLE IF EXISTS downtime_categories;
DROP TABLE IF EXISTS problem_report_categories;
DROP TABLE IF EXISTS preventive_schedule_types;
DROP TABLE IF EXISTS frequency_types;

SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE maintenance_types (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(100) NOT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_maintenance_types_code (code)
);

CREATE TABLE work_order_priorities (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(100) NOT NULL,
    level INT NOT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_work_order_priorities_code (code)
);

CREATE TABLE work_order_statuses (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(100) NOT NULL,
    sequence INT NOT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_work_order_statuses_code (code)
);

CREATE TABLE downtime_categories (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(100) NOT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_downtime_categories_code (code)
);

CREATE TABLE problem_report_categories (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(100) NOT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_problem_report_categories_code (code)
);

CREATE TABLE preventive_schedule_types (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(100) NOT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_preventive_schedule_types_code (code)
);

CREATE TABLE frequency_types (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(100) NOT NULL,
    interval_days INT NOT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_frequency_types_code (code)
);

CREATE TABLE preventive_schedules (
    id INT AUTO_INCREMENT PRIMARY KEY,
    asset_id INT NOT NULL,
    schedule_name VARCHAR(200) NOT NULL,
    schedule_type_id INT NOT NULL,
    frequency_type_id INT NOT NULL,
    frequency_value INT NOT NULL,
    next_due_date DATETIME NOT NULL,
    last_generated_at DATETIME NULL,
    estimated_duration_minutes INT NULL,
    checklist_template TEXT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    KEY ix_preventive_schedules_asset_id (asset_id),
    KEY ix_preventive_schedules_schedule_type_id (schedule_type_id),
    KEY ix_preventive_schedules_frequency_type_id (frequency_type_id),
    CONSTRAINT fk_pm_schedules_assets FOREIGN KEY (asset_id) REFERENCES assets(id),
    CONSTRAINT fk_pm_schedules_schedule_types FOREIGN KEY (schedule_type_id) REFERENCES preventive_schedule_types(id),
    CONSTRAINT fk_pm_schedules_frequency_types FOREIGN KEY (frequency_type_id) REFERENCES frequency_types(id)
);

CREATE TABLE problem_reports (
    id INT AUTO_INCREMENT PRIMARY KEY,
    report_number VARCHAR(80) NOT NULL,
    asset_id INT NOT NULL,
    title VARCHAR(200) NOT NULL,
    description TEXT NULL,
    category_id INT NOT NULL,
    priority_id INT NOT NULL,
    status_id INT NOT NULL,
    reported_by VARCHAR(150) NULL,
    reported_at DATETIME NOT NULL,
    downtime_start DATETIME NULL,
    downtime_end DATETIME NULL,
    downtime_minutes INT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_problem_reports_report_number (report_number),
    KEY ix_problem_reports_asset_id (asset_id),
    KEY ix_problem_reports_category_id (category_id),
    KEY ix_problem_reports_priority_id (priority_id),
    KEY ix_problem_reports_status_id (status_id),
    CONSTRAINT fk_problem_reports_assets FOREIGN KEY (asset_id) REFERENCES assets(id),
    CONSTRAINT fk_problem_reports_categories FOREIGN KEY (category_id) REFERENCES problem_report_categories(id),
    CONSTRAINT fk_problem_reports_priorities FOREIGN KEY (priority_id) REFERENCES work_order_priorities(id),
    CONSTRAINT fk_problem_reports_statuses FOREIGN KEY (status_id) REFERENCES work_order_statuses(id)
);

CREATE TABLE work_orders (
    id INT AUTO_INCREMENT PRIMARY KEY,
    wo_number VARCHAR(80) NOT NULL,
    asset_id INT NOT NULL,
    problem_report_id INT NULL,
    title VARCHAR(200) NOT NULL,
    description TEXT NULL,
    maintenance_type_id INT NOT NULL,
    priority_id INT NOT NULL,
    status_id INT NOT NULL,
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
    result TEXT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    preventive_schedule_id INT NULL,
    preventive_schedule_period_key VARCHAR(120) NULL,
    UNIQUE KEY uq_work_orders_wo_number (wo_number),
    UNIQUE KEY uq_work_orders_pm_period (preventive_schedule_id, preventive_schedule_period_key),
    KEY ix_work_orders_asset_id (asset_id),
    KEY ix_work_orders_problem_report_id (problem_report_id),
    KEY ix_work_orders_maintenance_type_id (maintenance_type_id),
    KEY ix_work_orders_priority_id (priority_id),
    KEY ix_work_orders_status_id (status_id),
    KEY ix_work_orders_assigned_to (assigned_to),
    CONSTRAINT fk_work_orders_assets FOREIGN KEY (asset_id) REFERENCES assets(id),
    CONSTRAINT fk_work_orders_problem_reports FOREIGN KEY (problem_report_id) REFERENCES problem_reports(id) ON DELETE SET NULL,
    CONSTRAINT fk_work_orders_technicians FOREIGN KEY (assigned_to) REFERENCES technicians(id) ON DELETE SET NULL,
    CONSTRAINT fk_work_orders_pm_schedules FOREIGN KEY (preventive_schedule_id) REFERENCES preventive_schedules(id) ON DELETE SET NULL,
    CONSTRAINT fk_work_orders_maintenance_types FOREIGN KEY (maintenance_type_id) REFERENCES maintenance_types(id),
    CONSTRAINT fk_work_orders_priorities FOREIGN KEY (priority_id) REFERENCES work_order_priorities(id),
    CONSTRAINT fk_work_orders_statuses FOREIGN KEY (status_id) REFERENCES work_order_statuses(id)
);

CREATE TABLE downtime_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    asset_id INT NOT NULL,
    work_order_id INT NULL,
    problem_report_id INT NULL,
    downtime_category_id INT NOT NULL,
    start_time DATETIME NOT NULL,
    end_time DATETIME NULL,
    duration_minutes INT NULL,
    description TEXT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_downtime_logs_work_order_id (work_order_id),
    UNIQUE KEY uq_downtime_logs_problem_report_id (problem_report_id),
    KEY ix_downtime_logs_asset_id (asset_id),
    KEY ix_downtime_logs_downtime_category_id (downtime_category_id),
    CONSTRAINT fk_downtime_logs_assets FOREIGN KEY (asset_id) REFERENCES assets(id),
    CONSTRAINT fk_downtime_logs_work_orders FOREIGN KEY (work_order_id) REFERENCES work_orders(id) ON DELETE CASCADE,
    CONSTRAINT fk_downtime_logs_problem_reports FOREIGN KEY (problem_report_id) REFERENCES problem_reports(id) ON DELETE SET NULL,
    CONSTRAINT fk_downtime_logs_categories FOREIGN KEY (downtime_category_id) REFERENCES downtime_categories(id)
);

CREATE TABLE work_order_photos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    work_order_id INT NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    content_type VARCHAR(120) NOT NULL,
    size_bytes BIGINT NOT NULL DEFAULT 0,
    file_data LONGBLOB NOT NULL,
    uploaded_by VARCHAR(150) NULL,
    uploaded_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    KEY ix_work_order_photos_work_order_id (work_order_id),
    CONSTRAINT fk_work_order_photos_work_orders FOREIGN KEY (work_order_id) REFERENCES work_orders(id) ON DELETE CASCADE
);

CREATE TABLE work_order_spareparts (
    id INT AUTO_INCREMENT PRIMARY KEY,
    work_order_id INT NOT NULL,
    sparepart_id INT NOT NULL,
    qty_used DECIMAL(18,2) NOT NULL,
    used_by VARCHAR(150) NULL,
    used_at DATETIME NOT NULL,
    KEY ix_work_order_spareparts_work_order_id (work_order_id),
    KEY ix_work_order_spareparts_sparepart_id (sparepart_id),
    CONSTRAINT fk_wo_spareparts_work_orders FOREIGN KEY (work_order_id) REFERENCES work_orders(id) ON DELETE CASCADE,
    CONSTRAINT fk_wo_spareparts_spareparts FOREIGN KEY (sparepart_id) REFERENCES spareparts(id)
);

CREATE TABLE inventory_transactions (
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
    KEY ix_inventory_transactions_sparepart_id (sparepart_id),
    CONSTRAINT fk_inventory_transactions_spareparts FOREIGN KEY (sparepart_id) REFERENCES spareparts(id)
);

CREATE TABLE contractor_work_plans (
    id INT AUTO_INCREMENT PRIMARY KEY,
    vendor_name VARCHAR(150) NOT NULL,
    vendor_pic_name VARCHAR(150) NOT NULL,
    vendor_pic_phone VARCHAR(50) NULL,
    worker_count INT NOT NULL DEFAULT 0,
    internal_pic_name VARCHAR(150) NOT NULL,
    department_area VARCHAR(120) NOT NULL,
    work_title VARCHAR(200) NOT NULL,
    work_description TEXT NULL,
    work_area VARCHAR(120) NOT NULL,
    work_location VARCHAR(200) NULL,
    asset_id INT NULL,
    additional_notes TEXT NULL,
    start_at DATETIME NOT NULL,
    end_at DATETIME NOT NULL,
    estimated_duration_minutes INT NOT NULL DEFAULT 0,
    status VARCHAR(40) NOT NULL DEFAULT 'PLANNED',
    permit_document_status VARCHAR(40) NOT NULL DEFAULT 'NOT_UPLOADED',
    working_at_height TINYINT(1) NOT NULL DEFAULT 0,
    hot_work TINYINT(1) NOT NULL DEFAULT 0,
    welding TINYINT(1) NOT NULL DEFAULT 0,
    electrical_work TINYINT(1) NOT NULL DEFAULT 0,
    confined_space TINYINT(1) NOT NULL DEFAULT 0,
    heavy_equipment_activity TINYINT(1) NOT NULL DEFAULT 0,
    chemical_handling TINYINT(1) NOT NULL DEFAULT 0,
    shutdown_activity TINYINT(1) NOT NULL DEFAULT 0,
    loto_required TINYINT(1) NOT NULL DEFAULT 0,
    need_safety_standby TINYINT(1) NOT NULL DEFAULT 0,
    created_by VARCHAR(150) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by VARCHAR(150) NULL,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    permit_uploaded_by VARCHAR(150) NULL,
    permit_uploaded_at DATETIME NULL,
    work_order_id INT NULL,
    KEY ix_contractor_work_plans_vendor_name (vendor_name),
    KEY ix_contractor_work_plans_work_area (work_area),
    KEY ix_contractor_work_plans_status (status),
    KEY ix_contractor_work_plans_start_at (start_at),
    KEY ix_contractor_work_plans_asset_id (asset_id),
    KEY ix_contractor_work_plans_work_order_id (work_order_id),
    CONSTRAINT fk_contractor_work_plans_assets FOREIGN KEY (asset_id) REFERENCES assets(id) ON DELETE SET NULL,
    CONSTRAINT fk_contractor_work_plans_work_orders FOREIGN KEY (work_order_id) REFERENCES work_orders(id) ON DELETE SET NULL
);

CREATE TABLE contractor_work_documents (
    id INT AUTO_INCREMENT PRIMARY KEY,
    contractor_work_plan_id INT NOT NULL,
    document_type VARCHAR(40) NOT NULL,
    document_status VARCHAR(40) NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    content_type VARCHAR(120) NOT NULL,
    size_bytes BIGINT NOT NULL DEFAULT 0,
    file_data LONGBLOB NOT NULL,
    uploaded_by VARCHAR(150) NULL,
    uploaded_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expires_at DATETIME NULL,
    notes TEXT NULL,
    KEY ix_contractor_work_documents_plan_id (contractor_work_plan_id),
    KEY ix_contractor_work_documents_type (document_type),
    CONSTRAINT fk_contractor_work_documents_plans FOREIGN KEY (contractor_work_plan_id) REFERENCES contractor_work_plans(id) ON DELETE CASCADE
);

CREATE TABLE contractor_work_audits (
    id INT AUTO_INCREMENT PRIMARY KEY,
    contractor_work_plan_id INT NOT NULL,
    action VARCHAR(80) NOT NULL,
    field_name VARCHAR(120) NULL,
    old_value TEXT NULL,
    new_value TEXT NULL,
    performed_by VARCHAR(150) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    KEY ix_contractor_work_audits_plan_id (contractor_work_plan_id),
    KEY ix_contractor_work_audits_created_at (created_at),
    CONSTRAINT fk_contractor_work_audits_plans FOREIGN KEY (contractor_work_plan_id) REFERENCES contractor_work_plans(id) ON DELETE CASCADE
);
