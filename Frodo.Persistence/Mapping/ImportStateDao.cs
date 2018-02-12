using System;
using Frodo.Core;
using NodaTime;

namespace Frodo.Persistence.Mapping
{
    public class ImportStateDao : Dao, IEntityProjection<ImportState>
    {
        public Guid UserId { get; set; }

        public OffsetDateTime? LastImportedDate { get; set; }
    }
}