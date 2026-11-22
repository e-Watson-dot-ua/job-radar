namespace JobRadar.Domain.Extractions;

/// <summary>
/// LLM extraction result for one posting with one prompt version; keyed by (PostingId, PromptVersion),
/// so a new prompt version adds a row and older results stay as history.
/// </summary>
public sealed class PostingExtraction
{
    private PostingExtraction() { } // for EF Core

    public Guid PostingId { get; private set; }

    /// <summary>Prompt file the result came from, e.g. "extraction.v1".</summary>
    public string PromptVersion { get; private set; } = null!;
    public string ModelName { get; private set; } = null!;
    public DateTimeOffset ExtractedAt { get; private set; }
    public ExtractedFields Fields { get; private set; } = null!;

    /// <summary>The model's answer exactly as received, as JSON; kept for debugging and re-validation.</summary>
    public string RawModelOutput { get; private set; } = null!;

    /// <summary>First result for this posting and prompt version. Only valid fields can be stored.</summary>
    public static PostingExtraction Create(
        Guid postingId, string promptVersion, string modelName, ExtractedFields fields, string rawModelOutput, DateTimeOffset now)
    {
        if (postingId == Guid.Empty)
        {
            throw new ArgumentException("Posting id must not be empty.", nameof(postingId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(promptVersion);

        var extraction = new PostingExtraction { PostingId = postingId, PromptVersion = promptVersion };
        extraction.Replace(modelName, fields, rawModelOutput, now);
        return extraction;
    }

    /// <summary>
    /// Re-extraction with the same prompt version (posting content changed): replaces the result, keeps the key.
    /// Only valid fields can be stored; the exception lists every problem.
    /// </summary>
    public void Replace(string modelName, ExtractedFields fields, string rawModelOutput, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelName);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentException.ThrowIfNullOrWhiteSpace(rawModelOutput);

        var problems = fields.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid extracted fields: {string.Join(" ", problems)}", nameof(fields));
        }

        ModelName = modelName;
        Fields = fields;
        RawModelOutput = rawModelOutput;
        ExtractedAt = now;
    }
}
