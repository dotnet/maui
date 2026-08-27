using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DotNet.Release.Core;

/// <summary>
/// Deterministic serialization and hashing for the plan files.
/// </summary>
/// <remarks>
/// Determinism matters because the plan's SHA-256 is the single pinned value carried across
/// the job boundary into the production publish job. Same plan in, same bytes out.
/// </remarks>
public static class ReleasePlanSerializer
{
    public static string Serialize(ReleasePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return JsonSerializer.Serialize(plan, PlanJsonContext.Default.ReleasePlan);
    }

    public static string Serialize(ResolvedRelease resolved)
    {
        ArgumentNullException.ThrowIfNull(resolved);
        return JsonSerializer.Serialize(resolved, PlanJsonContext.Default.ResolvedRelease);
    }

    public static string Serialize(FilterReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return JsonSerializer.Serialize(report, PlanJsonContext.Default.FilterReport);
    }

    public static Result<ReleasePlan> DeserializePlan(string json) =>
        Deserialize(json, PlanJsonContext.Default.ReleasePlan, "release plan");

    public static Result<ResolvedRelease> DeserializeResolved(string json) =>
        Deserialize(json, PlanJsonContext.Default.ResolvedRelease, "resolved release");

    public static string Serialize(ReleaseSetMarker marker)
    {
        ArgumentNullException.ThrowIfNull(marker);
        return JsonSerializer.Serialize(marker, PlanJsonContext.Default.ReleaseSetMarker);
    }

    public static Result<ReleaseSetMarker> DeserializeSetMarker(string json) =>
        Deserialize(json, PlanJsonContext.Default.ReleaseSetMarker, "release set marker");

    private static Result<T> Deserialize<T>(string json, JsonTypeInfo<T> typeInfo, string what)
        where T : class
    {
        try
        {
            var value = JsonSerializer.Deserialize(json, typeInfo);
            return value is null
                ? Result<T>.Failure(ErrorCodes.PlanSchemaInvalid, $"The {what} is empty.")
                : Result<T>.Success(value);
        }
        catch (JsonException ex)
        {
            return Result<T>.Failure(ErrorCodes.PlanSchemaInvalid, $"The {what} is not valid: {ex.Message}");
        }
    }

    /// <summary>SHA-256 of the plan's canonical bytes, as lower-case hex.</summary>
    public static string ComputeHash(ReleasePlan plan) => ComputeHash(Serialize(plan));

    /// <summary>SHA-256 of UTF-8 text, as lower-case hex.</summary>
    public static string ComputeHash(string content) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

    /// <summary>
    /// Verifies that plan content matches the hash pinned by the preparing stage.
    /// </summary>
    public static Result<ReleasePlan> VerifyAndDeserialize(string json, string expectedHash)
    {
        var actual = ComputeHash(json);
        if (!string.Equals(actual, expectedHash?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Result<ReleasePlan>.Failure(
                ErrorCodes.PlanHashMismatch,
                $"Release plan hash '{actual}' does not match the prepared hash '{expectedHash}'.");
        }

        return DeserializePlan(json);
    }
}

[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    UseStringEnumConverter = true,
    WriteIndented = true)]
[JsonSerializable(typeof(ReleasePlan))]
[JsonSerializable(typeof(ResolvedRelease))]
[JsonSerializable(typeof(FilterReport))]
[JsonSerializable(typeof(ReleaseSetMarker))]
internal sealed partial class PlanJsonContext : JsonSerializerContext;
