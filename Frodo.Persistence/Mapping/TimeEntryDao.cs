using System;
using Frodo.Core;
using NodaTime;

namespace Frodo.Persistence.Mapping
{
    public class TimeEntryDao : Dao, IEntityProjection<TimeEntry>
    {
        public string TaskId { get; set; }

        public Guid UserId { get; set; }

        public OffsetDateTime Start { get; set; }

        public OffsetDateTime End { get; set; }

        public Duration Duration { get; set; }

        public string Description { get; set; }

        public bool IsExported { get; set; }

        public string ExternalUid { get; set; }

        public string Activity { get; set; }
    }
}