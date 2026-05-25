using Microsoft.Extensions.Logging;
using epj.RouteGenerator;
using MaintenanceApp.Views;
using MaintenanceApp.Navigation;
using InputKit.Handlers;
using MaintenanceApp.Handler;
using MaintenanceApp.Repositories;
using MaintenanceApp.Services;

namespace MaintenanceApp
{
    [AutoRoutes("Page")]
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register the custom handler for RefreshView
            builder
                .UseMauiApp<App>()
                .ConfigureMauiHandlers(handlers =>
                {
#if __ANDROID__
                    handlers.AddHandler(typeof(RefreshView), typeof(CustomRefreshViewHandler));
#endif
                });
#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddTransient<ICmmsRepository, CmmsRepository>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MachineTroublePage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<CmmsDashboardPage>();
            builder.Services.AddTransient<AssetListPage>();
            builder.Services.AddTransient<AssetDetailPage>();
            builder.Services.AddTransient<WorkOrderListPage>();
            builder.Services.AddTransient<WorkOrderDetailPage>();
            builder.Services.AddTransient<CreateWorkOrderPage>();
            builder.Services.AddTransient<ContractorMonitoringPage>();
            builder.Services.AddTransient<SparepartListPage>();
            builder.Services.AddTransient<TechnicianListPage>();

            return builder.Build();
        }
    }
}
