using HomeBudget.Application.Abstractions;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.ChangeExpenseAmount;

/// <summary>
/// Handles commands that change expense amounts.
/// </summary>
public sealed class ChangeExpenseAmountCommandHandler : ICommandHandler<ChangeExpenseAmountCommand>
{
    private readonly IBudgetRepository _budgetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeExpenseAmountCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    public ChangeExpenseAmountCommandHandler(IBudgetRepository budgetRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);

        _budgetRepository = budgetRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        ChangeExpenseAmountCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budget = await _budgetRepository.GetRequiredByIdAsync(
            command.BudgetId,
            command.OwnerId,
            cancellationToken);
        var amount = ExecutionCommandAmounts.CreateAmount(command.Amount, command.CurrencyCode);
        var convertedAmount = ExecutionCommandAmounts.CreateConvertedAmount(command.ConvertedAmount, budget.DefaultCurrency);

        budget.ChangeExpenseAmount(
            new ExpenseId(command.ExpenseId),
            amount,
            convertedAmount,
            command.ConversionDate);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
