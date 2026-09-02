using Xunit;

namespace DotNet.Release.Tests;

public class ReleasePlanSerializerTests
{
    private static ReleasePlan Plan() => StagePlanner.Create(TestData.Resolved(), TestData.Policy(),
        [TestData.Drop("SkiaSharp", "3.119.0"), TestData.Drop("HarfBuzzSharp", "8.3.1")], new StageOptions(), TestData.Now, TestData.ToolVersion);

    [Fact]
    public void Round_trips_without_losing_anything()
    {
        var original = Plan();
        var json = ReleasePlanSerializer.Serialize(original);

        var restored = ReleasePlanSerializer.DeserializePlan(json);
        // Compared through the serialized form rather than record equality: a record's
        // synthesized Equals uses reference equality for its IReadOnlyList members, so
        // Assert.Equal on the plans would compare list identities, not contents. The JSON is
        // the actual contract crossing the job boundary, so it is the right thing to assert.
        Assert.Equal(json, ReleasePlanSerializer.Serialize(restored));

        Assert.Equal(original.Source, restored.Source);
        Assert.Equal(
            original.AllPackages.Select(p => (p.Id, p.Version, p.NormalizedVersion, p.FileName, p.Sha256)),
            restored.AllPackages.Select(p => (p.Id, p.Version, p.NormalizedVersion, p.FileName, p.Sha256)));
    }

    /// <summary>
    /// The plan hash is the single pinned value carried into the production publish job, so
    /// serialization must be byte-stable for identical input.
    /// </summary>
    [Fact]
    public void Serialization_is_deterministic()
    {
        Assert.Equal(ReleasePlanSerializer.Serialize(Plan()), ReleasePlanSerializer.Serialize(Plan()));
        Assert.Equal(ReleasePlanSerializer.ComputeHash(Plan()), ReleasePlanSerializer.ComputeHash(Plan()));
    }

    [Fact]
    public void Hash_changes_when_any_package_changes()
    {
        var original = Plan();
        var tampered = original with
        {
            Sets = [original.Sets[0] with
            {
                Packages = [original.Sets[0].Packages[0] with { Sha256 = TestData.Hash("tampered") }],
            }],
        };

        Assert.NotEqual(ReleasePlanSerializer.ComputeHash(original), ReleasePlanSerializer.ComputeHash(tampered));
    }

    [Fact]
    public void VerifyAndDeserialize_accepts_the_matching_hash()
    {
        var json = ReleasePlanSerializer.Serialize(Plan());

        Assert.Equal(Plan().Source, ReleasePlanSerializer.VerifyAndDeserialize(json, ReleasePlanSerializer.ComputeHash(json)).Source);
    }

    [Fact]
    public void VerifyAndDeserialize_rejects_a_tampered_plan()
    {
        var json = ReleasePlanSerializer.Serialize(Plan());
        var hash = ReleasePlanSerializer.ComputeHash(json);

        Assert.Throws<DotNetReleaseException>(() => ReleasePlanSerializer.VerifyAndDeserialize(json.Replace("SkiaSharp", "EvilSharp", StringComparison.Ordinal),
            hash));
    }

    [Fact]
    public void Hash_comparison_ignores_case_and_surrounding_whitespace()
    {
        var json = ReleasePlanSerializer.Serialize(Plan());
        var hash = ReleasePlanSerializer.ComputeHash(json);

        Assert.NotNull(ReleasePlanSerializer.VerifyAndDeserialize(json, $"  {hash.ToUpperInvariant()}  "));
    }

    [Fact]
    public void Enums_serialize_as_readable_strings_for_the_audit_trail()
    {
        var json = ReleasePlanSerializer.Serialize(TestData.Resolved());

        Assert.Contains("\"GitHubRepository\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Resolved_release_round_trips()
    {
        var original = TestData.Resolved();

        var restored = ReleasePlanSerializer.DeserializeResolved(ReleasePlanSerializer.Serialize(original));

        Assert.Equal(original, restored);
    }

    /// <summary>
    /// The plan gates a production push, so an unrecognised schema must fail rather than be
    /// silently reinterpreted by an older tool.
    /// </summary>
    [Fact]
    public void An_unsupported_plan_schema_version_fails_closed()
    {
        var json = ReleasePlanSerializer.Serialize(Plan()).Replace("\"schemaVersion\": 1", "\"schemaVersion\": 2", StringComparison.Ordinal);

        Assert.Throws<DotNetReleaseException>(() => ReleasePlanSerializer.DeserializePlan(json));
    }

    [Fact]
    public void An_unsupported_resolved_schema_version_fails_closed()
    {
        var json = ReleasePlanSerializer.Serialize(TestData.Resolved()).Replace("\"schemaVersion\": 1", "\"schemaVersion\": 99", StringComparison.Ordinal);

        Assert.Throws<DotNetReleaseException>(() => ReleasePlanSerializer.DeserializeResolved(json));
    }

    [Fact]
    public void Malformed_plan_json_fails_closed()
    {
        Assert.Throws<DotNetReleaseException>(
            () => ReleasePlanSerializer.DeserializePlan("{ not json"));
    }

    [Fact]
    public void AllPackages_enumerates_sets_in_publication_order()
    {
        var plan = StagePlanner.Create(TestData.Resolved(workload: true, "dotnet/maui"), TestData.Policy(),
            [
                TestData.Drop("Microsoft.Maui.Controls", "10.0.0"),
                TestData.Drop("Microsoft.NET.Sdk.Maui.Manifest-10.0.100", "10.0.0"),
            ], new StageOptions(), TestData.Now, TestData.ToolVersion);

        Assert.Equal(
            ["Microsoft.Maui.Controls", "Microsoft.NET.Sdk.Maui.Manifest-10.0.100"],
            plan.AllPackages.Select(p => p.Id));
    }
}
