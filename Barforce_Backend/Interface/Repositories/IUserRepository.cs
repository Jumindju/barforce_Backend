using System.Threading.Tasks;
using Barforce_Backend.Model.User;

namespace Barforce_Backend.Interface.Repositories
{
    public interface IUserRepository
    {
        Task Register(UserRegister newUser);
        Task<bool> UsernameExists(string userName);
        Task<bool> EMailExists(string email);
        Task<string> Login(string userName, string password);
        Task ResetPassword(int userId, string newPassword);
        Task<string> VerifyMail(string userName, string password, int verifyToken);
        Task LogOff(int userId);
        Task<UserDto> ReadUserById(int userId);
    }
}