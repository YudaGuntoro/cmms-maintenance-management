using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.WorkOrderSparepartRepository;

public class WorkOrderSparepartRepository : IWorkOrderSparepartRepository
{
    private readonly MaintenanceDbContext Context;

    public WorkOrderSparepartRepository(MaintenanceDbContext context) => Context = context;

    public async Task<WorkOrderSparepartUsage?> UseSparepartAsync(int workOrderId, WorkOrderSparepartRequest request)
    {
        if (request.QtyUsed <= 0)
        {
            throw new InvalidOperationException("Qty pemakaian sparepart harus lebih besar dari 0.");
        }

        var workOrder = await Context.WorkOrders.FirstOrDefaultAsync(x => x.Id == workOrderId);
        if (workOrder == null)
        {
            return null;
        }

        var sparepart = await Context.Spareparts.FirstOrDefaultAsync(x => x.Id == request.SparepartId);
        if (sparepart == null)
        {
            throw new InvalidOperationException("Sparepart tidak ditemukan.");
        }

        if (sparepart.StockQty - request.QtyUsed < 0)
        {
            throw new InvalidOperationException("Stock sparepart tidak boleh minus.");
        }

        sparepart.StockQty -= request.QtyUsed;
        sparepart.UpdatedAt = DateTime.Now;

        var usage = new WorkOrderSparepartUsage
        {
            WorkOrderId = workOrderId,
            SparepartId = request.SparepartId,
            QtyUsed = request.QtyUsed,
            UsedBy = request.UsedBy,
            UsedAt = request.UsedAt ?? DateTime.Now
        };

        Context.WorkOrderSparepartUsages.Add(usage);
        Context.InventoryTransactions.Add(new InventoryTransaction
        {
            SparepartId = sparepart.Id,
            TransactionType = "ISSUE",
            Quantity = request.QtyUsed * -1,
            BalanceAfter = sparepart.StockQty,
            ReferenceType = "WORK_ORDER",
            ReferenceId = workOrderId,
            PerformedBy = request.UsedBy,
            TransactionAt = usage.UsedAt,
            Remarks = $"Used for {workOrder.WoNumber}"
        });

        await Context.SaveChangesAsync();
        return usage;
    }
}
