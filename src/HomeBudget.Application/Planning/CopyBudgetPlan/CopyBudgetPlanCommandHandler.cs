using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Planning.CopyBudgetPlan;

/// <summary>
/// Handles commands that copy budget plans to another period.
/// </summary>
public sealed class CopyBudgetPlanCommandHandler : ICommandHandler<CopyBudgetPlanCommand, Guid>
{
    private readonly IBudgetPlanRepository _budgetPlanRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyBudgetPlanCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetPlanRepository">The budget plan repository.</param>
    public CopyBudgetPlanCommandHandler(IBudgetPlanRepository budgetPlanRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetPlanRepository);

        _budgetPlanRepository = budgetPlanRepository;
    }

    /// <inheritdoc />
    public async Task<Guid> HandleAsync(
        CopyBudgetPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sourceBudgetPlan = await _budgetPlanRepository.GetRequiredByIdAsync(
            command.SourceBudgetPlanId,
            command.OwnerId,
            cancellationToken);

        var targetPeriod = new BudgetPeriod(command.Year, command.Month);
        var budgetPlan = sourceBudgetPlan.CopyTo(
            new BudgetPlanId(Guid.NewGuid()),
            targetPeriod,
            () => new PlannedIncomeId(Guid.NewGuid()),
            () => new CategoryAllocationId(Guid.NewGuid()),
            () => new SavingContributionId(Guid.NewGuid()),
            copyPlannedIncomes: command.CopyPlannedIncomes,
            copyExpenseCategoryAllocations: command.CopyExpenseCategoryAllocations,
            copySavingContributions: command.CopySavingContributions);

        await _budgetPlanRepository.AddAsync(budgetPlan, cancellationToken);

        return budgetPlan.Id.Value;
    }
}
