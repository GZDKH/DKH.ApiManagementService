using System.Reflection;
using DKH.ApiManagementService.Application.Abstractions;
using DKH.ApiManagementService.Domain.Entities;
using DKH.Platform.EntityFrameworkCore;
using DKH.Platform.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DKH.ApiManagementService.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : PlatformDbContext<AppDbContext>(options),
        IAppDbContext
{
    public DbSet<ApiKeyEntity> ApiKeys { get; init; } = null!;

    public DbSet<ApiKeyUsageEntity> ApiKeyUsageRecords { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    protected override Guid? GetCurrentUserId()
    {
        var currentUser = this.GetService<IPlatformCurrentUser>();
        return currentUser?.UserId;
    }
}
