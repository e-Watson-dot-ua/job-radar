namespace JobRadar.Domain.Validation;

/// <summary>
/// Building block for validation written as a list of rules.
/// A <c>Validate()</c> method returns a collection expression where each line is one rule:
/// <code>
/// [
///     .. Rule(Min &lt; 0, "salary.min: must not be negative."),
///     .. Rule(Min &gt; Max, "salary.min: must not be greater than salary.max."),
/// ]
/// </code>
/// <c>..</c> spreads whatever the rule returns: its message when broken, nothing otherwise.
/// Messages start with the field path (e.g. <c>salary.min:</c>) so a stored error says what to fix.
/// Validation returns problems instead of throwing because invalid LLM output is expected now and then.
/// </summary>
internal static class ValidationRules
{
    /// <summary>The message when the rule is broken; nothing when it holds.</summary>
    public static IEnumerable<string> Rule(bool broken, string message) => broken ? [message] : [];
}
