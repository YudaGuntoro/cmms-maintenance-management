using Maintenance.Domain.Cmms;

namespace Maintenance.Repository.TechnicianRepository;

public interface ITechnicianRepository
{
    Task<List<Technician>> GetTechniciansAsync(TechnicianStatus? status, TechnicianSkillType? skillType);
    Task<Technician?> GetTechnicianAsync(int id);
    Task<Technician> CreateTechnicianAsync(Technician technician);
    Task<Technician?> UpdateTechnicianAsync(int id, Technician technician);
    Task<bool> DeleteTechnicianAsync(int id);
}
