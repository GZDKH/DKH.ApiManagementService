using DKH.ApiManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DKH.ApiManagementService.Infrastructure.Persistence.Configurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKeyEntity>
{
    public void Configure(EntityTypeBuilder<ApiKeyEntity> builder)
    {
        builder.ToTable("api_keys");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.KeyHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.KeyPrefix)
            .IsRequired()
            .HasMaxLength(48);

        builder.Property(x => x.Scope)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Permissions)
            .HasColumnType("jsonb");

        builder.Property(x => x.Description)
            .HasMaxLength(1024);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.CreationTime);

        builder.Property(x => x.LastModificationTime);

        builder.HasIndex(x => x.KeyHash)
            .IsUnique();

        builder.HasIndex(x => x.Scope);

        builder.HasIndex(x => x.Status);

        builder.HasMany(x => x.UsageRecords)
            .WithOne(x => x.ApiKey)
            .HasForeignKey(x => x.ApiKeyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
