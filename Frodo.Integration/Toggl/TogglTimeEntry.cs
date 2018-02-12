using System.Linq;
using Frodo.Core;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;

namespace Frodo.Integration.Toggl
{
    internal sealed class TogglTimeEntry
    {
        public long Id { get; set; }
        public string Guid { get; set; }
        public long Wid { get; set; }
        public long Pid { get; set; }
        public bool Billable { get; set; }
        public string Start { get; set; }
        public string Stop { get; set; }
        public long Duration { get; set; }
        public string Description { get; set; }
        public bool Duronly { get; set; }
        public string At { get; set; }
        public long Uid { get; set; }

        public TimeEntry ToTimeEntry()
        {
            return new TimeEntry
            {

                Start = OffsetDateTimePattern.GeneralIso.Parse(Start).Value,
                End = OffsetDateTimePattern.GeneralIso.Parse(Stop).Value,
                Duration = NodaTime.Duration.FromSeconds(Duration),
                Description = Description,
                ExternalUid = Id.ToString(),
            };
        }
    }
}