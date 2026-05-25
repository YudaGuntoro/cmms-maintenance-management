using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.ProblemReportService;
using Maintenance.Persistence.Services.TelegramService;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/problem-reports")]
public class ProblemReportsController : CmmsControllerBase
{
    private readonly IProblemReportService _service;
    private readonly ITelegramNotificationService _telegramService;

    public ProblemReportsController(IProblemReportService service, ITelegramNotificationService telegramService)
    {
        _service = service;
        _telegramService = telegramService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery(Name = "asset_id")] int? assetId,
        [FromQuery] ProblemReportStatus? status,
        [FromQuery] ProblemReportCategory? category,
        [FromQuery(Name = "reported_by")] string? reportedBy)
    {
        return ApiOk(await _service.GetProblemReportsAsync(assetId, status, category, reportedBy));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetProblemReportAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpGet("next-number")]
    public async Task<IActionResult> GetNextNumber()
    {
        return ApiOk(await _service.GetNextReportNumberAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProblemReport report)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(report.ReportedBy))
            {
                report.ReportedBy = User.FindFirstValue("full_name") ?? User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
            }

            var result = await _service.CreateProblemReportAsync(report);
            await _telegramService.NotifyProblemReportCreatedAsync(result);
            return ApiCreated(result);
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProblemReport report)
    {
        try
        {
            var result = await _service.UpdateProblemReportAsync(id, report);
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
        return await _service.DeleteProblemReportAsync(id) ? ApiNoContent() : ApiNotFound();
    }
}
