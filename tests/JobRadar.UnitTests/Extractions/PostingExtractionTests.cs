using JobRadar.Domain.Extractions;

namespace JobRadar.UnitTests.Extractions;

public class PostingExtractionTests
{
    private static readonly Guid PostingId = Guid.CreateVersion7();
    private static readonly DateTimeOffset FirstRun = new(2026, 11, 22, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset SecondRun = FirstRun.AddHours(6);
    private const string RawOutput = """{"seniority":"senior"}""";

    private static readonly ExtractedFields Fields = new(
        Seniority: Seniority.Senior,
        Skills: new Skills(["C#"], [".NET"], [], []),
        Salary: null,
        RemotePolicy: RemotePolicy.Remote,
        Country: "DE",
        City: null,
        SpokenLanguages: [],
        VisaSponsorship: null,
        CitizenshipOrClearanceRequired: null);

    [Fact]
    public void Create_stores_the_result()
    {
        var extraction = PostingExtraction.Create(PostingId, "extraction.v1", "qwen3:8b", Fields, RawOutput, FirstRun);

        Assert.Equal(PostingId, extraction.PostingId);
        Assert.Equal("extraction.v1", extraction.PromptVersion);
        Assert.Equal("qwen3:8b", extraction.ModelName);
        Assert.Equal(Fields, extraction.Fields);
        Assert.Equal(RawOutput, extraction.RawModelOutput);
        Assert.Equal(FirstRun, extraction.ExtractedAt);
    }

    [Fact]
    public void Invalid_fields_are_rejected_with_all_problems_listed()
    {
        var invalid = Fields with { Country = "Germany", Salary = new Salary(-1, null, null, null) };

        var error = Assert.Throws<ArgumentException>(
            () => PostingExtraction.Create(PostingId, "extraction.v1", "qwen3:8b", invalid, RawOutput, FirstRun));

        Assert.Contains("country:", error.Message);
        Assert.Contains("salary.min:", error.Message);
    }

    [Theory]
    [InlineData("", "qwen3:8b")]
    [InlineData("extraction.v1", " ")]
    public void Blank_prompt_version_or_model_is_rejected(string promptVersion, string modelName)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => PostingExtraction.Create(PostingId, promptVersion, modelName, Fields, RawOutput, FirstRun));
    }

    [Fact]
    public void Empty_posting_id_is_rejected()
    {
        Assert.Throws<ArgumentException>(
            () => PostingExtraction.Create(Guid.Empty, "extraction.v1", "qwen3:8b", Fields, RawOutput, FirstRun));
    }

    [Fact]
    public void Replace_updates_the_result_and_keeps_the_key()
    {
        var extraction = PostingExtraction.Create(PostingId, "extraction.v1", "qwen3:8b", Fields, RawOutput, FirstRun);
        var newFields = Fields with { Seniority = Seniority.Lead };

        extraction.Replace("qwen3:14b", newFields, """{"seniority":"lead"}""", SecondRun);

        Assert.Equal(PostingId, extraction.PostingId);
        Assert.Equal("extraction.v1", extraction.PromptVersion);
        Assert.Equal("qwen3:14b", extraction.ModelName);
        Assert.Equal(Seniority.Lead, extraction.Fields.Seniority);
        Assert.Equal(SecondRun, extraction.ExtractedAt);
    }
}
