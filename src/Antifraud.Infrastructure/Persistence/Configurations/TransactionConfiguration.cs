using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Antifraud.Domain.Entities;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        // Primary Key
        builder.HasKey(t => t.Id);

        // Transaction ID
        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => TransactionId.From(value))
            .HasColumnName("id")
            .IsRequired();

        // Source Account ID
        builder.Property(t => t.SourceAccountId)
            .HasConversion(
                id => id.Value,
                value => AccountId.From(value))
            .HasColumnName("source_account_id")
            .IsRequired();

        // Target Account ID
        builder.Property(t => t.TargetAccountId)
            .HasConversion(
                id => id.Value,
                value => AccountId.From(value))
            .HasColumnName("target_account_id")
            .IsRequired();

        // Transfer Type ID
        builder.Property(t => t.TransferTypeId)
            .HasConversion(
                id => id.Value,
                value => TransferTypeId.From(value))
            .HasColumnName("transfer_type_id")
            .IsRequired();

        // Money Value Object
        builder.OwnsOne(t => t.Value, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Transaction Status
        builder.Property(t => t.Status)
            .HasConversion(
                status => status.Value,
                value => TransactionStatus.From(value))
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        // Timestamps
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(t => t.SourceAccountId)
            .HasDatabaseName("ix_transactions_source_account_id");

        builder.HasIndex(t => t.TargetAccountId)
            .HasDatabaseName("ix_transactions_target_account_id");

        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("ix_transactions_created_at");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("ix_transactions_status");

        // Composite index for daily queries
        builder.HasIndex(t => new { t.SourceAccountId, t.CreatedAt })
            .HasDatabaseName("ix_transactions_source_account_created_at");

        // Ignore domain events
        builder.Ignore(t => t.DomainEvents);
    }
}