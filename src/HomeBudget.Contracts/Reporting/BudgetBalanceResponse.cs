namespace HomeBudget.Contracts.Reporting;

/// <summary>
/// Represents a monthly plan-versus-actual budget balance for UI reads.
/// </summary>
public sealed record BudgetBalanceResponse(
    int Year,
    int Month,
    Guid BudgetPlanId,
    Guid? BudgetId,
    string CurrencyCode,
    string BudgetPlanStatus,
    string? BudgetStatus,
    decimal PlannedIncome,
    decimal ActualIncome,
    decimal IncomeDifference,
    decimal PlannedExpenses,
    decimal ActualExpenses,
    decimal ExpenseDifference,
    decimal PlannedSavings,
    decimal ActualSavings,
    decimal SavingsDifference,
    decimal PlannedResult,
    decimal ActualResult,
    decimal ResultDifference);
