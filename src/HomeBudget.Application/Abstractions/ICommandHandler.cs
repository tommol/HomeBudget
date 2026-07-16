namespace HomeBudget.Application.Abstractions;

/// <summary>
/// Handles an application command that does not return a value.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the specified command.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handles an application command that returns a value.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResult">The type of the command result.</typeparam>
public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Handles the specified command.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The command result.</returns>
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
