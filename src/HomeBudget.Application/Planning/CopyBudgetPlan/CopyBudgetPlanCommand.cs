using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.CopyBudgetPlan;

/// <summary>
/// Represents a command that copies an existing budget plan to another period.
/// </summary>
/// <param name="OwnerId">The identifier of the source budget plan owner.</param>
/// <param name="SourceBudgetPlanId">The identifier of the budget plan to copy.</param>
/// <param name="Year">The target budget period year.</param>
/// <param name="Month">The target budget period month.</param>
/// <param name="CopyPlannedIncomes">A value indicating whether planned incomes should be copied.</param>
/// <param name="CopyExpenseCategoryAllocations">A value indicating whether expense category allocations should be copied.</param>
/// <param name="CopySavingContributions">A value indicating whether saving contributions should be copied.</param>
public sealed record CopyBudgetPlanCommand(
    Guid OwnerId,
    Guid SourceBudgetPlanId,
    int Year,
    int Month,
    bool CopyPlannedIncomes = true,
    bool CopyExpenseCategoryAllocations = true,
    bool CopySavingContributions = true) : ICommand<Guid>;
