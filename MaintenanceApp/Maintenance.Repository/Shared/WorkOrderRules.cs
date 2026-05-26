using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.Shared;

public static class WorkOrderRules
{
    public static async Task<string> GenerateWorkOrderNumberAsync(MaintenanceDbContext db, MaintenanceType type)
    {
        var prefix = type switch
        {
            MaintenanceType.PREVENTIVE => "PM",
            MaintenanceType.CONTRACTOR_SUPERVISION => "CS",
            _ => "WO"
        };
        var datePart = DateTime.Now.ToString("yyyyMMdd");
        var countToday = await db.WorkOrders.CountAsync(x => x.WoNumber.StartsWith($"{prefix}-{datePart}"));
        return $"{prefix}-{datePart}-{countToday + 1:0000}";
    }

    public static void CalculateWorkOrderDurations(WorkOrder workOrder)
    {
        if (workOrder.RepairStart.HasValue && workOrder.RepairEnd.HasValue)
        {
            workOrder.RepairMinutes = CalculateMinutes(workOrder.RepairStart.Value, workOrder.RepairEnd.Value);
        }

        if (workOrder.DowntimeStart.HasValue && workOrder.DowntimeEnd.HasValue)
        {
            workOrder.DowntimeMinutes = CalculateMinutes(workOrder.DowntimeStart.Value, workOrder.DowntimeEnd.Value);
        }
    }

    public static void CalculateDowntimeDuration(DowntimeLog downtimeLog)
    {
        downtimeLog.DurationMinutes = downtimeLog.EndTime.HasValue
            ? CalculateMinutes(downtimeLog.StartTime, downtimeLog.EndTime.Value)
            : null;
    }

    public static void ValidateWorkOrderForSave(WorkOrder workOrder)
    {
        if (workOrder.MaintenanceTypeId.GetValueOrDefault() <= 0)
        {
            throw new InvalidOperationException("Type wajib dipilih.");
        }

        if (workOrder.PriorityId.GetValueOrDefault() <= 0)
        {
            throw new InvalidOperationException("Priority wajib dipilih.");
        }

        if (workOrder.StatusId.GetValueOrDefault() <= 0)
        {
            throw new InvalidOperationException("Status wajib dipilih.");
        }

        if (workOrder.Status == WorkOrderStatus.COMPLETED && !workOrder.CompletedAt.HasValue)
        {
            throw new InvalidOperationException("completed_at wajib saat status COMPLETED.");
        }

        if (workOrder.Status != WorkOrderStatus.CLOSED)
        {
            return;
        }

        if (!workOrder.CompletedAt.HasValue)
        {
            throw new InvalidOperationException("Work order tidak boleh CLOSED jika belum COMPLETED.");
        }

        if (!workOrder.ClosedAt.HasValue)
        {
            throw new InvalidOperationException("closed_at wajib saat status CLOSED.");
        }

        if (workOrder.MaintenanceType == MaintenanceType.BREAKDOWN &&
            (string.IsNullOrWhiteSpace(workOrder.FailureCode) || string.IsNullOrWhiteSpace(workOrder.RootCause)))
        {
            throw new InvalidOperationException("WO BREAKDOWN wajib punya failure_code dan root_cause sebelum CLOSED.");
        }
    }

    public static void EnsureStatus(WorkOrder workOrder, WorkOrderStatus expected, WorkOrderStatus next)
    {
        if (workOrder.Status != expected)
        {
            throw new InvalidOperationException($"Flow status normal adalah OPEN -> ASSIGNED -> IN_PROGRESS -> COMPLETED -> CLOSED. Status saat ini {workOrder.Status}, tidak bisa menjadi {next}.");
        }
    }

    public static DowntimeCategory MapDowntimeCategory(WorkOrder workOrder)
    {
        if (workOrder.MaintenanceType == MaintenanceType.PREVENTIVE)
        {
            return DowntimeCategory.OPERATIONAL;
        }

        if (workOrder.MaintenanceType == MaintenanceType.BREAKDOWN)
        {
            return DowntimeCategory.MECHANICAL;
        }

        return DowntimeCategory.OPERATIONAL;
    }

    public static string BuildPreventivePeriodKey(PreventiveSchedule schedule)
    {
        return $"{schedule.Id}:{schedule.FrequencyType}:{schedule.NextDueDate:yyyyMMdd}";
    }

    public static DateTime CalculateNextDueDate(PreventiveSchedule schedule)
    {
        var value = schedule.FrequencyValue <= 0 ? 1 : schedule.FrequencyValue;
        return schedule.FrequencyType switch
        {
            FrequencyType.DAILY => schedule.NextDueDate.AddDays(value),
            FrequencyType.WEEKLY => schedule.NextDueDate.AddDays(7 * value),
            FrequencyType.MONTHLY => schedule.NextDueDate.AddMonths(value),
            FrequencyType.YEARLY => schedule.NextDueDate.AddYears(value),
            FrequencyType.RUNNING_HOURS => schedule.NextDueDate.AddDays(value),
            _ => schedule.NextDueDate.AddDays(value)
        };
    }

    private static int CalculateMinutes(DateTime start, DateTime end)
    {
        if (end < start)
        {
            throw new InvalidOperationException("End time tidak boleh lebih kecil dari start time.");
        }

        return (int)Math.Round((end - start).TotalMinutes, MidpointRounding.AwayFromZero);
    }
}
