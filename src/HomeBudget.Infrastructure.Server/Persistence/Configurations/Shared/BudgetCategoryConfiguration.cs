using HomeBudget.Domain.Shared;
using HomeBudget.Infrastructure.Server.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeBudget.Infrastructure.Server.Persistence.Configurations.Shared;

internal sealed class BudgetCategoryConfiguration : IEntityTypeConfiguration<BudgetCategory>
{
    public void Configure(EntityTypeBuilder<BudgetCategory> builder)
    {
        builder.ToTable("BudgetCategories", DatabaseSchemas.Shared);

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .HasConversion(
                id => id.Value,
                value => new BudgetCategoryId(value))
            .ValueGeneratedNever();

        builder.Property(category => category.OwnerId)
            .HasConversion(
                id => id.Value,
                value => new OwnerId(value));

        builder.Property(category => category.Name)
            .HasMaxLength(100);

        builder.Property(category => category.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(category => new { category.OwnerId, category.Name });
    }
}
