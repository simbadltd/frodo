namespace Frodo.Core.Events
{
    public sealed class NewUserCreatedEvent : IDomainEvent
    {
        public User User { get; }

        public NewUserCreatedEvent(User user)
        {
            User = user;
        }

        public override string ToString()
        {
            return $"New user [{User.Login}@{User.Email}] created";
        }
    }
}