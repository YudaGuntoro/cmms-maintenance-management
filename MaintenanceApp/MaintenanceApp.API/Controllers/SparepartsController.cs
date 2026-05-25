using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.SparepartService;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/spareparts")]
public class SparepartsController : CmmsControllerBase
{
    private readonly ISparepartService _service;

    public SparepartsController(ISparepartService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery(Name = "low_stock_only")] bool? lowStockOnly)
    {
        return ApiOk(await _service.GetSparepartsAsync(search, lowStockOnly));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetSparepartAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Sparepart sparepart)
    {
        try
        {
            return ApiCreated(await _service.CreateSparepartAsync(sparepart));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Sparepart sparepart)
    {
        try
        {
            var result = await _service.UpdateSparepartAsync(id, sparepart);
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
        return await _service.DeleteSparepartAsync(id) ? ApiNoContent() : ApiNotFound();
    }
}
