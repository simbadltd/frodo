using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NodaTime;

namespace Frodo.Core
{
    public sealed class TimeEntry : Entity
    {
        public string TaskId { get; set; }

        public Guid UserId { get; set; }

        public OffsetDateTime Start { get; set; }

        public OffsetDateTime End { get; set; }

        public Duration Duration { get; set; }

        public string Description { get; set; }

        public bool IsExported { get; set; }

        public string ExternalUid { get; set; }

        public Activity Activity { get; set; }

        public void MapTaskId(User user)
        {
            if (user.TaskIdMap == null) return;

            if (user.TaskIdMap.ContainsKey(TaskId))
            {
                TaskId = user.TaskIdMap[TaskId];
            }
        }

        public TimeEntry Clone()
        {
            return new TimeEntry
            {
                TaskId = TaskId,
                UserId = UserId,
                Start = Start,
                End = End,
                Duration = Duration,
                Description = Description,
                IsExported = IsExported,
                ExternalUid = ExternalUid,
                Activity = Activity,
            };
        }
    }
}