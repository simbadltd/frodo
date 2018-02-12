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
                return View["Index", userRepository.GetAll().FirstOrDefault()];
            };
        }
    }
}