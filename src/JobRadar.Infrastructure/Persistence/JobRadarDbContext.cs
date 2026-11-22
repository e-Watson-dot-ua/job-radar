using JobRadar.Domain.Extractions;
using JobRadar.Domain.Postings;
using JobRadar.Domain.Sources;
using Microsoft.EntityFrameworkCore;

namespace JobRadar.Infrastructure.Persistence;

/// <summary>
/// EF Core access to the Job Radar database. Table and column names are snake_case as in brief §3;
/// enums are stored as lowercase text. Mappings live in <c>Configurations/</c>.
/// </summary>
public sealed class JobRadarDbContext(DbContextOptions<JobRadarDbContext> options) : DbContext(options)
{
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<Posting> Postings => Set<Posting>();
    public DbSet<PostingExtraction> PostingExtractions => Set<PostingExtraction>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSnakeCaseNamingConvention();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<ExtractionStatus>().HaveConversion<LowercaseEnumConverter<ExtractionStatus>>();
        configurationBuilder.Properties<Seniority>().HaveConversion<LowercaseEnumConverter<Seniority>>();
        configurationBuilder.Properties<RemotePolicy>().HaveConversion<LowercaseEnumConverter<RemotePolicy>>();
        configurationBuilder.Properties<SalaryPeriod>().HaveConversion<LowercaseEnumConverter<SalaryPeriod>>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobRadarDbContext).Assembly);
}
