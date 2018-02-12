﻿using System;

namespace Frodo.Integration.Jira.JiraModel
{   
    [Serializable()]
    public class JiraApiException : Exception
    {
        public JiraApiException() : base() { }
        public JiraApiException(string message) : base(message) { }
        public JiraApiException(string message, Exception inner) : base(message, inner) { }
    }
}
