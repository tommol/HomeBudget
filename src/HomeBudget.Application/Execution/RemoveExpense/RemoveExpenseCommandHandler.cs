using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.RemoveExpense;

/// <summary>
/// Handles commands that remove expenses from executed budgets.
/// </summary>
public sealed class RemoveExpenseCommandHandler : ICommandHandler<RemoveExpenseCommand>
{
    private readonly IBudgetRepository _budgetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveExpenseCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    public RemoveExpenseCommandHandler(IBudgetRepository budgetRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);

        _budgetRepository = budgetRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        RemoveExpenseCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budget = await _budgetRepository.GetRequiredByIdAsync(
            command.BudgetId,
            command.OwnerId,
            cancellationToken);

        budget.RemoveExpense(new ExpenseId(command.ExpenseId), command.RemovalReason);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
