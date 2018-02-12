namespace Frodo.Infrastructure.Configuration
{
    public sealed class EmailSettings : IEmailSettings
    {
        public string SmtpServer { get; set; }
        
        public int SmtpPort { get; set; }
        
        public bool EnableSsl { get; set; }
        
        public string User { get; set; }
        
        public string Password { get; set; }
        
        public string FromMailAddress { get; set; }
        
        public int Timeout { get; set; }
    }
}