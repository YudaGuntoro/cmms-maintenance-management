using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.AssetService;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/assets")]
public class AssetsController : CmmsControllerBase
{
    private readonly IAssetService _service;

    public AssetsController(IAssetService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] AssetStatus? status)
    {
        return ApiOk(await _service.GetAssetsAsync(search, status));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetAssetAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Asset asset)
    {
        try
        {
            return ApiCreated(await _service.CreateAssetAsync(asset));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Asset asset)
    {
        try
        {
            var result = await _service.UpdateAssetAsync(id, asset);
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
        return await _service.DeleteAssetAsync(id) ? ApiNoContent() : ApiNotFound();
    }
}
