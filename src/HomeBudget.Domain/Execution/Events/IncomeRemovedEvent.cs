using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when an income entry is soft-removed from a budget.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="IncomeId">The identifier of the removed income.</param>
/// <param name="CategoryId">The identifier of the income category.</param>
/// <param name="Title">The income title.</param>
/// <param name="Amount">The original income amount.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when applicable.</param>
/// <param name="ConversionDate">The date of the currency conversion, when applicable.</param>
/// <param name="OccurredDate">The date when the income occurred.</param>
/// <param name="RemovalReason">The reason why the income was removed.</param>
/// <param name="RemovedOnUtc">The date and time when the income was removed in UTC.</param>
public sealed record IncomeRemovedEvent(
    BudgetId BudgetId,
    IncomeId IncomeId,
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
