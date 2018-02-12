using FluentValidation;

namespace Frodo.WebApp.Modules.Register
       {
           public class NewUserValidator : AbstractValidator<NewUser>
           {
               public NewUserValidator()
               {
                   RuleFor(x => x.Login).NotEmpty().WithMessage($"{nameof(NewUser.Login)} is required");
                   RuleFor(x => x.Email).NotEmpty().WithMessage($"{nameof(NewUser.Email)} is required");
                   RuleFor(x => x.Password).NotEmpty().WithMessage($"{nameof(NewUser.Password)} is required");
                   RuleFor(x => x.Password).Equal(x => x.ConfirmPassword).WithMessage($"{nameof(NewUser.Password)} and {nameof(NewUser.ConfirmPassword)} are different");
               }
           }
}