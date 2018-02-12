namespace Frodo.Infrastructure.Configuration
{
    public interface ISettings
    {
        int Port { get; set; }

        bool IsSysadminAccountEnabled { get; set; }

        string Domain { get; set; }
        
        IEmailSettings EmailSettings { get; set; }
    }
}