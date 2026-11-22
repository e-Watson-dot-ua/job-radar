using JobRadar.Domain.Postings;
using JobRadar.Domain.Sources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Table <c>postings</c>. <c>Content</c> and <c>Extraction</c> are C# groupings only: their properties
/// are columns of this table, named exactly as in brief §3.
/// </summary>
internal sealed class PostingConfiguration : IEntityTypeConfiguration<Posting>
{
    public void Configure(EntityTypeBuilder<Posting> builder)
    {
        builder.ToTable("postings");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        // Sources are reference data: refuse to delete one that still has postings.
        builder.HasOne<Source>().WithMany().HasForeignKey(p => p.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(p => new { p.SourceId, p.ExternalId }).IsUnique();

        builder.ComplexProperty(p => p.Content, content =>
        {
            content.Property(c => c.Url).HasColumnName("url").HasConversion<string>();
            content.Property(c => c.Title).HasColumnName("title");
            content.Property(c => c.Company).HasColumnName("company");
            content.Property(c => c.LocationText).HasColumnName("location_text");
            content.Property(c => c.RemoteFlag).HasColumnName("remote_flag");
            content.PrimitiveCollection(c => c.Tags).HasColumnName("tags");
            content.Property(c => c.DescriptionHtml).HasColumnName("description_html");
            content.Property(c => c.DescriptionText).HasColumnName("description_text");
            content.Property(c => c.RawPayload).HasColumnName("raw_payload").HasColumnType("jsonb");
            content.Property(c => c.PublishedAt).HasColumnName("published_at");
        });

        builder.ComplexProperty(p => p.Extraction, extraction =>
        {
            extraction.Property(e => e.Status).HasColumnName("extraction_status");
            extraction.Property(e => e.Attempts).HasColumnName("extraction_attempts");
            extraction.Property(e => e.LastError).HasColumnName("last_extraction_error");
            extraction.Property(e => e.NextAttemptAt).HasColumnName("next_attempt_at");
        });
    }
}
