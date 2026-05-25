namespace MaintenanceApp.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void Login_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Login", "Username dan password wajib diisi.", "OK");
                return;
            }

            Preferences.Set("cmms_is_logged_in", true);
            Preferences.Set("cmms_username", UsernameEntry.Text.Trim());
            await Shell.Current.GoToAsync("//dashboard");
        }
    }
}
