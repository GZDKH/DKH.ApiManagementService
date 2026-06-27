using System.Text.Json;
using DKH.ApiManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DKH.ApiManagementService.Infrastructure.Persistence.Configurations;

public sealed class ReportedModuleComponentConfiguration : IEntityTypeConfiguration<ReportedModuleComponentEntity>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<ReportedModuleComponentEntity> builder)
    {
        builder.ToTable("reported_module_components");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ModuleId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Kind)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.Version)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Category)
            .HasMaxLength(256);

        builder.Property(x => x.RequiresEntitlement)
            .HasMaxLength(256);

        builder.Property(x => x.State)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        // Name/Description (localized maps) and Provides/Requires (record lists) are stored as jsonb
        // on PostgreSQL. We supply an explicit JSON value converter + comparer (rather than relying on
        // Npgsql's implicit Dictionary/list mapping) so the model is also valid under the in-memory
        // provider used by the unit tests.
        ConfigureJson(builder.Property(x => x.Name).IsRequired());
        ConfigureJson(builder.Property(x => x.Description));
        ConfigureJson(builder.Property(x => x.Provides).IsRequired());
        ConfigureJson(builder.Property(x => x.Requires).IsRequired());

        // ModuleId is the natural key for upsert — enforce at the DB level.
        builder.HasIndex(x => x.ModuleId)
            .IsUnique();
    }

    private static void ConfigureJson<T>(PropertyBuilder<T> property)
    {
        property
            .HasColumnType("jsonb")
            .HasConversion(
                value => JsonSerializer.Serialize(value, JsonOptions),
                json => JsonSerializer.Deserialize<T>(json, JsonOptions)!,
                new ValueComparer<T>(
                    (left, right) => JsonSerializer.Serialize(left, JsonOptions) == JsonSerializer.Serialize(right, JsonOptions),
                    value => value == null ? 0 : JsonSerializer.Serialize(value, JsonOptions).GetHashCode(),
                    value => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, JsonOptions), JsonOptions)!));
    }
}
