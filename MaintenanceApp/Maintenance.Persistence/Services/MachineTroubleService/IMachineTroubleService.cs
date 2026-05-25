using Maintenance.Domain.Mapping.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintenance.Persistence.Services.MachineTroubleService
{
    public interface IMachineTroubleService
    {
        Task<IEnumerable<MachineStatus>> GetMachineStatusTrouble();
    }
}
