using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Frodo.Core;
using NodaTime;

namespace Frodo.Integration.Toggl
{
    public sealed class TogglTimeEntriesImportService : ITimeEntriesImportService
    {
        public ICollection<TimeEntry> Import(User user, OffsetDateTime start, OffsetDateTime end)
        {
            var result = new List<TimeEntry>();
            var client = new TogglClient(user);

            var togglTimeEntries = client.GetTimeEntries(start, end);
            foreach (var togglTimeEntry in togglTimeEntries)
            {
                if (togglTimeEntry.Duration < 0.001)
                {
                    // [kk] we should consider only finished non-zero tasks
                    continue;
                }
                
                var timeEntry = togglTimeEntry.ToTimeEntry();
                timeEntry.UserId = user.Id;

                result.Add(timeEntry);
            }

            return result;
        }
    }
}