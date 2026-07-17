using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when a saving entry is soft-removed from a budget.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="SavingId">The identifier of the removed saving.</param>
/// <param name="CategoryId">The identifier of the saving category.</param>
/// <param name="Title">The saving title.</param>
/// <param name="Amount">The original saving amount.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when applicable.</param>
/// <param name="ConversionDate">The date of the currency conversion, when applicable.</param>
/// <param name="OccurredDate">The date when the saving occurred.</param>
/// <param name="RemovalReason">The reason why the saving was removed.</param>
/// <param name="RemovedOnUtc">The date and time when the saving was removed in UTC.</param>
public sealed record SavingRemovedEvent(
    BudgetId BudgetId,
    SavingId SavingId,
    BudgetCategoryId CategoryId,
    string Title,
    Money Amount,
    Money? ConvertedAmount,
    DateOnly? ConversionDate,
    DateOnly OccurredDate,
    string RemovalReason,
    DateTimeOffset RemovedOnUtc) : IDomainEvent
{
    /// <inheritdoc />
    public DateTimeOffset OccurredOnUtc => RemovedOnUtc;
}
