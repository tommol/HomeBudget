using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.ChangeIncomeOccurredDate;

/// <summary>
/// Handles commands that change income occurred dates.
/// </summary>
public sealed class ChangeIncomeOccurredDateCommandHandler : ICommandHandler<ChangeIncomeOccurredDateCommand>
{
    private readonly IBudgetRepository _budgetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeIncomeOccurredDateCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    public ChangeIncomeOccurredDateCommandHandler(IBudgetRepository budgetRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);

        _budgetRepository = budgetRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        ChangeIncomeOccurredDateCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budget = await _budgetRepository.GetRequiredByIdAsync(
            command.BudgetId,
            command.OwnerId,
            cancellationToken);

        budget.ChangeIncomeOccurredDate(new IncomeId(command.IncomeId), command.OccurredDate);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
