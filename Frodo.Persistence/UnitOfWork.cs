using System;
using System.Collections.Generic;
using System.Linq;
using Frodo.Core.Events;
using Frodo.Core.Repositories;
using Frodo.Persistence.Mapping;

namespace Frodo.Persistence
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly ICollection<UowAction> _actions = new List<UowAction>();

        private readonly object _syncRoot = new object();

        private readonly IStorageAdapter _storageAdapter;
        
        private List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        
        private readonly Lazy<IDomainEventDispatcher> _dispatcher;

        public UnitOfWork(IStorageAdapter storageAdapter, Lazy<IDomainEventDispatcher> dispatcher)
        {
            _storageAdapter = storageAdapter;
            _dispatcher = dispatcher;
        }

        public void Commit()
        {
            lock (_syncRoot)
            {
                var events = _domainEvents;
                _domainEvents = new List<IDomainEvent>();

                foreach (var @event in events)
                {
                    _dispatcher.Value.Dispatch(@event);
                }                
                
                ApplyActions();
            }
        }
        
        public void TrackEvents(ICollection<IDomainEvent> events)
        {
            lock (_syncRoot)
            {
                _domainEvents.AddRange(events);
            }
        }        

        private void ApplyActions()
        {
            lock (_syncRoot)
            {
                var actionsCopy = _actions.OrderBy(x => x.ActionType).ToList();
                _actions.Clear();

                _storageAdapter.Transaction(
                    tw =>
                    {
                        foreach (var uowAction in actionsCopy)
                        {
                            ApplyAction(uowAction, tw);
                        }
                    });
            }
        }

        private static void ApplyAction(UowAction action, ITransactionWrapper transaction)
        {
            switch (action.ActionType)
            {
                case UowActionType.Save:
                    transaction.Save(action.Model, action.EntityType);
                    break;
                case UowActionType.DeleteAll:
                    transaction.DeleteAll(action.EntityType);
                    break;
                case UowActionType.Delete:
                    transaction.Delete(action.Id, action.EntityType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Save(object model, Type type)
        {
            // todo [kk]: check that type is derived from PersistenceModel

            lock (_syncRoot)
            {
                _actions.Add(new UowAction
                {
                    ActionType = UowActionType.Save,
                    Model = model as Dao,
                    EntityType = type,
                });
            }
        }

        public void Delete(Guid id, Type type)
        {
            lock (_syncRoot)
            {
                _actions.Add(new UowAction
                {
                    ActionType = UowActionType.Delete,
                    EntityType = type,
                    Id = id,
                });
            }
        }

        public void DeleteAll(Type type)
        {
            lock (_syncRoot)
            {
                _actions.Add(new UowAction
                {
                    ActionType = UowActionType.DeleteAll,
                    EntityType = type,
                });
            }
        }

        private enum UowActionType
        {
            Save = 0,
            DeleteAll = 1,
            Delete = 2,
        }

        private sealed class UowAction
        {
            public Dao Model { get; set; }

            public UowActionType ActionType { get; set; }

            public Type EntityType { get; set; }

            public Guid Id { get; set; }
        }
    }
}