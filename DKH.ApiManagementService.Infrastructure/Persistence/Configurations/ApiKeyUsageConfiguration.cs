using DKH.ApiManagementService.Domain.Entities;
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

        builder.HasIndex(x => x.ApiKeyId);

        builder.HasIndex(x => x.Timestamp);
    }
}
