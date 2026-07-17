using HomeBudget.Domain.Kernel;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Domain.Execution;

/// <summary>
/// Represents the event raised when an income title changes.
/// </summary>
/// <param name="BudgetId">The identifier of the budget.</param>
/// <param name="IncomeId">The identifier of the income.</param>
/// <param name="CategoryId">The identifier of the income category.</param>
/// <param name="PreviousTitle">The previous income title.</param>
/// <param name="NewTitle">The new income title.</param>
/// <param name="OccurredOnUtc">The date and time when the event occurred in UTC.</param>
public sealed record IncomeTitleChangedEvent(
    BudgetId BudgetId,
    IncomeId IncomeId,
    BudgetCategoryId CategoryId,
    string PreviousTitle,
    string NewTitle,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;
