using Bookings.Application.Behaviors;
using FluentValidation;
using Mediator;

namespace Bookings.Application.Tests.Behaviors
{
    public sealed class ValidationBehaviorTests
    {
        private const string HandlerResult = "handled";

        private int _nextCalls;

        [Fact]
        public async Task Handle_WithValidMessage_CallsNextAndReturnsItsResult()
        {
            var behavior = CreateBehavior(new NameRequiredValidator());

            var result = await behavior.Handle(new TestCommand("Ana", 30), Next, CancellationToken.None);

            Assert.Equal(HandlerResult, result);
            Assert.Equal(1, _nextCalls);
        }

        [Fact]
        public async Task Handle_WithInvalidMessage_ThrowsValidationExceptionAndDoesNotCallNext()
        {
            var behavior = CreateBehavior(new NameRequiredValidator());

            var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
                await behavior.Handle(new TestCommand("", 30), Next, CancellationToken.None));

            Assert.Equal(nameof(TestCommand.Name), Assert.Single(exception.Errors).PropertyName);
            Assert.Equal(0, _nextCalls);
        }

        [Fact]
        public async Task Handle_WithSeveralFailingValidators_ReportsErrorsFromAllOfThem()
        {
            var behavior = CreateBehavior(new NameRequiredValidator(), new AdultAgeValidator());

            var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
                await behavior.Handle(new TestCommand("", 12), Next, CancellationToken.None));

            Assert.Equal(
                [nameof(TestCommand.Age), nameof(TestCommand.Name)],
                exception.Errors.Select(error => error.PropertyName).Order());
            Assert.Equal(0, _nextCalls);
        }

        [Fact]
        public async Task Handle_WithoutValidators_CallsNext()
        {
            var behavior = CreateBehavior();

            var result = await behavior.Handle(new TestCommand("", 0), Next, CancellationToken.None);

            Assert.Equal(HandlerResult, result);
            Assert.Equal(1, _nextCalls);
        }

        private static ValidationBehavior<TestCommand, string> CreateBehavior(params IValidator<TestCommand>[] validators) =>
            new(validators);

        private ValueTask<string> Next(TestCommand message, CancellationToken cancellationToken)
        {
            _nextCalls++;
            return ValueTask.FromResult(HandlerResult);
        }

        public sealed record TestCommand(string Name, int Age) : ICommand<string>;

        private sealed class NameRequiredValidator : AbstractValidator<TestCommand>
        {
            public NameRequiredValidator() => RuleFor(command => command.Name).NotEmpty();
        }

        private sealed class AdultAgeValidator : AbstractValidator<TestCommand>
        {
            public AdultAgeValidator() => RuleFor(command => command.Age).GreaterThanOrEqualTo(18);
        }
    }
}
