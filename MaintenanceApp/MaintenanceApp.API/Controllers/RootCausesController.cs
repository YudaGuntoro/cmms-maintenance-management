using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.RootCauseService;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/root-causes")]
public class RootCausesController : CmmsControllerBase
{
    private readonly IRootCauseService _service;

    public RootCausesController(IRootCauseService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get() => ApiOk(await _service.GetRootCausesAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetRootCauseAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RootCause rootCause)
    {
        try
        {
            return ApiCreated(await _service.CreateRootCauseAsync(rootCause));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] RootCause rootCause)
    {
        try
        {
            var result = await _service.UpdateRootCauseAsync(id, rootCause);
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
        return await _service.DeleteRootCauseAsync(id) ? ApiNoContent() : ApiNotFound();
    }
}
