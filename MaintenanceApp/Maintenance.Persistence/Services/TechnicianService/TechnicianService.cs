using Maintenance.Domain.Cmms;
using Maintenance.Repository.TechnicianRepository;

namespace Maintenance.Persistence.Services.TechnicianService;

public class TechnicianService : ITechnicianService
{
    protected readonly ITechnicianRepository Context;

    public TechnicianService(ITechnicianRepository data) => Context = data;

    public Task<List<Technician>> GetTechniciansAsync(TechnicianStatus? status, TechnicianSkillType? skillType)
    {
        return Context.GetTechniciansAsync(status, skillType);
    }

    public Task<Technician?> GetTechnicianAsync(int id)
    {
        return Context.GetTechnicianAsync(id);
    }

    public Task<Technician> CreateTechnicianAsync(Technician technician)
    {
        return Context.CreateTechnicianAsync(technician);
    }

    public Task<Technician?> UpdateTechnicianAsync(int id, Technician technician)
    {
        return Context.UpdateTechnicianAsync(id, technician);
    }

    public Task<bool> DeleteTechnicianAsync(int id)
    {
        return Context.DeleteTechnicianAsync(id);
    }
}
