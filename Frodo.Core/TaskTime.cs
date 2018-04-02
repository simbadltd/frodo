using System;
using NodaTime;

namespace Frodo.Core
{
    public sealed class TaskTime
    {
        public TaskTimeType Type { get; set; }

        public double Value { get; set; }

        public Duration ApplyToDuration(Duration src)
        {
            switch (Type)
            {
                case TaskTimeType.Minutes:
                    return Duration.FromMinutes(Value);

                case TaskTimeType.Percentage:
                    return Duration.FromSeconds(src.TotalSeconds * Value / 100D);

                default:
                    throw new InvalidOperationException("Invalid Type");
            }
        }
    }
}