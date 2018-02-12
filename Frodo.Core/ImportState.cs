using System;
using NodaTime;

namespace Frodo.Core
{
    public sealed class ImportState : Entity
    {
        public Guid UserId { get; set; }

        public OffsetDateTime LastImportedDate { get; set; }
    }
}