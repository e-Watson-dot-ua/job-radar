using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JobRadar.Infrastructure.Persistence;

/// <summary>Stores an enum as lowercase text (<c>Pending</c> → <c>pending</c>), matching the brief's column values.</summary>
internal sealed class LowercaseEnumConverter<TEnum>() : ValueConverter<TEnum, string>(
    value => value.ToString().ToLowerInvariant(),
    text => Enum.Parse<TEnum>(text, true))
    where TEnum : struct, Enum;
