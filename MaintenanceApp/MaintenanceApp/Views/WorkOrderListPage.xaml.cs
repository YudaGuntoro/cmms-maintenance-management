using MaintenanceApp.Mapping.Cmms;
using MaintenanceApp.ViewsModel;

namespace MaintenanceApp.Views
{
    public partial class WorkOrderListPage : ContentPage
    {
        private readonly WorkOrderListViewModel _viewModel = new();

        public WorkOrderListPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }

        private async void Filter_Clicked(object sender, EventArgs e)
        {
            await _viewModel.LoadAsync();
        }

        private async void Create_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("create-work-order");
        }

        private async void WorkOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var workOrder = e.CurrentSelection.FirstOrDefault() as WorkOrderDto;
            if (workOrder == null)
            {
                return;
            }

            ((CollectionView)sender).SelectedItem = null;
            await Shell.Current.GoToAsync($"work-order-detail?workOrderId={workOrder.Id}");
        }
    }
}
