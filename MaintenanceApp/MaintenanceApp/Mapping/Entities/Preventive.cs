using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaintenanceApp.Mapping.Entities
{
    public class Preventive
    {
        public int Id { get; set; }
        public string Line { get; set; }
        public string Machine { get; set; }
        public string Technician { get; set; }
        public string Action { get; set; }
        public byte[] Image { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
