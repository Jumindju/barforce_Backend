﻿using System.Threading.Tasks;
using Barforce_Backend.Model.User;

namespace Barforce_Backend.Interface.Helper
{
    public interface ITokenHelper
    {
        Task<string> GetUserToken(AuthUser user);
    }
}