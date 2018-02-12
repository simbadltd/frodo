using System;

namespace Frodo.Core
{
    public sealed class EntityConsistencyException: Exception
    {
        public EntityConsistencyException(Entity entity, string because = "no description"):base(string.Format("Entity {1} consistency is broken : {0}", because, entity))
        {
        }
    }
}