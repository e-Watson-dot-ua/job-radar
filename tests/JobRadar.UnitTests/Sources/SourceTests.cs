using JobRadar.Domain.Sources;

namespace JobRadar.UnitTests.Sources;

public class SourceTests
{
    private static readonly Uri BaseUrl = new("https://www.arbeitnow.com/api/job-board-api");
    private static readonly Uri AttributionUrl = new("https://www.arbeitnow.com");

    [Fact]
    public void Valid_input_creates_source_with_trimmed_name()
    {
        var source = new Source(1, "  Arbeitnow  ", BaseUrl, AttributionUrl);

        Assert.Equal(1, source.Id);
        Assert.Equal("Arbeitnow", source.Name);
        Assert.Equal(BaseUrl, source.BaseUrl);
        Assert.Equal(AttributionUrl, source.AttributionUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_name_is_rejected(string name)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Source(1, name, BaseUrl, AttributionUrl));
    }

    [Fact]
    public void Relative_url_is_rejected()
    {
        var relative = new Uri("/jobs", UriKind.Relative);

        var error = Assert.Throws<ArgumentException>(() => new Source(1, "Arbeitnow", BaseUrl, relative));
        Assert.Equal("attributionUrl", error.ParamName);
    }
}
