using System.Collections.Generic;

namespace Frodo.Integration.Jira.JiraModel
{
    public class Comments
    {
        public int startAt { get; set; }
        public int maxResults { get; set; }
        public int total { get; set; }
        public List<Comment> comments { get; set; }
    }
}