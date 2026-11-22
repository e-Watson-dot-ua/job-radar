using System.Diagnostics;
using JobRadar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobRadar.MigrationService;

/// <summary>
/// Applies pending EF Core migrations once, then stops the process. Aspire starts the other services
/// only after this finishes successfully; a failure exits with code 1, so they do not start.
/// The run appears as one span ("Apply migrations") in the dashboard's Traces view.
/// </summary>
internal sealed class MigrationWorker(
    IServiceProvider services,
    IHostApplicationLifetime lifetime,
    ILogger<MigrationWorker> logger) : BackgroundService
{
    // Same name as the application, so ServiceDefaults already exports it.
    private static readonly ActivitySource ActivitySource = new("JobRadar.MigrationService");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity("Apply migrations");
        try
        {
            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<JobRadarDbContext>();

            // The retry strategy covers a database that is still starting up.
            List<string> pending = [];
            await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                pending = [.. await db.Database.GetPendingMigrationsAsync(stoppingToken)];
                await db.Database.MigrateAsync(stoppingToken);
            });

            logger.LogInformation("Applied {Count} migration(s): {Migrations}", pending.Count, string.Join(", ", pending));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            activity?.AddException(ex);
            logger.LogError(ex, "Applying migrations failed");
            Environment.ExitCode = 1;
        }

        lifetime.StopApplication();
    }
}
