using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Planning;

namespace HomeBudget.Application.Planning.RemoveSavingContribution;

/// <summary>
/// Handles commands that remove saving contributions from budget plans.
/// </summary>
public sealed class RemoveSavingContributionCommandHandler : ICommandHandler<RemoveSavingContributionCommand>
{
    private readonly IBudgetPlanRepository _budgetPlanRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveSavingContributionCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetPlanRepository">The budget plan repository.</param>
    public RemoveSavingContributionCommandHandler(IBudgetPlanRepository budgetPlanRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetPlanRepository);

        _budgetPlanRepository = budgetPlanRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        RemoveSavingContributionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budgetPlan = await _budgetPlanRepository.GetRequiredByIdAsync(
            command.BudgetPlanId,
            command.OwnerId,
            cancellationToken);

        budgetPlan.RemoveSavingContribution(new SavingContributionId(command.SavingContributionId));

        await _budgetPlanRepository.UpdateAsync(budgetPlan, cancellationToken);
    }
}
