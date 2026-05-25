using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.CmmsMasterDataService;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api")]
public class CmmsMasterDataController : CmmsControllerBase
{
    private readonly ICmmsMasterDataService _service;

    public CmmsMasterDataController(ICmmsMasterDataService service) => _service = service;

    [HttpGet("maintenance-types")]
    public async Task<IActionResult> GetMaintenanceTypes()
    {
        return ApiOk(await _service.GetMaintenanceTypesAsync());
    }

    [HttpGet("maintenance-types/{id:int}")]
    public async Task<IActionResult> GetMaintenanceType(int id)
    {
        var result = await _service.GetMaintenanceTypeAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost("maintenance-types")]
    public async Task<IActionResult> CreateMaintenanceType([FromBody] MaintenanceTypeMaster item)
    {
        try
        {
            return ApiCreated(await _service.CreateMaintenanceTypeAsync(item));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("maintenance-types/{id:int}")]
    public async Task<IActionResult> UpdateMaintenanceType(int id, [FromBody] MaintenanceTypeMaster item)
    {
        try
        {
            var result = await _service.UpdateMaintenanceTypeAsync(id, item);
            return result == null ? ApiNotFound() : ApiOk(result, "Data updated successfully");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpDelete("maintenance-types/{id:int}")]
    public async Task<IActionResult> DeleteMaintenanceType(int id)
    {
        return await _service.DeleteMaintenanceTypeAsync(id) ? ApiNoContent() : ApiNotFound();
    }

    [HttpGet("work-order-priorities")]
    public async Task<IActionResult> GetWorkOrderPriorities()
    {
        return ApiOk(await _service.GetWorkOrderPrioritiesAsync());
    }

    [HttpGet("work-order-priorities/{id:int}")]
    public async Task<IActionResult> GetWorkOrderPriority(int id)
    {
        var result = await _service.GetWorkOrderPriorityAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost("work-order-priorities")]
    public async Task<IActionResult> CreateWorkOrderPriority([FromBody] WorkOrderPriorityMaster item)
    {
        try
        {
            return ApiCreated(await _service.CreateWorkOrderPriorityAsync(item));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("work-order-priorities/{id:int}")]
    public async Task<IActionResult> UpdateWorkOrderPriority(int id, [FromBody] WorkOrderPriorityMaster item)
    {
        try
        {
            var result = await _service.UpdateWorkOrderPriorityAsync(id, item);
            return result == null ? ApiNotFound() : ApiOk(result, "Data updated successfully");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpDelete("work-order-priorities/{id:int}")]
    public async Task<IActionResult> DeleteWorkOrderPriority(int id)
    {
        return await _service.DeleteWorkOrderPriorityAsync(id) ? ApiNoContent() : ApiNotFound();
    }

    [HttpGet("work-order-statuses")]
    public async Task<IActionResult> GetWorkOrderStatuses()
    {
        return ApiOk(await _service.GetWorkOrderStatusesAsync());
    }

    [HttpGet("work-order-statuses/{id:int}")]
    public async Task<IActionResult> GetWorkOrderStatus(int id)
    {
        var result = await _service.GetWorkOrderStatusAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost("work-order-statuses")]
    public async Task<IActionResult> CreateWorkOrderStatus([FromBody] WorkOrderStatusMaster item)
    {
        try
        {
            return ApiCreated(await _service.CreateWorkOrderStatusAsync(item));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("work-order-statuses/{id:int}")]
    public async Task<IActionResult> UpdateWorkOrderStatus(int id, [FromBody] WorkOrderStatusMaster item)
    {
        try
        {
            var result = await _service.UpdateWorkOrderStatusAsync(id, item);
            return result == null ? ApiNotFound() : ApiOk(result, "Data updated successfully");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpDelete("work-order-statuses/{id:int}")]
    public async Task<IActionResult> DeleteWorkOrderStatus(int id)
    {
        return await _service.DeleteWorkOrderStatusAsync(id) ? ApiNoContent() : ApiNotFound();
    }

    [HttpGet("downtime-categories")]
    public async Task<IActionResult> GetDowntimeCategories()
    {
        return ApiOk(await _service.GetDowntimeCategoriesAsync());
    }

    [HttpGet("downtime-categories/{id:int}")]
    public async Task<IActionResult> GetDowntimeCategory(int id)
    {
        var result = await _service.GetDowntimeCategoryAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost("downtime-categories")]
    public async Task<IActionResult> CreateDowntimeCategory([FromBody] DowntimeCategoryMaster item)
    {
        try
        {
            return ApiCreated(await _service.CreateDowntimeCategoryAsync(item));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("downtime-categories/{id:int}")]
    public async Task<IActionResult> UpdateDowntimeCategory(int id, [FromBody] DowntimeCategoryMaster item)
    {
        try
        {
            var result = await _service.UpdateDowntimeCategoryAsync(id, item);
            return result == null ? ApiNotFound() : ApiOk(result, "Data updated successfully");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpDelete("downtime-categories/{id:int}")]
    public async Task<IActionResult> DeleteDowntimeCategory(int id)
    {
        return await _service.DeleteDowntimeCategoryAsync(id) ? ApiNoContent() : ApiNotFound();
    }

    [HttpGet("problem-report-categories")]
    public async Task<IActionResult> GetProblemReportCategories()
    {
        return ApiOk(await _service.GetProblemReportCategoriesAsync());
    }

    [HttpGet("problem-report-categories/{id:int}")]
    public async Task<IActionResult> GetProblemReportCategory(int id)
    {
        var result = await _service.GetProblemReportCategoryAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost("problem-report-categories")]
    public async Task<IActionResult> CreateProblemReportCategory([FromBody] ProblemReportCategoryMaster item)
    {
        try
        {
            return ApiCreated(await _service.CreateProblemReportCategoryAsync(item));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("problem-report-categories/{id:int}")]
    public async Task<IActionResult> UpdateProblemReportCategory(int id, [FromBody] ProblemReportCategoryMaster item)
    {
        try
        {
            var result = await _service.UpdateProblemReportCategoryAsync(id, item);
            return result == null ? ApiNotFound() : ApiOk(result, "Data updated successfully");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpDelete("problem-report-categories/{id:int}")]
    public async Task<IActionResult> DeleteProblemReportCategory(int id)
    {
        return await _service.DeleteProblemReportCategoryAsync(id) ? ApiNoContent() : ApiNotFound();
    }

    [HttpGet("preventive-schedule-types")]
    public async Task<IActionResult> GetPreventiveScheduleTypes()
    {
        return ApiOk(await _service.GetPreventiveScheduleTypesAsync());
    }

    [HttpGet("preventive-schedule-types/{id:int}")]
    public async Task<IActionResult> GetPreventiveScheduleType(int id)
    {
        var result = await _service.GetPreventiveScheduleTypeAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost("preventive-schedule-types")]
    public async Task<IActionResult> CreatePreventiveScheduleType([FromBody] PreventiveScheduleTypeMaster item)
    {
        try
        {
            return ApiCreated(await _service.CreatePreventiveScheduleTypeAsync(item));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("preventive-schedule-types/{id:int}")]
    public async Task<IActionResult> UpdatePreventiveScheduleType(int id, [FromBody] PreventiveScheduleTypeMaster item)
    {
        try
        {
            var result = await _service.UpdatePreventiveScheduleTypeAsync(id, item);
            return result == null ? ApiNotFound() : ApiOk(result, "Data updated successfully");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpDelete("preventive-schedule-types/{id:int}")]
    public async Task<IActionResult> DeletePreventiveScheduleType(int id)
    {
        return await _service.DeletePreventiveScheduleTypeAsync(id) ? ApiNoContent() : ApiNotFound();
    }

    [HttpGet("frequency-types")]
    public async Task<IActionResult> GetFrequencyTypes()
    {
        return ApiOk(await _service.GetFrequencyTypesAsync());
    }

    [HttpGet("frequency-types/{id:int}")]
    public async Task<IActionResult> GetFrequencyType(int id)
    {
        var result = await _service.GetFrequencyTypeAsync(id);
        return result == null ? ApiNotFound() : ApiOk(result);
    }

    [HttpPost("frequency-types")]
    public async Task<IActionResult> CreateFrequencyType([FromBody] FrequencyTypeMaster item)
    {
        try
        {
            return ApiCreated(await _service.CreateFrequencyTypeAsync(item));
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPut("frequency-types/{id:int}")]
    public async Task<IActionResult> UpdateFrequencyType(int id, [FromBody] FrequencyTypeMaster item)
    {
        try
        {
            var result = await _service.UpdateFrequencyTypeAsync(id, item);
            return result == null ? ApiNotFound() : ApiOk(result, "Data updated successfully");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpDelete("frequency-types/{id:int}")]
    public async Task<IActionResult> DeleteFrequencyType(int id)
    {
        return await _service.DeleteFrequencyTypeAsync(id) ? ApiNoContent() : ApiNotFound();
    }
}
