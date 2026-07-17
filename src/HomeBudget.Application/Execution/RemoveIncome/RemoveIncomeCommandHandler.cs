using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.RemoveIncome;

/// <summary>
/// Handles commands that remove income from executed budgets.
/// </summary>
public sealed class RemoveIncomeCommandHandler : ICommandHandler<RemoveIncomeCommand>
{
    private readonly IBudgetRepository _budgetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveIncomeCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    public RemoveIncomeCommandHandler(IBudgetRepository budgetRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);

        _budgetRepository = budgetRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        RemoveIncomeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budget = await _budgetRepository.GetRequiredByIdAsync(
            command.BudgetId,
            command.OwnerId,
            cancellationToken);

        budget.RemoveIncome(new IncomeId(command.IncomeId), command.RemovalReason);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
