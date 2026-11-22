using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JobRadar.Infrastructure.Persistence;

/// <summary>
/// Lets <c>dotnet ef</c> create the DbContext without starting Aspire (see docs/ef-core-migrations.md).
/// The connection string is a placeholder: creating a migration builds the model but never connects.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<JobRadarDbContext>
{
    public JobRadarDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<JobRadarDbContext>()
            .UseNpgsql("Host=localhost;Database=jobradar_design_time")
            .Options);
}
