using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Planning;

namespace HomeBudget.Application.Planning.ChangeExpenseCategoryAllocationFlexibility;

/// <summary>
/// Handles commands that change expense category allocation flexibility.
/// </summary>
public sealed class ChangeExpenseCategoryAllocationFlexibilityCommandHandler
    : ICommandHandler<ChangeExpenseCategoryAllocationFlexibilityCommand>
{
    private readonly IBudgetPlanRepository _budgetPlanRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeExpenseCategoryAllocationFlexibilityCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetPlanRepository">The budget plan repository.</param>
    public ChangeExpenseCategoryAllocationFlexibilityCommandHandler(IBudgetPlanRepository budgetPlanRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetPlanRepository);

        _budgetPlanRepository = budgetPlanRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        ChangeExpenseCategoryAllocationFlexibilityCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budgetPlan = await _budgetPlanRepository.GetRequiredByIdAsync(
            command.BudgetPlanId,
            command.OwnerId,
            cancellationToken);
        var flexibility = PlanningCommandParsers.ParseCategoryAllocationFlexibility(
            command.Flexibility,
            nameof(command.Flexibility));

        budgetPlan.ChangeExpenseCategoryAllocationFlexibility(
            new CategoryAllocationId(command.CategoryAllocationId),
            flexibility);

        await _budgetPlanRepository.UpdateAsync(budgetPlan, cancellationToken);
    }
}
