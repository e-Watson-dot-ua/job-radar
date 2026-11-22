using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JobRadar.Infrastructure.Persistence;

/// <summary>
/// Lets <c>dotnet ef</c> create the DbContext without starting Aspire (see docs/ef-core-migrations.md).
/// Creating a migration or a script builds the model but never connects. Commands that do connect
/// (e.g. <c>migrations remove</c>) reach only our own <c>jobradar-postgres</c> container on port 17300,
/// never another PostgreSQL on the default port 5432. No password here: such commands fail harmlessly.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<JobRadarDbContext>
{
    public JobRadarDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<JobRadarDbContext>()
            .UseNpgsql("Host=localhost;Port=17300;Database=jobradar")
            .Options);
}
