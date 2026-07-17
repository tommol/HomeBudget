using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Planning.AddExpenseCategoryAllocation;

/// <summary>
/// Handles commands that add expense category allocations to budget plans.
/// </summary>
public sealed class AddExpenseCategoryAllocationCommandHandler
    : ICommandHandler<AddExpenseCategoryAllocationCommand, Guid>
{
    private readonly IBudgetPlanRepository _budgetPlanRepository;
    private readonly IBudgetCategoryRepository _budgetCategoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddExpenseCategoryAllocationCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetPlanRepository">The budget plan repository.</param>
    /// <param name="budgetCategoryRepository">The budget category repository.</param>
    public AddExpenseCategoryAllocationCommandHandler(
        IBudgetPlanRepository budgetPlanRepository,
        IBudgetCategoryRepository budgetCategoryRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetPlanRepository);
        ArgumentNullException.ThrowIfNull(budgetCategoryRepository);

        _budgetPlanRepository = budgetPlanRepository;
        _budgetCategoryRepository = budgetCategoryRepository;
    }

    /// <inheritdoc />
    public async Task<Guid> HandleAsync(
        AddExpenseCategoryAllocationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budgetPlan = await _budgetPlanRepository.GetRequiredByIdAsync(
            command.BudgetPlanId,
            command.OwnerId,
            cancellationToken);
        var category = await _budgetCategoryRepository.GetRequiredByIdAsync(
            command.CategoryId,
            command.OwnerId,
            cancellationToken);
        var flexibility = PlanningCommandParsers.ParseCategoryAllocationFlexibility(
            command.Flexibility,
            nameof(command.Flexibility));

        var allocation = budgetPlan.AddExpenseCategoryAllocation(
            new CategoryAllocationId(Guid.NewGuid()),
            category,
            new Money(command.Amount, budgetPlan.DefaultCurrency),
            flexibility);

        await _budgetPlanRepository.UpdateAsync(budgetPlan, cancellationToken);

        return allocation.Id.Value;
    }
}
