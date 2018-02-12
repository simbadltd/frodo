using System.Collections.Generic;

namespace Frodo.Integration.Jira.JiraModel
{
    public class IssueCreateMeta
    {
        public string expand { get; set; }
        public List<ProjectMeta> projects { get; set; }
    }
}
