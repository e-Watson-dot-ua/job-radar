using JobRadar.Domain.Postings;

namespace JobRadar.UnitTests.Postings;

public class ContentHasherTests
{
    [Fact]
    public void Same_input_gives_same_hash()
    {
        var first = ContentHasher.Compute("Backend Developer", "We build things.");
        var second = ContentHasher.Compute("Backend Developer", "We build things.");

        Assert.Equal(first, second);
        Assert.Matches("^[0-9a-f]{64}$", first);
    }

    [Theory]
    [InlineData("Frontend Developer", "We build things.")]
    [InlineData("Backend Developer", "We build other things.")]
    public void Changed_title_or_text_gives_different_hash(string title, string text)
    {
        var original = ContentHasher.Compute("Backend Developer", "We build things.");

        Assert.NotEqual(original, ContentHasher.Compute(title, text));
    }

    [Fact]
    public void Title_and_text_boundary_matters()
    {
        // "ab" + "c" must not collide with "a" + "bc".
        Assert.NotEqual(ContentHasher.Compute("ab", "c"), ContentHasher.Compute("a", "bc"));
    }
}
