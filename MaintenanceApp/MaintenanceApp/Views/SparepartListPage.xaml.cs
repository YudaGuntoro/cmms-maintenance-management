using MaintenanceApp.ViewsModel;

namespace MaintenanceApp.Views
{
    public partial class SparepartListPage : ContentPage
    {
        private readonly SparepartListViewModel _viewModel = new();

        public SparepartListPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }

        private async void RefreshView_Refreshing(object sender, EventArgs e)
        {
            await _viewModel.LoadAsync();
        }
    }
}
