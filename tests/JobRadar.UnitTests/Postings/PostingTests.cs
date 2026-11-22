using JobRadar.Domain.Postings;

namespace JobRadar.UnitTests.Postings;

public class PostingTests
{
    private static readonly DateTimeOffset FirstRun = new(2026, 11, 22, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset SecondRun = FirstRun.AddHours(6);

    [Fact]
    public void Create_stores_content_as_pending()
    {
        var content = TestContent.Valid();

        var posting = Posting.Create(1, "backend-developer-123", content, FirstRun);

        Assert.NotEqual(Guid.Empty, posting.Id);
        Assert.Equal(1, posting.SourceId);
        Assert.Equal("backend-developer-123", posting.ExternalId);
        Assert.Same(content, posting.Content);
        Assert.Equal(ContentHasher.Compute(content.Title, content.DescriptionText), posting.ContentHash);
        Assert.Equal(FirstRun, posting.FirstSeenAt);
        Assert.Equal(FirstRun, posting.LastSeenAt);
        Assert.Equal(ExtractionState.Pending, posting.Extraction);
    }

    [Fact]
    public void Seen_again_unchanged_only_moves_last_seen()
    {
        var posting = Posting.Create(1, "backend-developer-123", TestContent.Valid(), FirstRun);
        var hash = posting.ContentHash;

        var change = posting.RecordSeen(TestContent.Valid(), SecondRun);

        Assert.Equal(PostingChange.Unchanged, change);
        Assert.Equal(FirstRun, posting.FirstSeenAt);
        Assert.Equal(SecondRun, posting.LastSeenAt);
        Assert.Equal(hash, posting.ContentHash);
    }

    [Fact]
    public void Seen_again_changed_replaces_content_and_returns_to_pending()
    {
        var posting = Posting.Create(1, "backend-developer-123", TestContent.Valid(), FirstRun);
        var oldHash = posting.ContentHash;

        var change = posting.RecordSeen(TestContent.Valid(title: "Senior Backend Developer"), SecondRun);

        Assert.Equal(PostingChange.Updated, change);
        Assert.Equal("Senior Backend Developer", posting.Content.Title);
        Assert.NotEqual(oldHash, posting.ContentHash);
        Assert.Equal(SecondRun, posting.LastSeenAt);
        Assert.Equal(ExtractionState.Pending, posting.Extraction);
    }

    [Fact]
    public void Changing_only_tags_counts_as_unchanged()
    {
        // The brief defines the hash over title and description text only.
        var posting = Posting.Create(1, "backend-developer-123", TestContent.Valid(), FirstRun);

        var change = posting.RecordSeen(TestContent.Valid() with { Tags = ["Go"] }, SecondRun);

        Assert.Equal(PostingChange.Unchanged, change);
        Assert.Equal(["C#", ".NET"], posting.Content.Tags);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_external_id_is_rejected(string externalId)
    {
        Assert.ThrowsAny<ArgumentException>(() => Posting.Create(1, externalId, TestContent.Valid(), FirstRun));
    }
}
