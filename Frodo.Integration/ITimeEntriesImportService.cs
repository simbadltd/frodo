using System;
using System.Collections.Generic;
using Frodo.Core;
using NodaTime;

namespace Frodo.Integration
{
    public interface ITimeEntriesImportService
    {
        ICollection<TimeEntry> Import(User user, OffsetDateTime start, OffsetDateTime end);
    }
}