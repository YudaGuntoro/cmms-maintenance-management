using MaintenanceApp.ViewsModel;

namespace MaintenanceApp.Views
{
    public partial class AssetDetailPage : ContentPage, IQueryAttributable
    {
        private readonly AssetDetailViewModel _viewModel = new();
        private int _assetId;

        public AssetDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("assetId", out var value) && int.TryParse(value?.ToString(), out var assetId))
            {
                _assetId = assetId;
                await _viewModel.LoadAsync(_assetId);
            }
        }
    }
}
