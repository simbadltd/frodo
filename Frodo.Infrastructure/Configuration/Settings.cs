namespace Frodo.Infrastructure.Configuration
{
    public class Settings : ISettings
    {
        public Settings()
        {
            Domain = "localhost";
            Port = 8484;
            IsSysadminAccountEnabled = true;
            EmailSettings = new EmailSettings
            {
                FromMailAddress = "frodo@oneincsystems.com",
                SmtpServer = "Mailtest.OneIncSystems.com",
                SmtpPort = 25,
                EnableSsl = false,
            };
        }

        public int Port { get; set; }

        public bool IsSysadminAccountEnabled { get; set; }

        public string Domain { get; set; }
        
        public IEmailSettings EmailSettings { get; set; }
    }
}