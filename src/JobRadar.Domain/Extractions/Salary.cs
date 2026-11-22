using static JobRadar.Domain.Validation.ValidationRules;

namespace JobRadar.Domain.Extractions;

/// <summary>
/// Salary as stated in the posting; any part may be <c>null</c>. Currency is an ISO 4217 code.
/// A posting without salary information has <see cref="None"/>, never a <c>null</c> salary.
/// </summary>
public sealed record Salary(decimal? Min, decimal? Max, string? Currency, SalaryPeriod? Period)
{
    public static Salary None { get; } = new(null, null, null, null);

    /// <summary>
    /// Problems found; empty when valid. One line per rule: amounts must not be negative,
    /// min must not exceed max, and a stated currency must be a known ISO 4217 code.
    /// </summary>
    public IEnumerable<string> Validate() =>
    [
        .. Rule(Min < 0, "salary.min: must not be negative."),
        .. Rule(Max < 0, "salary.max: must not be negative."),
        .. Rule(Min > Max, "salary.min: must not be greater than salary.max."),
        .. Rule(Currency is not null && !CurrencyCodes.IsKnown(Currency), $"salary.currency: '{Currency}' is not an ISO 4217 code."),
    ];
}
