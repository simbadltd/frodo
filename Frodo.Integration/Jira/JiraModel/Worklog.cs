using System.Collections.Generic;

namespace Frodo.Integration.Jira.JiraModel
{
    public class TempoAuthor
    {
        public string name { get; set; }
    }

    public class Worklog
    {
        //public string self { get; set; }
        //public Author author { get; set; }
        //public Author updateAuthor { get; set; }
        public string comment { get; set; }
        //public string created { get; set; }
        //public string updated { get; set; }
        public string started { get; set; }
        //public string timeSpent { get; set; }
        public int timeSpentSeconds { get; set; }
        //public string id { get; set; }
    }

    public class TempoWorklogAttribute
    {
        public string key { get; set; }

        public string value { get; set; }
    }

    public class TempoIssue
    {
        public string key { get; set; }
    }

    public class TempoWorklog
    {
        public List<TempoWorklogAttribute> worklogAttributes { get; set; }
        public TempoIssue issue { get; set; }
        public string comment { get; set; }
        public string dateStarted { get; set; }
        public int timeSpentSeconds { get; set; }
        public TempoAuthor author { get; set; }
    }
}