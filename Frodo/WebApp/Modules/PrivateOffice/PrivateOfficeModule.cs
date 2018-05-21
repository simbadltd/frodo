using System.Linq;
using Frodo.Core;
using Frodo.Core.Repositories;
using Nancy;
using Nancy.Security;

namespace Frodo.WebApp.Modules.PrivateOffice
{
    public class PrivateOfficeModule : NancyModule
    {
        public PrivateOfficeModule(IRepository<User> userRepository) : base("/privateOffice")
        {
            Get["/"] = p =>
            {
                this.RequiresAuthentication();
                var user = userRepository.FindByLogin(Context.CurrentUser.UserName);

                return View["Index", user];
            };
        }
    }
}