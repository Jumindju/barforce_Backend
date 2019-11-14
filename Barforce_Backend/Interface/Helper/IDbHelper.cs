using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Barforce_Backend.Interface.Helper
{
    public interface IDbHelper
    {
        Task<IDbConnection> GetConnection(CancellationToken ct = default);
    }
}
