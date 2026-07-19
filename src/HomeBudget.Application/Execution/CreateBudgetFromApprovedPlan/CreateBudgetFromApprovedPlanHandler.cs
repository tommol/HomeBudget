using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning;
using HomeBudget.Domain.Execution;
using HomeBudget.Domain.Planning;

namespace HomeBudget.Application.Execution.CreateBudgetFromApprovedPlan;

/// <summary>
/// Creates an executed budget when a budget plan is approved.
/// </summary>
public sealed class CreateBudgetFromApprovedPlanHandler
    : IDomainEventHandler<BudgetPlanStatusChangedEvent>
{
    private readonly IBudgetPlanRepository _budgetPlanRepository;
    private readonly IBudgetRepository _budgetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBudgetFromApprovedPlanHandler"/> class.
    /// </summary>
    /// <param name="budgetPlanRepository">The budget plan repository.</param>
    /// <param name="budgetRepository">The executed budget repository.</param>
    public CreateBudgetFromApprovedPlanHandler(
        IBudgetPlanRepository budgetPlanRepository,
        IBudgetRepository budgetRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetPlanRepository);
        ArgumentNullException.ThrowIfNull(budgetRepository);

        _budgetPlanRepository = budgetPlanRepository;
        _budgetRepository = budgetRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        BudgetPlanStatusChangedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        if (domainEvent.NewStatus != BudgetPlanStatus.Active)
        {
            return;
        }

        var budgetId = new BudgetId(domainEvent.BudgetPlanId.Value);

        if (await _budgetRepository.GetByIdAsync(budgetId, cancellationToken) is not null)
        {
            return;
        }

        var budgetPlan = await _budgetPlanRepository.GetRequiredByIdAsync(
            domainEvent.BudgetPlanId.Value,
            cancellationToken);

        var budget = new Budget(
            budgetId,
            budgetPlan.OwnerId,
            budgetPlan.Period,
            budgetPlan.DefaultCurrency,
            budgetPlan.Id);

        await _budgetRepository.AddAsync(budget, cancellationToken);
    }
}
