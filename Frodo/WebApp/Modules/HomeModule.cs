using System.Linq;
using Frodo.Core;
using Frodo.Core.Repositories;
using Nancy;

namespace Frodo.WebApp.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule(IRepository<User> userRepository)
        {
            Get["/"] = p =>
            {
                return View["Index", userRepository.GetAll().FirstOrDefault()];
            };
        }
    }
}