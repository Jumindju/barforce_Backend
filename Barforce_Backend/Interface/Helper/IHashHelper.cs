using System.Threading.Tasks;

namespace Barforce_Backend.Interface.Helper
{
    public interface IHashHelper
    {
        string GetHash(string clearPw, string salt);
        bool IsCorrectPassword(string clearPw, string salt, string hashedPw);
        Task<string> CreateSalt();
    }
}