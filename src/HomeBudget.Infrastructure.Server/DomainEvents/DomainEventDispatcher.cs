using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Kernel;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace HomeBudget.Infrastructure.Server.DomainEvents;

internal sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);
        var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))
            ?? throw new InvalidOperationException($"Handler type {handlerType.Name} does not define HandleAsync.");

        foreach (var handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            Task? task;

            try
            {
                task = (Task?)handleMethod.Invoke(handler, [domainEvent, cancellationToken]);
            }
            catch (TargetInvocationException exception) when (exception.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }

            if (task is null)
            {
                throw new InvalidOperationException($"Handler {handler.GetType().Name} returned null.");
            }

            await task.ConfigureAwait(false);
        }
    }
}
