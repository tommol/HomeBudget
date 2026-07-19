using HomeBudget.Domain.Planning;
using HomeBudget.Domain.Shared;
using HomeBudget.Infrastructure.Server.Persistence;
using HomeBudget.Infrastructure.Server.Persistence.Configurations.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeBudget.Infrastructure.Server.Persistence.Configurations.Planning;

internal sealed class BudgetPlanConfiguration : IEntityTypeConfiguration<BudgetPlan>
{
    public void Configure(EntityTypeBuilder<BudgetPlan> builder)
    {
        builder.ToTable("BudgetPlans", DatabaseSchemas.Planning);

        builder.HasKey(budgetPlan => budgetPlan.Id);

        builder.Property(budgetPlan => budgetPlan.Id)
            .HasConversion(
                id => id.Value,
                value => new BudgetPlanId(value))
            .ValueGeneratedNever();

        builder.Property(budgetPlan => budgetPlan.OwnerId)
            .HasConversion(
                id => id.Value,
                value => new OwnerId(value));

        builder.Property(budgetPlan => budgetPlan.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(budgetPlan => budgetPlan.BudgetFitRisk)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(budgetPlan => budgetPlan.DefaultCurrency)
            .HasConversion(
                currency => currency.Code,
                code => new Currency(code))
            .HasMaxLength(3);

        builder.Property<int>("_periodYear")
            .HasColumnName("PeriodYear");

        builder.Property<int>("_periodMonth")
            .HasColumnName("PeriodMonth");

        builder.Ignore(budgetPlan => budgetPlan.Period);

        builder.OwnsOne(budgetPlan => budgetPlan.TotalPlannedIncome, money => money.ConfigureMoney("TotalPlannedIncome"));
        builder.OwnsOne(budgetPlan => budgetPlan.TotalAllocatedExpenses, money => money.ConfigureMoney("TotalAllocatedExpenses"));
        builder.OwnsOne(budgetPlan => budgetPlan.TotalSavingContributions, money => money.ConfigureMoney("TotalSavingContributions"));
        builder.OwnsOne(budgetPlan => budgetPlan.PlannedFinancialResult, money => money.ConfigureMoney("PlannedFinancialResult"));

        builder.HasIndex("OwnerId", "_periodYear", "_periodMonth")
            .IsUnique();

        builder.OwnsMany(budgetPlan => budgetPlan.PlannedIncomes, incomes =>
        {
            incomes.ToTable("PlannedIncomes", DatabaseSchemas.Planning);

            incomes.Property<BudgetPlanId>("BudgetPlanId")
                .HasConversion(
                    id => id.Value,
                    value => new BudgetPlanId(value));

            incomes.WithOwner()
                .HasForeignKey("BudgetPlanId");

            incomes.HasKey(income => income.Id);

            incomes.Property(income => income.Id)
                .HasConversion(
                    id => id.Value,
                    value => new PlannedIncomeId(value))
                .ValueGeneratedNever();

            incomes.Property(income => income.CategoryId)
                .HasConversion(
                    id => id.Value,
                    value => new BudgetCategoryId(value));

            incomes.Property(income => income.Title)
                .HasMaxLength(100);

            incomes.OwnsOne(income => income.Amount, money => money.ConfigureMoney("Amount"));
            incomes.OwnsOne(income => income.ConvertedAmount, money => money.ConfigureMoney("ConvertedAmount"));
        });

        builder.OwnsMany(budgetPlan => budgetPlan.ExpenseCategoryAllocations, allocations =>
        {
            allocations.ToTable("ExpenseCategoryAllocations", DatabaseSchemas.Planning);

            allocations.Property<BudgetPlanId>("BudgetPlanId")
                .HasConversion(
                    id => id.Value,
                    value => new BudgetPlanId(value));

            allocations.WithOwner()
                .HasForeignKey("BudgetPlanId");

            allocations.HasKey(allocation => allocation.Id);

            allocations.Property(allocation => allocation.Id)
                .HasConversion(
                    id => id.Value,
                    value => new CategoryAllocationId(value))
                .ValueGeneratedNever();

            allocations.Property(allocation => allocation.CategoryId)
                .HasConversion(
                    id => id.Value,
                    value => new BudgetCategoryId(value));

            allocations.Property(allocation => allocation.Flexibility)
                .HasConversion<string>()
                .HasMaxLength(20);

            allocations.OwnsOne(allocation => allocation.Amount, money => money.ConfigureMoney("Amount"));
        });

        builder.OwnsMany(budgetPlan => budgetPlan.SavingContributions, contributions =>
        {
            contributions.ToTable("SavingContributions", DatabaseSchemas.Planning);

            contributions.Property<BudgetPlanId>("BudgetPlanId")
                .HasConversion(
                    id => id.Value,
                    value => new BudgetPlanId(value));

            contributions.WithOwner()
                .HasForeignKey("BudgetPlanId");

            contributions.HasKey(contribution => contribution.Id);

            contributions.Property(contribution => contribution.Id)
                .HasConversion(
                    id => id.Value,
                    value => new SavingContributionId(value))
                .ValueGeneratedNever();

            contributions.Property(contribution => contribution.CategoryId)
                .HasConversion(
                    id => id.Value,
                    value => new BudgetCategoryId(value));

            contributions.OwnsOne(contribution => contribution.Amount, money => money.ConfigureMoney("Amount"));
        });

        builder.Navigation(budgetPlan => budgetPlan.TotalPlannedIncome)
            .IsRequired();

        builder.Navigation(budgetPlan => budgetPlan.TotalAllocatedExpenses)
            .IsRequired();

        builder.Navigation(budgetPlan => budgetPlan.TotalSavingContributions)
            .IsRequired();

        builder.Navigation(budgetPlan => budgetPlan.PlannedFinancialResult)
            .IsRequired();

        builder.Navigation(budgetPlan => budgetPlan.PlannedIncomes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(budgetPlan => budgetPlan.ExpenseCategoryAllocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(budgetPlan => budgetPlan.SavingContributions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

    }
}
