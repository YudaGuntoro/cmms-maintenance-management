using Maintenance.Domain.Mapping.Entities;
using Maintenance.Domain.Response;
using Maintenance.Persistence.Services.MachineTroubleService;
using Maintenance.Persistence.Services.PreventiveService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Maintenance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineTroubleController : ControllerBase
    {
        protected readonly IMachineTroubleService Context;
        private readonly ILogger<MachineTroubleController> _logger;

        public MachineTroubleController(IMachineTroubleService Data, ILogger<MachineTroubleController> logger)
        {
            Context = Data;
            _logger = logger;
        }

        /// <summary>
        /// Get Balance
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [Route("v1/GetMachineTrouble")]
        public async Task<IActionResult> GetListMachineTroubleAsync()
        {
            try
            {
                var result = await Context.GetMachineStatusTrouble();

                if (result == null || !result.Any())
                {
                    var response = new ApiResponse<IEnumerable<MachineStatus>>
                    {
                        Success = false,
                        Message = "Data Not Found",
                        Data = null
                    };
                    return NotFound(response);
                }

                var successResponse = new ApiResponse<IEnumerable<MachineStatus>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Success = true,
                    Message = "Data retrieved successfully",
                    Data = result
                };
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Data = null,
                    Message = ex.Message,
                };

                return StatusCode(500, errorResponse);
            }
        }
    }
}
