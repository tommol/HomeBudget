using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.ChangeIncomeTitle;

/// <summary>
/// Handles commands that change income titles.
/// </summary>
public sealed class ChangeIncomeTitleCommandHandler : ICommandHandler<ChangeIncomeTitleCommand>
{
    private readonly IBudgetRepository _budgetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeIncomeTitleCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    public ChangeIncomeTitleCommandHandler(IBudgetRepository budgetRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);

        _budgetRepository = budgetRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        ChangeIncomeTitleCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budget = await _budgetRepository.GetRequiredByIdAsync(
            command.BudgetId,
            command.OwnerId,
            cancellationToken);

        budget.ChangeIncomeTitle(new IncomeId(command.IncomeId), command.Title);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
