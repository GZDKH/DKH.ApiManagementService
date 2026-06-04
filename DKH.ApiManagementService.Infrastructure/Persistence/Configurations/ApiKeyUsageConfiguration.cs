using DKH.ApiManagementService.Domain.Entities;
using DKH.ApiManagementService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DKH.ApiManagementService.Infrastructure.Persistence.Configurations;

public class ApiKeyUsageConfiguration : IEntityTypeConfiguration<ApiKeyUsageEntity>
{
    public void Configure(EntityTypeBuilder<ApiKeyUsageEntity> builder)
    {
        builder.ToTable("api_key_usage");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Endpoint)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(512);

        builder.Property(x => x.Environment)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(ApiKeyEnvironment.Production)
            .HasMaxLength(32);

        builder.Property(x => x.RateLimitTier)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(ApiKeyRateLimitTier.Standard)
            .HasMaxLength(32);

        builder.Property(x => x.RateLimitRequestsPerMinute)
            .HasDefaultValue(600)
            .IsRequired();

        builder.HasIndex(x => x.ApiKeyId);

        builder.HasIndex(x => x.CustomerId);

        builder.HasIndex(x => x.Environment);

        builder.HasIndex(x => x.RateLimitTier);

        builder.HasIndex(x => new { x.CustomerId, x.Environment });

        builder.HasIndex(x => x.Timestamp);
    }
}
