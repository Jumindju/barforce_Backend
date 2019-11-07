using System;
using System.Threading.Tasks;

namespace Barforce_Backend.Interface.Helper
{
    public interface IEmailHelper
    {
        Task SendVerifyMail(string receiverAddress, int verifyNum);
    }
}