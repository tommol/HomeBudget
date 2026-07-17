using HomeBudget.Application.Abstractions;

namespace HomeBudget.Application.Execution.CloseBudget;

/// <summary>
/// Handles commands that close executed budgets.
/// </summary>
public sealed class CloseBudgetCommandHandler : ICommandHandler<CloseBudgetCommand>
{
    private readonly IBudgetRepository _budgetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloseBudgetCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    public CloseBudgetCommandHandler(IBudgetRepository budgetRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);

        _budgetRepository = budgetRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        CloseBudgetCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budget = await _budgetRepository.GetRequiredByIdAsync(
            command.BudgetId,
            command.OwnerId,
            cancellationToken);

        budget.Close();

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
