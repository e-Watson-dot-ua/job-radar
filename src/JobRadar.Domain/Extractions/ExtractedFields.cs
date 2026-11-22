using static JobRadar.Domain.Validation.ValidationRules;

namespace JobRadar.Domain.Extractions;

/// <summary>
/// Structured fields the LLM extracted from one posting; anything the posting does not state is
/// <c>null</c> or empty, never a guess. Country is an ISO 3166-1 alpha-2 code.
/// </summary>
public sealed record ExtractedFields(
    Seniority? Seniority,
    Skills Skills,
    Salary Salary,
    RemotePolicy? RemotePolicy,
    string? Country,
    string? City,
    IReadOnlyList<SpokenLanguageRequirement> SpokenLanguages,
    bool? VisaSponsorship,
    bool? CitizenshipOrClearanceRequired)
{
    private ExtractedFields() : this(null, Skills.None, Salary.None, null, null, null, [], null, null) { } // for EF Core

    /// <summary>
    /// Problems found; empty when valid. LLM output breaking these rules is expected now and then,
    /// so problems are returned (and stored as the extraction error) instead of thrown.
    /// Each line is one rule: skills and salary check themselves, <c>Rule</c> adds its message only when broken.
    /// </summary>
    public IReadOnlyList<string> Validate() =>
    [
        .. Skills?.Validate() ?? ["skills: missing."],
        .. Salary?.Validate() ?? ["salary: missing."],
        .. Rule(Country is not null && !CountryCodes.IsKnown(Country), $"country: '{Country}' is not an ISO 3166-1 alpha-2 code."),
        .. Rule(City is not null && string.IsNullOrWhiteSpace(City), "city: blank; use null when not stated."),
        .. Rule(SpokenLanguages is null, "spokenLanguages: missing."),
        .. Rule(SpokenLanguages?.Any(l => l is null || string.IsNullOrWhiteSpace(l.Language)) == true,
                "spokenLanguages: contains an entry without a language."),
    ];
}
