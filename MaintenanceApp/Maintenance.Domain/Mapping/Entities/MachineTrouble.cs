using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintenance.Domain.Mapping.Entities
{
    public class MachineTrouble
    {
        public int Id { get; set; }
        public string LineName { get; set; }
        public string MachineName { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
