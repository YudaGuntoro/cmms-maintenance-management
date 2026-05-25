using Maintenance.Domain.Mapping.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintenance.Persistence.Services.PreventiveService
{
    public interface IPreventiveService
    {
        Task<bool> InsertPreventive(RequestPreventive data);
    }
}
