using MaintenanceApp.Mapping.Cmms;
using MaintenanceApp.ViewsModel;

namespace MaintenanceApp.Views
{
    public partial class CreateWorkOrderPage : ContentPage
    {
        private readonly CreateWorkOrderViewModel _viewModel = new();

        public CreateWorkOrderPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAssetsAsync();
            TypePicker.SelectedItem ??= _viewModel.MaintenanceType;
            PriorityPicker.SelectedItem ??= _viewModel.Priority;
        }

        private async void Submit_Clicked(object sender, EventArgs e)
        {
            try
            {
                _viewModel.SelectedAsset = AssetPicker.SelectedItem as AssetDto;
                _viewModel.Title = TitleEntry.Text ?? string.Empty;
                _viewModel.Description = DescriptionEditor.Text ?? string.Empty;
                _viewModel.MaintenanceType = TypePicker.SelectedItem?.ToString() ?? "CORRECTIVE";
                _viewModel.Priority = PriorityPicker.SelectedItem?.ToString() ?? "MEDIUM";
                _viewModel.ScheduledDate = ScheduledDatePicker.Date;

                var result = await _viewModel.SubmitAsync();
                await DisplayAlert("Success", $"Created {result?.WoNumber}", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
