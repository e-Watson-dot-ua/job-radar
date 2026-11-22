using static JobRadar.Domain.Validation.ValidationRules;

namespace JobRadar.Domain.Extractions;

/// <summary>Skills named in the posting, by kind; each list is empty when none are named.</summary>
public sealed record Skills(
    IReadOnlyList<string> PrimaryLanguages,
    IReadOnlyList<string> Frameworks,
    IReadOnlyList<string> CloudPlatforms,
    IReadOnlyList<string> OtherSkills)
{
    public static Skills None { get; } = new([], [], [], []);

    /// <summary>
    /// Problems found; empty when valid. Every list is checked the same way by <c>Check</c>:
    /// it must be present (empty is fine) and must not contain blank entries.
    /// </summary>
    public IEnumerable<string> Validate() =>
    [
        .. Check("primaryLanguages", PrimaryLanguages),
        .. Check("frameworks", Frameworks),
        .. Check("cloudPlatforms", CloudPlatforms),
        .. Check("otherSkills", OtherSkills),
    ];

    private static IEnumerable<string> Check(string name, IReadOnlyList<string>? list) =>
    [
        .. Rule(list is null, $"skills.{name}: missing."),
        .. Rule(list?.Any(string.IsNullOrWhiteSpace) == true, $"skills.{name}: contains a blank entry."),
    ];
}
