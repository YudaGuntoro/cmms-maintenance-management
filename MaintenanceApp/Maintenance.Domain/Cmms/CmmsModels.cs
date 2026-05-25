using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Maintenance.Domain.Cmms;

public enum AssetCriticalityLevel
{
    LOW,
    MEDIUM,
    HIGH,
    CRITICAL
}

public enum AssetStatus
{
    ACTIVE,
    INACTIVE,
    UNDER_MAINTENANCE,
    RETIRED
}

public enum TechnicianSkillType
{
    MECHANICAL,
    ELECTRICAL,
    UTILITY,
    GENERAL
}

public enum TechnicianStatus
{
    ACTIVE,
    INACTIVE
}

public enum MaintenanceType
{
    BREAKDOWN,
    CORRECTIVE,
    PREVENTIVE,
    PREDICTIVE,
    INSPECTION,
    CONTRACTOR_SUPERVISION
}

public enum WorkOrderPriority
{
    LOW,
    MEDIUM,
    HIGH,
    URGENT
}

public enum WorkOrderStatus
{
    DRAFT,
    OPEN,
    ASSIGNED,
    IN_PROGRESS,
    PENDING,
    COMPLETED,
    CLOSED,
    CANCELLED
}

public enum ProblemReportCategory
{
    DOWNTIME,
    BREAKDOWN,
    QUALITY,
    SAFETY,
    OTHER
}

public enum ProblemReportStatus
{
    PENDING,
    IN_PROGRESS,
    COMPLETED,
    CANCELLED
}

public enum FrequencyType
{
    DAILY,
    WEEKLY,
    MONTHLY,
    YEARLY,
    RUNNING_HOURS
}

public enum DowntimeCategory
{
    MECHANICAL,
    ELECTRICAL,
    UTILITY,
    OPERATIONAL,
    MATERIAL,
    PLANNED_STOP,
    OTHER
}

public enum PreventiveScheduleType
{
    DAILY,
    WEEKLY,
    MONTHLY,
    YEARLY
}

internal static class CmmsEnumResolver
{
    public static T Resolve<T>(string? code, T fallback) where T : struct, Enum
    {
        return Enum.TryParse<T>(code, true, out var parsed) ? parsed : fallback;
    }
}

public class MaintenanceTypeMaster
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class WorkOrderPriorityMaster
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class WorkOrderStatusMaster
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("sequence")]
    public int Sequence { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class DowntimeCategoryMaster
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class ProblemReportCategoryMaster
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class PreventiveScheduleTypeMaster
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class FrequencyTypeMaster
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("interval_days")]
    public int IntervalDays { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public enum AppUserRole
{
    ADMIN,
    SUPERVISOR,
    TECHNICIAN,
    OPERATOR,
    VIEWER
}

public enum AppUserStatus
{
    ACTIVE,
    INACTIVE
}

public enum ContractorWorkStatus
{
    PLANNED,
    WAITING_PERMIT_DOCUMENT,
    READY_TO_START,
    ONGOING,
    FINISHED,
    CANCELLED,
    EXPIRED
}

public enum ContractorDocumentStatus
{
    NOT_UPLOADED,
    UPLOADED,
    EXPIRED,
    NEED_REVISION
}

public enum ContractorDocumentType
{
    PERMIT,
    JSA,
    ASSIGNMENT_LETTER,
    WORKER_CERTIFICATE,
    SAFETY_DOCUMENT,
    OTHER
}

public class Plant
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("plant_code")]
    public string PlantCode { get; set; } = string.Empty;

    [JsonPropertyName("plant_name")]
    public string PlantName { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<ProductionLine> ProductionLines { get; set; } = new List<ProductionLine>();
}

public class ProductionLine
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("plant_id")]
    public int PlantId { get; set; }

    [JsonPropertyName("line_code")]
    public string LineCode { get; set; } = string.Empty;

    [JsonPropertyName("line_name")]
    public string LineName { get; set; } = string.Empty;

    [JsonPropertyName("area")]
    public string Area { get; set; } = string.Empty;

    [JsonIgnore]
    public Plant? Plant { get; set; }
}

public class Asset
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("asset_code")]
    public string AssetCode { get; set; } = string.Empty;

    [JsonPropertyName("asset_name")]
    public string AssetName { get; set; } = string.Empty;

    [JsonPropertyName("asset_type")]
    public string AssetType { get; set; } = string.Empty;

    [JsonPropertyName("plant")]
    public string Plant { get; set; } = string.Empty;

    [JsonPropertyName("area")]
    public string Area { get; set; } = string.Empty;

    [JsonPropertyName("production_line")]
    public string ProductionLine { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("serial_number")]
    public string? SerialNumber { get; set; }

    [JsonPropertyName("installation_date")]
    public DateTime? InstallationDate { get; set; }

    [JsonPropertyName("criticality_level")]
    public AssetCriticalityLevel CriticalityLevel { get; set; } = AssetCriticalityLevel.MEDIUM;

    [JsonPropertyName("status")]
    public AssetStatus Status { get; set; } = AssetStatus.ACTIVE;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();

    [JsonIgnore]
    public ICollection<PreventiveSchedule> PreventiveSchedules { get; set; } = new List<PreventiveSchedule>();
}

public class Technician
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("employee_no")]
    public string EmployeeNo { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("skill_type")]
    public TechnicianSkillType SkillType { get; set; } = TechnicianSkillType.GENERAL;

    [JsonPropertyName("shift")]
    public string Shift { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public TechnicianStatus Status { get; set; } = TechnicianStatus.ACTIVE;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public ICollection<WorkOrder> AssignedWorkOrders { get; set; } = new List<WorkOrder>();
}

public class AppUser
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("role")]
    public AppUserRole Role { get; set; } = AppUserRole.VIEWER;

    [JsonPropertyName("status")]
    public AppUserStatus Status { get; set; } = AppUserStatus.ACTIVE;

    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    [JsonIgnore]
    public string PasswordSalt { get; set; } = string.Empty;

    [JsonPropertyName("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public class WorkOrder
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("wo_number")]
    public string WoNumber { get; set; } = string.Empty;

    [JsonPropertyName("asset_id")]
    public int AssetId { get; set; }

    [JsonPropertyName("problem_report_id")]
    public int? ProblemReportId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("maintenance_type_id")]
    public int? MaintenanceTypeId { get; set; }

    private MaintenanceType _maintenanceType = MaintenanceType.CORRECTIVE;

    [NotMapped]
    [JsonPropertyName("maintenance_type")]
    public MaintenanceType MaintenanceType
    {
        get => CmmsEnumResolver.Resolve(MaintenanceTypeDetail?.Code, _maintenanceType);
        set => _maintenanceType = value;
    }

    [JsonPropertyName("priority_id")]
    public int? PriorityId { get; set; }

    private WorkOrderPriority _priority = WorkOrderPriority.MEDIUM;

    [NotMapped]
    [JsonPropertyName("priority")]
    public WorkOrderPriority Priority
    {
        get => CmmsEnumResolver.Resolve(PriorityDetail?.Code, _priority);
        set => _priority = value;
    }

    [JsonPropertyName("status_id")]
    public int? StatusId { get; set; }

    private WorkOrderStatus _status = WorkOrderStatus.OPEN;

    [NotMapped]
    [JsonPropertyName("status")]
    public WorkOrderStatus Status
    {
        get => CmmsEnumResolver.Resolve(StatusDetail?.Code, _status);
        set => _status = value;
    }

    [JsonPropertyName("maintenance_type_detail")]
    public MaintenanceTypeMaster? MaintenanceTypeDetail { get; set; }

    [JsonPropertyName("priority_detail")]
    public WorkOrderPriorityMaster? PriorityDetail { get; set; }

    [JsonPropertyName("status_detail")]
    public WorkOrderStatusMaster? StatusDetail { get; set; }

    [JsonPropertyName("reported_by")]
    public string? ReportedBy { get; set; }

    [JsonPropertyName("assigned_to")]
    public int? AssignedTo { get; set; }

    [JsonPropertyName("reported_at")]
    public DateTime? ReportedAt { get; set; }

    [JsonPropertyName("scheduled_at")]
    public DateTime? ScheduledAt { get; set; }

    [JsonPropertyName("started_at")]
    public DateTime? StartedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("closed_at")]
    public DateTime? ClosedAt { get; set; }

    [JsonPropertyName("downtime_start")]
    public DateTime? DowntimeStart { get; set; }

    [JsonPropertyName("downtime_end")]
    public DateTime? DowntimeEnd { get; set; }

    [JsonPropertyName("downtime_minutes")]
    public int? DowntimeMinutes { get; set; }

    [JsonPropertyName("repair_start")]
    public DateTime? RepairStart { get; set; }

    [JsonPropertyName("repair_end")]
    public DateTime? RepairEnd { get; set; }

    [JsonPropertyName("repair_minutes")]
    public int? RepairMinutes { get; set; }

    [JsonPropertyName("failure_code")]
    public string? FailureCode { get; set; }

    [JsonPropertyName("root_cause")]
    public string? RootCause { get; set; }

    [JsonPropertyName("action_taken")]
    public string? ActionTaken { get; set; }

    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("preventive_schedule_id")]
    public int? PreventiveScheduleId { get; set; }

    [JsonPropertyName("preventive_schedule_period_key")]
    public string? PreventiveSchedulePeriodKey { get; set; }

    [JsonIgnore]
    public Asset? Asset { get; set; }

    [JsonIgnore]
    public ProblemReport? ProblemReport { get; set; }

    [JsonIgnore]
    public Technician? AssignedTechnician { get; set; }

    [JsonIgnore]
    public PreventiveSchedule? PreventiveSchedule { get; set; }

    [JsonIgnore]
    public ICollection<DowntimeLog> DowntimeLogs { get; set; } = new List<DowntimeLog>();

    [JsonIgnore]
    public ICollection<WorkOrderSparepartUsage> SparepartUsages { get; set; } = new List<WorkOrderSparepartUsage>();

    [JsonIgnore]
    public ICollection<WorkOrderPhoto> Photos { get; set; } = new List<WorkOrderPhoto>();
}

public class ProblemReport
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("report_number")]
    public string ReportNumber { get; set; } = string.Empty;

    [JsonPropertyName("asset_id")]
    public int AssetId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("category_id")]
    public int? CategoryId { get; set; }

    private ProblemReportCategory _category = ProblemReportCategory.OTHER;

    [NotMapped]
    [JsonPropertyName("category")]
    public ProblemReportCategory Category
    {
        get => CmmsEnumResolver.Resolve(CategoryDetail?.Code, _category);
        set => _category = value;
    }

    [JsonPropertyName("priority_id")]
    public int? PriorityId { get; set; }

    private WorkOrderPriority _priority = WorkOrderPriority.MEDIUM;

    [NotMapped]
    [JsonPropertyName("priority")]
    public WorkOrderPriority Priority
    {
        get => CmmsEnumResolver.Resolve(PriorityDetail?.Code, _priority);
        set => _priority = value;
    }

    [JsonPropertyName("status_id")]
    public int? StatusId { get; set; }

    private ProblemReportStatus _status = ProblemReportStatus.PENDING;

    [NotMapped]
    [JsonPropertyName("status")]
    public ProblemReportStatus Status
    {
        get => CmmsEnumResolver.Resolve(StatusDetail?.Code, _status);
        set => _status = value;
    }

    [JsonPropertyName("category_detail")]
    public ProblemReportCategoryMaster? CategoryDetail { get; set; }

    [JsonPropertyName("priority_detail")]
    public WorkOrderPriorityMaster? PriorityDetail { get; set; }

    [JsonPropertyName("status_detail")]
    public WorkOrderStatusMaster? StatusDetail { get; set; }

    [JsonPropertyName("reported_by")]
    public string? ReportedBy { get; set; }

    [JsonPropertyName("reported_at")]
    public DateTime ReportedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("downtime_start")]
    public DateTime? DowntimeStart { get; set; }

    [JsonPropertyName("downtime_end")]
    public DateTime? DowntimeEnd { get; set; }

    [JsonPropertyName("downtime_minutes")]
    public int? DowntimeMinutes { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public Asset? Asset { get; set; }

    [JsonIgnore]
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();

    [JsonIgnore]
    public ICollection<DowntimeLog> DowntimeLogs { get; set; } = new List<DowntimeLog>();
}

public class WorkOrderPhoto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("work_order_id")]
    public int WorkOrderId { get; set; }

    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = string.Empty;

    [JsonPropertyName("size_bytes")]
    public long SizeBytes { get; set; }

    [JsonPropertyName("uploaded_by")]
    public string? UploadedBy { get; set; }

    [JsonPropertyName("uploaded_at")]
    public DateTime UploadedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public byte[] FileData { get; set; } = Array.Empty<byte>();

    [JsonIgnore]
    public WorkOrder? WorkOrder { get; set; }
}

public class ContractorWorkPlan
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("vendor_name")]
    public string VendorName { get; set; } = string.Empty;

    [JsonPropertyName("vendor_pic_name")]
    public string VendorPicName { get; set; } = string.Empty;

    [JsonPropertyName("vendor_pic_phone")]
    public string? VendorPicPhone { get; set; }

    [JsonPropertyName("worker_count")]
    public int WorkerCount { get; set; }

    [JsonPropertyName("internal_pic_name")]
    public string InternalPicName { get; set; } = string.Empty;

    [JsonPropertyName("department_area")]
    public string DepartmentArea { get; set; } = string.Empty;

    [JsonPropertyName("work_title")]
    public string WorkTitle { get; set; } = string.Empty;

    [JsonPropertyName("work_description")]
    public string? WorkDescription { get; set; }

    [JsonPropertyName("work_area")]
    public string WorkArea { get; set; } = string.Empty;

    [JsonPropertyName("work_location")]
    public string? WorkLocation { get; set; }

    [JsonPropertyName("asset_id")]
    public int? AssetId { get; set; }

    [JsonPropertyName("additional_notes")]
    public string? AdditionalNotes { get; set; }

    [JsonPropertyName("start_at")]
    public DateTime StartAt { get; set; }

    [JsonPropertyName("end_at")]
    public DateTime EndAt { get; set; }

    [JsonPropertyName("estimated_duration_minutes")]
    public int EstimatedDurationMinutes { get; set; }

    [JsonPropertyName("status")]
    public ContractorWorkStatus Status { get; set; } = ContractorWorkStatus.PLANNED;

    [JsonPropertyName("permit_document_status")]
    public ContractorDocumentStatus PermitDocumentStatus { get; set; } = ContractorDocumentStatus.NOT_UPLOADED;

    [JsonPropertyName("working_at_height")]
    public bool WorkingAtHeight { get; set; }

    [JsonPropertyName("hot_work")]
    public bool HotWork { get; set; }

    [JsonPropertyName("welding")]
    public bool Welding { get; set; }

    [JsonPropertyName("electrical_work")]
    public bool ElectricalWork { get; set; }

    [JsonPropertyName("confined_space")]
    public bool ConfinedSpace { get; set; }

    [JsonPropertyName("heavy_equipment_activity")]
    public bool HeavyEquipmentActivity { get; set; }

    [JsonPropertyName("chemical_handling")]
    public bool ChemicalHandling { get; set; }

    [JsonPropertyName("shutdown_activity")]
    public bool ShutdownActivity { get; set; }

    [JsonPropertyName("loto_required")]
    public bool LotoRequired { get; set; }

    [JsonPropertyName("need_safety_standby")]
    public bool NeedSafetyStandby { get; set; }

    [JsonPropertyName("created_by")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("updated_by")]
    public string? UpdatedBy { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("permit_uploaded_by")]
    public string? PermitUploadedBy { get; set; }

    [JsonPropertyName("permit_uploaded_at")]
    public DateTime? PermitUploadedAt { get; set; }

    [JsonPropertyName("work_order_id")]
    public int? WorkOrderId { get; set; }

    [NotMapped]
    [JsonPropertyName("has_high_risk")]
    public bool HasHighRisk => RiskTags.Count > 0;

    [NotMapped]
    [JsonPropertyName("risk_tags")]
    public List<string> RiskTags
    {
        get
        {
            var tags = new List<string>();
            if (WorkingAtHeight) tags.Add("Working at Height");
            if (HotWork) tags.Add("Hot Work");
            if (Welding) tags.Add("Welding");
            if (ElectricalWork) tags.Add("Electrical Work");
            if (ConfinedSpace) tags.Add("Confined Space");
            if (HeavyEquipmentActivity) tags.Add("Heavy Equipment");
            if (ChemicalHandling) tags.Add("Chemical Handling");
            if (ShutdownActivity) tags.Add("Shutdown Activity");
            if (LotoRequired) tags.Add("LOTO Required");
            if (NeedSafetyStandby) tags.Add("Need Safety Standby");
            return tags;
        }
    }

    [JsonIgnore]
    public Asset? Asset { get; set; }

    [JsonIgnore]
    public WorkOrder? SupervisionWorkOrder { get; set; }

    [JsonPropertyName("documents")]
    public ICollection<ContractorWorkDocument> Documents { get; set; } = new List<ContractorWorkDocument>();

    [JsonPropertyName("audits")]
    public ICollection<ContractorWorkAudit> Audits { get; set; } = new List<ContractorWorkAudit>();
}

public class ContractorWorkDocument
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("contractor_work_plan_id")]
    public int ContractorWorkPlanId { get; set; }

    [JsonPropertyName("document_type")]
    public ContractorDocumentType DocumentType { get; set; } = ContractorDocumentType.PERMIT;

    [JsonPropertyName("document_status")]
    public ContractorDocumentStatus DocumentStatus { get; set; } = ContractorDocumentStatus.UPLOADED;

    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = string.Empty;

    [JsonPropertyName("size_bytes")]
    public long SizeBytes { get; set; }

    [JsonPropertyName("uploaded_by")]
    public string? UploadedBy { get; set; }

    [JsonPropertyName("uploaded_at")]
    public DateTime UploadedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonIgnore]
    public byte[] FileData { get; set; } = Array.Empty<byte>();

    [JsonIgnore]
    public ContractorWorkPlan? ContractorWorkPlan { get; set; }
}

public class ContractorWorkAudit
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("contractor_work_plan_id")]
    public int ContractorWorkPlanId { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("field_name")]
    public string? FieldName { get; set; }

    [JsonPropertyName("old_value")]
    public string? OldValue { get; set; }

    [JsonPropertyName("new_value")]
    public string? NewValue { get; set; }

    [JsonPropertyName("performed_by")]
    public string? PerformedBy { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public ContractorWorkPlan? ContractorWorkPlan { get; set; }
}

public class ContractorWorkPlanFilter
{
    public string? Vendor { get; set; }
    public string? Area { get; set; }
    public ContractorWorkStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Risk { get; set; }
    public string? PicMtc { get; set; }
}

public class ContractorWorkReminder
{
    [JsonPropertyName("contractor_work_plan_id")]
    public int ContractorWorkPlanId { get; set; }

    [JsonPropertyName("vendor_name")]
    public string VendorName { get; set; } = string.Empty;

    [JsonPropertyName("work_title")]
    public string WorkTitle { get; set; } = string.Empty;

    [JsonPropertyName("work_area")]
    public string WorkArea { get; set; } = string.Empty;

    [JsonPropertyName("start_at")]
    public DateTime StartAt { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "INFO";

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class PreventiveSchedule
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("asset_id")]
    public int AssetId { get; set; }

    [JsonPropertyName("schedule_name")]
    public string ScheduleName { get; set; } = string.Empty;

    [JsonPropertyName("schedule_type_id")]
    public int? ScheduleTypeId { get; set; }

    private PreventiveScheduleType _scheduleType = PreventiveScheduleType.MONTHLY;

    [NotMapped]
    [JsonPropertyName("schedule_type")]
    public PreventiveScheduleType ScheduleType
    {
        get => CmmsEnumResolver.Resolve(ScheduleTypeDetail?.Code, _scheduleType);
        set => _scheduleType = value;
    }

    [JsonPropertyName("frequency_type_id")]
    public int? FrequencyTypeId { get; set; }

    private FrequencyType _frequencyType = FrequencyType.MONTHLY;

    [NotMapped]
    [JsonPropertyName("frequency_type")]
    public FrequencyType FrequencyType
    {
        get => CmmsEnumResolver.Resolve(FrequencyTypeDetail?.Code, _frequencyType);
        set => _frequencyType = value;
    }

    [JsonPropertyName("schedule_type_detail")]
    public PreventiveScheduleTypeMaster? ScheduleTypeDetail { get; set; }

    [JsonPropertyName("frequency_type_detail")]
    public FrequencyTypeMaster? FrequencyTypeDetail { get; set; }

    [JsonPropertyName("frequency_value")]
    public int FrequencyValue { get; set; } = 1;

    [JsonPropertyName("next_due_date")]
    public DateTime NextDueDate { get; set; }

    [JsonPropertyName("last_generated_at")]
    public DateTime? LastGeneratedAt { get; set; }

    [JsonPropertyName("estimated_duration_minutes")]
    public int? EstimatedDurationMinutes { get; set; }

    [JsonPropertyName("checklist_template")]
    public string? ChecklistTemplate { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public Asset? Asset { get; set; }

    [JsonIgnore]
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}

public class DowntimeLog
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("asset_id")]
    public int AssetId { get; set; }

    [JsonPropertyName("work_order_id")]
    public int? WorkOrderId { get; set; }

    [JsonPropertyName("problem_report_id")]
    public int? ProblemReportId { get; set; }

    [JsonPropertyName("downtime_category_id")]
    public int? DowntimeCategoryId { get; set; }

    private DowntimeCategory _downtimeCategory = DowntimeCategory.OPERATIONAL;

    [NotMapped]
    [JsonPropertyName("downtime_category")]
    public DowntimeCategory DowntimeCategory
    {
        get => CmmsEnumResolver.Resolve(DowntimeCategoryDetail?.Code, _downtimeCategory);
        set => _downtimeCategory = value;
    }

    [JsonPropertyName("downtime_category_detail")]
    public DowntimeCategoryMaster? DowntimeCategoryDetail { get; set; }

    [JsonPropertyName("start_time")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("end_time")]
    public DateTime? EndTime { get; set; }

    [JsonPropertyName("duration_minutes")]
    public int? DurationMinutes { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public Asset? Asset { get; set; }

    [JsonIgnore]
    public WorkOrder? WorkOrder { get; set; }

    [JsonIgnore]
    public ProblemReport? ProblemReport { get; set; }
}

public class Sparepart
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("part_code")]
    public string PartCode { get; set; } = string.Empty;

    [JsonPropertyName("part_name")]
    public string PartName { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = "PCS";

    [JsonPropertyName("stock_qty")]
    public decimal StockQty { get; set; }

    [JsonPropertyName("minimum_stock")]
    public decimal MinimumStock { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("supplier")]
    public string? Supplier { get; set; }

    [JsonPropertyName("lead_time_days")]
    public int? LeadTimeDays { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("is_critical")]
    public bool IsCritical { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public ICollection<WorkOrderSparepartUsage> WorkOrderUsages { get; set; } = new List<WorkOrderSparepartUsage>();
}

public class WorkOrderSparepartUsage
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("work_order_id")]
    public int WorkOrderId { get; set; }

    [JsonPropertyName("sparepart_id")]
    public int SparepartId { get; set; }

    [JsonPropertyName("qty_used")]
    public decimal QtyUsed { get; set; }

    [JsonPropertyName("used_by")]
    public string? UsedBy { get; set; }

    [JsonPropertyName("used_at")]
    public DateTime UsedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public WorkOrder? WorkOrder { get; set; }

    [JsonIgnore]
    public Sparepart? Sparepart { get; set; }
}

public class InventoryTransaction
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sparepart_id")]
    public int SparepartId { get; set; }

    [JsonPropertyName("transaction_type")]
    public string TransactionType { get; set; } = "ISSUE";

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("balance_after")]
    public decimal BalanceAfter { get; set; }

    [JsonPropertyName("reference_type")]
    public string? ReferenceType { get; set; }

    [JsonPropertyName("reference_id")]
    public int? ReferenceId { get; set; }

    [JsonPropertyName("performed_by")]
    public string? PerformedBy { get; set; }

    [JsonPropertyName("transaction_at")]
    public DateTime TransactionAt { get; set; } = DateTime.Now;

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }

    [JsonIgnore]
    public Sparepart? Sparepart { get; set; }
}

public class FailureCode
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class RootCause
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class WorkOrderAssignRequest
{
    [JsonPropertyName("assigned_to")]
    public int AssignedTo { get; set; }

    [JsonPropertyName("scheduled_at")]
    public DateTime? ScheduledAt { get; set; }
}

public class WorkOrderCompleteRequest
{
    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("downtime_start")]
    public DateTime? DowntimeStart { get; set; }

    [JsonPropertyName("downtime_end")]
    public DateTime? DowntimeEnd { get; set; }

    [JsonPropertyName("repair_start")]
    public DateTime? RepairStart { get; set; }

    [JsonPropertyName("repair_end")]
    public DateTime? RepairEnd { get; set; }

    [JsonPropertyName("failure_code")]
    public string? FailureCode { get; set; }

    [JsonPropertyName("root_cause")]
    public string? RootCause { get; set; }

    [JsonPropertyName("action_taken")]
    public string? ActionTaken { get; set; }

    [JsonPropertyName("result")]
    public string? Result { get; set; }
}

public class WorkOrderCloseRequest
{
    [JsonPropertyName("closed_at")]
    public DateTime? ClosedAt { get; set; }

    [JsonPropertyName("failure_code")]
    public string? FailureCode { get; set; }

    [JsonPropertyName("root_cause")]
    public string? RootCause { get; set; }

    [JsonPropertyName("action_taken")]
    public string? ActionTaken { get; set; }

    [JsonPropertyName("result")]
    public string? Result { get; set; }
}

public class WorkOrderSparepartRequest
{
    [JsonPropertyName("sparepart_id")]
    public int SparepartId { get; set; }

    [JsonPropertyName("qty_used")]
    public decimal QtyUsed { get; set; }

    [JsonPropertyName("used_by")]
    public string? UsedBy { get; set; }

    [JsonPropertyName("used_at")]
    public DateTime? UsedAt { get; set; }
}

public class TelegramSettings
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonIgnore]
    public string BotToken { get; set; } = string.Empty;

    [JsonPropertyName("chat_id")]
    public string? ChatId { get; set; }

    [JsonPropertyName("is_enabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public class TelegramSettingsResponse
{
    [JsonPropertyName("has_bot_token")]
    public bool HasBotToken { get; set; }

    [JsonPropertyName("bot_token_preview")]
    public string? BotTokenPreview { get; set; }

    [JsonPropertyName("chat_id")]
    public string? ChatId { get; set; }

    [JsonPropertyName("is_enabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

public class TelegramSettingsUpdateRequest
{
    [JsonPropertyName("bot_token")]
    public string? BotToken { get; set; }

    [JsonPropertyName("chat_id")]
    public string? ChatId { get; set; }

    [JsonPropertyName("is_enabled")]
    public bool IsEnabled { get; set; }
}

public class TelegramTestRequest
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class TelegramChatResponse
{
    [JsonPropertyName("chat_id")]
    public string ChatId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("last_message_at")]
    public DateTime? LastMessageAt { get; set; }
}

public class ReliabilityKpiResponse
{
    [JsonPropertyName("asset_id")]
    public int? AssetId { get; set; }

    [JsonPropertyName("period_start")]
    public DateTime PeriodStart { get; set; }

    [JsonPropertyName("period_end")]
    public DateTime PeriodEnd { get; set; }

    [JsonPropertyName("failure_count")]
    public int FailureCount { get; set; }

    [JsonPropertyName("total_downtime_minutes")]
    public int TotalDowntimeMinutes { get; set; }

    [JsonPropertyName("total_repair_minutes")]
    public int TotalRepairMinutes { get; set; }

    [JsonPropertyName("operating_minutes")]
    public int OperatingMinutes { get; set; }

    [JsonPropertyName("mttr_minutes")]
    public decimal MttrMinutes { get; set; }

    [JsonPropertyName("mtbf_minutes")]
    public decimal MtbfMinutes { get; set; }

    [JsonPropertyName("availability_percent")]
    public decimal AvailabilityPercent { get; set; }
}

public class DashboardSummaryResponse
{
    [JsonPropertyName("total_assets")]
    public int TotalAssets { get; set; }

    [JsonPropertyName("open_work_orders")]
    public int OpenWorkOrders { get; set; }

    [JsonPropertyName("overdue_preventive_maintenance")]
    public int OverduePreventiveMaintenance { get; set; }

    [JsonPropertyName("low_stock_spareparts")]
    public int LowStockSpareparts { get; set; }

    [JsonPropertyName("top_assets_by_downtime")]
    public List<AssetMetricSummary> TopAssetsByDowntime { get; set; } = new();

    [JsonPropertyName("top_assets_by_failure_count")]
    public List<AssetMetricSummary> TopAssetsByFailureCount { get; set; } = new();

    [JsonPropertyName("work_order_status_summary")]
    public List<NameValueSummary> WorkOrderStatusSummary { get; set; } = new();

    [JsonPropertyName("downtime_category_summary")]
    public List<NameValueSummary> DowntimeCategorySummary { get; set; } = new();
}

public class AssetMetricSummary
{
    [JsonPropertyName("asset_id")]
    public int AssetId { get; set; }

    [JsonPropertyName("asset_code")]
    public string AssetCode { get; set; } = string.Empty;

    [JsonPropertyName("asset_name")]
    public string AssetName { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public int Value { get; set; }
}

public class NameValueSummary
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public int Value { get; set; }
}

public class UserResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("role")]
    public AppUserRole Role { get; set; }

    [JsonPropertyName("status")]
    public AppUserStatus Status { get; set; }

    [JsonPropertyName("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

public class CreateUserRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("role")]
    public AppUserRole Role { get; set; } = AppUserRole.VIEWER;

    [JsonPropertyName("status")]
    public AppUserStatus Status { get; set; } = AppUserStatus.ACTIVE;
}

public class UpdateUserRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("role")]
    public AppUserRole Role { get; set; } = AppUserRole.VIEWER;

    [JsonPropertyName("status")]
    public AppUserStatus Status { get; set; } = AppUserStatus.ACTIVE;
}

public class ChangePasswordRequest
{
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [JsonPropertyName("user")]
    public UserResponse User { get; set; } = new();
}
