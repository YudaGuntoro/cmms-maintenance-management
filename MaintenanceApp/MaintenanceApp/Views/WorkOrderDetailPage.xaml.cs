using MaintenanceApp.ViewsModel;

namespace MaintenanceApp.Views
{
    public partial class WorkOrderDetailPage : ContentPage, IQueryAttributable
    {
        private readonly WorkOrderDetailViewModel _viewModel = new();
        private int _workOrderId;

        public WorkOrderDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("workOrderId", out var value) && int.TryParse(value?.ToString(), out var id))
            {
                _workOrderId = id;
                await _viewModel.LoadAsync(_workOrderId);
            }
        }

        private async void Assign_Clicked(object sender, EventArgs e)
        {
            try
            {
                var input = await DisplayPromptAsync("Assign", "Technician ID");
                if (int.TryParse(input, out var technicianId))
                {
                    await _viewModel.AssignAsync(technicianId);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void Start_Clicked(object sender, EventArgs e)
        {
            try
            {
                await _viewModel.StartAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void Complete_Clicked(object sender, EventArgs e)
        {
            try
            {
                var failureCode = await DisplayPromptAsync("Complete", "Failure code");
                var rootCause = await DisplayPromptAsync("Complete", "Root cause");
                var actionTaken = await DisplayPromptAsync("Complete", "Action taken");
                await _viewModel.CompleteAsync(failureCode, rootCause, actionTaken);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void Close_Clicked(object sender, EventArgs e)
        {
            try
            {
                var failureCode = await DisplayPromptAsync("Close", "Failure code");
                var rootCause = await DisplayPromptAsync("Close", "Root cause");
                var actionTaken = await DisplayPromptAsync("Close", "Action taken");
                await _viewModel.CloseAsync(failureCode, rootCause, actionTaken);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
