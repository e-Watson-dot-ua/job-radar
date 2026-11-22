namespace JobRadar.Domain.Postings;

/// <summary>A job posting as fetched from a source, plus its extraction pipeline state.</summary>
public sealed class Posting
{
    private Posting() { } // for EF Core

    /// <summary>Time-ordered UUID (version 7), generated when the posting is first seen.</summary>
    public Guid Id { get; private set; }
    public int SourceId { get; private set; }

    /// <summary>The posting's id at the source; unique together with <see cref="SourceId"/>.</summary>
    public string ExternalId { get; private set; } = null!;

    /// <summary>Everything the source gave us; replaced as a whole when it changes.</summary>
    public PostingContent Content { get; private set; } = null!;

    /// <summary>Hash of title and description text; a change sends the posting back to extraction.</summary>
    public string ContentHash { get; private set; } = null!;
    public DateTimeOffset FirstSeenAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }

    /// <summary>Where the posting is in the LLM extraction pipeline.</summary>
    public ExtractionState Extraction { get; private set; } = ExtractionState.Pending;

    /// <summary>New posting: stored as Pending so the extraction worker picks it up.</summary>
    public static Posting Create(int sourceId, string externalId, PostingContent content, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sourceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
        ArgumentNullException.ThrowIfNull(content);

        var posting = new Posting
        {
            Id = Guid.CreateVersion7(now),
            SourceId = sourceId,
            ExternalId = externalId,
            FirstSeenAt = now,
            LastSeenAt = now,
        };
        posting.Apply(content, ContentHasher.Compute(content.Title, content.DescriptionText));
        return posting;
    }

    /// <summary>
    /// Posting seen again in a later run. Unchanged hash: only <see cref="LastSeenAt"/> moves.
    /// Changed hash: content is replaced and the posting goes back to Pending with a fresh set of attempts.
    /// </summary>
    public PostingChange RecordSeen(PostingContent content, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(content);

        LastSeenAt = now;

        var hash = ContentHasher.Compute(content.Title, content.DescriptionText);
        if (hash == ContentHash)
        {
            return PostingChange.Unchanged;
        }

        Apply(content, hash);
        return PostingChange.Updated;
    }

    private void Apply(PostingContent content, string hash)
    {
        Content = content;
        ContentHash = hash;
        Extraction = ExtractionState.Pending;
    }
}
