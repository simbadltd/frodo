using System;
using System.Collections.Generic;
using System.Globalization;
using Frodo.Core;
using Frodo.Integration.Jira.JiraModel;
using NodaTime;

namespace Frodo.Integration.Jira
{
    public sealed class JiraTimeEntriesExportService : ITimeEntriesExportService
    {
        public bool Export(User user, TimeEntry timeEntry)
        {
            var client = Client(user);

            if (timeEntry.IsExported)
            {
                throw new InvalidOperationException("Cannot export already exported time entry");
            }

            if (Math.Abs(timeEntry.Duration.TotalSeconds) < 0.001)
            {
                // [kk] Zero time entries should not be logged
                return true;
            }

            var worklog = ToTempoWorklog(user, timeEntry);
            return client.AddTempoWorklog(worklog);
        }

        private TempoWorklog ToTempoWorklog(User user, TimeEntry timeEntry)
        {
            var worklog = new TempoWorklog
            {
                worklogAttributes = new List<TempoWorklogAttribute>
                {
                    new TempoWorklogAttribute
                    {
                        key = "_Activity_",
                        value = TempoActivity(timeEntry.Activity),
                    }
                },

                issue = new TempoIssue
                {
                    key = timeEntry.TaskId,
                },

                author = new TempoAuthor
                {
                    name = user.JiraUserName,
                },

                comment = timeEntry.Description,
                dateStarted = ToIso8601String(timeEntry.Start),
                timeSpentSeconds = (int) Math.Round(timeEntry.Duration.TotalSeconds, MidpointRounding.AwayFromZero),
            };

            return worklog;
        }

        private string TempoActivity(Activity activity)
        {
            /*
             * Bugfixing
             * Code%20Review
             * Code%20Review%20Fixes
             * Design%2FAnalysis
             * Development
             * Environment%20Setup
             * Estimation
             * Integration%20Testing
             * Technical%20Control
             * Testing
             * Other
             * AutomationPerformanceTesting
             * PM
             */
            
            switch (activity)
            {
                case Activity.Development:
                    return "Development";
                case Activity.Analysis:
                    return "Design%2FAnalysis";
                case Activity.Testing:
                    return "Testing";
                case Activity.Other:
                    return "Other";
                default:
                    throw new ArgumentOutOfRangeException(nameof(activity), activity, null);
            }
        }

        public static string ToIso8601String(OffsetDateTime dt)
        {
            //return dt.ToString("yyyy-MM-ddTHH:mm:ss.fffo<+HHmm>", CultureInfo.InvariantCulture); //todo[kk]: It seems Tempo cannot work with offsets?
            return dt.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);
        }


        private static JiraClient Client(User user)
        {
            var client = new JiraClient(new JiraAccount
            {
                ServerUrl = "https://oneinc.atlassian.net/", // todo[kk]: move to settings
                User = user.Email,
                Password = user.JiraAccountPassword,
            });

            return client;
        }
    }
}