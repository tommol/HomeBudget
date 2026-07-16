namespace HomeBudget.Domain.Shared;

/// <summary>
/// Defines whether a budget category is used for income, expenses, or savings.
/// </summary>
public enum BudgetCategoryType
{
    /// <summary>
    /// Indicates that the budget category is used for income.
    /// </summary>
    Income = 0,

    /// <summary>
    /// Indicates that the budget category is used for expenses.
    /// </summary>
    Expense = 1,

    /// <summary>
    /// Indicates that the budget category is used for savings.
    /// </summary>
    Saving = 2
}
