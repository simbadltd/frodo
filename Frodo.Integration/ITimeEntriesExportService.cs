using System;
using System.Collections.Generic;
using Frodo.Core;

namespace Frodo.Integration
{
    public interface ITimeEntriesExportService
    {
        bool Export(User user, TimeEntry timeEntry);
    }
}