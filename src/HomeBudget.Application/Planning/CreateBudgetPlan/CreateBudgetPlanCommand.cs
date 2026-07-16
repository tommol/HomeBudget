using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.CreateBudgetPlan;

/// <summary>
/// Represents a command that creates a budget plan.
/// </summary>
/// <param name="OwnerId">The identifier of the budget plan owner.</param>
/// <param name="Year">The budget period year.</param>
/// <param name="Month">The budget period month.</param>
/// <param name="DefaultCurrencyCode">The default currency code of the budget plan.</param>
public sealed record CreateBudgetPlanCommand(
    Guid OwnerId,
    int Year,
    int Month,
    string DefaultCurrencyCode) : ICommand<Guid>;
