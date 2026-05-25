using MaintenanceApp.Mapping.Entities;
using MaintenanceApp.Singletone;
using MaintenanceApp.Mapping.Response;
using MaintenanceApp.Views.PopUp;
using Newtonsoft.Json;
using RestSharp;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MauiPopup;
using SkiaSharp;
using System.Net;

namespace MaintenanceApp.Views
{
    public partial class PreventivePage : ContentPage
    {
        FileResult? photo;
        private string Url;

        public PreventivePage()
        {
            InitializeComponent();
            LoadConnectionDataAsync();
        }

        private async void LoadConnectionDataAsync()
        {
            var connection = await SqliteDbContext.Instance.GetConnectionAsync();
            if (connection != null)
            {
                Url = connection.Url;
            }
        }

        private async void BtnSubmit_Clicked(object sender, EventArgs e)
        {
            if (photo != null)
            {
                bool isSuccess = false;
                try
                {
                    PopupAction.DisplayPopup(new PopupPage());
                    var imageBytes = await ImageFileToByteArrayAsync(photo);
                    var data = new Preventive
                    {
                        Line = TBxLine.Text,
                        Machine = TBxMachine.Text,
                        Technician = TBxTechnician.Text,
                        Action = TBxAction.Text,
                        TimeStamp = DateTime.Now,
                        Image = imageBytes
                    };
                    
                    string apiUrl = $"{Url}/api/Preventive/v1/InsertPreventive"; // Fixed URL typo
                    var client = new RestClient();
                    var request = new RestRequest(apiUrl,Method.Post);
                    request.AddJsonBody(data);

                    var response = await client.ExecuteAsync(request);

                    if (response.IsSuccessful && response.StatusCode == HttpStatusCode.OK)
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponse<Preventive>>(response.Content);
                        await DisplayAlert("Info", apiResponse.Message, "OK");
                        isSuccess = apiResponse.Success;
                        if (isSuccess)
                        {
                            photo = null;
                            TBxLine.Text = null;
                            TBxMachine.Text = null;
                            TBxTechnician.Text = null;
                            TBxAction.Text = null;
                            CapturedImage.Source = ImageSource.FromFile("Resources/Images/camera.png");
                        }
                        else
                        {
                            await DisplayAlert("Error", "Operation failed", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", $"Failed to communicate with the server: {response.StatusCode} - {response.StatusDescription}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }
                finally
                {
                    await PopupAction.ClosePopup();
                }
            }
            else
            {
                await DisplayAlert("Error", "Photo not captured", "OK");
            }
        }
        private async void OnImageTapped(object sender, EventArgs e)
        {
            try
            {
                // Capture the photo
                photo = await MediaPicker.CapturePhotoAsync();
                if (photo != null)
                {
                    // Load the photo into a memory stream
                    using var stream = await photo.OpenReadAsync();
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);

                    // Reset the memory stream position to the beginning
                    memoryStream.Position = 0;

                    // Display the photo in the Image control using the memory stream
                    CapturedImage.Source = ImageSource.FromStream(() => memoryStream);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Photo capture failed: {ex.Message}", "OK");
            }
        }

        private async Task<byte[]> ImageFileToByteArrayAsync(FileResult photo)
        {
            using var stream = await photo.OpenReadAsync();
            using var skStream = new SKManagedStream(stream);
            using var originalBitmap = SKBitmap.Decode(skStream);

            // Define the desired width and height for the resized image
            int desiredWidth = 800;  // Example width
            int desiredHeight = (int)((double)desiredWidth / originalBitmap.Width * originalBitmap.Height);

            // Create a resized bitmap
            using var resizedBitmap = originalBitmap.Resize(new SKImageInfo(desiredWidth, desiredHeight), SKFilterQuality.High);

            // Encode the resized bitmap to PNG format
            using var image = SKImage.FromBitmap(resizedBitmap);
            using var imageData = image.Encode(SKEncodedImageFormat.Png, 100);

            return imageData.ToArray();
        }
    }
}
