using Maintenance.Domain.Mapping.Entities;
using Maintenance.Domain.Mapping.Request;
using Maintenance.Infrastructure.Context;

namespace Maintenance.Repository.PreventiveRepository
{
    public class PreventiveRepository : IPreventiveRepository
    {
        private readonly MaintenanceDbContext Context;

        public PreventiveRepository(MaintenanceDbContext context) => Context = context;

        public async Task<bool> InsertPreventiveAsync(RequestPreventive data)
        {
            var preventive = new Preventive
            {
                Line = data.Line,
                Machine = data.Machine,
                Technician = data.Technician,
                Action = data.Action,
                Image = data.Image ?? Array.Empty<byte>(),
                TimeStamp = data.TimeStamp == default ? DateTime.Now : data.TimeStamp
            };

            Context.PreventiveHistory.Add(preventive);
            return await Context.SaveChangesAsync() > 0;
        }
    }
}
