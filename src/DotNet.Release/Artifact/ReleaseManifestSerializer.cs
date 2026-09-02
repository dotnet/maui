using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>
/// Deterministic serialization and hashing for release manifests.
/// </summary>
/// <remarks>
/// Determinism matters because the manifest's SHA-256 is the single pinned value carried
/// across the job boundary into the production publish job. Same manifest in, same bytes out.
/// </remarks>
internal static class ReleaseManifestSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string Serialize(ReleaseManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        return JsonSerializer.Serialize(manifest, Options);
    }

    /// <summary>The only manifest schema this tool understands.</summary>
    public const int SupportedSchemaVersion = 1;

    public static ReleaseManifest DeserializeManifest(string json)
    {
        var manifest = Deserialize<ReleaseManifest>(json, "release manifest");

        // Fail closed on a schema this tool does not understand. Without this an older tool
        // reading a future manifest would silently reinterpret the one file that gates a
        // production push.
        if (manifest.SchemaVersion != SupportedSchemaVersion)
        {
            throw new DotNetReleaseException($"Unsupported release manifest schemaVersion '{manifest.SchemaVersion}'; " +
                $"this tool understands {SupportedSchemaVersion}.");
        }

        return manifest;
    }

    private static T Deserialize<T>(string json, string what)
        where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, Options) ?? throw new DotNetReleaseException($"The {what} is empty.");
        }
        catch (JsonException ex)
        {
            throw new DotNetReleaseException($"The {what} is not valid: {ex.Message}");
        }
    }

    /// <summary>SHA-256 of the manifest's canonical bytes, as lower-case hex.</summary>
    public static string ComputeHash(ReleaseManifest manifest) => ComputeHash(Serialize(manifest));

    /// <summary>
    /// SHA-256 of UTF-8 text, as lower-case hex.
    /// </summary>
    /// <remarks>The serializer writes UTF-8 without a byte-order mark.</remarks>
    public static string ComputeHash(string content) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

    /// <summary>
    /// Verifies that manifest content matches the hash pinned by the preparing stage.
    /// </summary>
    public static ReleaseManifest VerifyAndDeserialize(string json, string expectedHash)
    {
        var actual = ComputeHash(json);
        if (!string.Equals(actual, expectedHash?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new DotNetReleaseException($"Release manifest hash '{actual}' does not match the prepared hash '{expectedHash}'.");
        }

        return DeserializeManifest(json);
    }
}
