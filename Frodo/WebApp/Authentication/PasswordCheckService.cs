using System;
using Frodo.Common;
using Frodo.Core;

namespace Frodo.WebApp.Authentication
{
    public sealed class PasswordCheckService : IPasswordCheckService
    {
        public bool Check(User user, string password)
        {
            var correctPasswordHash = user.PasswordHash;
            var passedPasswordHash = string.Concat(user.Salt, password).Md5();

            return string.Equals(correctPasswordHash, passedPasswordHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}