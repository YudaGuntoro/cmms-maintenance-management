using Maintenance.Persistence.Services.KpiService;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/kpi")]
public class KpiController : CmmsControllerBase
{
    private readonly IKpiService _service;

    public KpiController(IKpiService service) => _service = service;

    [HttpGet("reliability")]
    public async Task<IActionResult> Reliability(
        [FromQuery(Name = "asset_id")] int? assetId,
        [FromQuery(Name = "start_date")] DateTime? startDate,
        [FromQuery(Name = "end_date")] DateTime? endDate)
    {
        return ApiOk(await _service.GetReliabilityKpiAsync(assetId, startDate, endDate));
    }
}
