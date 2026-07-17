namespace HomeBudget.Contracts.Planning;

/// <summary>
/// Represents a request to copy a budget plan to another period.
/// </summary>
/// <param name="Year">The target budget period year.</param>
/// <param name="Month">The target budget period month.</param>
/// <param name="CopyPlannedIncomes">A value indicating whether planned incomes should be copied.</param>
/// <param name="CopyExpenseCategoryAllocations">A value indicating whether expense category allocations should be copied.</param>
/// <param name="CopySavingContributions">A value indicating whether saving contributions should be copied.</param>
public sealed record CopyBudgetPlanRequest(
    int Year,
    int Month,
    bool CopyPlannedIncomes = true,
    bool CopyExpenseCategoryAllocations = true,
    bool CopySavingContributions = true);
