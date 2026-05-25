using Maintenance.Domain.Mapping.Request;
using Maintenance.Repository.PreventiveRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintenance.Persistence.Services.PreventiveService
{
    public class PreventiveService : IPreventiveService
    {
        protected readonly IPreventiveRepository Context;
        public PreventiveService(IPreventiveRepository Data) => Context = Data;
        public Task<bool> InsertPreventive(RequestPreventive data)
        {
            return Context.InsertPreventiveAsync(data);
        }
    }
}
