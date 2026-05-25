using MaintenanceApp.Mapping.Entities;
using MaintenanceApp.Mapping.Response;
using MaintenanceApp.Navigation;
using MaintenanceApp.Singletone;
using MaintenanceApp.ViewsModel;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Windows.Input;

namespace MaintenanceApp.Views
{
    public partial class MachineTroublePage : ContentPage
    {
        private readonly INavigationService _navigationService;
        private string Url;
        private MachineTroubleViewModel _viewModel;
        public bool IsRefreshing { get; set; } = false;
        public bool IsBusy { get; set; }


        public ICommand RefreshCommand { get; set; }
  
        // Default constructor
        public MachineTroublePage()
        {
            InitializeComponent();

        }

        // Constructor with INavigationService
        public MachineTroublePage(INavigationService navigationService)
        {
            InitializeComponent();
            _navigationService = navigationService;
            LoadConnectionDataAsync();

            RefreshCommand = new Command(async () => await RefreshData());
        }

        /// <summary>
        /// Loads the connection data from SQLite and fetches the machine trouble data.
        /// </summary>
        private async void LoadConnectionDataAsync()
        {
            try
            {
                var connection = await SqliteDbContext.Instance.GetConnectionAsync();
                if (connection != null)
                {
                    Url = connection.Url;
                    await GetListMachineTroubleAsync();
                }
            }
            catch (Exception ex)
            {
                // Handle any exception that occurs during the request
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Fetches the list of machine troubles from the API.
        /// </summary>
        public async Task GetListMachineTroubleAsync()
        {
            try
            {
                IsRefreshing = true;
                IsBusy = true; // Show loading indicator

                // Build the API URL
                string apiUrl = $"{Url}/api/MachineTrouble/v1/GetMachineTrouble";
                var client = new RestClient();
                var request = new RestRequest(apiUrl, Method.Get);
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<IEnumerable<MachineStatus>>>(response.Content);

                    if (apiResponse.Success && apiResponse.Data != null)
                    {
                        MachineTroubleListView.ItemsSource = apiResponse.Data;
                    }
                    else
                    {
                        await DisplayAlert("Failed", apiResponse.Message ?? "No data available", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Failed", $"Error: {response.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsRefreshing = false;
                IsBusy = false; // Hide loading indicator
                
            }
        }

        private async Task RefreshData()
        {
            IsBusy = true;
            // Simulate a data load or API call
            await Task.Delay(2000); // Simulate data loading
                                    // Load your data here, for example:
                                    // MachineStatus.Clear();
                                    // var data = await YourDataService.GetDataAsync();
                                    // foreach (var item in data) MachineStatus.Add(item);

            IsBusy = false;
        }
        
        private async void Button_Clicked(object sender, EventArgs e)
        {
            await GetListMachineTroubleAsync();
        }
    }
}
