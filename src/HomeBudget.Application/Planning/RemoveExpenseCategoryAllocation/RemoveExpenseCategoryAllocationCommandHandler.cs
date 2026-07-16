using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Planning;

namespace HomeBudget.Application.Planning.RemoveExpenseCategoryAllocation;

/// <summary>
/// Handles commands that remove expense category allocations from budget plans.
/// </summary>
public sealed class RemoveExpenseCategoryAllocationCommandHandler
    : ICommandHandler<RemoveExpenseCategoryAllocationCommand>
{
    private readonly IBudgetPlanRepository _budgetPlanRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveExpenseCategoryAllocationCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetPlanRepository">The budget plan repository.</param>
    public RemoveExpenseCategoryAllocationCommandHandler(IBudgetPlanRepository budgetPlanRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetPlanRepository);

        _budgetPlanRepository = budgetPlanRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        RemoveExpenseCategoryAllocationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budgetPlan = await _budgetPlanRepository.GetRequiredByIdAsync(command.BudgetPlanId, cancellationToken);

        budgetPlan.RemoveExpenseCategoryAllocation(new CategoryAllocationId(command.CategoryAllocationId));

        await _budgetPlanRepository.UpdateAsync(budgetPlan, cancellationToken);
    }
}
