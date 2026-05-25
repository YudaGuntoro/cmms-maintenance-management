using Maintenance.Domain.Mapping.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintenance.Repository.MachineTroubleRepository
{
    public interface IMachineTroubleRepository
    {
        Task<IEnumerable<MachineStatus>> GetMachineStatusTroubleAsync();
    }
}
