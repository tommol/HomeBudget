using HomeBudget.Application.Abstractions;
using HomeBudget.Application.Planning;
using HomeBudget.Domain.Execution;

namespace HomeBudget.Application.Execution.ChangeSavingCategory;

/// <summary>
/// Handles commands that change saving categories.
/// </summary>
public sealed class ChangeSavingCategoryCommandHandler : ICommandHandler<ChangeSavingCategoryCommand>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IBudgetCategoryRepository _budgetCategoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeSavingCategoryCommandHandler"/> class.
    /// </summary>
    /// <param name="budgetRepository">The budget repository.</param>
    /// <param name="budgetCategoryRepository">The budget category repository.</param>
    public ChangeSavingCategoryCommandHandler(
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
        ChangeSavingCategoryCommand command,
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

        budget.ChangeSavingCategory(new SavingId(command.SavingId), category);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
    }
}
