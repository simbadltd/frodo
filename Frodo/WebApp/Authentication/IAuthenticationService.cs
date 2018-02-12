using System;

namespace Frodo.WebApp.Authentication
{
    public interface IAuthenticationService
    {
        AuthenticatedUser Login(string login, string password);

        LogoutResult Logout(Guid authenticatedUserGuid);

        AuthenticatedUser FindByGuid(Guid authenticatedUserGuid);
    }
}