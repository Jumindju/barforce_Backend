using System;
using System.Threading.Tasks;
using Barforce_Backend.Model.User;

namespace Barforce_Backend.Interface.Repositories
{
    public interface IUserRepository
    {
        Task Register(UserRegister newUser);
        void Verify(Guid verifyGuid);
        Task<bool> UsernameExists(string userName);
        Task<bool> EMailExists(string email);
    }
}