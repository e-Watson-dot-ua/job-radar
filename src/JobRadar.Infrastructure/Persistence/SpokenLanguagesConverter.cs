using System.Text.Json;
using JobRadar.Domain.Extractions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JobRadar.Infrastructure.Persistence;

/// <summary>
/// Stores the spoken-language list as JSON text, e.g. <c>[{"language":"German","level":"B2"}]</c>.
/// Needed because EF Core 10 cannot map a JSON list inside a column-mapped complex type
/// (https://github.com/dotnet/efcore/issues/36558); EF sees one plain jsonb value instead.
/// </summary>
internal sealed class SpokenLanguagesConverter() : ValueConverter<IReadOnlyList<SpokenLanguageRequirement>, string>(
    list => Serialize(list),
    json => Deserialize(json))
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    private static string Serialize(IReadOnlyList<SpokenLanguageRequirement> list) =>
        JsonSerializer.Serialize(list, Options);

    private static IReadOnlyList<SpokenLanguageRequirement> Deserialize(string json) =>
        JsonSerializer.Deserialize<SpokenLanguageRequirement[]>(json, Options) ?? [];
}
