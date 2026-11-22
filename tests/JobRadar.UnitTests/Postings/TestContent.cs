using JobRadar.Domain.Postings;

namespace JobRadar.UnitTests.Postings;

/// <summary>Builds valid posting content for tests; override only what a test cares about.</summary>
internal static class TestContent
{
    public static PostingContent Valid(string title = "Backend Developer", string text = "We build things.") => new(
        url: new Uri("https://www.arbeitnow.com/jobs/backend-developer-123"),
        title: title,
        company: "Acme GmbH",
        locationText: "Berlin",
        remoteFlag: null,
        tags: ["C#", ".NET"],
        descriptionHtml: $"<p>{text}</p>",
        descriptionText: text,
        rawPayload: """{"slug":"backend-developer-123"}""",
        publishedAt: new DateTimeOffset(2026, 11, 21, 8, 0, 0, TimeSpan.Zero));
}
