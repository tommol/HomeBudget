using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.ChangeExpenseOccurredDate;

/// <summary>
/// Handles commands that change expense occurred dates.
/// </summary>
public sealed class ChangeExpenseOccurredDateCommandHandler : ICommandHandler<ChangeExpenseOccurredDateCommand>
{
    private readonly IBudgetRepository _budgetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeExpenseOccurredDateCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    public ChangeExpenseOccurredDateCommandHandler(IBudgetRepository budgetRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);

        _budgetRepository = budgetRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        ChangeExpenseOccurredDateCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budget = await _budgetRepository.GetRequiredByIdAsync(
            command.BudgetId,
            command.OwnerId,
            cancellationToken);

        budget.ChangeExpenseOccurredDate(new ExpenseId(command.ExpenseId), command.OccurredDate);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
