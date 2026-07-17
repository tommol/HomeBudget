using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.ChangeExpenseTitle;

/// <summary>
/// Handles commands that change expense titles.
/// </summary>
public sealed class ChangeExpenseTitleCommandHandler : ICommandHandler<ChangeExpenseTitleCommand>
{
    private readonly IBudgetRepository _budgetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeExpenseTitleCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    public ChangeExpenseTitleCommandHandler(IBudgetRepository budgetRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);

        _budgetRepository = budgetRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        ChangeExpenseTitleCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budget = await _budgetRepository.GetRequiredByIdAsync(
            command.BudgetId,
            command.OwnerId,
            cancellationToken);

        budget.ChangeExpenseTitle(new ExpenseId(command.ExpenseId), command.Title);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
