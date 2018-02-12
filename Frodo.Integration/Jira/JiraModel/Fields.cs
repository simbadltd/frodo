namespace Frodo.Integration.Jira.JiraModel
{
    public class Fields
    {
        /// <summary>
        /// Release notes field
        /// </summary>
        public string customfield_10400 { get; set; }

        /// <summary>
        /// Available From field
        /// </summary>
        public Author reporter { get; set; }
        public int aggregatetimeoriginalestimate { get; set; }
        public string created { get; set; }
        public string updated { get; set; }
        public string description { get; set; }
        public Priority priority { get; set; }
        public int timeoriginalestimate { get; set; }
        public int aggregatetimespent { get; set; }

        public string summary { get; set; }

        public Issue parent { get; set; }
    }
}