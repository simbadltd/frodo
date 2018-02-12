using System;
using Frodo.WebApp.Authentication;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;

namespace Frodo.WebApp
{
    public sealed class UserMapper : IUserMapper
    {
        private readonly IAuthenticationService _authenticationService;

        public UserMapper(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            return _authenticationService.FindByGuid(identifier);
        }
    }
}