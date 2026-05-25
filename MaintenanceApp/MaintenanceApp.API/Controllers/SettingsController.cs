using Maintenance.Domain.Cmms;
using Maintenance.Persistence.Services.TelegramService;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : CmmsControllerBase
{
    private readonly ITelegramNotificationService _telegramService;

    public SettingsController(ITelegramNotificationService telegramService) => _telegramService = telegramService;

    [HttpGet("telegram")]
    public async Task<IActionResult> GetTelegramSettings()
    {
        return ApiOk(await _telegramService.GetSettingsAsync());
    }

    [HttpPut("telegram")]
    public async Task<IActionResult> UpdateTelegramSettings([FromBody] TelegramSettingsUpdateRequest request)
    {
        try
        {
            return ApiOk(await _telegramService.UpdateSettingsAsync(request), "Telegram settings updated");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpGet("telegram/chats")]
    public async Task<IActionResult> GetTelegramChats()
    {
        try
        {
            return ApiOk(await _telegramService.GetRecentChatsAsync());
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }

    [HttpPost("telegram/test")]
    public async Task<IActionResult> TestTelegram([FromBody] TelegramTestRequest request)
    {
        try
        {
            await _telegramService.SendTestMessageAsync(request.Message);
            return ApiOk("OK", "Telegram test message sent");
        }
        catch (Exception ex)
        {
            return ApiBadRequest(ex);
        }
    }
}
