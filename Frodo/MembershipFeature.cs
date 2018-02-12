using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Frodo.Core;
using Frodo.Core.Events;
using Frodo.Core.Repositories;

namespace Frodo
{
    public sealed class MembershipFeature : IMembershipFeature
    {
        private readonly IRepository<User> _userRepository;
        
        private readonly IUnitOfWork _unitOfWork;

        public MembershipFeature(IRepository<User> userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public RegisterResult RegisterUser(string login, string email, string password)
        {
            var sanitizedLogin = login.Trim();
            var sanitizedEmail = email.Trim();

            var existingUser = _userRepository.FindByLogin(sanitizedLogin) ?? _userRepository.FindByEmail(sanitizedEmail);
            if (existingUser != null)
            {
                return RegisterResult.UserExists;
            }

            var newUser = CreateNewUser(sanitizedLogin, sanitizedEmail, password);
            newUser.Publish(new NewUserCreatedEvent(newUser));
            
            _userRepository.Save(newUser);
            _unitOfWork.Commit();

            return RegisterResult.Successful;
        }

        private static User CreateNewUser(string login, string email, string password)
        {
            var result = new User
            {
                Salt = Guid.NewGuid().ToString("N"),
                Email = email,
                Login = login,
                IsActive = false,
                ActivationKey = Guid.NewGuid().ToString("N")
            };
            
            result.ChangePassword(password);

            return result;
        }

        public enum RegisterResult
        {
            Successful,
            UserExists,
        }
    }
}