using System.Collections.Generic;
using Frodo.Core;

namespace Frodo.Persistence.Mapping
{
    public class UserDao : Dao, IEntityProjection<User>
    {
        public string Login { get; set; }

        public string PasswordHash { get; set; }

        public string Salt { get; set; }

        public string TogglApiToken { get; set; }

        public string Email { get; set; }
        
        public bool IsActive { get; set; }
        
        public string ActivationKey { get; set; }
        
        public string JiraUserName { get; set; }
        
        public string JiraAccountPassword { get; set; }

        public Dictionary<string, string> TaskIdMap { get; set; }
        
        public Dictionary<string, Activity> ActivityMap { get; set; }
    }
}