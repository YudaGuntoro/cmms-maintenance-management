using Maintenance.Domain.Mapping.Entities;
using Maintenance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.MachineTroubleRepository;

public class MachineTroubleRepository : IMachineTroubleRepository
{
    private readonly MaintenanceDbContext Context;

    public MachineTroubleRepository(MaintenanceDbContext context) => Context = context;

    public async Task<IEnumerable<MachineStatus>> GetMachineStatusTroubleAsync()
    {
        return await Context.MachineStatusHistory
            .AsNoTracking()
            .Where(x => x.Status != "Run" && x.EndTime == null)
            .OrderByDescending(x => x.StartTime)
            .ToListAsync();
    }
}
