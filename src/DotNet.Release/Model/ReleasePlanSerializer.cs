using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.Release;

/// <summary>
/// Deterministic serialization and hashing for the plan files.
/// </summary>
/// <remarks>
/// Determinism matters because the plan's SHA-256 is the single pinned value carried across
/// the job boundary into the production publish job. Same plan in, same bytes out.
/// </remarks>
internal static class ReleasePlanSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string Serialize(ReleasePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return JsonSerializer.Serialize(plan, Options);
    }

    public static string Serialize(ResolvedRelease resolved)
    {
        ArgumentNullException.ThrowIfNull(resolved);
        return JsonSerializer.Serialize(resolved, Options);
    }

    public static string Serialize(PruneReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return JsonSerializer.Serialize(report, Options);
    }

    /// <summary>The only plan schema this tool understands.</summary>
    public const int SupportedSchemaVersion = 1;

    public static ReleasePlan DeserializePlan(string json)
    {
        var plan = Deserialize<ReleasePlan>(json, "release plan");

        // Fail closed on a schema this tool does not understand. Without this an older tool
        // reading a future plan would silently reinterpret the one file that gates a
        // production push.
        if (plan.SchemaVersion != SupportedSchemaVersion)
        {
            throw new DotNetReleaseException($"Unsupported release plan schemaVersion '{plan.SchemaVersion}'; " +
                $"this tool understands {SupportedSchemaVersion}.");
        }

        return plan;
    }

    public static ResolvedRelease DeserializeResolved(string json)
    {
        var resolved = Deserialize<ResolvedRelease>(json, "resolved release");

        if (resolved.SchemaVersion != SupportedSchemaVersion)
        {
            throw new DotNetReleaseException($"Unsupported resolved release schemaVersion '{resolved.SchemaVersion}'; " +
                $"this tool understands {SupportedSchemaVersion}.");
        }

        return resolved;
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

    /// <summary>SHA-256 of the plan's canonical bytes, as lower-case hex.</summary>
    public static string ComputeHash(ReleasePlan plan) => ComputeHash(Serialize(plan));

    /// <summary>
    /// SHA-256 of UTF-8 text, as lower-case hex.
    /// </summary>
    /// <remarks>The serializer writes UTF-8 without a byte-order mark.</remarks>
    public static string ComputeHash(string content) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

    /// <summary>
    /// Verifies that plan content matches the hash pinned by the preparing stage.
    /// </summary>
    public static ReleasePlan VerifyAndDeserialize(string json, string expectedHash)
    {
        var actual = ComputeHash(json);
        if (!string.Equals(actual, expectedHash?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new DotNetReleaseException($"Release plan hash '{actual}' does not match the prepared hash '{expectedHash}'.");
        }

        return DeserializePlan(json);
    }
}
