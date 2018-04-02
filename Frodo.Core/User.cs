using System.Collections.Generic;
using Frodo.Common;

namespace Frodo.Core
{
    public sealed class User : Entity
    {
        public string Login { get; set; }

        public string PasswordHash { get; set; }

        public string Salt { get; set; }
        
        public string Email { get; set; }

        public string TogglApiToken { get; set; }

        public string JiraUserName { get; set; }

        public string JiraAccountPassword { get; set; }
        
        public bool IsActive { get; set; }
        
        public string ActivationKey { get; set; }        

        public Dictionary<string, string> TaskIdMap { get; set; }

        public Dictionary<string, Activity> ActivityMap { get; set; }

        public void ChangePassword(string passwordRaw)
        {
            PasswordHash = CalculatePasswordHash(passwordRaw);
        }
        
        public string CalculatePasswordHash(string passwordRaw)
        {
            return string.Concat(Salt, passwordRaw).Md5();
        }
    }
}

