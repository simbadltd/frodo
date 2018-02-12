using FluentValidation;
using Frodo.WebApp.Modules.Register;

namespace Frodo.WebApp.Modules.Login
       {
           public class LoginAttemptValidator : AbstractValidator<LoginAttempt>
           {
               public LoginAttemptValidator()
               {
                   RuleFor(x => x.Login).NotEmpty().WithMessage($"{nameof(LoginAttempt.Login)} is required");
                   RuleFor(x => x.Password).NotEmpty().WithMessage($"{nameof(LoginAttempt.Password)} is required");
               }
           }
}