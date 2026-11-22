namespace JobRadar.Domain.Extractions;

/// <summary>A spoken language the posting requires; <c>Level</c> is <c>null</c> when not stated.</summary>
public sealed record SpokenLanguageRequirement(string Language, string? Level);
