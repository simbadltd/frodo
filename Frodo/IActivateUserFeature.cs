using System;

namespace Frodo
{
    public interface IActivateUserFeature
    {
        ActivateUserFeature.ActivationResult ActivateUser(Guid userId, string activationKey);
    }
}