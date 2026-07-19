using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;

namespace HomeBudget.Application.Planning.CreateBudgetPlan;

/// <summary>
/// Handles commands that create budget plans.
/// </summary>
public sealed class CreateBudgetPlanCommandHandler : ICommandHandler<CreateBudgetPlanCommand, Guid>
{
    private readonly IBudgetPlanRepository _budgetPlanRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBudgetPlanCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetPlanRepository">The budget plan repository.</param>
    public CreateBudgetPlanCommandHandler(IBudgetPlanRepository budgetPlanRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetPlanRepository);

        _budgetPlanRepository = budgetPlanRepository;
    }

    /// <inheritdoc />
    public async Task<Guid> HandleAsync(
        CreateBudgetPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budgetPlanId = new BudgetPlanId(Guid.NewGuid());
        var ownerId = new OwnerId(command.OwnerId);
        var period = new BudgetPeriod(command.Year, command.Month);
        var defaultCurrency = new Currency(command.DefaultCurrencyCode);

        if (await _budgetPlanRepository.ExistsByOwnerIdAndPeriodAsync(ownerId, period, cancellationToken))
        {
            throw new InvalidOperationException("Budget plan already exists for this owner and period.");
        }

        var budgetPlan = new BudgetPlan(
            budgetPlanId,
            ownerId,
            period,
            defaultCurrency);

        await _budgetPlanRepository.AddAsync(budgetPlan, cancellationToken);

        return budgetPlanId.Value;
    }
}
