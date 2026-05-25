using Maintenance.Persistence.Services.AuthService;
using Maintenance.Persistence.Services.AssetService;
using Maintenance.Persistence.Services.CmmsMasterDataService;
using Maintenance.Persistence.Services.ContractorMonitoringService;
using Maintenance.Persistence.Services.DashboardService;
using Maintenance.Persistence.Services.DowntimeLogService;
using Maintenance.Persistence.Services.FailureCodeService;
using Maintenance.Persistence.Services.KpiService;
using Maintenance.Persistence.Services.MachineTroubleService;
using Maintenance.Persistence.Services.PreventiveScheduleService;
using Maintenance.Persistence.Services.PreventiveService;
using Maintenance.Persistence.Services.ProblemReportService;
using Maintenance.Persistence.Services.RootCauseService;
using Maintenance.Persistence.Services.SparepartService;
using Maintenance.Persistence.Services.TechnicianService;
using Maintenance.Persistence.Services.TelegramService;
using Maintenance.Persistence.Services.UserService;
using Maintenance.Persistence.Services.WorkOrderService;
using Maintenance.Persistence.Services.WorkOrderSparepartService;
using Maintenance.Repository.AuthRepository;
using Maintenance.Repository.AssetRepository;
using Maintenance.Repository.CmmsMasterDataRepository;
using Maintenance.Repository.ContractorMonitoringRepository;
using Maintenance.Repository.DashboardRepository;
using Maintenance.Repository.DowntimeLogRepository;
using Maintenance.Repository.FailureCodeRepository;
using Maintenance.Repository.KpiRepository;
using Maintenance.Repository.MachineTroubleRepository;
using Maintenance.Repository.PreventiveScheduleRepository;
using Maintenance.Repository.PreventiveRepository;
using Maintenance.Repository.ProblemReportRepository;
using Maintenance.Repository.RootCauseRepository;
using Maintenance.Repository.SparepartRepository;
using Maintenance.Repository.TechnicianRepository;
using Maintenance.Repository.UserRepository;
using Maintenance.Repository.WorkOrderRepository;
using Maintenance.Repository.WorkOrderSparepartRepository;
using Microsoft.Extensions.DependencyInjection;

namespace Maintenance.Persistence.IoC
{
    public static class DependencyContainer
    {
        public static void AddIoCService(this IServiceCollection services)
        {
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICmmsMasterDataService, CmmsMasterDataService>();
            services.AddScoped<ITechnicianService, TechnicianService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IWorkOrderService, WorkOrderService>();
            services.AddScoped<IWorkOrderSparepartService, WorkOrderSparepartService>();
            services.AddScoped<IContractorMonitoringService, ContractorMonitoringService>();
            services.AddScoped<IPreventiveScheduleService, PreventiveScheduleService>();
            services.AddScoped<IDowntimeLogService, DowntimeLogService>();
            services.AddScoped<ISparepartService, SparepartService>();
            services.AddScoped<IFailureCodeService, FailureCodeService>();
            services.AddScoped<IRootCauseService, RootCauseService>();
            services.AddScoped<IKpiService, KpiService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IPreventiveService, PreventiveService>();
            services.AddScoped<IMachineTroubleService, MachineTroubleService>();
            services.AddScoped<IProblemReportService, ProblemReportService>();
            services.AddScoped<ITelegramNotificationService, TelegramNotificationService>();

            services.AddScoped<IAssetRepository, AssetRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<ICmmsMasterDataRepository, CmmsMasterDataRepository>();
            services.AddScoped<ITechnicianRepository, TechnicianRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
            services.AddScoped<IWorkOrderSparepartRepository, WorkOrderSparepartRepository>();
            services.AddScoped<IContractorMonitoringRepository, ContractorMonitoringRepository>();
            services.AddScoped<IPreventiveScheduleRepository, PreventiveScheduleRepository>();
            services.AddScoped<IDowntimeLogRepository, DowntimeLogRepository>();
            services.AddScoped<ISparepartRepository, SparepartRepository>();
            services.AddScoped<IFailureCodeRepository, FailureCodeRepository>();
            services.AddScoped<IRootCauseRepository, RootCauseRepository>();
            services.AddScoped<IKpiRepository, KpiRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IPreventiveRepository, PreventiveRepository>();
            services.AddScoped<IMachineTroubleRepository, MachineTroubleRepository>();
            services.AddScoped<IProblemReportRepository, ProblemReportRepository>();
        }
    }
}
