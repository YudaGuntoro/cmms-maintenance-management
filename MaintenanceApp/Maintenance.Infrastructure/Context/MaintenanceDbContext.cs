using Maintenance.Domain.Cmms;
using Maintenance.Domain.Mapping.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Infrastructure.Context;

public class MaintenanceDbContext : DbContext
{
    public MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options) : base(options)
    {
    }

    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<ProductionLine> ProductionLines => Set<ProductionLine>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<MaintenanceTypeMaster> MaintenanceTypes => Set<MaintenanceTypeMaster>();
    public DbSet<WorkOrderPriorityMaster> WorkOrderPriorities => Set<WorkOrderPriorityMaster>();
    public DbSet<WorkOrderStatusMaster> WorkOrderStatuses => Set<WorkOrderStatusMaster>();
    public DbSet<DowntimeCategoryMaster> DowntimeCategories => Set<DowntimeCategoryMaster>();
    public DbSet<ProblemReportCategoryMaster> ProblemReportCategories => Set<ProblemReportCategoryMaster>();
    public DbSet<PreventiveScheduleTypeMaster> PreventiveScheduleTypes => Set<PreventiveScheduleTypeMaster>();
    public DbSet<FrequencyTypeMaster> FrequencyTypes => Set<FrequencyTypeMaster>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<ProblemReport> ProblemReports => Set<ProblemReport>();
    public DbSet<WorkOrderPhoto> WorkOrderPhotos => Set<WorkOrderPhoto>();
    public DbSet<ContractorWorkPlan> ContractorWorkPlans => Set<ContractorWorkPlan>();
    public DbSet<ContractorWorkDocument> ContractorWorkDocuments => Set<ContractorWorkDocument>();
    public DbSet<ContractorWorkAudit> ContractorWorkAudits => Set<ContractorWorkAudit>();
    public DbSet<PreventiveSchedule> PreventiveSchedules => Set<PreventiveSchedule>();
    public DbSet<DowntimeLog> DowntimeLogs => Set<DowntimeLog>();
    public DbSet<Sparepart> Spareparts => Set<Sparepart>();
    public DbSet<WorkOrderSparepartUsage> WorkOrderSparepartUsages => Set<WorkOrderSparepartUsage>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<FailureCode> FailureCodes => Set<FailureCode>();
    public DbSet<RootCause> RootCauses => Set<RootCause>();
    public DbSet<TelegramSettings> TelegramSettings => Set<TelegramSettings>();

    public DbSet<Preventive> PreventiveHistory => Set<Preventive>();
    public DbSet<MachineStatus> MachineStatusHistory => Set<MachineStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Plant>(entity =>
        {
            entity.ToTable("plants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PlantCode).HasColumnName("plant_code").HasMaxLength(50).IsRequired();
            entity.Property(x => x.PlantName).HasColumnName("plant_name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.PlantCode).IsUnique();
        });

        modelBuilder.Entity<ProductionLine>(entity =>
        {
            entity.ToTable("production_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PlantId).HasColumnName("plant_id");
            entity.Property(x => x.LineCode).HasColumnName("line_code").HasMaxLength(50).IsRequired();
            entity.Property(x => x.LineName).HasColumnName("line_name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Area).HasColumnName("area").HasMaxLength(100).IsRequired();
            entity.HasOne(x => x.Plant).WithMany(x => x.ProductionLines).HasForeignKey(x => x.PlantId);
            entity.HasIndex(x => x.LineCode).IsUnique();
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("assets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssetCode).HasColumnName("asset_code").HasMaxLength(80).IsRequired();
            entity.Property(x => x.AssetName).HasColumnName("asset_name").HasMaxLength(200).IsRequired();
            entity.Property(x => x.AssetType).HasColumnName("asset_type").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Plant).HasColumnName("plant").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Area).HasColumnName("area").HasMaxLength(100).IsRequired();
            entity.Property(x => x.ProductionLine).HasColumnName("production_line").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Location).HasColumnName("location").HasMaxLength(200).IsRequired();
            entity.Property(x => x.Manufacturer).HasColumnName("manufacturer").HasMaxLength(150);
            entity.Property(x => x.Model).HasColumnName("model").HasMaxLength(150);
            entity.Property(x => x.SerialNumber).HasColumnName("serial_number").HasMaxLength(150);
            entity.Property(x => x.InstallationDate).HasColumnName("installation_date");
            entity.Property(x => x.CriticalityLevel).HasColumnName("criticality_level").HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.AssetCode).IsUnique();
        });

        modelBuilder.Entity<Technician>(entity =>
        {
            entity.ToTable("technicians");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EmployeeNo).HasColumnName("employee_no").HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(150);
            entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(x => x.SkillType).HasColumnName("skill_type").HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Shift).HasColumnName("shift").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.EmployeeNo).IsUnique();
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Username).HasColumnName("username").HasMaxLength(80).IsRequired();
            entity.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(150);
            entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(x => x.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(x => x.PasswordSalt).HasColumnName("password_salt").HasMaxLength(255).IsRequired();
            entity.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<MaintenanceTypeMaster>(entity =>
        {
            entity.ToTable("maintenance_types");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<WorkOrderPriorityMaster>(entity =>
        {
            entity.ToTable("work_order_priorities");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Level).HasColumnName("level");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<WorkOrderStatusMaster>(entity =>
        {
            entity.ToTable("work_order_statuses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Sequence).HasColumnName("sequence");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<DowntimeCategoryMaster>(entity =>
        {
            entity.ToTable("downtime_categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<ProblemReportCategoryMaster>(entity =>
        {
            entity.ToTable("problem_report_categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<PreventiveScheduleTypeMaster>(entity =>
        {
            entity.ToTable("preventive_schedule_types");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<FrequencyTypeMaster>(entity =>
        {
            entity.ToTable("frequency_types");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(x => x.IntervalDays).HasColumnName("interval_days");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.ToTable("work_orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.WoNumber).HasColumnName("wo_number").HasMaxLength(80).IsRequired();
            entity.Property(x => x.AssetId).HasColumnName("asset_id");
            entity.Property(x => x.ProblemReportId).HasColumnName("problem_report_id");
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description").HasColumnType("text");
            entity.Property(x => x.MaintenanceTypeId).HasColumnName("maintenance_type_id").IsRequired();
            entity.Property(x => x.PriorityId).HasColumnName("priority_id").IsRequired();
            entity.Property(x => x.StatusId).HasColumnName("status_id").IsRequired();
            entity.Property(x => x.ReportedBy).HasColumnName("reported_by").HasMaxLength(150);
            entity.Property(x => x.AssignedTo).HasColumnName("assigned_to");
            entity.Property(x => x.ReportedAt).HasColumnName("reported_at");
            entity.Property(x => x.ScheduledAt).HasColumnName("scheduled_at");
            entity.Property(x => x.StartedAt).HasColumnName("started_at");
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
            entity.Property(x => x.ClosedAt).HasColumnName("closed_at");
            entity.Property(x => x.DowntimeStart).HasColumnName("downtime_start");
            entity.Property(x => x.DowntimeEnd).HasColumnName("downtime_end");
            entity.Property(x => x.DowntimeMinutes).HasColumnName("downtime_minutes");
            entity.Property(x => x.RepairStart).HasColumnName("repair_start");
            entity.Property(x => x.RepairEnd).HasColumnName("repair_end");
            entity.Property(x => x.RepairMinutes).HasColumnName("repair_minutes");
            entity.Property(x => x.FailureCode).HasColumnName("failure_code").HasMaxLength(80);
            entity.Property(x => x.RootCause).HasColumnName("root_cause").HasMaxLength(80);
            entity.Property(x => x.ActionTaken).HasColumnName("action_taken").HasColumnType("text");
            entity.Property(x => x.Result).HasColumnName("result").HasColumnType("text");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.PreventiveScheduleId).HasColumnName("preventive_schedule_id");
            entity.Property(x => x.PreventiveSchedulePeriodKey).HasColumnName("preventive_schedule_period_key").HasMaxLength(120);
            entity.HasOne(x => x.Asset).WithMany(x => x.WorkOrders).HasForeignKey(x => x.AssetId);
            entity.HasOne(x => x.ProblemReport).WithMany(x => x.WorkOrders).HasForeignKey(x => x.ProblemReportId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.AssignedTechnician).WithMany(x => x.AssignedWorkOrders).HasForeignKey(x => x.AssignedTo).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.PreventiveSchedule).WithMany(x => x.WorkOrders).HasForeignKey(x => x.PreventiveScheduleId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.MaintenanceTypeDetail).WithMany().HasForeignKey(x => x.MaintenanceTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PriorityDetail).WithMany().HasForeignKey(x => x.PriorityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.StatusDetail).WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.WoNumber).IsUnique();
            entity.HasIndex(x => x.ProblemReportId);
            entity.HasIndex(x => x.MaintenanceTypeId);
            entity.HasIndex(x => x.PriorityId);
            entity.HasIndex(x => x.StatusId);
            entity.HasIndex(x => new { x.PreventiveScheduleId, x.PreventiveSchedulePeriodKey }).IsUnique();
        });

        modelBuilder.Entity<ProblemReport>(entity =>
        {
            entity.ToTable("problem_reports");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReportNumber).HasColumnName("report_number").HasMaxLength(80).IsRequired();
            entity.Property(x => x.AssetId).HasColumnName("asset_id");
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description").HasColumnType("text");
            entity.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();
            entity.Property(x => x.PriorityId).HasColumnName("priority_id").IsRequired();
            entity.Property(x => x.StatusId).HasColumnName("status_id").IsRequired();
            entity.Property(x => x.ReportedBy).HasColumnName("reported_by").HasMaxLength(150);
            entity.Property(x => x.ReportedAt).HasColumnName("reported_at");
            entity.Property(x => x.DowntimeStart).HasColumnName("downtime_start");
            entity.Property(x => x.DowntimeEnd).HasColumnName("downtime_end");
            entity.Property(x => x.DowntimeMinutes).HasColumnName("downtime_minutes");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId);
            entity.HasOne(x => x.CategoryDetail).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PriorityDetail).WithMany().HasForeignKey(x => x.PriorityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.StatusDetail).WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.ReportNumber).IsUnique();
            entity.HasIndex(x => x.AssetId);
            entity.HasIndex(x => x.StatusId);
            entity.HasIndex(x => x.CategoryId);
            entity.HasIndex(x => x.PriorityId);
        });

        modelBuilder.Entity<WorkOrderPhoto>(entity =>
        {
            entity.ToTable("work_order_photos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.WorkOrderId).HasColumnName("work_order_id");
            entity.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(120).IsRequired();
            entity.Property(x => x.SizeBytes).HasColumnName("size_bytes");
            entity.Property(x => x.FileData).HasColumnName("file_data").HasColumnType("longblob").IsRequired();
            entity.Property(x => x.UploadedBy).HasColumnName("uploaded_by").HasMaxLength(150);
            entity.Property(x => x.UploadedAt).HasColumnName("uploaded_at");
            entity.HasOne(x => x.WorkOrder).WithMany(x => x.Photos).HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.WorkOrderId);
        });

        modelBuilder.Entity<ContractorWorkPlan>(entity =>
        {
            entity.ToTable("contractor_work_plans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.VendorName).HasColumnName("vendor_name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.VendorPicName).HasColumnName("vendor_pic_name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.VendorPicPhone).HasColumnName("vendor_pic_phone").HasMaxLength(50);
            entity.Property(x => x.WorkerCount).HasColumnName("worker_count");
            entity.Property(x => x.InternalPicName).HasColumnName("internal_pic_name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.DepartmentArea).HasColumnName("department_area").HasMaxLength(120).IsRequired();
            entity.Property(x => x.WorkTitle).HasColumnName("work_title").HasMaxLength(200).IsRequired();
            entity.Property(x => x.WorkDescription).HasColumnName("work_description").HasColumnType("text");
            entity.Property(x => x.WorkArea).HasColumnName("work_area").HasMaxLength(120).IsRequired();
            entity.Property(x => x.WorkLocation).HasColumnName("work_location").HasMaxLength(200);
            entity.Property(x => x.AssetId).HasColumnName("asset_id");
            entity.Property(x => x.AdditionalNotes).HasColumnName("additional_notes").HasColumnType("text");
            entity.Property(x => x.StartAt).HasColumnName("start_at");
            entity.Property(x => x.EndAt).HasColumnName("end_at");
            entity.Property(x => x.EstimatedDurationMinutes).HasColumnName("estimated_duration_minutes");
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.PermitDocumentStatus).HasColumnName("permit_document_status").HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.WorkingAtHeight).HasColumnName("working_at_height");
            entity.Property(x => x.HotWork).HasColumnName("hot_work");
            entity.Property(x => x.Welding).HasColumnName("welding");
            entity.Property(x => x.ElectricalWork).HasColumnName("electrical_work");
            entity.Property(x => x.ConfinedSpace).HasColumnName("confined_space");
            entity.Property(x => x.HeavyEquipmentActivity).HasColumnName("heavy_equipment_activity");
            entity.Property(x => x.ChemicalHandling).HasColumnName("chemical_handling");
            entity.Property(x => x.ShutdownActivity).HasColumnName("shutdown_activity");
            entity.Property(x => x.LotoRequired).HasColumnName("loto_required");
            entity.Property(x => x.NeedSafetyStandby).HasColumnName("need_safety_standby");
            entity.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(150);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(150);
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.PermitUploadedBy).HasColumnName("permit_uploaded_by").HasMaxLength(150);
            entity.Property(x => x.PermitUploadedAt).HasColumnName("permit_uploaded_at");
            entity.Property(x => x.WorkOrderId).HasColumnName("work_order_id");
            entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.SupervisionWorkOrder).WithMany().HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => x.VendorName);
            entity.HasIndex(x => x.WorkArea);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.StartAt);
        });

        modelBuilder.Entity<ContractorWorkDocument>(entity =>
        {
            entity.ToTable("contractor_work_documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ContractorWorkPlanId).HasColumnName("contractor_work_plan_id");
            entity.Property(x => x.DocumentType).HasColumnName("document_type").HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.DocumentStatus).HasColumnName("document_status").HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(120).IsRequired();
            entity.Property(x => x.SizeBytes).HasColumnName("size_bytes");
            entity.Property(x => x.FileData).HasColumnName("file_data").HasColumnType("longblob").IsRequired();
            entity.Property(x => x.UploadedBy).HasColumnName("uploaded_by").HasMaxLength(150);
            entity.Property(x => x.UploadedAt).HasColumnName("uploaded_at");
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            entity.Property(x => x.Notes).HasColumnName("notes").HasColumnType("text");
            entity.HasOne(x => x.ContractorWorkPlan).WithMany(x => x.Documents).HasForeignKey(x => x.ContractorWorkPlanId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.ContractorWorkPlanId);
            entity.HasIndex(x => x.DocumentType);
        });

        modelBuilder.Entity<ContractorWorkAudit>(entity =>
        {
            entity.ToTable("contractor_work_audits");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ContractorWorkPlanId).HasColumnName("contractor_work_plan_id");
            entity.Property(x => x.Action).HasColumnName("action").HasMaxLength(80).IsRequired();
            entity.Property(x => x.FieldName).HasColumnName("field_name").HasMaxLength(120);
            entity.Property(x => x.OldValue).HasColumnName("old_value").HasColumnType("text");
            entity.Property(x => x.NewValue).HasColumnName("new_value").HasColumnType("text");
            entity.Property(x => x.PerformedBy).HasColumnName("performed_by").HasMaxLength(150);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne(x => x.ContractorWorkPlan).WithMany(x => x.Audits).HasForeignKey(x => x.ContractorWorkPlanId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.ContractorWorkPlanId);
            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<PreventiveSchedule>(entity =>
        {
            entity.ToTable("preventive_schedules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssetId).HasColumnName("asset_id");
            entity.Property(x => x.ScheduleName).HasColumnName("schedule_name").HasMaxLength(200).IsRequired();
            entity.Property(x => x.ScheduleTypeId).HasColumnName("schedule_type_id").IsRequired();
            entity.Property(x => x.FrequencyTypeId).HasColumnName("frequency_type_id").IsRequired();
            entity.Property(x => x.FrequencyValue).HasColumnName("frequency_value");
            entity.Property(x => x.NextDueDate).HasColumnName("next_due_date");
            entity.Property(x => x.LastGeneratedAt).HasColumnName("last_generated_at");
            entity.Property(x => x.EstimatedDurationMinutes).HasColumnName("estimated_duration_minutes");
            entity.Property(x => x.ChecklistTemplate).HasColumnName("checklist_template").HasColumnType("text");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(x => x.Asset).WithMany(x => x.PreventiveSchedules).HasForeignKey(x => x.AssetId);
            entity.HasOne(x => x.ScheduleTypeDetail).WithMany().HasForeignKey(x => x.ScheduleTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FrequencyTypeDetail).WithMany().HasForeignKey(x => x.FrequencyTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.ScheduleTypeId);
            entity.HasIndex(x => x.FrequencyTypeId);
        });

        modelBuilder.Entity<DowntimeLog>(entity =>
        {
            entity.ToTable("downtime_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssetId).HasColumnName("asset_id");
            entity.Property(x => x.WorkOrderId).HasColumnName("work_order_id");
            entity.Property(x => x.ProblemReportId).HasColumnName("problem_report_id");
            entity.Property(x => x.DowntimeCategoryId).HasColumnName("downtime_category_id").IsRequired();
            entity.Property(x => x.StartTime).HasColumnName("start_time");
            entity.Property(x => x.EndTime).HasColumnName("end_time");
            entity.Property(x => x.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(x => x.Description).HasColumnName("description").HasColumnType("text");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne(x => x.Asset).WithMany().HasForeignKey(x => x.AssetId);
            entity.HasOne(x => x.WorkOrder).WithMany(x => x.DowntimeLogs).HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProblemReport).WithMany(x => x.DowntimeLogs).HasForeignKey(x => x.ProblemReportId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.DowntimeCategoryDetail).WithMany().HasForeignKey(x => x.DowntimeCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.WorkOrderId).IsUnique();
            entity.HasIndex(x => x.ProblemReportId).IsUnique();
            entity.HasIndex(x => x.DowntimeCategoryId);
        });

        modelBuilder.Entity<Sparepart>(entity =>
        {
            entity.ToTable("spareparts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartCode).HasColumnName("part_code").HasMaxLength(80).IsRequired();
            entity.Property(x => x.PartName).HasColumnName("part_name").HasMaxLength(200).IsRequired();
            entity.Property(x => x.Category).HasColumnName("category").HasMaxLength(100);
            entity.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(30);
            entity.Property(x => x.StockQty).HasColumnName("stock_qty").HasPrecision(18, 2);
            entity.Property(x => x.MinimumStock).HasColumnName("minimum_stock").HasPrecision(18, 2);
            entity.Property(x => x.Location).HasColumnName("location").HasMaxLength(150);
            entity.Property(x => x.Supplier).HasColumnName("supplier").HasMaxLength(150);
            entity.Property(x => x.LeadTimeDays).HasColumnName("lead_time_days");
            entity.Property(x => x.Price).HasColumnName("price").HasPrecision(18, 2);
            entity.Property(x => x.IsCritical).HasColumnName("is_critical");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.PartCode).IsUnique();
        });

        modelBuilder.Entity<WorkOrderSparepartUsage>(entity =>
        {
            entity.ToTable("work_order_spareparts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.WorkOrderId).HasColumnName("work_order_id");
            entity.Property(x => x.SparepartId).HasColumnName("sparepart_id");
            entity.Property(x => x.QtyUsed).HasColumnName("qty_used").HasPrecision(18, 2);
            entity.Property(x => x.UsedBy).HasColumnName("used_by").HasMaxLength(150);
            entity.Property(x => x.UsedAt).HasColumnName("used_at");
            entity.HasOne(x => x.WorkOrder).WithMany(x => x.SparepartUsages).HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Sparepart).WithMany(x => x.WorkOrderUsages).HasForeignKey(x => x.SparepartId);
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.ToTable("inventory_transactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SparepartId).HasColumnName("sparepart_id");
            entity.Property(x => x.TransactionType).HasColumnName("transaction_type").HasMaxLength(50);
            entity.Property(x => x.Quantity).HasColumnName("quantity").HasPrecision(18, 2);
            entity.Property(x => x.BalanceAfter).HasColumnName("balance_after").HasPrecision(18, 2);
            entity.Property(x => x.ReferenceType).HasColumnName("reference_type").HasMaxLength(50);
            entity.Property(x => x.ReferenceId).HasColumnName("reference_id");
            entity.Property(x => x.PerformedBy).HasColumnName("performed_by").HasMaxLength(150);
            entity.Property(x => x.TransactionAt).HasColumnName("transaction_at");
            entity.Property(x => x.Remarks).HasColumnName("remarks").HasColumnType("text");
            entity.HasOne(x => x.Sparepart).WithMany().HasForeignKey(x => x.SparepartId);
        });

        modelBuilder.Entity<FailureCode>(entity =>
        {
            entity.ToTable("failure_codes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Category).HasColumnName("category").HasMaxLength(100);
            entity.Property(x => x.Description).HasColumnName("description").HasColumnType("text");
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<RootCause>(entity =>
        {
            entity.ToTable("root_causes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description").HasColumnType("text");
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<TelegramSettings>(entity =>
        {
            entity.ToTable("telegram_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BotToken).HasColumnName("bot_token").HasMaxLength(255).IsRequired();
            entity.Property(x => x.ChatId).HasColumnName("chat_id").HasMaxLength(120);
            entity.Property(x => x.IsEnabled).HasColumnName("is_enabled");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Preventive>(entity =>
        {
            entity.ToTable("preventivehistory");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Line).HasColumnName("Line");
            entity.Property(x => x.Machine).HasColumnName("Machine");
            entity.Property(x => x.Technician).HasColumnName("Technician");
            entity.Property(x => x.Action).HasColumnName("Action");
            entity.Property(x => x.Image).HasColumnName("Image");
            entity.Property(x => x.TimeStamp).HasColumnName("TimeStamp");
        });

        modelBuilder.Entity<MachineStatus>(entity =>
        {
            entity.ToTable("machinestatushistory");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.LineName).HasColumnName("LineName");
            entity.Property(x => x.MachineName).HasColumnName("MachineName");
            entity.Property(x => x.Status).HasColumnName("Status");
            entity.Property(x => x.StartTime).HasColumnName("StartTime");
            entity.Property(x => x.EndTime).HasColumnName("EndTime");
        });
    }
}
