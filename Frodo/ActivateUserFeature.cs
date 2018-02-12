using System;
using Frodo.Core;
using Frodo.Core.Repositories;

namespace Frodo
{
    public sealed class ActivateUserFeature : IActivateUserFeature
    {
        private readonly IRepository<User> _userRepository;

        private readonly IUnitOfWork _unitOfWork;

        public ActivateUserFeature(IRepository<User> userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public ActivationResult ActivateUser(Guid userId, string activationKey)
        {
            var user = _userRepository.Get(userId);
            if (user != null && user.IsActive == false)
            {
                if (string.Equals(user.ActivationKey, activationKey, StringComparison.OrdinalIgnoreCase))
                {
                    user.IsActive = true;
                    _userRepository.Save(user);
                    _unitOfWork.Commit();

                    return new ActivationResult
                    {
                        IsSuccessful = true,
                        UserName = user.Login,
                    };
                }
            }

            return new ActivationResult
            {
                IsSuccessful = false,
            };
        }

        public sealed class ActivationResult
        {
            public bool IsSuccessful { get; set; }
            
            public string UserName { get; set; }
        }
    }
}