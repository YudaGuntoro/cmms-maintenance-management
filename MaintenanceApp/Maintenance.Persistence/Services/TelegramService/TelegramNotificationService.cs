using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Maintenance.Domain.Cmms;
using Maintenance.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Maintenance.Persistence.Services.TelegramService;

public class TelegramNotificationService : ITelegramNotificationService
{
    private const int SingletonSettingsId = 1;
    private readonly MaintenanceDbContext Context;
    private readonly IHttpClientFactory HttpClientFactory;
    private readonly ILogger<TelegramNotificationService> Logger;

    public TelegramNotificationService(MaintenanceDbContext context, IHttpClientFactory httpClientFactory, ILogger<TelegramNotificationService> logger)
    {
        Context = context;
        HttpClientFactory = httpClientFactory;
        Logger = logger;
    }

    public async Task<TelegramSettingsResponse> GetSettingsAsync()
    {
        return ToResponse(await GetSettingsEntityAsync(createIfMissing: false));
    }

    public async Task<TelegramSettingsResponse> UpdateSettingsAsync(TelegramSettingsUpdateRequest request)
    {
        var settings = await GetSettingsEntityAsync(createIfMissing: true) ?? new TelegramSettings { Id = SingletonSettingsId };
        if (settings.Id == 0)
        {
            Context.TelegramSettings.Add(settings);
        }

        if (!string.IsNullOrWhiteSpace(request.BotToken))
        {
            settings.BotToken = request.BotToken.Trim();
        }

        settings.ChatId = string.IsNullOrWhiteSpace(request.ChatId) ? null : request.ChatId.Trim();
        settings.IsEnabled = request.IsEnabled;
        settings.UpdatedAt = DateTime.Now;

        if (settings.IsEnabled && string.IsNullOrWhiteSpace(settings.BotToken))
        {
            throw new InvalidOperationException("Bot token wajib diisi sebelum Telegram notification diaktifkan.");
        }

        if (settings.IsEnabled && string.IsNullOrWhiteSpace(settings.ChatId))
        {
            throw new InvalidOperationException("Group chat id wajib diisi sebelum Telegram notification diaktifkan.");
        }

        await Context.SaveChangesAsync();
        return ToResponse(settings);
    }

    public async Task<List<TelegramChatResponse>> GetRecentChatsAsync()
    {
        var settings = await GetSettingsEntityAsync(createIfMissing: false);
        if (settings == null || string.IsNullOrWhiteSpace(settings.BotToken))
        {
            throw new InvalidOperationException("Bot token belum diset.");
        }

        var client = HttpClientFactory.CreateClient("telegram");
        using var response = await client.GetAsync($"{BuildTelegramUrl(settings.BotToken, "getUpdates")}?allowed_updates=%5B%22message%22%2C%22my_chat_member%22%5D");
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(ReadTelegramError(content, response.StatusCode));
        }

        using var document = JsonDocument.Parse(content);
        if (!document.RootElement.TryGetProperty("ok", out var okElement) || !okElement.GetBoolean())
        {
            throw new InvalidOperationException(ReadTelegramError(content, response.StatusCode));
        }

        var chats = new Dictionary<string, TelegramChatResponse>();
        if (!document.RootElement.TryGetProperty("result", out var resultElement) || resultElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        foreach (var update in resultElement.EnumerateArray())
        {
            if (!TryReadChat(update, out var chat, out var messageAt))
            {
                continue;
            }

            if (!chats.TryGetValue(chat.ChatId, out var existing) || (messageAt.HasValue && (!existing.LastMessageAt.HasValue || messageAt > existing.LastMessageAt)))
            {
                chat.LastMessageAt = messageAt;
                chats[chat.ChatId] = chat;
            }
        }

        return chats.Values
            .OrderByDescending(x => x.LastMessageAt ?? DateTime.MinValue)
            .ToList();
    }

    public async Task SendTestMessageAsync(string? message)
    {
        var settings = await GetUsableSettingsAsync(throwIfDisabled: false);
        var text = string.IsNullOrWhiteSpace(message)
            ? "CMMS Telegram notification test berhasil."
            : message.Trim();

        await SendMessageAsync(settings, $"<b>CMMS Test Notification</b>\n{Escape(text)}");
    }

    public async Task NotifyProblemReportCreatedAsync(ProblemReport report)
    {
        try
        {
            var settings = await GetNotificationSettingsOrNullAsync();
            if (settings == null)
            {
                return;
            }

            var asset = await Context.Assets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == report.AssetId);
            var message = BuildProblemReportMessage(report, asset);
            await SendMessageAsync(settings, message);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to send Telegram problem report notification.");
        }
    }

    public async Task NotifyWorkOrderCreatedAsync(WorkOrder workOrder)
    {
        try
        {
            var settings = await GetNotificationSettingsOrNullAsync();
            if (settings == null)
            {
                return;
            }

            var asset = await Context.Assets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == workOrder.AssetId);
            var technician = workOrder.AssignedTo.HasValue
                ? await Context.Technicians.AsNoTracking().FirstOrDefaultAsync(x => x.Id == workOrder.AssignedTo.Value)
                : null;
            var report = workOrder.ProblemReportId.HasValue
                ? await Context.ProblemReports.AsNoTracking().FirstOrDefaultAsync(x => x.Id == workOrder.ProblemReportId.Value)
                : null;

            var message = BuildWorkOrderMessage(workOrder, asset, technician, report);
            await SendMessageAsync(settings, message);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to send Telegram work order notification.");
        }
    }

    public async Task NotifyContractorWorkPlanCreatedAsync(ContractorWorkPlan plan)
    {
        try
        {
            var settings = await GetNotificationSettingsOrNullAsync();
            if (settings == null)
            {
                return;
            }

            var asset = plan.AssetId.HasValue
                ? await Context.Assets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == plan.AssetId.Value)
                : null;
            var message = BuildContractorWorkPlanMessage(plan, asset);
            await SendMessageAsync(settings, message);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to send Telegram contractor work plan notification.");
        }
    }

    private async Task<TelegramSettings?> GetSettingsEntityAsync(bool createIfMissing)
    {
        var settings = await Context.TelegramSettings.FirstOrDefaultAsync(x => x.Id == SingletonSettingsId);
        if (settings != null || !createIfMissing)
        {
            return settings;
        }

        settings = new TelegramSettings
        {
            Id = SingletonSettingsId,
            BotToken = string.Empty,
            ChatId = null,
            IsEnabled = false,
            UpdatedAt = DateTime.Now
        };
        Context.TelegramSettings.Add(settings);
        return settings;
    }

    private async Task<TelegramSettings> GetUsableSettingsAsync(bool throwIfDisabled)
    {
        var settings = await GetSettingsEntityAsync(createIfMissing: false);
        if (settings == null || string.IsNullOrWhiteSpace(settings.BotToken) || string.IsNullOrWhiteSpace(settings.ChatId))
        {
            throw new InvalidOperationException("Telegram settings belum lengkap.");
        }

        if (throwIfDisabled && !settings.IsEnabled)
        {
            throw new InvalidOperationException("Telegram notification belum aktif.");
        }

        return settings;
    }

    private async Task<TelegramSettings?> GetNotificationSettingsOrNullAsync()
    {
        var settings = await GetSettingsEntityAsync(createIfMissing: false);
        if (settings == null || !settings.IsEnabled || string.IsNullOrWhiteSpace(settings.BotToken) || string.IsNullOrWhiteSpace(settings.ChatId))
        {
            return null;
        }

        return settings;
    }

    private async Task SendMessageAsync(TelegramSettings settings, string text)
    {
        if (string.IsNullOrWhiteSpace(settings.BotToken) || string.IsNullOrWhiteSpace(settings.ChatId))
        {
            throw new InvalidOperationException("Telegram settings belum lengkap.");
        }

        var client = HttpClientFactory.CreateClient("telegram");
        using var response = await client.PostAsJsonAsync(BuildTelegramUrl(settings.BotToken, "sendMessage"), new
        {
            chat_id = settings.ChatId,
            text,
            parse_mode = "HTML",
            disable_web_page_preview = true
        });

        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(ReadTelegramError(content, response.StatusCode));
        }

        using var document = JsonDocument.Parse(content);
        if (!document.RootElement.TryGetProperty("ok", out var okElement) || !okElement.GetBoolean())
        {
            throw new InvalidOperationException(ReadTelegramError(content, response.StatusCode));
        }
    }

    private static string BuildProblemReportMessage(ProblemReport report, Asset? asset)
    {
        var lines = new List<string>
        {
            "<b>New Problem Report</b>",
            $"Report: <b>{Escape(report.ReportNumber)}</b>",
            $"Asset: {Escape(AssetLabel(asset, report.AssetId))}",
            $"Category: {Escape(report.Category.ToString())}",
            $"Priority: <b>{Escape(report.Priority.ToString())}</b>",
            $"Status: {Escape(report.Status.ToString())}",
            $"Title: {Escape(report.Title)}",
            $"Reported By: {Escape(report.ReportedBy ?? "-")}",
            $"Reported At: {FormatDate(report.ReportedAt)}"
        };

        if (!string.IsNullOrWhiteSpace(report.Description))
        {
            lines.Add($"Description: {Escape(report.Description)}");
        }

        if (report.Category == ProblemReportCategory.DOWNTIME)
        {
            lines.Add($"Downtime Start: {FormatDate(report.DowntimeStart)}");
            if (report.DowntimeEnd.HasValue)
            {
                lines.Add($"Downtime End: {FormatDate(report.DowntimeEnd)}");
                lines.Add($"Downtime Minutes: {FormatNumber(report.DowntimeMinutes)}");
            }
            else
            {
                lines.Add("Downtime Status: Ongoing");
            }
        }

        lines.Add("");
        lines.Add("Action: Please review and create a work order if maintenance follow-up is required.");
        return string.Join("\n", lines);
    }

    private static string BuildWorkOrderMessage(WorkOrder workOrder, Asset? asset, Technician? technician, ProblemReport? report)
    {
        var lines = new List<string>
        {
            "<b>New Work Order</b>",
            $"WO: <b>{Escape(workOrder.WoNumber)}</b>",
            report == null ? "Source: Scheduled / Manual" : $"Source Report: {Escape(report.ReportNumber)}",
            $"Asset: {Escape(AssetLabel(asset, workOrder.AssetId))}",
            $"Type: {Escape(workOrder.MaintenanceType.ToString())}",
            $"Priority: <b>{Escape(workOrder.Priority.ToString())}</b>",
            $"Status: {Escape(workOrder.Status.ToString())}",
            $"Title: {Escape(workOrder.Title)}",
            $"Reported By: {Escape(workOrder.ReportedBy ?? "-")}",
            $"Reported At: {FormatDate(workOrder.ReportedAt)}",
            $"Assigned To: {Escape(technician?.Name ?? "-")}",
            $"Scheduled At: {FormatDate(workOrder.ScheduledAt)}"
        };

        if (!string.IsNullOrWhiteSpace(workOrder.Description))
        {
            lines.Add($"Description: {Escape(workOrder.Description)}");
        }

        if (workOrder.DowntimeStart.HasValue || workOrder.DowntimeEnd.HasValue)
        {
            lines.Add($"Downtime Start: {FormatDate(workOrder.DowntimeStart)}");
            lines.Add($"Downtime End: {FormatDate(workOrder.DowntimeEnd)}");
            lines.Add($"Downtime Minutes: {FormatNumber(workOrder.DowntimeMinutes)}");
        }

        lines.Add("");
        lines.Add("Action: Maintenance team please follow up from CMMS.");
        return string.Join("\n", lines);
    }

    private static string BuildContractorWorkPlanMessage(ContractorWorkPlan plan, Asset? asset)
    {
        var lines = new List<string>
        {
            "<b>New Contractor Work Plan</b>",
            $"Vendor: <b>{Escape(plan.VendorName)}</b>",
            $"Work: {Escape(plan.WorkTitle)}",
            $"Area: {Escape(plan.WorkArea)}",
            $"Location: {Escape(plan.WorkLocation ?? "-")}",
            $"Asset: {Escape(plan.AssetId.HasValue ? AssetLabel(asset, plan.AssetId.Value) : "-")}",
            $"PIC Vendor: {Escape(plan.VendorPicName)}",
            $"PIC MTC: {Escape(plan.InternalPicName)}",
            $"Workers: {plan.WorkerCount:N0}",
            $"Schedule: {FormatDate(plan.StartAt)} - {FormatDate(plan.EndAt)}",
            $"Status: {Escape(plan.Status.ToString())}",
            $"Permit: {Escape(plan.PermitDocumentStatus.ToString())}",
            $"Risk: {Escape(plan.HasHighRisk ? string.Join(", ", plan.RiskTags) : "Normal")}"
        };

        lines.Add("");
        lines.Add("Action: MTC team please prepare area, safety, and contractor coordination.");
        return string.Join("\n", lines);
    }

    private static TelegramSettingsResponse ToResponse(TelegramSettings? settings)
    {
        if (settings == null)
        {
            return new TelegramSettingsResponse();
        }

        return new TelegramSettingsResponse
        {
            HasBotToken = !string.IsNullOrWhiteSpace(settings.BotToken),
            BotTokenPreview = MaskToken(settings.BotToken),
            ChatId = settings.ChatId,
            IsEnabled = settings.IsEnabled,
            UpdatedAt = settings.UpdatedAt
        };
    }

    private static string? MaskToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var trimmed = token.Trim();
        if (trimmed.Length <= 12)
        {
            return "configured";
        }

        return $"{trimmed[..8]}...{trimmed[^4..]}";
    }

    private static string BuildTelegramUrl(string token, string method)
    {
        return $"https://api.telegram.org/bot{token.Trim()}/{method}";
    }

    private static string ReadTelegramError(string content, HttpStatusCode statusCode)
    {
        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.TryGetProperty("description", out var description))
            {
                return $"Telegram API error ({(int)statusCode}): {description.GetString()}";
            }
        }
        catch
        {
            // Use fallback below.
        }

        return $"Telegram API error ({(int)statusCode}).";
    }

    private static bool TryReadChat(JsonElement update, out TelegramChatResponse chat, out DateTime? messageAt)
    {
        chat = new TelegramChatResponse();
        messageAt = null;

        JsonElement message;
        if (update.TryGetProperty("message", out var messageElement))
        {
            message = messageElement;
        }
        else if (update.TryGetProperty("my_chat_member", out var memberElement))
        {
            message = memberElement;
        }
        else
        {
            return false;
        }

        if (!message.TryGetProperty("chat", out var chatElement))
        {
            return false;
        }

        if (!chatElement.TryGetProperty("id", out var idElement))
        {
            return false;
        }

        var title = ReadString(chatElement, "title")
            ?? ReadString(chatElement, "username")
            ?? string.Join(" ", new[] { ReadString(chatElement, "first_name"), ReadString(chatElement, "last_name") }.Where(x => !string.IsNullOrWhiteSpace(x)))
            ?? "Telegram Chat";

        chat = new TelegramChatResponse
        {
            ChatId = idElement.GetRawText(),
            Title = title,
            Type = ReadString(chatElement, "type") ?? "-"
        };

        if (message.TryGetProperty("date", out var dateElement) && dateElement.TryGetInt64(out var unixSeconds))
        {
            messageAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
        }

        return true;
    }

    private static string? ReadString(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static string AssetLabel(Asset? asset, int fallbackId)
    {
        return asset == null ? $"Asset #{fallbackId}" : $"{asset.AssetCode} - {asset.AssetName}";
    }

    private static string Escape(string value)
    {
        return WebUtility.HtmlEncode(value);
    }

    private static string FormatDate(DateTime? value)
    {
        return value.HasValue ? value.Value.ToString("dd MMM yyyy, HH:mm") : "-";
    }

    private static string FormatNumber(int? value)
    {
        return value.HasValue ? value.Value.ToString("N0") : "-";
    }
}
