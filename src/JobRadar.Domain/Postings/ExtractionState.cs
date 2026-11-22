namespace JobRadar.Domain.Postings;

/// <summary>
/// Extraction pipeline state of a posting: status, attempts so far, last error,
/// and the earliest time a failed extraction may be retried (<c>null</c> when no retry is scheduled).
/// </summary>
public sealed record ExtractionState(ExtractionStatus Status, int Attempts, string? LastError, DateTimeOffset? NextAttemptAt)
{
    /// <summary>Waiting for extraction with a fresh set of attempts.</summary>
    public static ExtractionState Pending { get; } = new(ExtractionStatus.Pending, 0, null, null);
}
