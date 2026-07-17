using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.RemoveSaving;

/// <summary>
/// Handles commands that remove savings from executed budgets.
/// </summary>
public sealed class RemoveSavingCommandHandler : ICommandHandler<RemoveSavingCommand>
{
    private readonly IBudgetRepository _budgetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveSavingCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    public RemoveSavingCommandHandler(IBudgetRepository budgetRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);

        _budgetRepository = budgetRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        RemoveSavingCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budget = await _budgetRepository.GetRequiredByIdAsync(
            command.BudgetId,
            command.OwnerId,
            cancellationToken);

        budget.RemoveSaving(new SavingId(command.SavingId), command.RemovalReason);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
