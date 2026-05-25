using Maintenance.Domain.Mapping.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Maintenance.Repository.PreventiveRepository
{
    public interface IPreventiveRepository
    {

        Task<bool> InsertPreventiveAsync(RequestPreventive data);
    }
}
