using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Planning.ActivateBudgetPlan;

/// <summary>
/// Handles commands that activate budget plans.
/// </summary>
public sealed class ActivateBudgetPlanCommandHandler : ICommandHandler<ActivateBudgetPlanCommand>
{
    private readonly IBudgetPlanRepository _budgetPlanRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivateBudgetPlanCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetPlanRepository">The budget plan repository.</param>
    public ActivateBudgetPlanCommandHandler(IBudgetPlanRepository budgetPlanRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetPlanRepository);

        _budgetPlanRepository = budgetPlanRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        ActivateBudgetPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budgetPlan = await _budgetPlanRepository.GetRequiredByIdAsync(command.BudgetPlanId, cancellationToken);

        budgetPlan.Activate();

        await _budgetPlanRepository.UpdateAsync(budgetPlan, cancellationToken);
    }
}
