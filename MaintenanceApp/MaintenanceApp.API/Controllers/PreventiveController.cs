using Maintenance.Domain.Mapping.Entities;
using Maintenance.Domain.Mapping.Request;
using Maintenance.Domain.Response;
using Maintenance.Persistence.Services.PreventiveService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Maintenance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreventiveController : ControllerBase
    {
        protected readonly IPreventiveService Context;
        private readonly ILogger<PreventiveController> _logger;
        public PreventiveController(IPreventiveService Data, ILogger<PreventiveController> logger)
        {
            Context = Data;
            _logger = logger;
        }
        /// <summary>
        /// Stock In Controller V-2 Wiht Error Response 
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [Route("v1/InsertPreventive")]
        public async Task<IActionResult> BarcodeFinishGoodInspect(RequestPreventive data)
        {
            try
            {
                Console.WriteLine("hit");
                _logger.LogInformation("Home page accessed.");
                // Call the update method
                bool isUpdated = await Context.InsertPreventive(data);
                if (isUpdated)
                {
                    var response = new ApiResponse<string>
                    {
                        Success = true,
                        Data = "", // If there's no data to return
                        Message = "Insert Success.", // Provide a success message
                        StatusCode = (int)HttpStatusCode.OK
                    };
                    return Ok(response);
                }
                else
                {
                    var response = new ApiResponse<string>
                    {
                        Success = false,
                        Data = "", // If there's no data to return
                        Message = "No records updated", // Provide a message indicating no records were updated
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = null
                };

                return StatusCode(500, errorResponse);
            }
        }
    }
}
