using DKH.ApiManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DKH.ApiManagementService.Infrastructure.Persistence.Configurations;

public class AiProviderConfiguration : IEntityTypeConfiguration<AiProviderEntity>
{
    public void Configure(EntityTypeBuilder<AiProviderEntity> builder)
    {
        builder.ToTable("ai_providers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.ProviderType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.DisplayName)
            .HasMaxLength(256);

        builder.Property(x => x.BaseUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.Models)
            .HasColumnType("jsonb");

        builder.Property(x => x.ApiKeyReference)
            .HasMaxLength(512);

        builder.Property(x => x.CreationTime);

        builder.Property(x => x.LastModificationTime);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasIndex(x => x.ProviderType);

        builder.HasIndex(x => x.Status);
    }
}
