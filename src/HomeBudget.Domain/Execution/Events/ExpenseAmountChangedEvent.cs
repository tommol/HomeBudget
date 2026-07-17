using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when an expense amount changes.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="ExpenseId">The identifier of the expense.</param>
/// <param name="CategoryId">The identifier of the expense category.</param>
/// <param name="PreviousAmount">The previous expense amount.</param>
/// <param name="NewAmount">The new expense amount.</param>
/// <param name="PreviousConvertedAmount">The previous amount converted to the budget default currency, when applicable.</param>
/// <param name="NewConvertedAmount">The new amount converted to the budget default currency, when applicable.</param>
/// <param name="PreviousConversionDate">The previous conversion date, when applicable.</param>
/// <param name="NewConversionDate">The new conversion date, when applicable.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record ExpenseAmountChangedEvent(
    BudgetId BudgetId,
    ExpenseId ExpenseId,
    BudgetCategoryId CategoryId,
    Money PreviousAmount,
    Money NewAmount,
    Money? PreviousConvertedAmount,
    Money? NewConvertedAmount,
    DateOnly? PreviousConversionDate,
    DateOnly? NewConversionDate,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
