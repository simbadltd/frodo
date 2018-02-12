using System;
using System.Collections.Generic;

namespace Frodo.Core.Events
{
    internal interface IDomainEventsSource
    {
        Guid Id { get; }

        ICollection<IDomainEvent> ExtractEvents();
    }
}