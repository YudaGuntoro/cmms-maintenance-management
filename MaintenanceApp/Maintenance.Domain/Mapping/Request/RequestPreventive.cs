using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintenance.Domain.Mapping.Request
{
    public class RequestPreventive
    {
        public string Line { get; set; }
        public string Machine { get; set; }
        public string Technician { get; set; }
        public string Action { get; set; }
        public byte[]? Image { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
