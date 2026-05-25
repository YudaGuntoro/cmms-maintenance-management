using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.PreventiveScheduleService;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/preventive-schedules")]
public class PreventiveSchedulesController : CmmsControllerBase
{
    private readonly IPreventiveScheduleService _service;

    public PreventiveSchedulesController(IPreventiveScheduleService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery(Name = "is_active")] bool? isActive)
    {
        return ApiOk(await _service.GetPreventiveSchedulesAsync(isActive));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetPreventiveScheduleAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PreventiveSchedule schedule)
    {
        try
        {
            return ApiCreated(await _service.CreatePreventiveScheduleAsync(schedule));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PreventiveSchedule schedule)
    {
        try
        {
            var result = await _service.UpdatePreventiveScheduleAsync(id, schedule);
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
        return await _service.DeletePreventiveScheduleAsync(id) ? ApiNoContent() : ApiNotFound();
    }

    [HttpPost("generate-due")]
    public async Task<IActionResult> GenerateDue([FromQuery(Name = "due_date")] DateTime? dueDate)
    {
        return ApiCreated(await _service.GenerateDuePreventiveWorkOrdersAsync(dueDate), "Preventive work orders generated");
    }
}
