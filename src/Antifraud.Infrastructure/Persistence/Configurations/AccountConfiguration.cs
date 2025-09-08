using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Antifraud.Domain.Entities;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        // Primary Key
        builder.HasKey(a => a.Id);

        // Account ID
        builder.Property(a => a.Id)
            .HasConversion(
                id => id.Value,
                value => AccountId.From(value))
            .HasColumnName("id")
            .IsRequired();

        // Account Number
        builder.Property(a => a.AccountNumber)
            .HasColumnName("account_number")
            .HasMaxLength(50)
            .IsRequired();

        // Holder Name
        builder.Property(a => a.HolderName)
            .HasColumnName("holder_name")
            .HasMaxLength(200)
            .IsRequired();

        // Created At
        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Is Active
        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        // Indexes
        builder.HasIndex(a => a.AccountNumber)
            .IsUnique()
            .HasDatabaseName("ix_accounts_account_number");

        builder.HasIndex(a => a.IsActive)
            .HasDatabaseName("ix_accounts_is_active");
    }
}