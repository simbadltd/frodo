using System;
using Frodo.Core;
using NodaTime;

namespace Frodo.WebApp.Models
{
    public sealed class TimeEntryDto
    {
        public string TaskId { get; set; }

        public string Duration { get; set; }

        public string Description { get; set; }

        public string Activity { get; set; }

        public static TimeEntryDto From(TimeEntry entity)
        {
            return new TimeEntryDto
            {
                TaskId = entity.TaskId,
                Activity = entity.Activity.ToString(),
                Description = entity.Description,
                Duration = Duration2String(entity.Duration)
            };
        }

        private static string Duration2String(Duration d)
        {
            if (d.TotalHours < 1)
            {
                return $"{Math.Round(d.TotalMinutes)}m";
            }

            var allMinutes = d.TotalMinutes;
            var hours = Math.Floor(allMinutes / 60);
            var minutes = Math.Round(allMinutes - hours * 60);

            return $"{hours}h {minutes}m";
        }
    }
}