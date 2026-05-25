using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.ContractorMonitoringService;
using Maintenance.Persistence.Services.TelegramService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/contractor-monitoring")]
public class ContractorMonitoringController : CmmsControllerBase
{
    private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png" };

    private readonly IContractorMonitoringService _service;
    private readonly ITelegramNotificationService _telegramService;

    public ContractorMonitoringController(IContractorMonitoringService service, ITelegramNotificationService telegramService)
    {
        _service = service;
        _telegramService = telegramService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? vendor,
        [FromQuery] string? area,
        [FromQuery] ContractorWorkStatus? status,
        [FromQuery(Name = "start_date")] DateTime? startDate,
        [FromQuery(Name = "end_date")] DateTime? endDate,
        [FromQuery] string? risk,
        [FromQuery(Name = "pic_mtc")] string? picMtc)
    {
        return ApiOk(await _service.GetPlansAsync(new ContractorWorkPlanFilter
        {
            Vendor = vendor,
            Area = area,
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            Risk = risk,
            PicMtc = picMtc
        }));
    }

    [HttpGet("reminders")]
    public async Task<IActionResult> GetReminders()
    {
        return ApiOk(await _service.GetRemindersAsync(DateTime.Now));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetPlanAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ContractorWorkPlan plan)
    {
        try
        {
            var result = await _service.CreatePlanAsync(plan, CurrentUserName());
            await _telegramService.NotifyContractorWorkPlanCreatedAsync(result);
            return ApiCreated(result);
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [AllowAnonymous]
    [HttpPost("public")]
    public async Task<IActionResult> CreatePublic([FromBody] ContractorWorkPlan plan)
    {
        try
        {
            plan.Status = ContractorWorkStatus.WAITING_PERMIT_DOCUMENT;
            plan.PermitDocumentStatus = ContractorDocumentStatus.NOT_UPLOADED;

            var result = await _service.CreatePlanAsync(plan, "Vendor Public Form");
            await _telegramService.NotifyContractorWorkPlanCreatedAsync(result);
            return ApiCreated(result, "Contractor plan submitted");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ContractorWorkPlan plan)
    {
        try
        {
            var result = await _service.UpdatePlanAsync(id, plan, CurrentUserName());
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
        return await _service.DeletePlanAsync(id) ? ApiNoContent() : ApiNotFound();
    }

    [HttpPost("{id:int}/documents")]
    [RequestSizeLimit(10_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
    public async Task<IActionResult> UploadDocument(
        int id,
        [FromForm] IFormFile file,
        [FromForm(Name = "document_type")] ContractorDocumentType documentType = ContractorDocumentType.PERMIT,
        [FromForm(Name = "document_status")] ContractorDocumentStatus? documentStatus = null,
        [FromForm(Name = "expires_at")] DateTime? expiresAt = null,
        [FromForm] string? notes = null)
    {
        return await UploadDocumentCore(id, file, documentType, documentStatus, expiresAt, notes, CurrentUserName());
    }

    [AllowAnonymous]
    [HttpPost("public/{id:int}/documents")]
    [RequestSizeLimit(10_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
    public async Task<IActionResult> UploadPublicDocument(
        int id,
        [FromForm] IFormFile file,
        [FromForm(Name = "document_type")] ContractorDocumentType documentType = ContractorDocumentType.PERMIT,
        [FromForm(Name = "document_status")] ContractorDocumentStatus? documentStatus = null,
        [FromForm(Name = "expires_at")] DateTime? expiresAt = null,
        [FromForm] string? notes = null)
    {
        return await UploadDocumentCore(id, file, documentType, documentStatus, expiresAt, notes, "Vendor Public Form");
    }

    private async Task<IActionResult> UploadDocumentCore(
        int id,
        IFormFile file,
        ContractorDocumentType documentType,
        ContractorDocumentStatus? documentStatus,
        DateTime? expiresAt,
        string? notes,
        string? performedBy)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("Dokumen wajib diupload.");
            }

            if (file.Length > 10_000_000)
            {
                throw new InvalidOperationException("Ukuran dokumen maksimal 10 MB.");
            }

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedDocumentExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Format dokumen yang didukung: PDF, JPG, PNG.");
            }

            byte[] fileBytes;
            await using (var memory = new MemoryStream())
            {
                await file.CopyToAsync(memory);
                fileBytes = memory.ToArray();
            }

            var result = await _service.AddDocumentAsync(id, new ContractorWorkDocument
            {
                DocumentType = documentType,
                DocumentStatus = documentStatus ?? ContractorDocumentStatus.UPLOADED,
                FileName = Path.GetFileName(file.FileName),
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? GuessContentType(extension) : file.ContentType,
                SizeBytes = file.Length,
                FileData = fileBytes,
                ExpiresAt = expiresAt,
                Notes = notes
            }, performedBy);

            return result == null ? ApiNotFound() : ApiCreated(result, "Document uploaded");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpGet("{id:int}/documents/{documentId:int}/content")]
    public async Task<IActionResult> GetDocumentContent(int id, int documentId)
    {
        var document = await _service.GetDocumentAsync(id, documentId);
        if (document == null || document.FileData.Length == 0)
        {
            return ApiNotFound();
        }

        return File(document.FileData, document.ContentType, document.FileName);
    }

    [HttpDelete("{id:int}/documents/{documentId:int}")]
    public async Task<IActionResult> DeleteDocument(int id, int documentId)
    {
        return await _service.DeleteDocumentAsync(id, documentId, CurrentUserName()) ? ApiNoContent() : ApiNotFound();
    }

    [HttpPost("{id:int}/supervision-work-order")]
    public async Task<IActionResult> CreateSupervisionWorkOrder(int id)
    {
        try
        {
            var result = await _service.CreateSupervisionWorkOrderAsync(id, CurrentUserName());
            if (result != null)
            {
                await _telegramService.NotifyWorkOrderCreatedAsync(result);
            }

            return result == null ? ApiNotFound() : ApiCreated(result, "Contractor supervision work order created");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    private string? CurrentUserName()
    {
        return User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
    }

    private static string GuessContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }

}
