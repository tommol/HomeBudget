using HomeBudget.Domain.Shared;
using HomeBudget.Infrastructure.Server.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeBudget.Infrastructure.Server.Persistence.Configurations.Identity;

internal sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("UserAccounts", DatabaseSchemas.Auth);

        builder.HasKey(userAccount => userAccount.Id);

        builder.Property(userAccount => userAccount.Id)
            .ValueGeneratedNever();

        builder.Property(userAccount => userAccount.OwnerId)
            .HasConversion(
                ownerId => ownerId.Value,
                value => new OwnerId(value));

        builder.Property(userAccount => userAccount.Issuer)
            .HasMaxLength(512);

        builder.Property(userAccount => userAccount.Subject)
            .HasMaxLength(200);

        builder.Property(userAccount => userAccount.Email)
            .HasMaxLength(320);

        builder.Property(userAccount => userAccount.DisplayName)
            .HasMaxLength(200);

        builder.HasIndex(userAccount => new { userAccount.Issuer, userAccount.Subject })
            .IsUnique();

        builder.HasIndex(userAccount => userAccount.OwnerId)
            .IsUnique();
    }
}
