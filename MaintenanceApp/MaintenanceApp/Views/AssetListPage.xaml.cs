using MaintenanceApp.Mapping.Cmms;
using MaintenanceApp.ViewsModel;

namespace MaintenanceApp.Views
{
    public partial class AssetListPage : ContentPage
    {
        private readonly AssetListViewModel _viewModel = new();

        public AssetListPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }

        private async void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            await _viewModel.LoadAsync();
        }

        private async void Filter_Clicked(object sender, EventArgs e)
        {
            await _viewModel.LoadAsync();
        }

        private async void Assets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var asset = e.CurrentSelection.FirstOrDefault() as AssetDto;
            if (asset == null)
            {
                return;
            }

            ((CollectionView)sender).SelectedItem = null;
            await Shell.Current.GoToAsync($"asset-detail?assetId={asset.Id}");
        }
    }
}
