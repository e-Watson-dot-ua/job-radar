using JobRadar.Domain.Extractions;
using JobRadar.Domain.Postings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Table <c>posting_extractions</c>, keyed by (posting_id, prompt_version). <c>Fields</c> with its nested
/// <c>Skills</c> and <c>Salary</c> are columns of this table, named exactly as in brief §3;
/// spoken languages are one jsonb column, written through <see cref="SpokenLanguagesConverter"/>.
/// </summary>
internal sealed class PostingExtractionConfiguration : IEntityTypeConfiguration<PostingExtraction>
{
    public void Configure(EntityTypeBuilder<PostingExtraction> builder)
    {
        builder.ToTable("posting_extractions");
        builder.HasKey(e => new { e.PostingId, e.PromptVersion });
        builder.HasOne<Posting>().WithMany().HasForeignKey(e => e.PostingId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.RawModelOutput).HasColumnType("jsonb");

        builder.ComplexProperty(e => e.Fields, fields =>
        {
            fields.Property(f => f.Seniority).HasColumnName("seniority");
            fields.Property(f => f.RemotePolicy).HasColumnName("remote_policy");
            fields.Property(f => f.Country).HasColumnName("country");
            fields.Property(f => f.City).HasColumnName("city");
            fields.Property(f => f.VisaSponsorship).HasColumnName("visa_sponsorship");
            fields.Property(f => f.CitizenshipOrClearanceRequired).HasColumnName("citizenship_or_clearance_required");

            fields.ComplexProperty(f => f.Skills, skills =>
            {
                skills.PrimitiveCollection(s => s.PrimaryLanguages).HasColumnName("primary_languages");
                skills.PrimitiveCollection(s => s.Frameworks).HasColumnName("frameworks");
                skills.PrimitiveCollection(s => s.CloudPlatforms).HasColumnName("cloud_platforms");
                skills.PrimitiveCollection(s => s.OtherSkills).HasColumnName("other_skills");
            });

            fields.ComplexProperty(f => f.Salary, salary =>
            {
                salary.Property(s => s.Min).HasColumnName("salary_min").HasPrecision(12, 2);
                salary.Property(s => s.Max).HasColumnName("salary_max").HasPrecision(12, 2);
                salary.Property(s => s.Currency).HasColumnName("salary_currency");
                salary.Property(s => s.Period).HasColumnName("salary_period");
            });

            fields.Property(f => f.SpokenLanguages)
                .HasColumnName("required_spoken_languages")
                .HasColumnType("jsonb")
                .HasConversion(new SpokenLanguagesConverter(), new SpokenLanguagesComparer());
        });
    }
}
