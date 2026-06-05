using DKH.ApiManagementService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DKH.ApiManagementService.Infrastructure.Persistence.Configurations;

public sealed class WebhookSubscriptionConfiguration : IEntityTypeConfiguration<WebhookSubscriptionEntity>
{
    public void Configure(EntityTypeBuilder<WebhookSubscriptionEntity> builder)
    {
        builder.ToTable("webhook_subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApiKeyId);

        builder.Property(x => x.CustomerId);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.CallbackUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.Events)
            .HasColumnType("jsonb");

        builder.Property(x => x.SigningSecretHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.SigningSecretPrefix)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.RetryMaxAttempts)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(x => x.RetryBackoffSeconds)
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(x => x.DlqEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.LastDeliveryError)
            .HasMaxLength(2048);

        builder.Property(x => x.FailureCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.RotationCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(x => x.ApiKeyId);

        builder.HasIndex(x => x.CustomerId);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => new { x.CustomerId, x.Status });
    }
}
