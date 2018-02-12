namespace Frodo
{
    public interface IMembershipFeature
    {
        MembershipFeature.RegisterResult RegisterUser(string login, string email, string password);
    }
}