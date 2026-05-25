using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.TechnicianService;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/technicians")]
public class TechniciansController : CmmsControllerBase
{
    private readonly ITechnicianService _service;

    public TechniciansController(ITechnicianService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] TechnicianStatus? status, [FromQuery(Name = "skill_type")] TechnicianSkillType? skillType)
    {
        return ApiOk(await _service.GetTechniciansAsync(status, skillType));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetTechnicianAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Technician technician)
    {
        try
        {
            return ApiCreated(await _service.CreateTechnicianAsync(technician));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Technician technician)
    {
        try
        {
            var result = await _service.UpdateTechnicianAsync(id, technician);
            return result == null ? ApiNotFound() : ApiOk(result, "Data updated successfully");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _service.DeleteTechnicianAsync(id) ? ApiNoContent() : ApiNotFound();
    }
}
