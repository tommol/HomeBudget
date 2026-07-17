using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Planning.AddSavingContribution;

/// <summary>
/// Handles commands that add saving contributions to budget plans.
/// </summary>
public sealed class AddSavingContributionCommandHandler
    : ICommandHandler<AddSavingContributionCommand, Guid>
{
    private readonly IBudgetPlanRepository _budgetPlanRepository;
    private readonly IBudgetCategoryRepository _budgetCategoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddSavingContributionCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetPlanRepository">The budget plan repository.</param>
    /// <param name="budgetCategoryRepository">The budget category repository.</param>
    public AddSavingContributionCommandHandler(
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
        AddSavingContributionCommand command,
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

        var contribution = budgetPlan.AddSavingContribution(
            new SavingContributionId(Guid.NewGuid()),
            category,
            new Money(command.Amount, budgetPlan.DefaultCurrency));

        await _budgetPlanRepository.UpdateAsync(budgetPlan, cancellationToken);

        return contribution.Id.Value;
    }
}
