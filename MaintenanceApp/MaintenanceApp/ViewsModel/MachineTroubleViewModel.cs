using MaintenanceApp.Mapping.Entities;
using MaintenanceApp.Mapping.Response;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MaintenanceApp.ViewsModel
{
    public class MachineTroubleViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        private ObservableCollection<MachineStatus> _machineStatus;
        public ObservableCollection<MachineStatus> MachineStatus
        {
            get => _machineStatus;
            set
            {
                _machineStatus = value;
                OnPropertyChanged(nameof(MachineStatus));
            }
        }

        public MachineTroubleViewModel()
        {
            // Initialize with empty list
            MachineStatus = new ObservableCollection<MachineStatus>();
        }

        public async Task LoadDataAsync(string url)
        {
            try
            {
                IsBusy = true;
                string apiUrl = $"{url}/api/MachineTrouble/v1/GetMachineTrouble";
                var client = new RestClient();
                var request = new RestRequest(apiUrl, Method.Get);

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<IEnumerable<MachineStatus>>>(response.Content);
                    if (apiResponse.Success && apiResponse.Data != null)
                    {
                        MachineStatus = new ObservableCollection<MachineStatus>(apiResponse.Data);
                    }
                    else
                    {
                        // Handle unsuccessful response
                    }
                }
                else
                {
                    // Handle HTTP error
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
