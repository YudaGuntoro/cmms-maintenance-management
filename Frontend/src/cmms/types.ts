export type ApiResponse<T> = {
  success: boolean;
  statusCode: number;
  message: string;
  data: T;
};

export const assetCriticalityLevels = ["LOW", "MEDIUM", "HIGH", "CRITICAL"] as const;
export const assetStatuses = ["ACTIVE", "INACTIVE", "UNDER_MAINTENANCE", "RETIRED"] as const;
export const technicianSkillTypes = ["MECHANICAL", "ELECTRICAL", "UTILITY", "GENERAL"] as const;
export const technicianStatuses = ["ACTIVE", "INACTIVE"] as const;
export const maintenanceTypes = ["BREAKDOWN", "CORRECTIVE", "PREVENTIVE", "PREDICTIVE", "INSPECTION", "CONTRACTOR_SUPERVISION"] as const;
export const workOrderPriorities = ["LOW", "MEDIUM", "HIGH", "URGENT"] as const;
export const workOrderStatuses = ["DRAFT", "OPEN", "ASSIGNED", "IN_PROGRESS", "PENDING", "COMPLETED", "CLOSED", "CANCELLED"] as const;
export const problemReportCategories = ["DOWNTIME", "BREAKDOWN", "QUALITY", "SAFETY", "OTHER"] as const;
export const problemReportStatuses = ["PENDING", "IN_PROGRESS", "COMPLETED", "CANCELLED"] as const;
export const frequencyTypes = ["DAILY", "WEEKLY", "MONTHLY", "YEARLY", "RUNNING_HOURS"] as const;
export const downtimeCategories = ["MECHANICAL", "ELECTRICAL", "UTILITY", "OPERATIONAL", "MATERIAL", "PLANNED_STOP", "OTHER"] as const;
export const appUserRoles = ["ADMIN", "SUPERVISOR", "TECHNICIAN", "OPERATOR", "VIEWER"] as const;
export const appUserStatuses = ["ACTIVE", "INACTIVE"] as const;
export const contractorWorkStatuses = ["PLANNED", "WAITING_PERMIT_DOCUMENT", "READY_TO_START", "ONGOING", "FINISHED", "CANCELLED", "EXPIRED"] as const;
export const contractorDocumentStatuses = ["NOT_UPLOADED", "UPLOADED", "EXPIRED", "NEED_REVISION"] as const;
export const contractorDocumentTypes = ["PERMIT", "JSA", "ASSIGNMENT_LETTER", "WORKER_CERTIFICATE", "SAFETY_DOCUMENT", "OTHER"] as const;

export type EntityValue = string | number | boolean | null | undefined;
export type EntityRecord = Record<string, EntityValue>;

export type Asset = {
  id: number;
  asset_code: string;
  asset_name: string;
  asset_type: string;
  plant: string;
  area: string;
  production_line: string;
  location: string;
  manufacturer?: string | null;
  model?: string | null;
  serial_number?: string | null;
  installation_date?: string | null;
  criticality_level: (typeof assetCriticalityLevels)[number];
  status: (typeof assetStatuses)[number];
  created_at: string;
  updated_at: string;
};

export type Technician = {
  id: number;
  employee_no: string;
  name: string;
  email?: string | null;
  phone?: string | null;
  skill_type: (typeof technicianSkillTypes)[number];
  shift: string;
  status: (typeof technicianStatuses)[number];
  created_at: string;
  updated_at: string;
};

export type Sparepart = {
  id: number;
  part_code: string;
  part_name: string;
  category?: string | null;
  unit: string;
  stock_qty: number;
  minimum_stock: number;
  location?: string | null;
  supplier?: string | null;
  lead_time_days?: number | null;
  price?: number | null;
  is_critical: boolean;
  created_at: string;
  updated_at: string;
};

export type WorkOrder = {
  id: number;
  wo_number: string;
  asset_id: number;
  problem_report_id?: number | null;
  title: string;
  description?: string | null;
  maintenance_type: (typeof maintenanceTypes)[number];
  priority: (typeof workOrderPriorities)[number];
  status: (typeof workOrderStatuses)[number];
  reported_by?: string | null;
  assigned_to?: number | null;
  reported_at?: string | null;
  scheduled_at?: string | null;
  started_at?: string | null;
  completed_at?: string | null;
  closed_at?: string | null;
  downtime_start?: string | null;
  downtime_end?: string | null;
  downtime_minutes?: number | null;
  repair_start?: string | null;
  repair_end?: string | null;
  repair_minutes?: number | null;
  failure_code?: string | null;
  root_cause?: string | null;
  action_taken?: string | null;
  result?: string | null;
  created_at: string;
  updated_at: string;
  preventive_schedule_id?: number | null;
  preventive_schedule_period_key?: string | null;
};

export type WorkOrderPhoto = {
  id: number;
  work_order_id: number;
  file_name: string;
  content_type: string;
  size_bytes: number;
  uploaded_by?: string | null;
  uploaded_at: string;
};

export type ContractorWorkDocument = {
  id: number;
  contractor_work_plan_id: number;
  document_type: (typeof contractorDocumentTypes)[number];
  document_status: (typeof contractorDocumentStatuses)[number];
  file_name: string;
  content_type: string;
  size_bytes: number;
  uploaded_by?: string | null;
  uploaded_at: string;
  expires_at?: string | null;
  notes?: string | null;
};

export type ContractorWorkAudit = {
  id: number;
  contractor_work_plan_id: number;
  action: string;
  field_name?: string | null;
  old_value?: string | null;
  new_value?: string | null;
  performed_by?: string | null;
  created_at: string;
};

export type ContractorWorkPlan = {
  id: number;
  vendor_name: string;
  vendor_pic_name: string;
  vendor_pic_phone?: string | null;
  worker_count: number;
  internal_pic_name: string;
  department_area: string;
  work_title: string;
  work_description?: string | null;
  work_area: string;
  work_location?: string | null;
  asset_id?: number | null;
  additional_notes?: string | null;
  start_at: string;
  end_at: string;
  estimated_duration_minutes: number;
  status: (typeof contractorWorkStatuses)[number];
  permit_document_status: (typeof contractorDocumentStatuses)[number];
  working_at_height: boolean;
  hot_work: boolean;
  welding: boolean;
  electrical_work: boolean;
  confined_space: boolean;
  heavy_equipment_activity: boolean;
  chemical_handling: boolean;
  shutdown_activity: boolean;
  loto_required: boolean;
  need_safety_standby: boolean;
  created_by?: string | null;
  created_at: string;
  updated_by?: string | null;
  updated_at: string;
  permit_uploaded_by?: string | null;
  permit_uploaded_at?: string | null;
  work_order_id?: number | null;
  has_high_risk: boolean;
  risk_tags: string[];
  documents?: ContractorWorkDocument[];
  audits?: ContractorWorkAudit[];
};

export type ContractorWorkReminder = {
  contractor_work_plan_id: number;
  vendor_name: string;
  work_title: string;
  work_area: string;
  start_at: string;
  type: string;
  severity: "INFO" | "WARNING" | "DANGER" | string;
  message: string;
};

export type DowntimeLog = {
  id: number;
  asset_id: number;
  work_order_id?: number | null;
  problem_report_id?: number | null;
  downtime_category: (typeof downtimeCategories)[number];
  start_time: string;
  end_time?: string | null;
  duration_minutes?: number | null;
  description?: string | null;
  created_at: string;
};

export type ProblemReport = {
  id: number;
  report_number: string;
  asset_id: number;
  title: string;
  description?: string | null;
  category: (typeof problemReportCategories)[number];
  priority: (typeof workOrderPriorities)[number];
  status: (typeof problemReportStatuses)[number];
  reported_by?: string | null;
  reported_at: string;
  downtime_start?: string | null;
  downtime_end?: string | null;
  downtime_minutes?: number | null;
  created_at: string;
  updated_at: string;
};

export type PreventiveSchedule = {
  id: number;
  asset_id: number;
  schedule_name: string;
  frequency_type: (typeof frequencyTypes)[number];
  frequency_value: number;
  next_due_date: string;
  last_generated_at?: string | null;
  estimated_duration_minutes?: number | null;
  checklist_template?: string | null;
  is_active: boolean;
  created_at: string;
  updated_at: string;
};

export type FailureCode = {
  id: number;
  code: string;
  name: string;
  category?: string | null;
  description?: string | null;
};

export type RootCause = {
  id: number;
  code: string;
  name: string;
  description?: string | null;
};

export type UserResponse = {
  id: number;
  username: string;
  technician_id?: number | null;
  full_name: string;
  email?: string | null;
  phone?: string | null;
  role: (typeof appUserRoles)[number];
  status: (typeof appUserStatuses)[number];
  last_login_at?: string | null;
  created_at: string;
  updated_at: string;
};

export type LoginResponse = {
  access_token: string;
  token_type: string;
  expires_at: string;
  user: UserResponse;
};

export type TelegramSettingsResponse = {
  has_bot_token: boolean;
  bot_token_preview?: string | null;
  chat_id?: string | null;
  is_enabled: boolean;
  updated_at?: string | null;
};

export type TelegramChatResponse = {
  chat_id: string;
  title: string;
  type: string;
  last_message_at?: string | null;
};

export type DashboardSummaryResponse = {
  total_assets: number;
  open_work_orders: number;
  overdue_preventive_maintenance: number;
  low_stock_spareparts: number;
  top_assets_by_downtime: AssetMetricSummary[];
  top_assets_by_failure_count: AssetMetricSummary[];
  work_order_status_summary: NameValueSummary[];
  downtime_category_summary: NameValueSummary[];
};

export type AssetMetricSummary = {
  asset_id: number;
  asset_code: string;
  asset_name: string;
  value: number;
};

export type NameValueSummary = {
  name: string;
  value: number;
};

export type ReliabilityKpiResponse = {
  asset_id?: number | null;
  period_start: string;
  period_end: string;
  failure_count: number;
  total_downtime_minutes: number;
  total_repair_minutes: number;
  operating_minutes: number;
  mttr_minutes: number;
  mtbf_minutes: number;
  availability_percent: number;
};
