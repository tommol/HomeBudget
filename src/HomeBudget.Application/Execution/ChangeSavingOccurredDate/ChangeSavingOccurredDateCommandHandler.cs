using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.ChangeSavingOccurredDate;

/// <summary>
/// Handles commands that change saving occurred dates.
/// </summary>
public sealed class ChangeSavingOccurredDateCommandHandler : ICommandHandler<ChangeSavingOccurredDateCommand>
{
    private readonly IBudgetRepository _budgetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeSavingOccurredDateCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    public ChangeSavingOccurredDateCommandHandler(IBudgetRepository budgetRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);

        _budgetRepository = budgetRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        ChangeSavingOccurredDateCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budget = await _budgetRepository.GetRequiredByIdAsync(
            command.BudgetId,
            command.OwnerId,
            cancellationToken);

        budget.ChangeSavingOccurredDate(new SavingId(command.SavingId), command.OccurredDate);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
