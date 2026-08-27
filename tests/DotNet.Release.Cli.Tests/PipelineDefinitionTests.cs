using System.Text.RegularExpressions;
using DotNet.Release.Core;
using Xunit;
using YamlDotNet.RepresentationModel;

namespace DotNet.Release.Cli.Tests;

/// <summary>
/// Guards the release pipeline against drifting away from the checked-in policy.
/// </summary>
/// <remarks>
/// <para>
/// <c>eng/pipelines/release.yml</c> is the shared release system: it is hooked up once in
/// Azure DevOps and everyone triggers it with parameters. Nothing consumes this repository as
/// a template, so this file is the only place these mistakes can be caught before a release.
/// </para>
/// <para>
/// The pipeline necessarily restates two things the policy already knows — the set of
/// releasable repositories, and which of them are workload repositories — because Azure
/// DevOps needs both at <i>compile</i> time: one to populate the run dialog, the other to
/// decide which stages exist. Restating them is unavoidable; letting them drift is not.
/// <c>WORKLOAD_MISMATCH</c> catches a mismatch at release time, and these tests catch it at
/// build time.
/// </para>
/// </remarks>
public class PipelineDefinitionTests
{
    private static string RepoRoot
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
            {
                directory = directory.Parent;
            }

            Assert.NotNull(directory);
            return directory.FullName;
        }
    }

    private static string PipelinePath => Path.Combine(RepoRoot, "eng", "pipelines", "release.yml");

    private static YamlMappingNode Pipeline
    {
        get
        {
            var stream = new YamlStream();
            using var reader = new StreamReader(PipelinePath);
            stream.Load(reader);
            return (YamlMappingNode)stream.Documents[0].RootNode;
        }
    }

    private static ReleasePolicy Policy =>
        ReleasePolicy.Parse(File.ReadAllText(Path.Combine(RepoRoot, "config", "repositories.json"))).Value;

    private static YamlMappingNode Parameter(string name) =>
        ((YamlSequenceNode)Pipeline["parameters"])
            .Cast<YamlMappingNode>()
            .Single(p => ((YamlScalarNode)p["name"]).Value == name);

    // ---- it must actually be a runnable pipeline, not a template fragment ----

    /// <summary>
    /// This is an entry point that gets hooked up in Azure DevOps, not a fragment someone
    /// extends. Missing any of these means it cannot be attached to a pipeline definition.
    /// </summary>
    [Theory]
    [InlineData("trigger")]
    [InlineData("pr")]
    [InlineData("parameters")]
    [InlineData("resources")]
    [InlineData("extends")]
    public void The_pipeline_is_a_complete_runnable_definition(string key)
    {
        Assert.True(Pipeline.Children.ContainsKey(key), $"release.yml must declare '{key}'.");
    }

    [Fact]
    public void It_never_triggers_itself()
    {
        // A release must only ever happen because a human asked for it.
        Assert.Equal("none", ((YamlScalarNode)Pipeline["trigger"]).Value);
        Assert.Equal("none", ((YamlScalarNode)Pipeline["pr"]).Value);
    }

    [Fact]
    public void It_extends_the_1ES_template_which_owns_publishing()
    {
        var repositories = ((YamlSequenceNode)Pipeline["resources"]["repositories"])
            .Cast<YamlMappingNode>()
            .Select(r => ((YamlScalarNode)r["repository"]).Value);

        Assert.Contains("1ESPipelineTemplates", repositories);
    }

    // ---- the run dialog must match the policy ----

    /// <summary>
    /// The dropdown an operator picks from must be exactly the set of repositories the policy
    /// allows. An entry here that the policy rejects wastes a release attempt; a repository in
    /// the policy that is missing here cannot be released at all.
    /// </summary>
    [Fact]
    public void The_repository_dropdown_matches_the_release_policy()
    {
        var offered = ((YamlSequenceNode)Parameter("ghRepo")["values"])
            .Cast<YamlScalarNode>()
            .Select(v => $"dotnet/{v.Value}")
            .OrderBy(v => v, StringComparer.Ordinal);

        var allowed = Policy.Repositories
            .Select(r => r.Repository.FullName)
            .OrderBy(v => v, StringComparer.Ordinal);

        Assert.Equal(allowed, offered);
    }

    /// <summary>
    /// The compile-time workload list decides which stages exist. If it disagrees with the
    /// policy, a workload release would publish packs and manifests through a single stage
    /// and lose the ordering guarantee that makes the release recoverable.
    /// </summary>
    [Fact]
    public void The_compile_time_workload_list_matches_the_release_policy()
    {
        var expression = ((YamlSequenceNode)Pipeline["variables"])
            .Cast<YamlMappingNode>()
            .Single(v => v.Children.ContainsKey("name") && ((YamlScalarNode)v["name"]).Value == "isWorkload")
            ["value"].ToString();

        var declared = Regex
            .Matches(expression, @"'(?<repo>[a-z0-9\-\.]+)'")
            .Select(m => m.Groups["repo"].Value)
            .Where(v => v != "dotnet")
            .Select(v => $"dotnet/{v}")
            .OrderBy(v => v, StringComparer.Ordinal);

        var workload = Policy.Repositories
            .Where(r => r.Workload)
            .Select(r => r.Repository.FullName)
            .OrderBy(v => v, StringComparer.Ordinal);

        Assert.Equal(workload, declared);
    }

    // ---- safe defaults ----

    /// <summary>
    /// The default run must be a dry run. This pipeline is shared and publishes to NuGet.org,
    /// where packages are immutable; someone opening the run dialog and pressing Run without
    /// reading it must not publish anything.
    /// </summary>
    [Fact]
    public void Publishing_is_opt_in()
    {
        Assert.Equal("false", ((YamlScalarNode)Parameter("publishPackages")["default"]).Value, ignoreCase: true);
        Assert.Equal("false", ((YamlScalarNode)Parameter("promoteWorkloadSet")["default"]).Value, ignoreCase: true);
    }

    /// <summary>
    /// A release is always pinned to an exact commit, so there is no safe default for it.
    /// Omitting the default makes Azure DevOps require a value at queue time.
    /// </summary>
    [Fact]
    public void The_commit_must_be_supplied_for_every_run()
    {
        Assert.False(Parameter("commitHash").Children.ContainsKey("default"));
    }

    [Fact]
    public void Every_operator_facing_parameter_is_labelled()
    {
        var unlabelled = ((YamlSequenceNode)Pipeline["parameters"])
            .Cast<YamlMappingNode>()
            .Where(p => ((YamlScalarNode)p["type"]).Value != "object")
            .Where(p => !p.Children.ContainsKey("displayName"))
            .Select(p => ((YamlScalarNode)p["name"]).Value);

        Assert.Empty(unlabelled);
    }

    // ---- the pieces it references must exist ----

    [Fact]
    public void Referenced_templates_exist()
    {
        var text = File.ReadAllText(PipelinePath);

        foreach (Match match in Regex.Matches(text, @"- template:\s*(?<path>/[^\s@]+)"))
        {
            var relative = match.Groups["path"].Value.TrimStart('/');
            Assert.True(
                File.Exists(Path.Combine(RepoRoot, relative)),
                $"release.yml references '{relative}', which does not exist.");
        }
    }

    /// <summary>
    /// The publish stage reads the plan hash across a stage boundary, which only resolves if
    /// the stage name, job name and step name in the expression all match the producer.
    /// A typo here yields an empty variable and an integrity check that compares against
    /// nothing.
    /// </summary>
    [Fact]
    public void The_publish_stage_reads_the_plan_hash_from_the_right_place()
    {
        var publish = File.ReadAllText(Path.Combine(RepoRoot, "eng", "pipelines", "stages", "publish-set.yml"));

        var reference = Regex.Match(
            publish,
            @"stageDependencies\.(?<stage>\w+)\.(?<job>\w+)\.outputs\['(?<step>\w+)\.(?<variable>\w+)'\]");

        Assert.True(reference.Success, "publish-set.yml must read the plan hash from the prepare stage.");

        var pipeline = File.ReadAllText(PipelinePath);
        Assert.Contains($"- stage: {reference.Groups["stage"].Value}", pipeline, StringComparison.Ordinal);
        Assert.Contains($"- job: {reference.Groups["job"].Value}", pipeline, StringComparison.Ordinal);
        Assert.Contains($"name: {reference.Groups["step"].Value}", pipeline, StringComparison.Ordinal);
        Assert.Contains($"variable={reference.Groups["variable"].Value}", pipeline, StringComparison.Ordinal);
    }

    /// <summary>
    /// The tool is built in the prepare stage and must be on PATH-like variable for every
    /// step that invokes it; the publish job re-derives it from the artifact instead.
    /// </summary>
    [Fact]
    public void The_tool_is_built_before_it_is_invoked()
    {
        var text = File.ReadAllText(PipelinePath);

        var setsToolPath = text.IndexOf("variable=toolPath", StringComparison.Ordinal);
        var firstUse = text.IndexOf("\"$(toolPath)\"", StringComparison.Ordinal);

        Assert.True(setsToolPath > 0, "release.yml must publish the tool and set toolPath.");
        Assert.True(firstUse > setsToolPath, "release.yml invokes the tool before building it.");
        Assert.Contains("dotnet publish", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// The publish job runs `checkout: none`, so the tool can only come from the artifact.
    /// </summary>
    [Fact]
    public void The_publish_job_takes_no_checkout_and_runs_as_a_production_release_job()
    {
        var publish = File.ReadAllText(Path.Combine(RepoRoot, "eng", "pipelines", "stages", "publish-set.yml"));

        Assert.Contains("checkout: none", publish, StringComparison.Ordinal);
        Assert.Contains("type: releaseJob", publish, StringComparison.Ordinal);
        Assert.Contains("isProduction: true", publish, StringComparison.Ordinal);
        Assert.Contains("1ES.PublishNuget@1", publish, StringComparison.Ordinal);
    }

    /// <summary>
    /// The manual gate must be its own agentless job: ManualValidation@0 requires
    /// `pool: server`, and an agentless job cannot download artifacts or run scripts.
    /// </summary>
    [Fact]
    public void The_approval_gate_is_a_separate_agentless_job()
    {
        var publish = File.ReadAllText(Path.Combine(RepoRoot, "eng", "pipelines", "stages", "publish-set.yml"));

        Assert.Contains("pool: server", publish, StringComparison.Ordinal);
        Assert.Contains("ManualValidation@0", publish, StringComparison.Ordinal);
        Assert.Contains("dependsOn: approval", publish, StringComparison.Ordinal);
    }

    /// <summary>
    /// Requirement 4: a dry run must be structurally incapable of publishing. The publish
    /// stages are emitted only when publishPackages is true, so on a dry run they do not
    /// exist in the expanded YAML at all.
    /// </summary>
    [Fact]
    public void Publish_stages_are_excluded_at_compile_time_on_a_dry_run()
    {
        var text = File.ReadAllText(PipelinePath);

        Assert.Contains("${{ if eq(parameters.publishPackages, true) }}", text, StringComparison.Ordinal);

        // Every publish-set include must sit inside that guard.
        var guard = text.IndexOf("${{ if eq(parameters.publishPackages, true) }}", StringComparison.Ordinal);
        foreach (Match match in Regex.Matches(text, @"publish-set\.yml"))
        {
            Assert.True(match.Index > guard, "A publish stage is emitted outside the publishPackages guard.");
        }
    }
}
