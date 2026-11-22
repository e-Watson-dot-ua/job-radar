namespace JobRadar.Domain.Postings;

/// <summary>
/// Result of seeing an existing posting again:
/// Unchanged - same content, only the last-seen time moved; Updated - content changed and was replaced.
/// </summary>
public enum PostingChange { Unchanged, Updated }
