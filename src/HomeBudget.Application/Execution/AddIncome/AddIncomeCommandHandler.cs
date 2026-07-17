using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.AddIncome;

/// <summary>
/// Handles commands that add income to executed budgets.
/// </summary>
public sealed class AddIncomeCommandHandler : ICommandHandler<AddIncomeCommand, Guid>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IBudgetCategoryRepository _budgetCategoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddIncomeCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    /// <param name="budgetCategoryRepository">The budget category repository.</param>
    public AddIncomeCommandHandler(
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
        AddIncomeCommand command,
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

        var income = budget.AddIncome(
            new IncomeId(Guid.NewGuid()),
            category,
            command.Title,
            amount,
            command.OccurredDate,
            convertedAmount,
            command.ConversionDate);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);

        return income.Id.Value;
    }
}
