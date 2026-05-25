using MaintenanceApp.Views;

namespace MaintenanceApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("asset-detail", typeof(AssetDetailPage));
            Routing.RegisterRoute("work-order-detail", typeof(WorkOrderDetailPage));
            Routing.RegisterRoute("create-work-order", typeof(CreateWorkOrderPage));
            foreach (var route in Routes.RouteTypeMap)
            {
                Routing.RegisterRoute(route.Key, route.Value);
            }
        }
    }
}
