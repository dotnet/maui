using Xunit;

namespace DotNet.Release.Tests;

public class ReleasePlanSerializerTests
{
    private static ReleasePlan Plan() => StagePlanner.Create(
        TestData.Resolved(),
        TestData.Policy(),
        [TestData.Drop("SkiaSharp", "3.119.0"), TestData.Drop("HarfBuzzSharp", "8.3.1")],
        new StageOptions(),
        TestData.Now,
        TestData.ToolVersion).Value;

    [Fact]
    public void Round_trips_without_losing_anything()
    {
        var original = Plan();
        var json = ReleasePlanSerializer.Serialize(original);

        var restored = ReleasePlanSerializer.DeserializePlan(json);
        Assert.True(restored.IsSuccess, string.Join("; ", restored.Errors));

        // Compared through the serialized form rather than record equality: a record's
        // synthesized Equals uses reference equality for its IReadOnlyList members, so
        // Assert.Equal on the plans would compare list identities, not contents. The JSON is
        // the actual contract crossing the job boundary, so it is the right thing to assert.
        Assert.Equal(json, ReleasePlanSerializer.Serialize(restored.Value));

        Assert.Equal(original.Source, restored.Value.Source);
        Assert.Equal(
            original.AllPackages.Select(p => (p.Id, p.Version, p.NormalizedVersion, p.FileName, p.Sha256)),
            restored.Value.AllPackages.Select(p => (p.Id, p.Version, p.NormalizedVersion, p.FileName, p.Sha256)));
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

        var result = ReleasePlanSerializer.VerifyAndDeserialize(json, ReleasePlanSerializer.ComputeHash(json));

        Assert.True(result.IsSuccess, string.Join("; ", result.Errors));
    }

    [Fact]
    public void VerifyAndDeserialize_rejects_a_tampered_plan()
    {
        var json = ReleasePlanSerializer.Serialize(Plan());
        var hash = ReleasePlanSerializer.ComputeHash(json);

        var result = ReleasePlanSerializer.VerifyAndDeserialize(
            json.Replace("SkiaSharp", "EvilSharp", StringComparison.Ordinal),
            hash);

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.PlanHashMismatch));
    }

    [Fact]
    public void Hash_comparison_ignores_case_and_surrounding_whitespace()
    {
        var json = ReleasePlanSerializer.Serialize(Plan());
        var hash = ReleasePlanSerializer.ComputeHash(json);

        Assert.True(ReleasePlanSerializer.VerifyAndDeserialize(json, $"  {hash.ToUpperInvariant()}  ").IsSuccess);
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

        Assert.True(restored.IsSuccess, string.Join("; ", restored.Errors));
        Assert.Equal(original, restored.Value);
    }

    /// <summary>
    /// The plan gates a production push, so an unrecognised schema must fail rather than be
    /// silently reinterpreted by an older tool.
    /// </summary>
    [Fact]
    public void An_unsupported_plan_schema_version_fails_closed()
    {
        var json = ReleasePlanSerializer.Serialize(Plan())
            .Replace("\"schemaVersion\": 1", "\"schemaVersion\": 2", StringComparison.Ordinal);

        var result = ReleasePlanSerializer.DeserializePlan(json);

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.PlanSchemaInvalid));
    }

    [Fact]
    public void An_unsupported_resolved_schema_version_fails_closed()
    {
        var json = ReleasePlanSerializer.Serialize(TestData.Resolved())
            .Replace("\"schemaVersion\": 1", "\"schemaVersion\": 99", StringComparison.Ordinal);

        var result = ReleasePlanSerializer.DeserializeResolved(json);

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.PlanSchemaInvalid));
    }

    [Fact]
    public void Malformed_plan_json_fails_closed()
    {
        var result = ReleasePlanSerializer.DeserializePlan("{ not json");

        Assert.True(result.IsFailure);
        Assert.True(result.HasError(ErrorCodes.PlanSchemaInvalid));
    }

    [Fact]
    public void AllPackages_enumerates_sets_in_publication_order()
    {
        var plan = StagePlanner.Create(
            TestData.Resolved(workload: true, "dotnet/maui"),
            TestData.Policy(),
            [
                TestData.Drop("Microsoft.Maui.Controls", "10.0.0"),
                TestData.Drop("Microsoft.NET.Sdk.Maui.Manifest-10.0.100", "10.0.0"),
            ],
            new StageOptions(),
            TestData.Now,
            TestData.ToolVersion).Value;

        Assert.Equal(
            ["Microsoft.Maui.Controls", "Microsoft.NET.Sdk.Maui.Manifest-10.0.100"],
            plan.AllPackages.Select(p => p.Id));
    }
}

public class AzurePipelineCommandTests
{
    /// <summary>
    /// The BAR ID is the only pipeline variable the tool emits, because the
    /// <c>darc gather-drop</c> step that follows needs the ID that <c>release plan</c>
    /// discovered. Everything else travels in the plan file.
    /// </summary>
    [Fact]
    public void BarId_is_emitted_as_an_output_variable()
    {
        Assert.Equal(
            "##vso[task.setvariable variable=BarId;isOutput=true]4242",
            AzurePipelineCommand.SetBarId(4242));
    }

    [Fact]
    public void PackagesToPublish_is_a_plain_variable()
    {
        Assert.Equal(
            "##vso[task.setvariable variable=NuGetPackagesToPublish]true",
            AzurePipelineCommand.SetPackagesToPublish(true));

        Assert.EndsWith("]false", AzurePipelineCommand.SetPackagesToPublish(false), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("100%", "100%AZP25")]
    [InlineData("a;b", "a%3Bb")]
    [InlineData("a]b", "a%5Db")]
    [InlineData("a\nb", "a%0Ab")]
    public void Values_that_would_break_the_command_syntax_are_escaped(string value, string expected)
    {
        Assert.Equal(
            $"##vso[task.setvariable variable=X]{expected}",
            AzurePipelineCommand.SetVariable("X", value));
    }

    [Fact]
    public void A_missing_variable_name_is_a_programming_error()
    {
        Assert.Throws<ArgumentException>(() => AzurePipelineCommand.SetVariable("  ", "x"));
    }
}
