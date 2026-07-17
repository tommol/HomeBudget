using HomeBudget.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeBudget.Infrastructure.Server.Persistence.Configurations.Shared;

internal static class MoneyConfigurationExtensions
{
    public static void ConfigureMoney<TOwner>(
        this OwnedNavigationBuilder<TOwner, Money> builder,
        string columnPrefix)
        where TOwner : class
    {
        builder.Property(money => money.Amount)
            .HasColumnName($"{columnPrefix}Amount")
            .HasPrecision(18, 2);

        builder.Property(money => money.Currency)
            .HasColumnName($"{columnPrefix}CurrencyCode")
            .HasConversion(
                currency => currency.Code,
                code => new Currency(code))
            .HasMaxLength(3);
    }
}
