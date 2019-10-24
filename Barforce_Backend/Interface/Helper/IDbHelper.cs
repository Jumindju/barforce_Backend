using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Barforce_Backend.Interface.Helper
{
    public interface IDbHelper
    {
        Task<IDbConnection> GetConnection(CancellationToken ct = default);
    }
}
