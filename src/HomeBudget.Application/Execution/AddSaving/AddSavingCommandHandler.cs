using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.AddSaving;

/// <summary>
/// Handles commands that add savings to executed budgets.
/// </summary>
public sealed class AddSavingCommandHandler : ICommandHandler<AddSavingCommand, Guid>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IBudgetCategoryRepository _budgetCategoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddSavingCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    /// <param name="budgetCategoryRepository">The budget category repository.</param>
    public AddSavingCommandHandler(
        IBudgetRepository budgetRepository,
        IBudgetCategoryRepository budgetCategoryRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);
        ArgumentNullException.ThrowIfNull(budgetCategoryRepository);

        _budgetRepository = budgetRepository;
        _budgetCategoryRepository = budgetCategoryRepository;
    }

    /// <inheritdoc />
    public async Task<Guid> HandleAsync(
        AddSavingCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var budget = await _budgetRepository.GetRequiredByIdAsync(
            command.BudgetId,
            command.OwnerId,
            cancellationToken);
        var category = await _budgetCategoryRepository.GetRequiredByIdAsync(
            command.CategoryId,
            command.OwnerId,
            cancellationToken);
        var amount = ExecutionCommandAmounts.CreateAmount(command.Amount, command.CurrencyCode);
        var convertedAmount = ExecutionCommandAmounts.CreateConvertedAmount(command.ConvertedAmount, budget.DefaultCurrency);

        var saving = budget.AddSaving(
            new SavingId(Guid.NewGuid()),
            category,
            command.Title,
            amount,
            command.OccurredDate,
            convertedAmount,
            command.ConversionDate);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);

        return saving.Id.Value;
    }
}
