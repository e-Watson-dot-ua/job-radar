using System.Security.Cryptography;
using System.Text;

namespace JobRadar.Domain.Postings;

/// <summary>
/// Change detector for postings: SHA-256 of title and description text, as lowercase hex.
/// Same input always gives the same hash; any change in either part gives a different one.
/// </summary>
public static class ContentHasher
{
    public static string Compute(string title, string descriptionText)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(descriptionText);

        var bytes = Encoding.UTF8.GetBytes(title + "\n" + descriptionText);
        return Convert.ToHexStringLower(SHA256.HashData(bytes));
    }
}
