using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when a saving amount changes.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="SavingId">The identifier of the saving.</param>
/// <param name="CategoryId">The identifier of the saving category.</param>
/// <param name="PreviousAmount">The previous saving amount.</param>
/// <param name="NewAmount">The new saving amount.</param>
/// <param name="PreviousConvertedAmount">The previous amount converted to the budget default currency, when applicable.</param>
/// <param name="NewConvertedAmount">The new amount converted to the budget default currency, when applicable.</param>
/// <param name="PreviousConversionDate">The previous conversion date, when applicable.</param>
/// <param name="NewConversionDate">The new conversion date, when applicable.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record SavingAmountChangedEvent(
    BudgetId BudgetId,
    SavingId SavingId,
    BudgetCategoryId CategoryId,
    Money PreviousAmount,
    Money NewAmount,
    Money? PreviousConvertedAmount,
    Money? NewConvertedAmount,
    DateOnly? PreviousConversionDate,
    DateOnly? NewConversionDate,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
