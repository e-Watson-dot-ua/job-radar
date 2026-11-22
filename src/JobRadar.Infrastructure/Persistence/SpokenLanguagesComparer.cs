using JobRadar.Domain.Extractions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JobRadar.Infrastructure.Persistence;

/// <summary>
/// Tells EF when a spoken-language list changed (same entries in the same order = unchanged),
/// so updates to the converted jsonb column are saved. Entries are records, compared by value.
/// </summary>
internal sealed class SpokenLanguagesComparer() : ValueComparer<IReadOnlyList<SpokenLanguageRequirement>>(
    (a, b) => a == null ? b == null : b != null && a.SequenceEqual(b),
    list => list.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
    list => list.ToArray());
