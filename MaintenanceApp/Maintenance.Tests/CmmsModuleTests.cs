using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Maintenance.Persistence.Services.ContractorMonitoringService;
using Maintenance.Persistence.Services.AssetService;
using Maintenance.Persistence.Services.KpiService;
using Maintenance.Persistence.Services.PreventiveScheduleService;
using Maintenance.Persistence.Services.WorkOrderService;
using Maintenance.Persistence.Services.WorkOrderSparepartService;
using Maintenance.Repository.AssetRepository;
using Maintenance.Repository.ContractorMonitoringRepository;
using Maintenance.Repository.KpiRepository;
using Maintenance.Repository.PreventiveScheduleRepository;
using Maintenance.Repository.WorkOrderRepository;
using Maintenance.Repository.WorkOrderSparepartRepository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maintenance.Tests;

public class CmmsModuleTests
{
    [Fact]
    public async Task CreateAssetAsync_CreatesAsset()
    {
        await using var db = CreateDbContext();
        var service = new AssetService(new AssetRepository(db));

        var asset = await service.CreateAssetAsync(new Asset
        {
            AssetCode = "AST-TEST-001",
            AssetName = "Test Machine",
            AssetType = "Filler",
            Plant = "Plant A",
            Area = "Packaging",
            ProductionLine = "Line 1",
            Location = "PK-01",
            CriticalityLevel = AssetCriticalityLevel.HIGH,
            Status = AssetStatus.ACTIVE
        });

        Assert.True(asset.Id > 0);
        Assert.Equal("AST-TEST-001", asset.AssetCode);
    }

    [Fact]
    public async Task WorkOrderFlow_CalculatesDurations_AndCreatesDowntimeLog()
    {
        await using var db = CreateDbContext();
        await SeedAssetAndTechnicianAsync(db);
        var service = new WorkOrderService(new WorkOrderRepository(db));

        var workOrder = await service.CreateWorkOrderAsync(new WorkOrder
        {
            AssetId = 1,
            Title = "Breakdown test",
            MaintenanceType = MaintenanceType.BREAKDOWN,
            Priority = WorkOrderPriority.URGENT,
            Status = WorkOrderStatus.OPEN
        });

        await service.AssignWorkOrderAsync(workOrder.Id, new WorkOrderAssignRequest { AssignedTo = 1 });
        await service.StartWorkOrderAsync(workOrder.Id);
        var completed = await service.CompleteWorkOrderAsync(workOrder.Id, new WorkOrderCompleteRequest
        {
            CompletedAt = new DateTime(2026, 5, 15, 10, 0, 0),
            DowntimeStart = new DateTime(2026, 5, 15, 8, 0, 0),
            DowntimeEnd = new DateTime(2026, 5, 15, 10, 0, 0),
            RepairStart = new DateTime(2026, 5, 15, 8, 30, 0),
            RepairEnd = new DateTime(2026, 5, 15, 10, 0, 0),
            FailureCode = "MECH-BRG",
            RootCause = "LACK-LUBE",
            ActionTaken = "Replaced bearing"
        });
        var closed = await service.CloseWorkOrderAsync(workOrder.Id, new WorkOrderCloseRequest
        {
            ClosedAt = new DateTime(2026, 5, 15, 10, 15, 0)
        });

        Assert.Equal(120, completed?.DowntimeMinutes);
        Assert.Equal(90, completed?.RepairMinutes);
        Assert.Equal(WorkOrderStatus.CLOSED, closed?.Status);
        Assert.Single(await db.DowntimeLogs.ToListAsync());
    }

    [Fact]
    public async Task CloseBreakdown_WithoutFailureAndRootCause_IsRejected()
    {
        await using var db = CreateDbContext();
        await SeedAssetAndTechnicianAsync(db);
        var service = new WorkOrderService(new WorkOrderRepository(db));

        var workOrder = await service.CreateWorkOrderAsync(new WorkOrder
        {
            AssetId = 1,
            Title = "Breakdown test",
            MaintenanceType = MaintenanceType.BREAKDOWN,
            Priority = WorkOrderPriority.HIGH
        });

        await service.AssignWorkOrderAsync(workOrder.Id, new WorkOrderAssignRequest { AssignedTo = 1 });
        await service.StartWorkOrderAsync(workOrder.Id);
        await service.CompleteWorkOrderAsync(workOrder.Id, new WorkOrderCompleteRequest
        {
            CompletedAt = DateTime.Now
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CloseWorkOrderAsync(workOrder.Id, new WorkOrderCloseRequest { ClosedAt = DateTime.Now }));
    }

    [Fact]
    public async Task GenerateDuePreventiveWorkOrdersAsync_DoesNotDuplicatePeriod()
    {
        await using var db = CreateDbContext();
        await SeedAssetAndTechnicianAsync(db);
        db.PreventiveSchedules.Add(new PreventiveSchedule
        {
            AssetId = 1,
            ScheduleName = "Weekly PM",
            ScheduleTypeId = 2,
            FrequencyTypeId = 2,
            FrequencyType = FrequencyType.WEEKLY,
            FrequencyValue = 1,
            NextDueDate = new DateTime(2026, 5, 1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = new PreventiveScheduleService(new PreventiveScheduleRepository(db));

        var generated = await service.GenerateDuePreventiveWorkOrdersAsync(new DateTime(2026, 5, 1));
        var secondRun = await service.GenerateDuePreventiveWorkOrdersAsync(new DateTime(2026, 5, 1));

        Assert.Single(generated);
        Assert.Empty(secondRun);
    }

    [Fact]
    public async Task ReliabilityKpiAsync_CalculatesMttrAndMtbf()
    {
        await using var db = CreateDbContext();
        await SeedAssetAndTechnicianAsync(db);
        var service = new WorkOrderService(new WorkOrderRepository(db));

        var workOrder = await service.CreateWorkOrderAsync(new WorkOrder
        {
            AssetId = 1,
            Title = "Breakdown for KPI",
            MaintenanceType = MaintenanceType.BREAKDOWN,
            Priority = WorkOrderPriority.URGENT
        });

        await service.AssignWorkOrderAsync(workOrder.Id, new WorkOrderAssignRequest { AssignedTo = 1 });
        await service.StartWorkOrderAsync(workOrder.Id);
        await service.CompleteWorkOrderAsync(workOrder.Id, new WorkOrderCompleteRequest
        {
            CompletedAt = new DateTime(2026, 5, 2, 2, 0, 0),
            DowntimeStart = new DateTime(2026, 5, 2, 0, 0, 0),
            DowntimeEnd = new DateTime(2026, 5, 2, 2, 0, 0),
            RepairStart = new DateTime(2026, 5, 2, 1, 0, 0),
            RepairEnd = new DateTime(2026, 5, 2, 2, 0, 0),
            FailureCode = "MECH-BRG",
            RootCause = "WEAR-TEAR"
        });
        await service.CloseWorkOrderAsync(workOrder.Id, new WorkOrderCloseRequest
        {
            ClosedAt = new DateTime(2026, 5, 2, 3, 0, 0)
        });

        var kpiService = new KpiService(new KpiRepository(db));
        var kpi = await kpiService.GetReliabilityKpiAsync(1, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31));

        Assert.Equal(1, kpi.FailureCount);
        Assert.Equal(120, kpi.TotalDowntimeMinutes);
        Assert.Equal(60, kpi.MttrMinutes);
        Assert.True(kpi.MtbfMinutes > 0);
    }

    [Fact]
    public async Task UseSparepartAsync_DecreasesStock_AndRejectsNegativeStock()
    {
        await using var db = CreateDbContext();
        await SeedAssetAndTechnicianAsync(db);
        db.Spareparts.Add(new Sparepart
        {
            Id = 1,
            PartCode = "SP-001",
            PartName = "Bearing",
            Unit = "PCS",
            StockQty = 5,
            MinimumStock = 2
        });
        await db.SaveChangesAsync();

        var workOrderService = new WorkOrderService(new WorkOrderRepository(db));
        var sparepartUsageService = new WorkOrderSparepartService(new WorkOrderSparepartRepository(db));
        var workOrder = await workOrderService.CreateWorkOrderAsync(new WorkOrder
        {
            AssetId = 1,
            Title = "Use sparepart",
            MaintenanceType = MaintenanceType.CORRECTIVE
        });

        await sparepartUsageService.UseSparepartAsync(workOrder.Id, new WorkOrderSparepartRequest
        {
            SparepartId = 1,
            QtyUsed = 2,
            UsedBy = "Unit Test"
        });

        Assert.Equal(3, (await db.Spareparts.FirstAsync()).StockQty);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sparepartUsageService.UseSparepartAsync(workOrder.Id, new WorkOrderSparepartRequest { SparepartId = 1, QtyUsed = 4 }));
    }

    [Fact]
    public async Task ContractorMonitoring_CreatesPlanReminderAndSupervisionWorkOrder()
    {
        await using var db = CreateDbContext();
        await SeedAssetAndTechnicianAsync(db);
        var service = new ContractorMonitoringService(new ContractorMonitoringRepository(db));
        var start = DateTime.Now.AddHours(8);

        var plan = await service.CreatePlanAsync(new ContractorWorkPlan
        {
            VendorName = "Vendor Test",
            VendorPicName = "Vendor PIC",
            VendorPicPhone = "08123456789",
            WorkerCount = 4,
            InternalPicName = "MTC Supervisor",
            DepartmentArea = "Packaging",
            WorkTitle = "Install cable tray",
            WorkDescription = "Tarik dan koneksi kabel panel",
            WorkArea = "Line 1",
            WorkLocation = "Panel room",
            AssetId = 1,
            StartAt = start,
            EndAt = start.AddHours(2),
            Status = ContractorWorkStatus.PLANNED,
            PermitDocumentStatus = ContractorDocumentStatus.NOT_UPLOADED,
            ElectricalWork = true,
            LotoRequired = true
        }, "planner");

        var reminders = await service.GetRemindersAsync(DateTime.Now);
        var workOrder = await service.CreateSupervisionWorkOrderAsync(plan.Id, "admin");

        Assert.Equal(120, plan.EstimatedDurationMinutes);
        Assert.Contains(reminders, x => x.Type == "PERMIT_ATTENTION");
        Assert.Contains(reminders, x => x.Type == "HIGH_RISK");
        Assert.Equal(MaintenanceType.CONTRACTOR_SUPERVISION, workOrder?.MaintenanceType);
        Assert.Equal(workOrder?.Id, (await db.ContractorWorkPlans.FirstAsync()).WorkOrderId);
    }

    private static MaintenanceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MaintenanceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MaintenanceDbContext(options);
    }

    private static async Task SeedAssetAndTechnicianAsync(MaintenanceDbContext db)
    {
        db.MaintenanceTypes.AddRange(
            new MaintenanceTypeMaster { Id = 1, Code = "PREVENTIVE", Name = "Preventive" },
            new MaintenanceTypeMaster { Id = 2, Code = "CORRECTIVE", Name = "Corrective" },
            new MaintenanceTypeMaster { Id = 3, Code = "BREAKDOWN", Name = "Breakdown" },
            new MaintenanceTypeMaster { Id = 4, Code = "PREDICTIVE", Name = "Predictive" },
            new MaintenanceTypeMaster { Id = 5, Code = "INSPECTION", Name = "Inspection" },
            new MaintenanceTypeMaster { Id = 6, Code = "CONTRACTOR_SUPERVISION", Name = "Contractor Supervision" });
        db.WorkOrderPriorities.AddRange(
            new WorkOrderPriorityMaster { Id = 1, Code = "LOW", Name = "Low", Level = 1 },
            new WorkOrderPriorityMaster { Id = 2, Code = "MEDIUM", Name = "Medium", Level = 2 },
            new WorkOrderPriorityMaster { Id = 3, Code = "HIGH", Name = "High", Level = 3 },
            new WorkOrderPriorityMaster { Id = 4, Code = "URGENT", Name = "Urgent", Level = 4 });
        db.WorkOrderStatuses.AddRange(
            new WorkOrderStatusMaster { Id = 1, Code = "OPEN", Name = "Open", Sequence = 1 },
            new WorkOrderStatusMaster { Id = 2, Code = "ASSIGNED", Name = "Assigned", Sequence = 2 },
            new WorkOrderStatusMaster { Id = 3, Code = "IN_PROGRESS", Name = "In Progress", Sequence = 3 },
            new WorkOrderStatusMaster { Id = 4, Code = "PENDING", Name = "Pending", Sequence = 4 },
            new WorkOrderStatusMaster { Id = 5, Code = "DRAFT", Name = "Draft", Sequence = 5 },
            new WorkOrderStatusMaster { Id = 6, Code = "COMPLETED", Name = "Completed", Sequence = 6 },
            new WorkOrderStatusMaster { Id = 7, Code = "CLOSED", Name = "Closed", Sequence = 7 },
            new WorkOrderStatusMaster { Id = 8, Code = "CANCELLED", Name = "Cancelled", Sequence = 8 });
        db.DowntimeCategories.AddRange(
            new DowntimeCategoryMaster { Id = 1, Code = "MECHANICAL", Name = "Mechanical" },
            new DowntimeCategoryMaster { Id = 2, Code = "ELECTRICAL", Name = "Electrical" },
            new DowntimeCategoryMaster { Id = 3, Code = "UTILITY", Name = "Utility" },
            new DowntimeCategoryMaster { Id = 4, Code = "OPERATIONAL", Name = "Operational" },
            new DowntimeCategoryMaster { Id = 5, Code = "PROCESS", Name = "Process" },
            new DowntimeCategoryMaster { Id = 6, Code = "MATERIAL", Name = "Material" },
            new DowntimeCategoryMaster { Id = 7, Code = "PLANNED_STOP", Name = "Planned Stop" },
            new DowntimeCategoryMaster { Id = 8, Code = "OTHER", Name = "Other" });
        db.ProblemReportCategories.AddRange(
            new ProblemReportCategoryMaster { Id = 1, Code = "DOWNTIME", Name = "Downtime" },
            new ProblemReportCategoryMaster { Id = 2, Code = "BREAKDOWN", Name = "Breakdown" },
            new ProblemReportCategoryMaster { Id = 3, Code = "QUALITY", Name = "Quality" },
            new ProblemReportCategoryMaster { Id = 4, Code = "SAFETY", Name = "Safety" },
            new ProblemReportCategoryMaster { Id = 5, Code = "OTHER", Name = "Other" });
        db.PreventiveScheduleTypes.AddRange(
            new PreventiveScheduleTypeMaster { Id = 1, Code = "DAILY", Name = "Daily" },
            new PreventiveScheduleTypeMaster { Id = 2, Code = "WEEKLY", Name = "Weekly" },
            new PreventiveScheduleTypeMaster { Id = 3, Code = "MONTHLY", Name = "Monthly" },
            new PreventiveScheduleTypeMaster { Id = 4, Code = "YEARLY", Name = "Yearly" });
        db.FrequencyTypes.AddRange(
            new FrequencyTypeMaster { Id = 1, Code = "DAILY", Name = "Daily", IntervalDays = 1 },
            new FrequencyTypeMaster { Id = 2, Code = "WEEKLY", Name = "Weekly", IntervalDays = 7 },
            new FrequencyTypeMaster { Id = 3, Code = "MONTHLY", Name = "Monthly", IntervalDays = 30 },
            new FrequencyTypeMaster { Id = 4, Code = "YEARLY", Name = "Yearly", IntervalDays = 365 },
            new FrequencyTypeMaster { Id = 5, Code = "RUNNING_HOURS", Name = "Running Hours", IntervalDays = 1 });
        db.Assets.Add(new Asset
        {
            Id = 1,
            AssetCode = "AST-001",
            AssetName = "Machine 1",
            AssetType = "Filler",
            Plant = "Plant A",
            Area = "Packaging",
            ProductionLine = "Line 1",
            Location = "PK-01",
            CriticalityLevel = AssetCriticalityLevel.CRITICAL,
            Status = AssetStatus.ACTIVE
        });
        db.Technicians.Add(new Technician
        {
            Id = 1,
            EmployeeNo = "EMP-001",
            Name = "Technician 1",
            SkillType = TechnicianSkillType.MECHANICAL,
            Shift = "A",
            Status = TechnicianStatus.ACTIVE
        });
        await db.SaveChangesAsync();
    }
}
