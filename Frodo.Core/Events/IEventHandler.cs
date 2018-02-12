namespace Frodo.Core.Events
{
    public interface IEventHandler<in T> where T : IDomainEvent
    {
        void Handle(T domainEvent);
    }
}