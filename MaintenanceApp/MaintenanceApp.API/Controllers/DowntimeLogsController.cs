using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.DowntimeLogService;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/downtime-logs")]
public class DowntimeLogsController : CmmsControllerBase
{
    private readonly IDowntimeLogService _service;

    public DowntimeLogsController(IDowntimeLogService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery(Name = "asset_id")] int? assetId, [FromQuery(Name = "work_order_id")] int? workOrderId)
    {
        return ApiOk(await _service.GetDowntimeLogsAsync(assetId, workOrderId));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetDowntimeLogAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DowntimeLog downtimeLog)
    {
        try
        {
            return ApiCreated(await _service.CreateDowntimeLogAsync(downtimeLog));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DowntimeLog downtimeLog)
    {
        try
        {
            var result = await _service.UpdateDowntimeLogAsync(id, downtimeLog);
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
        return await _service.DeleteDowntimeLogAsync(id) ? ApiNoContent() : ApiNotFound();
    }
}
