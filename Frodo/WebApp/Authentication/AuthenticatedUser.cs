using System;
using System.Collections.Generic;
using Frodo.Core;
using Nancy.Security;

namespace Frodo.WebApp.Authentication
{
    public sealed class AuthenticatedUser : IUserIdentity
    {
        public Guid Guid { get; set; }

        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public IEnumerable<string> Claims { get; set; }

        public static AuthenticatedUser FromUser(User user)
        {
            var result = new AuthenticatedUser
                             {
                                 Guid = Guid.NewGuid(),
                                 UserId = user.Id,
                                 UserName = user.Login,
                             };

            return result;
        }
    }
}
