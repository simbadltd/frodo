using System;
using Frodo.WebApp.Authentication;
using Nancy.Authentication.Forms;
using Nancy.ModelBinding;

namespace Frodo.WebApp.Modules.Login
{
    public sealed class LoginModule : Module
    {
        public LoginModule(IAuthenticationService authenticationService): base("/login")
        {
            Get["/"] = p => View["Index"];

            Get["/logout"] = p => this.LogoutAndRedirect("~/"); 

            Post["/"] = p =>
            {
                var loginAttempt = this.Bind<LoginAttempt>();
                var isValid = this.Validate(loginAttempt);

                if (isValid)
                {
                    var authenticatedUser = authenticationService.Login(loginAttempt.Login, loginAttempt.Password);
                    var isAuthencticated = authenticatedUser != null;

                    if (isAuthencticated)
                    {
                        return this.LoginAndRedirect(authenticatedUser.Guid, DateTime.Now.AddDays(7));
                    }
                    else
                    {
                        AddError("Login", "Wrong credentials");
                    }
                }

                return View["Index"];
            };
        }
    }
}