using Maintenance.Domain.Cmms;

namespace Maintenance.Persistence.Services.TelegramService;

public interface ITelegramNotificationService
{
    Task<TelegramSettingsResponse> GetSettingsAsync();
    Task<TelegramSettingsResponse> UpdateSettingsAsync(TelegramSettingsUpdateRequest request);
    Task<List<TelegramChatResponse>> GetRecentChatsAsync();
    Task SendTestMessageAsync(string? message);
    Task NotifyProblemReportCreatedAsync(ProblemReport report);
    Task NotifyWorkOrderCreatedAsync(WorkOrder workOrder);
    Task NotifyContractorWorkPlanCreatedAsync(ContractorWorkPlan plan);
}
