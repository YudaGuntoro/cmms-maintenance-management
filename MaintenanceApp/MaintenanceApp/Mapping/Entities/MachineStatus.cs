using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintenanceApp.Mapping.Entities
{
    public class MachineStatus
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("lineName")]
        public string LineName { get; set; }

        [JsonProperty("machineName")]
        public string MachineName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("startTime")]
        public DateTime StartTime { get; set; }

        [JsonProperty("endTime")]
        public DateTime EndTime { get; set; }
    }

}
