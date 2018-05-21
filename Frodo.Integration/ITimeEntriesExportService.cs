using System;
using System.Collections.Generic;
using Frodo.Core;

namespace Frodo.Integration
{
    public interface ITimeEntriesExportService
    {
        void Export(User user, TimeEntry timeEntry);
    }
}