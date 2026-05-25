using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.TelegramService;
using Maintenance.Persistence.Services.WorkOrderService;
using Maintenance.Persistence.Services.WorkOrderSparepartService;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/work-orders")]
public class WorkOrdersController : CmmsControllerBase
{
    private static readonly HashSet<string> AllowedPhotoExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IWorkOrderService _workOrderService;
    private readonly IWorkOrderSparepartService _workOrderSparepartService;
    private readonly ITelegramNotificationService _telegramService;

    public WorkOrdersController(IWorkOrderService workOrderService, IWorkOrderSparepartService workOrderSparepartService, ITelegramNotificationService telegramService)
    {
        _workOrderService = workOrderService;
        _workOrderSparepartService = workOrderSparepartService;
        _telegramService = telegramService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery(Name = "asset_id")] int? assetId,
        [FromQuery] WorkOrderStatus? status,
        [FromQuery] WorkOrderPriority? priority,
        [FromQuery(Name = "maintenance_type")] MaintenanceType? maintenanceType)
    {
        return ApiOk(await _workOrderService.GetWorkOrdersAsync(assetId, status, priority, maintenanceType));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _workOrderService.GetWorkOrderAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WorkOrder workOrder)
    {
        try
        {
            var result = await _workOrderService.CreateWorkOrderAsync(workOrder);
            await _telegramService.NotifyWorkOrderCreatedAsync(result);
            return ApiCreated(result);
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] WorkOrder workOrder)
    {
        try
        {
            var result = await _workOrderService.UpdateWorkOrderAsync(id, workOrder);
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
        return await _workOrderService.DeleteWorkOrderAsync(id) ? ApiNoContent() : ApiNotFound();
    }

    [HttpPatch("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] WorkOrderAssignRequest request)
    {
        try
        {
            var result = await _workOrderService.AssignWorkOrderAsync(id, request);
            return result == null ? ApiNotFound() : ApiOk(result, "Work order assigned");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPatch("{id:int}/start")]
    public async Task<IActionResult> Start(int id)
    {
        try
        {
            var result = await _workOrderService.StartWorkOrderAsync(id);
            return result == null ? ApiNotFound() : ApiOk(result, "Work order started");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPatch("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] WorkOrderCompleteRequest request)
    {
        try
        {
            var result = await _workOrderService.CompleteWorkOrderAsync(id, request);
            return result == null ? ApiNotFound() : ApiOk(result, "Work order completed");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPatch("{id:int}/close")]
    public async Task<IActionResult> Close(int id, [FromBody] WorkOrderCloseRequest request)
    {
        try
        {
            var result = await _workOrderService.CloseWorkOrderAsync(id, request);
            return result == null ? ApiNotFound() : ApiOk(result, "Work order closed");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPost("{id:int}/spareparts")]
    public async Task<IActionResult> UseSparepart(int id, [FromBody] WorkOrderSparepartRequest request)
    {
        try
        {
            var result = await _workOrderSparepartService.UseSparepartAsync(id, request);
            return result == null ? ApiNotFound() : ApiCreated(result, "Sparepart usage recorded");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpGet("{id:int}/photos")]
    public async Task<IActionResult> GetPhotos(int id)
    {
        if (await _workOrderService.GetWorkOrderAsync(id) == null)
        {
            return ApiNotFound();
        }

        return ApiOk(await _workOrderService.GetWorkOrderPhotosAsync(id));
    }

    [HttpPost("{id:int}/photos")]
    [RequestSizeLimit(10_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
    public async Task<IActionResult> UploadPhoto(int id, [FromForm] IFormFile file)
    {
        try
        {
            if (await _workOrderService.GetWorkOrderAsync(id) == null)
            {
                return ApiNotFound();
            }

            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("Photo file wajib diupload.");
            }

            if (file.Length > 10_000_000)
            {
                throw new InvalidOperationException("Ukuran photo maksimal 10 MB.");
            }

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("File harus berupa image.");
            }

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedPhotoExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Format image yang didukung: JPG, PNG, WEBP.");
            }

            byte[] photoBytes;
            await using (var memory = new MemoryStream())
            {
                await file.CopyToAsync(memory);
                photoBytes = memory.ToArray();
            }

            var photo = await _workOrderService.AddWorkOrderPhotoAsync(id, new WorkOrderPhoto
            {
                FileName = Path.GetFileName(file.FileName),
                ContentType = file.ContentType,
                SizeBytes = file.Length,
                FileData = photoBytes,
                UploadedBy = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name
            });

            return photo == null ? ApiNotFound() : ApiCreated(photo, "Photo uploaded");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpGet("{id:int}/photos/{photoId:int}/content")]
    public async Task<IActionResult> GetPhotoContent(int id, int photoId)
    {
        var photo = await _workOrderService.GetWorkOrderPhotoAsync(id, photoId);
        if (photo == null || photo.FileData.Length == 0)
        {
            return ApiNotFound();
        }

        return File(photo.FileData, photo.ContentType, photo.FileName);
    }

    [HttpDelete("{id:int}/photos/{photoId:int}")]
    public async Task<IActionResult> DeletePhoto(int id, int photoId)
    {
        var photo = await _workOrderService.DeleteWorkOrderPhotoAsync(id, photoId);
        if (photo == null)
        {
            return ApiNotFound();
        }

        return ApiNoContent();
    }
}
