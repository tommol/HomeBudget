using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Planning.ChangeSavingContributionAmount;

/// <summary>
/// Handles commands that change saving contribution amounts.
/// </summary>
public sealed class ChangeSavingContributionAmountCommandHandler
    : ICommandHandler<ChangeSavingContributionAmountCommand>
{
    private readonly IBudgetPlanRepository _budgetPlanRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeSavingContributionAmountCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetPlanRepository">The budget plan repository.</param>
    public ChangeSavingContributionAmountCommandHandler(IBudgetPlanRepository budgetPlanRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetPlanRepository);

        _budgetPlanRepository = budgetPlanRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        ChangeSavingContributionAmountCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budgetPlan = await _budgetPlanRepository.GetRequiredByIdAsync(
            command.BudgetPlanId,
            command.OwnerId,
            cancellationToken);

        budgetPlan.ChangeSavingContributionAmount(
            new SavingContributionId(command.SavingContributionId),
            new Money(command.Amount, budgetPlan.DefaultCurrency));

        await _budgetPlanRepository.UpdateAsync(budgetPlan, cancellationToken);
    }
}
