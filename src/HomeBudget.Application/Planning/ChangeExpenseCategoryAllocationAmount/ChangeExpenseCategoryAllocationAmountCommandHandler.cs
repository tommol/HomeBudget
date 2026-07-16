using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Planning.ChangeExpenseCategoryAllocationAmount;

/// <summary>
/// Handles commands that change expense category allocation amounts.
/// </summary>
public sealed class ChangeExpenseCategoryAllocationAmountCommandHandler
    : ICommandHandler<ChangeExpenseCategoryAllocationAmountCommand>
{
    private readonly IBudgetPlanRepository _budgetPlanRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeExpenseCategoryAllocationAmountCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetPlanRepository">The budget plan repository.</param>
    public ChangeExpenseCategoryAllocationAmountCommandHandler(IBudgetPlanRepository budgetPlanRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetPlanRepository);

        _budgetPlanRepository = budgetPlanRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        ChangeExpenseCategoryAllocationAmountCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budgetPlan = await _budgetPlanRepository.GetRequiredByIdAsync(command.BudgetPlanId, cancellationToken);

        budgetPlan.ChangeExpenseCategoryAllocationAmount(
            new CategoryAllocationId(command.CategoryAllocationId),
            new Money(command.Amount, budgetPlan.DefaultCurrency));

        await _budgetPlanRepository.UpdateAsync(budgetPlan, cancellationToken);
    }
}
