using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Frodo.Core.Events;
using Frodo.Infrastructure.Ioc;
using Frodo.Infrastructure.Logging;

namespace Frodo.Events
{
    public sealed class DomainEventDispatcher : IDomainEventDispatcher
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> CachedHandleMethods = new ConcurrentDictionary<Type, MethodInfo>();

        private readonly ILogger _logger;

        private readonly IIocContainer _iocContainer;

        public DomainEventDispatcher(ILogManager logManager, IIocContainer iocContainer)
        {
            _iocContainer = iocContainer;
            _logger = logManager.GetLogger();
        }

        public void Dispatch(IDomainEvent domainEvent)
        {
            var handlerInterfaceType = typeof(IEventHandler<>).MakeGenericType(domainEvent.GetType());
            var enumerableInterface = typeof(IEnumerable<>).MakeGenericType(handlerInterfaceType);

            var handlers = (IEnumerable)_iocContainer.Resolve(enumerableInterface);
            foreach (var handler in handlers)
            {
                var handlerType = handler.GetType();
                var handleMethod = CachedHandleMethods.GetOrAdd(handlerType, type => type.GetMethod(nameof(IEventHandler<IDomainEvent>.Handle)));
                handleMethod.Invoke(handler, new object[] { domainEvent });

                _logger.Trace(string.Concat("event::", domainEvent, " => ", handlerType.Name));
            }
        }
    }
}