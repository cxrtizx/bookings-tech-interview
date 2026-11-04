namespace Bookings.Domain.Abstractions
{
    public abstract class AggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        public IReadOnlyCollection<IDomainEvent> DomainEvents
        {
            get { return _domainEvents; }
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        protected void Raise(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
    }
}
