using DKH.ApiManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DKH.ApiManagementService.Infrastructure.Persistence.Configurations;

public class ModuleStateConfiguration : IEntityTypeConfiguration<ModuleStateEntity>
{
    public void Configure(EntityTypeBuilder<ModuleStateEntity> builder)
    {
        builder.ToTable("module_states");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ModuleId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.State)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Version)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(x => x.ModuleId)
            .IsUnique();
    }
}
