using MaintenanceApp.Navigation;

namespace MaintenanceApp.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly INavigationService _navigationService;

        public MainPage(INavigationService navigationService)
        {
            _navigationService = navigationService;
            InitializeComponent();
        }

        private async void BtnPreventivePage_Clicked(object sender, EventArgs e)
        {
            await _navigationService.GoToAsync(Routes.PreventivePage);
        }

        private async void BtnSetting_Clicked(object sender, EventArgs e)
        {
            await _navigationService.GoToAsync(Routes.SettingPage);
        }

        private async void BtnMachineTroublePage_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Info", "Coming Soon", "OK");
        }
    }

}
