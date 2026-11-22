using JobRadar.Domain.Postings;

namespace JobRadar.UnitTests.Postings;

public class PostingContentTests
{
    [Fact]
    public void Valid_content_is_created()
    {
        var content = TestContent.Valid();

        Assert.Equal("Backend Developer", content.Title);
        Assert.Null(content.RemoteFlag);
        Assert.Equal(["C#", ".NET"], content.Tags);
    }

    [Fact]
    public void Blank_title_is_rejected()
    {
        var error = Assert.Throws<ArgumentException>(() => TestContent.Valid(title: " "));
        Assert.Contains("Title", error.Message);
    }

    [Fact]
    public void Relative_url_is_rejected_also_when_copied()
    {
        // 'with' copies bypass the constructor, so validation must live in the properties.
        var error = Assert.Throws<ArgumentException>(
            () => TestContent.Valid() with { Url = new Uri("/jobs/1", UriKind.Relative) });
        Assert.Contains("Url", error.Message);
    }

    [Fact]
    public void Tags_are_copied_so_later_changes_to_the_source_list_do_not_leak_in()
    {
        var tags = new List<string> { "C#" };
        var content = TestContent.Valid() with { Tags = tags };

        tags.Add("Go");

        Assert.Equal(["C#"], content.Tags);
    }
}
