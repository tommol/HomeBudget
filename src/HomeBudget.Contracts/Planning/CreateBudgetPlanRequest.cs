namespace HomeBudget.Contracts.Planning;

/// <summary>
/// Represents a request to create a budget plan.
/// </summary>
/// <param name="Year">The budget period year.</param>
/// <param name="Month">The budget period month.</param>
/// <param name="DefaultCurrencyCode">The default currency code of the budget plan.</param>
public sealed record CreateBudgetPlanRequest(
    int Year,
    int Month,
    string DefaultCurrencyCode);
