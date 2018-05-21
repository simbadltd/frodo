using System;
using System.Collections.Generic;
using System.Linq;
using Frodo.Core;
using Frodo.Core.Repositories;
using Frodo.Infrastructure.Configuration;

namespace Frodo.WebApp.Authentication
{
    public sealed class AuthenticationService : IAuthenticationService
    {
        private static readonly User SystemUser = new User
        {
            Id = new Guid("69bcefa5-3b11-4fbe-a3e7-fdfac4b00234"),
            Login = "sa",
            PasswordHash = "202CB962AC59075B964B07152D234B70",
            Salt = string.Empty,
        };

        private static readonly object SyncRoot = new object();

        private static readonly IDictionary<Guid, AuthenticatedUser> AuthenticatedUsers =
            new Dictionary<Guid, AuthenticatedUser>();

        private readonly IRepository<User> _userRepository;

        private readonly IPasswordCheckService _passwordCheckService;

        private readonly bool _isSystemUserEnabled;

        public AuthenticationService(IRepository<User> userRepository, IPasswordCheckService passwordCheckService, ISettingsProvider settingsProvider)
        {
            _userRepository = userRepository;
            _passwordCheckService = passwordCheckService;
            _isSystemUserEnabled = settingsProvider.Settings.IsSysadminAccountEnabled;
        }

        public AuthenticatedUser Login(string login, string password)
        {
            var user = FindUserByLogin(login);

            var userNotExists = user == null;
            if (userNotExists)
            {
                return null;
            }

            var passwordInvalid = _passwordCheckService.Check(user, password) == false;
            if (passwordInvalid)
            {
                return null;
            }

            var authenticatedUser = AuthenticatedUser.FromUser(user);

            lock (SyncRoot)
            {
                LogoutUser(authenticatedUser.UserId);
                AuthenticatedUsers[authenticatedUser.Guid] = authenticatedUser;
            }

            return authenticatedUser;
        }

        private User FindUserByLogin(string login)
        {
            if (_isSystemUserEnabled && string.Equals(login, SystemUser.Login, StringComparison.OrdinalIgnoreCase))
            {
                return SystemUser;
            }

            return _userRepository.FindByLogin(login);
        }

        public LogoutResult Logout(Guid authenticatedUserGuid)
        {
            lock (SyncRoot)
            {
                if (AuthenticatedUsers.ContainsKey(authenticatedUserGuid))
                {
                    AuthenticatedUsers.Remove(authenticatedUserGuid);
                    return LogoutResult.LogoutSuccessful;
                }

                return LogoutResult.UserWasNotLoggedIn;
            }
        }

        private static void LogoutUser(Guid userId)
        {
            var authenticatedUsers = AuthenticatedUsers.Values.Where(x => x.UserId == userId).ToList();

            foreach (var authenticatedUser in authenticatedUsers)
            {
                AuthenticatedUsers.Remove(authenticatedUser.Guid);
            }
        }

        public AuthenticatedUser FindByGuid(Guid authenticatedUserGuid)
        {
            lock (SyncRoot)
            {
                if (AuthenticatedUsers.ContainsKey(authenticatedUserGuid))
                {
                    var authenticatedUser = AuthenticatedUsers[authenticatedUserGuid];

                    return authenticatedUser;
                }

                return null;
            }
        }
    }
}