using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.ChangeIncomeAmount;

/// <summary>
/// Handles commands that change income amounts.
/// </summary>
public sealed class ChangeIncomeAmountCommandHandler : ICommandHandler<ChangeIncomeAmountCommand>
{
    private readonly IBudgetRepository _budgetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeIncomeAmountCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    public ChangeIncomeAmountCommandHandler(IBudgetRepository budgetRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);

        _budgetRepository = budgetRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        ChangeIncomeAmountCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budget = await _budgetRepository.GetRequiredByIdAsync(
            command.BudgetId,
            command.OwnerId,
            cancellationToken);
        var amount = ExecutionCommandAmounts.CreateAmount(command.Amount, command.CurrencyCode);
        var convertedAmount = ExecutionCommandAmounts.CreateConvertedAmount(command.ConvertedAmount, budget.DefaultCurrency);

        budget.ChangeIncomeAmount(
            new IncomeId(command.IncomeId),
            amount,
            convertedAmount,
            command.ConversionDate);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
