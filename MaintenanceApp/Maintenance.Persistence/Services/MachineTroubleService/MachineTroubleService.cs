using Maintenance.Domain.Mapping.Entities;
using Maintenance.Repository.MachineTroubleRepository;
using Maintenance.Repository.PreventiveRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintenance.Persistence.Services.MachineTroubleService
{
    public class MachineTroubleService : IMachineTroubleService
    {
        protected readonly IMachineTroubleRepository Context;
        public MachineTroubleService(IMachineTroubleRepository Data) => Context = Data;

        public Task<IEnumerable<MachineStatus>> GetMachineStatusTrouble()
        {
            return Context.GetMachineStatusTroubleAsync();
        }
    }
}
