using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.CloseBudgetPlan;

/// <summary>
/// Handles commands that close budget plans.
/// </summary>
public sealed class CloseBudgetPlanCommandHandler : ICommandHandler<CloseBudgetPlanCommand>
{
    private readonly IBudgetPlanRepository _budgetPlanRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloseBudgetPlanCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetPlanRepository">The budget plan repository.</param>
    public CloseBudgetPlanCommandHandler(IBudgetPlanRepository budgetPlanRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetPlanRepository);

        _budgetPlanRepository = budgetPlanRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        CloseBudgetPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budgetPlan = await _budgetPlanRepository.GetRequiredByIdAsync(
            command.BudgetPlanId,
            command.OwnerId,
            cancellationToken);

        budgetPlan.Close();

        await _budgetPlanRepository.UpdateAsync(budgetPlan, cancellationToken);
    }
}
