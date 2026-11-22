using System.Runtime.CompilerServices;

namespace JobRadar.Domain.Sources;

/// <summary>A public job board that postings are fetched from.</summary>
public sealed class Source
{
    /// <summary>Fixed id assigned in code; each source client knows its own id (Arbeitnow = 1).</summary>
    public int Id { get; private set; }
    public string Name { get; private set; }
    public Uri BaseUrl { get; private set; }

    /// <summary>Page to link to wherever this source's postings are shown, as its terms require.</summary>
    public Uri AttributionUrl { get; private set; }

    public Source(int id, string name, Uri baseUrl, Uri attributionUrl)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureAbsolute(baseUrl);
        EnsureAbsolute(attributionUrl);

        Id = id;
        Name = name.Trim();
        BaseUrl = baseUrl;
        AttributionUrl = attributionUrl;
    }

    private static void EnsureAbsolute(Uri url, [CallerArgumentExpression(nameof(url))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(url, paramName);
        if (!url.IsAbsoluteUri)
        {
            throw new ArgumentException("URL must be absolute.", paramName);
        }
    }
}
