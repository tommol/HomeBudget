namespace HomeBudget.Application.Abstractions;

/// <summary>
/// Represents an application query that returns a value.
/// </summary>
/// <typeparam name="TResult">The type of the query result.</typeparam>
public interface IQuery<out TResult>
{
}
