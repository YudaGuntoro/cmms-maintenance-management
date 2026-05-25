using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintenance.Domain.Mapping.Entities
{
    public class Preventive
    {
        public int Id { get; set; }
        public string Line { get; set; } = string.Empty;
        public string Machine { get; set; } = string.Empty;
        public string Technician { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public byte[] Image { get; set; } = Array.Empty<byte>();
        public DateTime TimeStamp { get; set; }
    }
}
