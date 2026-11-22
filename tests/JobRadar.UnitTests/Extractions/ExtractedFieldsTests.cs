using JobRadar.Domain.Extractions;

namespace JobRadar.UnitTests.Extractions;

public class ExtractedFieldsTests
{
    private static readonly ExtractedFields NothingStated = new(
        Seniority: null,
        Skills: Skills.None,
        Salary: Salary.None,
        RemotePolicy: null,
        Country: null,
        City: null,
        SpokenLanguages: [],
        VisaSponsorship: null,
        CitizenshipOrClearanceRequired: null);

    [Fact]
    public void Nothing_stated_is_valid()
    {
        Assert.Empty(NothingStated.Validate());
    }

    [Fact]
    public void Fully_stated_valid_fields_pass()
    {
        var fields = NothingStated with
        {
            Seniority = Seniority.Senior,
            Skills = new Skills(["C#"], [".NET"], ["Azure"], ["Kubernetes"]),
            Salary = new Salary(60_000, 75_000, "EUR", SalaryPeriod.Year),
            RemotePolicy = RemotePolicy.Hybrid,
            Country = "DE",
            City = "Berlin",
            SpokenLanguages = [new SpokenLanguageRequirement("German", "B2"), new SpokenLanguageRequirement("English", null)],
        };

        Assert.Empty(fields.Validate());
    }

    [Fact]
    public void Negative_salary_is_rejected()
    {
        var fields = NothingStated with { Salary = new Salary(-1, null, "EUR", SalaryPeriod.Year) };

        Assert.Contains("salary.min: must not be negative.", fields.Validate());
    }

    [Fact]
    public void Salary_min_above_max_is_rejected()
    {
        var fields = NothingStated with { Salary = new Salary(80_000, 60_000, "EUR", SalaryPeriod.Year) };

        Assert.Contains("salary.min: must not be greater than salary.max.", fields.Validate());
    }

    [Theory]
    [InlineData("XYZ")]
    [InlineData("eur")]
    [InlineData("Euro")]
    public void Unknown_currency_is_rejected(string currency)
    {
        var fields = NothingStated with { Salary = new Salary(null, null, currency, null) };

        Assert.Single(fields.Validate());
    }

    [Theory]
    [InlineData("Germany")]
    [InlineData("de")]
    [InlineData("DEU")]
    public void Country_must_be_an_alpha2_code(string country)
    {
        var fields = NothingStated with { Country = country };

        Assert.Single(fields.Validate());
    }

    [Fact]
    public void Blank_skill_is_rejected()
    {
        var fields = NothingStated with { Skills = Skills.None with { Frameworks = ["ASP.NET Core", " "] } };

        Assert.Equal(["skills.frameworks: contains a blank entry."], fields.Validate());
    }

    [Fact]
    public void All_problems_are_reported_together()
    {
        var fields = NothingStated with
        {
            Salary = new Salary(80_000, 60_000, "XYZ", null),
            Country = "Germany",
        };

        Assert.Equal(3, fields.Validate().Count);
    }
}
