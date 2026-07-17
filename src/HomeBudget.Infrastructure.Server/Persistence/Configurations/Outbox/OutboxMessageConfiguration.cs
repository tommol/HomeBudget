using HomeBudget.Infrastructure.Server.Persistence;
using HomeBudget.Infrastructure.Server.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeBudget.Infrastructure.Server.Persistence.Configurations.Outbox;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", DatabaseSchemas.Outbox);

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever();

        builder.Property(message => message.Type)
            .HasMaxLength(500);

        builder.Property(message => message.Content);

        builder.Property(message => message.Error)
            .HasMaxLength(2000);

        builder.HasIndex(message => message.ProcessedOnUtc);
    }
}
