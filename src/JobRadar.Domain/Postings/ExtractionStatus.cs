namespace JobRadar.Domain.Postings;

/// <summary>
/// Where a posting is in the LLM extraction pipeline:
/// Pending - new or changed, waiting for extraction;
/// Processing - claimed by a worker;
/// Succeeded - fields extracted and stored;
/// Failed - retried with backoff until the attempt limit, then stays failed.
/// </summary>
public enum ExtractionStatus { Pending, Processing, Succeeded, Failed }
