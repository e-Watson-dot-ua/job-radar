namespace JobRadar.Domain.Postings;

/// <summary>
/// Posting fields as provided by a source; values the source does not give are <c>null</c>.
/// <see cref="DescriptionText"/> is the already cleaned text version of <see cref="DescriptionHtml"/>,
/// and <see cref="RawPayload"/> is the original item as JSON.
/// Validates itself, also when copied with <c>with { ... }</c>, so invalid content cannot exist.
/// </summary>
public sealed record PostingContent
{
    public PostingContent(
        Uri url,
        string title,
        string? company,
        string? locationText,
        bool? remoteFlag,
        IReadOnlyList<string> tags,
        string descriptionHtml,
        string descriptionText,
        string rawPayload,
        DateTimeOffset? publishedAt)
    {
        Url = url;
        Title = title;
        Company = company;
        LocationText = locationText;
        RemoteFlag = remoteFlag;
        Tags = tags;
        DescriptionHtml = descriptionHtml;
        DescriptionText = descriptionText;
        RawPayload = rawPayload;
        PublishedAt = publishedAt;
    }

    public Uri Url { get; init => field = value is { IsAbsoluteUri: true } ? value : throw Invalid("Url must be an absolute URL."); }
    public string Title { get; init => field = !string.IsNullOrWhiteSpace(value) ? value : throw Invalid("Title is required."); }
    public string? Company { get; init; }
    public string? LocationText { get; init; }

    /// <summary>Remote flag exactly as the source gives it; <c>null</c> when the source does not say.</summary>
    public bool? RemoteFlag { get; init; }
    public IReadOnlyList<string> Tags { get; init => field = value?.ToArray() ?? throw Invalid("Tags must not be null."); }
    public string DescriptionHtml { get; init => field = value ?? throw Invalid("DescriptionHtml must not be null."); }
    public string DescriptionText { get; init => field = value ?? throw Invalid("DescriptionText must not be null."); }
    public string RawPayload { get; init => field = !string.IsNullOrWhiteSpace(value) ? value : throw Invalid("RawPayload is required."); }
    public DateTimeOffset? PublishedAt { get; init; }

    private static ArgumentException Invalid(string message) => new($"Invalid posting content: {message}", "value");
}
