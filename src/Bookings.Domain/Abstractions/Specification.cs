using System.Linq.Expressions;

namespace Bookings.Domain.Abstractions
{
    public abstract class Specification<T>
    {
        private Func<T, bool>? _compiled;

        public abstract Expression<Func<T, bool>> ToExpression();

        public bool IsSatisfiedBy(T candidate) =>
            (_compiled ??= ToExpression().Compile())(candidate);
    }
}
