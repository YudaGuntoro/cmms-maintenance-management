using MaintenanceApp.Singletone;
using System.Net.NetworkInformation;

namespace MaintenanceApp.Views;

public partial class SettingPage : ContentPage
{
	public SettingPage()
	{
		InitializeComponent();
        LoadConnectionDataAsync();
    }
    private async void LoadConnectionDataAsync()
    {
        var connection = await SqliteDbContext.Instance.GetConnectionAsync();
        if (connection != null)
        {
            EntryIp.Text = connection.Url;
            //EntryPort.Text = connection.Port.ToString();
        }
    }
    private async void BtnSubmit_Clicked(object sender, EventArgs e)
    {
        bool success = await SqliteDbContext.Instance.UpdateConnectionAsync(EntryIp.Text);
        if (success)
        {
            // Display success message to the user
            await DisplayAlert("Success", "The IP and Port have been updated successfully.", "OK");
        }
        else
        {
            // Display error message to the user
            await DisplayAlert("Error", "Failed to update the IP and Port. Please try again.", "OK");
        }
    }
    private async Task<bool> PingHostAsync(string host)
    {
        using var ping = new Ping();
        try
        {
            var reply = await ping.SendPingAsync(host);
            return reply.Status == IPStatus.Success;
        }
        catch (PingException ex)
        {
            Console.WriteLine($"Ping exception: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during ping: {ex.Message}");
            return false;
        }
    }

    private async void BtnPingTest_Clicked(object sender, EventArgs e)
    {
        try
        {
            Uri uri = new Uri(EntryIp.Text);
            string ipAddress = uri.Host;

            // Ping the IP address
            bool isPingSuccessful = await PingHostAsync(ipAddress);

            // Display the result in an alert
            if (isPingSuccessful)
            {
                await DisplayAlert("Success", $"Ping to {ipAddress} was successful.", "OK");
            }
            else
            {
                await DisplayAlert("Failure", $"Ping to {ipAddress} failed.", "OK");
            }
        }
        catch (UriFormatException)
        {
            await DisplayAlert("Error", "The URL provided is not valid.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
    }
}