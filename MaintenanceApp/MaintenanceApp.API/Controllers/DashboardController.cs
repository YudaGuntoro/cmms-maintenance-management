using Maintenance.Persistence.Services.DashboardService;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : CmmsControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        return ApiOk(await _service.GetDashboardSummaryAsync());
    }
}
