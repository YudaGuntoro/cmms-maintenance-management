using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.FailureCodeService;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/failure-codes")]
public class FailureCodesController : CmmsControllerBase
{
    private readonly IFailureCodeService _service;

    public FailureCodesController(IFailureCodeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get() => ApiOk(await _service.GetFailureCodesAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetFailureCodeAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FailureCode failureCode)
    {
        try
        {
            return ApiCreated(await _service.CreateFailureCodeAsync(failureCode));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] FailureCode failureCode)
    {
        try
        {
            var result = await _service.UpdateFailureCodeAsync(id, failureCode);
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
        return await _service.DeleteFailureCodeAsync(id) ? ApiNoContent() : ApiNotFound();
    }
}
