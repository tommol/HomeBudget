using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.ChangeSavingAmount;

/// <summary>
/// Handles commands that change saving amounts.
/// </summary>
public sealed class ChangeSavingAmountCommandHandler : ICommandHandler<ChangeSavingAmountCommand>
{
    private readonly IBudgetRepository _budgetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeSavingAmountCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    public ChangeSavingAmountCommandHandler(IBudgetRepository budgetRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);

        _budgetRepository = budgetRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        ChangeSavingAmountCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budget = await _budgetRepository.GetRequiredByIdAsync(
            command.BudgetId,
            command.OwnerId,
            cancellationToken);
        var amount = ExecutionCommandAmounts.CreateAmount(command.Amount, command.CurrencyCode);
        var convertedAmount = ExecutionCommandAmounts.CreateConvertedAmount(command.ConvertedAmount, budget.DefaultCurrency);

        budget.ChangeSavingAmount(
            new SavingId(command.SavingId),
            amount,
            convertedAmount,
            command.ConversionDate);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
