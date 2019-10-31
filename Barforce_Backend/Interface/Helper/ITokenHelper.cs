using Barforce_Backend.Model.User;

namespace Barforce_Backend.Interface.Helper
{
    public interface ITokenHelper
    {
        string GetUserToken(AuthUser user);
    }
}