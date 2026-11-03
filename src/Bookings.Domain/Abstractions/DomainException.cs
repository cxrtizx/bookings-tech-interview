namespace Bookings.Domain.Abstractions
{
    public sealed class DomainException : Exception
    {
        public string Code { get; }

        public DomainException(DomainError domainError) : base(domainError.Message) => Code = domainError.Code;
    }
}
