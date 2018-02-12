using System;
using System.Collections.Generic;
using Frodo.Core.Events;

namespace Frodo.Core.Repositories
{
    public interface IUnitOfWork
    {
        void Commit();
        
        void TrackEvents(ICollection<IDomainEvent> events);
        
        void Save(object model, Type type);
        
        void Delete(Guid id, Type type);
        
        void DeleteAll(Type type);
    }
}