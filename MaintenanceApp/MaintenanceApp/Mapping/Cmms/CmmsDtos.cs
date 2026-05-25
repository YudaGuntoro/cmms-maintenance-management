using Newtonsoft.Json;

namespace MaintenanceApp.Mapping.Cmms
{
    public class AssetDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("asset_code")]
        public string AssetCode { get; set; } = string.Empty;

        [JsonProperty("asset_name")]
        public string AssetName { get; set; } = string.Empty;

        [JsonProperty("asset_type")]
        public string AssetType { get; set; } = string.Empty;

        [JsonProperty("plant")]
        public string Plant { get; set; } = string.Empty;

        [JsonProperty("area")]
        public string Area { get; set; } = string.Empty;

        [JsonProperty("production_line")]
        public string ProductionLine { get; set; } = string.Empty;

        [JsonProperty("location")]
        public string Location { get; set; } = string.Empty;

        [JsonProperty("manufacturer")]
        public string? Manufacturer { get; set; }

        [JsonProperty("model")]
        public string? Model { get; set; }

        [JsonProperty("serial_number")]
        public string? SerialNumber { get; set; }

        [JsonProperty("installation_date")]
        public DateTime? InstallationDate { get; set; }

        [JsonProperty("criticality_level")]
        public string CriticalityLevel { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class TechnicianDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("employee_no")]
        public string EmployeeNo { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("phone")]
        public string? Phone { get; set; }

        [JsonProperty("skill_type")]
        public string SkillType { get; set; } = string.Empty;

        [JsonProperty("shift")]
        public string Shift { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
    }

    public class WorkOrderDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("wo_number")]
        public string WoNumber { get; set; } = string.Empty;

        [JsonProperty("asset_id")]
        public int AssetId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("maintenance_type_id")]
        public int? MaintenanceTypeId { get; set; }

        [JsonProperty("maintenance_type")]
        public string MaintenanceType { get; set; } = "CORRECTIVE";

        [JsonProperty("priority_id")]
        public int? PriorityId { get; set; }

        [JsonProperty("priority")]
        public string Priority { get; set; } = "MEDIUM";

        [JsonProperty("status_id")]
        public int? StatusId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = "OPEN";

        [JsonProperty("maintenance_type_detail")]
        public MasterLookupDto? MaintenanceTypeDetail { get; set; }

        [JsonProperty("priority_detail")]
        public WorkOrderPriorityLookupDto? PriorityDetail { get; set; }

        [JsonProperty("status_detail")]
        public WorkOrderStatusLookupDto? StatusDetail { get; set; }

        [JsonProperty("reported_by")]
        public string? ReportedBy { get; set; }

        [JsonProperty("assigned_to")]
        public int? AssignedTo { get; set; }

        [JsonProperty("reported_at")]
        public DateTime? ReportedAt { get; set; }

        [JsonProperty("scheduled_at")]
        public DateTime? ScheduledAt { get; set; }

        [JsonProperty("started_at")]
        public DateTime? StartedAt { get; set; }

        [JsonProperty("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [JsonProperty("closed_at")]
        public DateTime? ClosedAt { get; set; }

        [JsonProperty("downtime_start")]
        public DateTime? DowntimeStart { get; set; }

        [JsonProperty("downtime_end")]
        public DateTime? DowntimeEnd { get; set; }

        [JsonProperty("downtime_minutes")]
        public int? DowntimeMinutes { get; set; }

        [JsonProperty("repair_start")]
        public DateTime? RepairStart { get; set; }

        [JsonProperty("repair_end")]
        public DateTime? RepairEnd { get; set; }

        [JsonProperty("repair_minutes")]
        public int? RepairMinutes { get; set; }

        [JsonProperty("failure_code")]
        public string? FailureCode { get; set; }

        [JsonProperty("root_cause")]
        public string? RootCause { get; set; }

        [JsonProperty("action_taken")]
        public string? ActionTaken { get; set; }

        [JsonProperty("result")]
        public string? Result { get; set; }
    }

    public class MasterLookupDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class WorkOrderPriorityLookupDto : MasterLookupDto
    {
        [JsonProperty("level")]
        public int Level { get; set; }
    }

    public class WorkOrderStatusLookupDto : MasterLookupDto
    {
        [JsonProperty("sequence")]
        public int Sequence { get; set; }
    }

    public class ContractorWorkPlanDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("vendor_name")]
        public string VendorName { get; set; } = string.Empty;

        [JsonProperty("vendor_pic_name")]
        public string VendorPicName { get; set; } = string.Empty;

        [JsonProperty("vendor_pic_phone")]
        public string? VendorPicPhone { get; set; }

        [JsonProperty("worker_count")]
        public int WorkerCount { get; set; }

        [JsonProperty("internal_pic_name")]
        public string InternalPicName { get; set; } = string.Empty;

        [JsonProperty("department_area")]
        public string DepartmentArea { get; set; } = string.Empty;

        [JsonProperty("work_title")]
        public string WorkTitle { get; set; } = string.Empty;

        [JsonProperty("work_description")]
        public string? WorkDescription { get; set; }

        [JsonProperty("work_area")]
        public string WorkArea { get; set; } = string.Empty;

        [JsonProperty("work_location")]
        public string? WorkLocation { get; set; }

        [JsonProperty("asset_id")]
        public int? AssetId { get; set; }

        [JsonProperty("additional_notes")]
        public string? AdditionalNotes { get; set; }

        [JsonProperty("start_at")]
        public DateTime StartAt { get; set; }

        [JsonProperty("end_at")]
        public DateTime EndAt { get; set; }

        [JsonProperty("estimated_duration_minutes")]
        public int EstimatedDurationMinutes { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = "PLANNED";

        [JsonProperty("permit_document_status")]
        public string PermitDocumentStatus { get; set; } = "NOT_UPLOADED";

        [JsonProperty("working_at_height")]
        public bool WorkingAtHeight { get; set; }

        [JsonProperty("hot_work")]
        public bool HotWork { get; set; }

        [JsonProperty("welding")]
        public bool Welding { get; set; }

        [JsonProperty("electrical_work")]
        public bool ElectricalWork { get; set; }

        [JsonProperty("confined_space")]
        public bool ConfinedSpace { get; set; }

        [JsonProperty("heavy_equipment_activity")]
        public bool HeavyEquipmentActivity { get; set; }

        [JsonProperty("chemical_handling")]
        public bool ChemicalHandling { get; set; }

        [JsonProperty("shutdown_activity")]
        public bool ShutdownActivity { get; set; }

        [JsonProperty("loto_required")]
        public bool LotoRequired { get; set; }

        [JsonProperty("need_safety_standby")]
        public bool NeedSafetyStandby { get; set; }

        [JsonProperty("work_order_id")]
        public int? WorkOrderId { get; set; }

        [JsonProperty("risk_tags")]
        public List<string> RiskTags { get; set; } = new();

        [JsonProperty("has_high_risk")]
        public bool HasHighRisk { get; set; }

        [JsonIgnore]
        public string ScheduleText => $"{StartAt:dd MMM HH:mm} - {EndAt:dd MMM HH:mm}";

        [JsonIgnore]
        public string RiskSummary => RiskTags.Count == 0 ? "Normal" : string.Join(", ", RiskTags);

        [JsonIgnore]
        public string DurationText => EstimatedDurationMinutes >= 60
            ? $"{EstimatedDurationMinutes / 60}h {EstimatedDurationMinutes % 60}m"
            : $"{EstimatedDurationMinutes}m";
    }

    public class ContractorWorkReminderDto
    {
        [JsonProperty("contractor_work_plan_id")]
        public int ContractorWorkPlanId { get; set; }

        [JsonProperty("vendor_name")]
        public string VendorName { get; set; } = string.Empty;

        [JsonProperty("work_title")]
        public string WorkTitle { get; set; } = string.Empty;

        [JsonProperty("work_area")]
        public string WorkArea { get; set; } = string.Empty;

        [JsonProperty("start_at")]
        public DateTime StartAt { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("severity")]
        public string Severity { get; set; } = "INFO";

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class SparepartDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("part_code")]
        public string PartCode { get; set; } = string.Empty;

        [JsonProperty("part_name")]
        public string PartName { get; set; } = string.Empty;

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; } = string.Empty;

        [JsonProperty("stock_qty")]
        public decimal StockQty { get; set; }

        [JsonProperty("minimum_stock")]
        public decimal MinimumStock { get; set; }

        [JsonProperty("location")]
        public string? Location { get; set; }

        [JsonProperty("supplier")]
        public string? Supplier { get; set; }

        [JsonProperty("is_critical")]
        public bool IsCritical { get; set; }

        [JsonIgnore]
        public bool IsLowStock => StockQty <= MinimumStock;
    }

    public class ReliabilityKpiDto
    {
        [JsonProperty("asset_id")]
        public int? AssetId { get; set; }

        [JsonProperty("period_start")]
        public DateTime PeriodStart { get; set; }

        [JsonProperty("period_end")]
        public DateTime PeriodEnd { get; set; }

        [JsonProperty("failure_count")]
        public int FailureCount { get; set; }

        [JsonProperty("total_downtime_minutes")]
        public int TotalDowntimeMinutes { get; set; }

        [JsonProperty("total_repair_minutes")]
        public int TotalRepairMinutes { get; set; }

        [JsonProperty("operating_minutes")]
        public int OperatingMinutes { get; set; }

        [JsonProperty("mttr_minutes")]
        public decimal MttrMinutes { get; set; }

        [JsonProperty("mtbf_minutes")]
        public decimal MtbfMinutes { get; set; }

        [JsonProperty("availability_percent")]
        public decimal AvailabilityPercent { get; set; }
    }

    public class DashboardSummaryDto
    {
        [JsonProperty("total_assets")]
        public int TotalAssets { get; set; }

        [JsonProperty("open_work_orders")]
        public int OpenWorkOrders { get; set; }

        [JsonProperty("overdue_preventive_maintenance")]
        public int OverduePreventiveMaintenance { get; set; }

        [JsonProperty("low_stock_spareparts")]
        public int LowStockSpareparts { get; set; }

        [JsonProperty("top_assets_by_downtime")]
        public List<AssetMetricDto> TopAssetsByDowntime { get; set; } = new();

        [JsonProperty("top_assets_by_failure_count")]
        public List<AssetMetricDto> TopAssetsByFailureCount { get; set; } = new();

        [JsonProperty("work_order_status_summary")]
        public List<NameValueDto> WorkOrderStatusSummary { get; set; } = new();

        [JsonProperty("downtime_category_summary")]
        public List<NameValueDto> DowntimeCategorySummary { get; set; } = new();
    }

    public class AssetMetricDto
    {
        [JsonProperty("asset_id")]
        public int AssetId { get; set; }

        [JsonProperty("asset_code")]
        public string AssetCode { get; set; } = string.Empty;

        [JsonProperty("asset_name")]
        public string AssetName { get; set; } = string.Empty;

        [JsonProperty("value")]
        public int Value { get; set; }
    }

    public class NameValueDto
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("value")]
        public int Value { get; set; }
    }

    public class AssignWorkOrderRequest
    {
        [JsonProperty("assigned_to")]
        public int AssignedTo { get; set; }

        [JsonProperty("scheduled_at")]
        public DateTime? ScheduledAt { get; set; }
    }

    public class CompleteWorkOrderRequest
    {
        [JsonProperty("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [JsonProperty("repair_start")]
        public DateTime? RepairStart { get; set; }

        [JsonProperty("repair_end")]
        public DateTime? RepairEnd { get; set; }

        [JsonProperty("downtime_start")]
        public DateTime? DowntimeStart { get; set; }

        [JsonProperty("downtime_end")]
        public DateTime? DowntimeEnd { get; set; }

        [JsonProperty("failure_code")]
        public string? FailureCode { get; set; }

        [JsonProperty("root_cause")]
        public string? RootCause { get; set; }

        [JsonProperty("action_taken")]
        public string? ActionTaken { get; set; }

        [JsonProperty("result")]
        public string? Result { get; set; }
    }

    public class CloseWorkOrderRequest
    {
        [JsonProperty("closed_at")]
        public DateTime? ClosedAt { get; set; }

        [JsonProperty("failure_code")]
        public string? FailureCode { get; set; }

        [JsonProperty("root_cause")]
        public string? RootCause { get; set; }

        [JsonProperty("action_taken")]
        public string? ActionTaken { get; set; }

        [JsonProperty("result")]
        public string? Result { get; set; }
    }
}
