using System.Collections.Generic;
using System.Linq;
using Frodo.Common;
using Frodo.Core;
using Frodo.Core.Repositories;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Validation;

namespace Frodo.WebApp.Modules.Register
{
    public class RegisterModule : Module
    {
        public RegisterModule(IMembershipFeature membershipFeature, IActivateUserFeature activateUserFeature): base("/register")
        {
            Get["/"] = p =>
            {
                return View["Index"];
            };
            
            Get["/{userId}/activate/{activationKey}"] = p =>
            {
                string userId = p.userId.Value.ToString();
                string activationKey = p.activationKey.Value.ToString();
                
                var activationResult = activateUserFeature.ActivateUser(userId.ToGuid(), activationKey);

                if (activationResult.IsSuccessful)
                {
                    return View["ActivationSuccess", activationResult];
                }
                else
                {
                    AddError("Membership", "Cannot activate user");
                    return View["Index"];
                }
            };

            Post["/"] = p =>
            {
                var registerAttempt = this.Bind<NewUser>();
                var isValid = this.Validate(registerAttempt);

                if (isValid)
                {
                    var registerResult = membershipFeature.RegisterUser(registerAttempt.Login, registerAttempt.Email,
                        registerAttempt.Password);

                    if (registerResult != MembershipFeature.RegisterResult.Successful)
                    {
                        AddError("Membership", "User has been already registered");
                    }
                }

                return View["Index"];
            };
        }
    }
}