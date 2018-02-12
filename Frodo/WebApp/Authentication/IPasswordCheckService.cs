using Frodo.Core;

namespace Frodo.WebApp.Authentication
{
    public interface IPasswordCheckService
    {
        bool Check(User user, string password);
    }
}