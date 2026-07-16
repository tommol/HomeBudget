namespace HomeBudget.Application.Abstractions;

/// <summary>
/// Represents an application command that does not return a value.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Represents an application command that returns a value.
/// </summary>
/// <typeparam name="TResult">The type of the command result.</typeparam>
public interface ICommand<out TResult> : ICommand
{
}
