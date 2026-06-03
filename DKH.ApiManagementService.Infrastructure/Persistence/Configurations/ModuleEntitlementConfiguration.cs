using DKH.ApiManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DKH.ApiManagementService.Infrastructure.Persistence.Configurations;

public class ModuleEntitlementConfiguration : IEntityTypeConfiguration<ModuleEntitlementEntity>
{
    public void Configure(EntityTypeBuilder<ModuleEntitlementEntity> builder)
    {
        builder.ToTable("module_entitlements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ScopeKind)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.ScopeValue)
            .HasMaxLength(256);

        builder.Property(x => x.ModuleId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Granted)
            .IsRequired();

        builder.HasIndex(x => new { x.ScopeKind, x.ScopeValue, x.ModuleId })
            .IsUnique();
    }
}
