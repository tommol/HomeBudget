using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when a saving is added to a budget.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="SavingId">The identifier of the added saving.</param>
/// <param name="CategoryId">The identifier of the saving category.</param>
/// <param name="Title">The saving title.</param>
/// <param name="Amount">The original saving amount.</param>
/// <param name="ConvertedAmount">The amount converted to the budget default currency, when applicable.</param>
/// <param name="ConversionDate">The date of the currency conversion, when applicable.</param>
/// <param name="OccurredDate">The date when the saving occurred.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record SavingAddedEvent(
    BudgetId BudgetId,
    SavingId SavingId,
    BudgetCategoryId CategoryId,
    string Title,
    Money Amount,
    Money? ConvertedAmount,
    DateOnly? ConversionDate,
    DateOnly OccurredDate,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
