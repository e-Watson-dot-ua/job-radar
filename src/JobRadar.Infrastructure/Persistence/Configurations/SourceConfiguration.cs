using JobRadar.Domain.Sources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

/// <summary>Table <c>sources</c>; Arbeitnow is seeded with id 1 so it exists from the first migration.</summary>
internal sealed class SourceConfiguration : IEntityTypeConfiguration<Source>
{
    public void Configure(EntityTypeBuilder<Source> builder)
    {
        builder.ToTable("sources");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.BaseUrl).HasConversion<string>();
        builder.Property(s => s.AttributionUrl).HasConversion<string>();

        builder.HasData(new Source(
            1,
            "Arbeitnow",
            new Uri("https://www.arbeitnow.com/api/job-board-api"),
            new Uri("https://www.arbeitnow.com")));
    }
}
