using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Planning.AddPlannedIncome;

/// <summary>
/// Handles commands that add planned income to budget plans.
/// </summary>
public sealed class AddPlannedIncomeCommandHandler : ICommandHandler<AddPlannedIncomeCommand, Guid>
{
    private readonly IBudgetPlanRepository _budgetPlanRepository;
    private readonly IBudgetCategoryRepository _budgetCategoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddPlannedIncomeCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetPlanRepository">The budget plan repository.</param>
    /// <param name="budgetCategoryRepository">The budget category repository.</param>
    public AddPlannedIncomeCommandHandler(
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
        AddPlannedIncomeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budgetPlan = await _budgetPlanRepository.GetRequiredByIdAsync(command.BudgetPlanId, cancellationToken);
        var category = await _budgetCategoryRepository.GetRequiredByIdAsync(command.CategoryId, cancellationToken);
        var amount = new Money(command.Amount, new Currency(command.CurrencyCode));
        var convertedAmount = command.ConvertedAmount is null
            ? null
            : new Money(command.ConvertedAmount.Value, budgetPlan.DefaultCurrency);

        var plannedIncome = budgetPlan.AddPlannedIncome(
            new PlannedIncomeId(Guid.NewGuid()),
            category,
            command.Title,
            amount,
            command.ExpectedDate,
            convertedAmount,
            command.ConversionDate);

        await _budgetPlanRepository.UpdateAsync(budgetPlan, cancellationToken);

        return plannedIncome.Id.Value;
    }
}
