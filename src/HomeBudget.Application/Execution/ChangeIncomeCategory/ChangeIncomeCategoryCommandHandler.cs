using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.ChangeIncomeCategory;

/// <summary>
/// Handles commands that change income categories.
/// </summary>
public sealed class ChangeIncomeCategoryCommandHandler : ICommandHandler<ChangeIncomeCategoryCommand>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IBudgetCategoryRepository _budgetCategoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeIncomeCategoryCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    /// <param name="budgetCategoryRepository">The budget category repository.</param>
    public ChangeIncomeCategoryCommandHandler(
        IBudgetRepository budgetRepository,
        IBudgetCategoryRepository budgetCategoryRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetRepository);
        ArgumentNullException.ThrowIfNull(budgetCategoryRepository);

        _budgetRepository = budgetRepository;
        _budgetCategoryRepository = budgetCategoryRepository;
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        ChangeIncomeCategoryCommand command,
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

        budget.ChangeIncomeCategory(new IncomeId(command.IncomeId), category);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
