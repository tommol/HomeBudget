using HomeBudget.Domain.Execution;
using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;
using HomeBudget.Infrastructure.Server.Persistence.Configurations.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeBudget.Infrastructure.Server.Persistence.Configurations.Execution;

internal sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets", DatabaseSchemas.Execution);

        builder.HasKey(budget => budget.Id);

        builder.Property(budget => budget.Id)
            .HasConversion(
                id => id.Value,
                value => new BudgetId(value))
            .ValueGeneratedNever();

        builder.Property(budget => budget.OwnerId)
            .HasConversion(
                id => id.Value,
                value => new OwnerId(value));

        builder.Property(budget => budget.SourceBudgetPlanId)
            .HasConversion(
                id => id.Value,
                value => new BudgetPlanId(value));

        builder.Property(budget => budget.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(budget => budget.DefaultCurrency)
            .HasConversion(
                currency => currency.Code,
                code => new Currency(code))
            .HasMaxLength(3);

        builder.Property<int>("_periodYear")
            .HasColumnName("PeriodYear");

        builder.Property<int>("_periodMonth")
            .HasColumnName("PeriodMonth");

        builder.Ignore(budget => budget.Period);

        builder.OwnsOne(budget => budget.TotalIncome, money => money.ConfigureMoney("TotalIncome"));
        builder.OwnsOne(budget => budget.TotalExpenses, money => money.ConfigureMoney("TotalExpenses"));
        builder.OwnsOne(budget => budget.TotalSavings, money => money.ConfigureMoney("TotalSavings"));
        builder.OwnsOne(budget => budget.ActualFinancialResult, money => money.ConfigureMoney("ActualFinancialResult"));

        builder.HasIndex(budget => budget.SourceBudgetPlanId)
            .IsUnique();

        builder.HasIndex("OwnerId", "_periodYear", "_periodMonth")
            .IsUnique();

        builder.OwnsMany(budget => budget.Incomes, incomes =>
        {
            incomes.ToTable("Incomes", DatabaseSchemas.Execution);

            incomes.Property<BudgetId>("BudgetId")
                .HasConversion(
                    id => id.Value,
                    value => new BudgetId(value));

            incomes.WithOwner()
                .HasForeignKey("BudgetId");

            incomes.HasKey(income => income.Id);

            incomes.Property(income => income.Id)
                .HasConversion(
                    id => id.Value,
                    value => new IncomeId(value))
                .ValueGeneratedNever();

            incomes.Property(income => income.CategoryId)
                .HasConversion(
                    id => id.Value,
                    value => new BudgetCategoryId(value));

            incomes.Property(income => income.Title)
                .HasMaxLength(100);

            incomes.Property(income => income.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            incomes.Property(income => income.RemovalReason)
                .HasMaxLength(300);

            incomes.OwnsOne(income => income.Amount, money => money.ConfigureMoney("Amount"));
            incomes.OwnsOne(income => income.ConvertedAmount, money => money.ConfigureMoney("ConvertedAmount"));
        });

        builder.OwnsMany(budget => budget.Expenses, expenses =>
        {
            expenses.ToTable("Expenses", DatabaseSchemas.Execution);

            expenses.Property<BudgetId>("BudgetId")
                .HasConversion(
                    id => id.Value,
                    value => new BudgetId(value));

            expenses.WithOwner()
                .HasForeignKey("BudgetId");

            expenses.HasKey(expense => expense.Id);

            expenses.Property(expense => expense.Id)
                .HasConversion(
                    id => id.Value,
                    value => new ExpenseId(value))
                .ValueGeneratedNever();

            expenses.Property(expense => expense.CategoryId)
                .HasConversion(
                    id => id.Value,
                    value => new BudgetCategoryId(value));

            expenses.Property(expense => expense.Title)
                .HasMaxLength(100);

            expenses.Property(expense => expense.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            expenses.Property(expense => expense.RemovalReason)
                .HasMaxLength(300);

            expenses.OwnsOne(expense => expense.Amount, money => money.ConfigureMoney("Amount"));
            expenses.OwnsOne(expense => expense.ConvertedAmount, money => money.ConfigureMoney("ConvertedAmount"));
        });

        builder.OwnsMany(budget => budget.Savings, savings =>
        {
            savings.ToTable("Savings", DatabaseSchemas.Execution);

            savings.Property<BudgetId>("BudgetId")
                .HasConversion(
                    id => id.Value,
                    value => new BudgetId(value));

            savings.WithOwner()
                .HasForeignKey("BudgetId");

            savings.HasKey(saving => saving.Id);

            savings.Property(saving => saving.Id)
                .HasConversion(
                    id => id.Value,
                    value => new SavingId(value))
                .ValueGeneratedNever();

            savings.Property(saving => saving.CategoryId)
                .HasConversion(
                    id => id.Value,
                    value => new BudgetCategoryId(value));

            savings.Property(saving => saving.Title)
                .HasMaxLength(100);

            savings.Property(saving => saving.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            savings.Property(saving => saving.RemovalReason)
                .HasMaxLength(300);

            savings.OwnsOne(saving => saving.Amount, money => money.ConfigureMoney("Amount"));
            savings.OwnsOne(saving => saving.ConvertedAmount, money => money.ConfigureMoney("ConvertedAmount"));
        });

        builder.Navigation(budget => budget.TotalIncome)
            .IsRequired();

        builder.Navigation(budget => budget.TotalExpenses)
            .IsRequired();

        builder.Navigation(budget => budget.TotalSavings)
            .IsRequired();

        builder.Navigation(budget => budget.ActualFinancialResult)
            .IsRequired();

        builder.Navigation(budget => budget.Incomes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(budget => budget.Expenses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(budget => budget.Savings)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
