using System;

namespace Frodo.Core
{
    public static class GlobalIdGenerator
    {
        public static Guid NewId()
        {
            return Guid.NewGuid();
        }
    }
}