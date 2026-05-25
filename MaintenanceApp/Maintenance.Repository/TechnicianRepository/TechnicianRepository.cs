using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Repository.TechnicianRepository;

public class TechnicianRepository : ITechnicianRepository
{
    private readonly MaintenanceDbContext Context;

    public TechnicianRepository(MaintenanceDbContext context) => Context = context;

    public async Task<List<Technician>> GetTechniciansAsync(TechnicianStatus? status, TechnicianSkillType? skillType)
    {
        var query = Context.Technicians.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (skillType.HasValue)
        {
            query = query.Where(x => x.SkillType == skillType.Value);
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }

    public Task<Technician?> GetTechnicianAsync(int id)
    {
        return Context.Technicians.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Technician> CreateTechnicianAsync(Technician technician)
    {
        technician.Id = 0;
        technician.CreatedAt = DateTime.Now;
        technician.UpdatedAt = DateTime.Now;
        Context.Technicians.Add(technician);
        await Context.SaveChangesAsync();
        return technician;
    }

    public async Task<Technician?> UpdateTechnicianAsync(int id, Technician technician)
    {
        var existing = await Context.Technicians.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return null;
        }

        existing.EmployeeNo = technician.EmployeeNo;
        existing.Name = technician.Name;
        existing.Email = technician.Email;
        existing.Phone = technician.Phone;
        existing.SkillType = technician.SkillType;
        existing.Shift = technician.Shift;
        existing.Status = technician.Status;
        existing.UpdatedAt = DateTime.Now;

        await Context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteTechnicianAsync(int id)
    {
        var existing = await Context.Technicians.FirstOrDefaultAsync(x => x.Id == id);
        if (existing == null)
        {
            return false;
        }

        Context.Technicians.Remove(existing);
        await Context.SaveChangesAsync();
        return true;
    }
}
