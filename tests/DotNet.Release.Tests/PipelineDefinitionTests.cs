using System.Text.RegularExpressions;
using Xunit;
using YamlDotNet.RepresentationModel;

namespace DotNet.Release.Tests;

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
        var firstUse = text.IndexOf("TOOL_PATH: $(toolPath)", StringComparison.Ordinal);

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
    /// Requirement 4: a dry run must be structurally incapable of publishing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the test that certifies the property everything else rests on, so it checks
    /// <b>nesting</b>, not position. An earlier version asserted only that each
    /// <c>publish-set.yml</c> reference appeared later in the file than the guard — which a
    /// sibling include placed after the guard would satisfy while being emitted
    /// unconditionally. Later in the file is not the same as inside the guard.
    /// </para>
    /// <para>
    /// Indentation is the structural property in YAML, so that is what is measured: every
    /// publish include must be more deeply indented than the guard, with no intervening line
    /// at or below the guard's own indentation (which would close it).
    /// </para>
    /// </remarks>
    [Fact]
    public void Every_publish_stage_is_nested_inside_the_publish_guard()
    {
        var lines = File.ReadAllLines(PipelinePath);

        var includes = Enumerable.Range(0, lines.Length)
            .Where(i => lines[i].Contains("publish-set.yml", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(includes);

        foreach (var include in includes)
        {
            Assert.True(
                EnclosingConditions(lines, include).Any(c =>
                    c.Contains("parameters.publishPackages, true", StringComparison.Ordinal)),
                $"release.yml line {include + 1} includes a publish stage that is not nested " +
                $"inside a publishPackages guard, so it would be emitted on a dry run: " +
                $"{lines[include].Trim()}");
        }
    }

    /// <summary>
    /// Walks outwards from a line, collecting every <c>${{ if }}</c> that encloses it.
    /// </summary>
    /// <remarks>
    /// Enclosure in YAML is indentation, so an ancestor is the nearest preceding line at a
    /// strictly smaller indent. This is what distinguishes "inside the guard" from "later in
    /// the file than the guard" — the distinction a positional check cannot make, and the one
    /// that decides whether a dry run can publish.
    /// </remarks>
    private static IReadOnlyList<string> EnclosingConditions(string[] lines, int index)
    {
        var conditions = new List<string>();
        var indent = Indent(lines[index]);

        for (var i = index - 1; i >= 0 && indent > 0; i--)
        {
            if (lines[i].Trim().Length == 0)
            {
                continue;
            }

            var candidate = Indent(lines[i]);
            if (candidate >= indent)
            {
                continue;
            }

            indent = candidate;

            if (lines[i].Contains("${{ if", StringComparison.Ordinal))
            {
                conditions.Add(lines[i]);
            }
        }

        return conditions;
    }

    private static int Indent(string line) => line.Length - line.TrimStart().Length;

    /// <summary>
    /// Queue-time parameters are attacker-controlled in a shared pipeline: anyone who can
    /// queue it supplies them, and the preparation tasks hold a production Maestro credential.
    /// Splicing them into script text lets a value containing a quote and a separator close
    /// the argument and run arbitrary commands. They must be passed as environment variables.
    /// </summary>
    [Theory]
    [InlineData("commitHash")]
    [InlineData("barBuildId")]
    [InlineData("includeFilters")]
    [InlineData("excludeFilters")]
    [InlineData("ghOwner")]
    [InlineData("ghRepo")]
    [InlineData("recoveryFilters")]
    public void Queue_time_parameters_are_never_spliced_into_script_text(string parameterName)
    {
        var reference = $"${{{{ parameters.{parameterName} }}}}";

        foreach (var path in new[]
        {
            PipelinePath,
            Path.Combine(RepoRoot, "eng", "pipelines", "stages", "publish-set.yml"),
        })
        {
            var lines = File.ReadAllLines(path);

            foreach (var i in ScriptLines(lines))
            {
                Assert.DoesNotContain(
                    reference,
                    lines[i]);
            }
        }
    }

    /// <summary>
    /// Indices of every line inside a PowerShell block scalar.
    /// </summary>
    /// <remarks>
    /// Only script bodies matter. A parameter in <c>name:</c>, in an <c>env:</c> binding, or
    /// in a template hand-off is inert; the same parameter inside <c>inlineScript: |</c> is
    /// executable text, and in a shared pipeline its value is supplied by whoever queued the
    /// run — into a task holding a production Maestro credential.
    /// </remarks>
    private static IReadOnlyList<int> ScriptLines(string[] lines)
    {
        var inside = new List<int>();

        for (var i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimEnd();
            if (!trimmed.EndsWith("inlineScript: |", StringComparison.Ordinal) &&
                !trimmed.EndsWith("- pwsh: |", StringComparison.Ordinal) &&
                !trimmed.EndsWith("script: |", StringComparison.Ordinal))
            {
                continue;
            }

            // A YAML block scalar's content indentation is set by its first non-empty line,
            // and the block ends at the first line indented less than that. Measuring from
            // the `|` line instead would wrongly swallow the step's sibling keys - including
            // the `env:` block, which is exactly where these parameters are supposed to live.
            var contentIndent = -1;

            for (var j = i + 1; j < lines.Length; j++)
            {
                if (lines[j].Trim().Length == 0)
                {
                    continue;
                }

                if (contentIndent < 0)
                {
                    contentIndent = Indent(lines[j]);
                }
                else if (Indent(lines[j]) < contentIndent)
                {
                    break;
                }

                inside.Add(j);
            }
        }

        return inside;
    }

    /// <summary>
    /// Every mutation in this pipeline is gated by a human. Promotion writes to BAR, so it
    /// needs its own approval rather than inheriting one from a later stage.
    /// </summary>
    [Fact]
    public void The_bar_promotion_stage_is_gated_by_its_own_approval()
    {
        var text = File.ReadAllText(PipelinePath);

        var promote = text[text.IndexOf("- stage: promote_workload_set", StringComparison.Ordinal)..];
        promote = promote[..promote.IndexOf("${{ if eq(parameters.publishPackages, true) }}", StringComparison.Ordinal)];

        Assert.Contains("ManualValidation@0", promote, StringComparison.Ordinal);
        Assert.Contains("pool: server", promote, StringComparison.Ordinal);
        Assert.Contains("dependsOn: promote_approval", promote, StringComparison.Ordinal);
    }

    /// <summary>
    /// A dry run and a real publish are otherwise indistinguishable in run history.
    /// </summary>
    [Fact]
    public void Runs_are_tagged_with_their_intent()
    {
        var text = File.ReadAllText(PipelinePath);

        Assert.Contains("##vso[build.addbuildtag]PUBLISH", text, StringComparison.Ordinal);
        Assert.Contains("##vso[build.addbuildtag]DRY-RUN", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// The exact dry-run contract: no task or script in the always-emitted pipeline contacts
    /// NuGet.org. All NuGet.org contact lives in publish-set.yml, whose every inclusion is
    /// guarded by publishPackages=true.
    /// </summary>
    [Fact]
    public void A_dry_run_contains_no_NuGet_org_contact()
    {
        var root = string.Join(
            "\n",
            File.ReadAllLines(PipelinePath)
                .Where(line => !line.TrimStart().StartsWith('#')));
        var publish = File.ReadAllText(
            Path.Combine(RepoRoot, "eng", "pipelines", "stages", "publish-set.yml"));

        // These are the only mechanisms this system uses to contact NuGet.org.
        Assert.DoesNotContain("1ES.PublishNuget@1", root, StringComparison.Ordinal);
        Assert.DoesNotContain("nuget.org (dotnetframework)", root, StringComparison.Ordinal);
        Assert.DoesNotContain("api.nuget.org", root, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("www.nuget.org/api/v2/package", root, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dotnet nuget push", root, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("NuGetCommand@2", root, StringComparison.Ordinal);

        Assert.Contains("1ES.PublishNuget@1", publish, StringComparison.Ordinal);
        Assert.Contains("nuget.org (dotnetframework)", publish, StringComparison.Ordinal);

        // The structural nesting test above proves this template is absent from a dry run.
        Every_publish_stage_is_nested_inside_the_publish_guard();
    }

    /// <summary>
    /// Preparation is the complete dry-run execution path. It may build the tool, read BAR,
    /// gather packages, stage them, and upload AzDO artifacts; it may not invoke either verb
    /// that queries NuGet.org.
    /// </summary>
    [Fact]
    public void The_dry_run_invokes_only_read_and_local_preparation_verbs()
    {
        var lines = File.ReadAllLines(PipelinePath);
        var promotionStage = Array.FindIndex(
            lines,
            line => line.Contains("- stage: promote_workload_set", StringComparison.Ordinal));
        var scripts = ScriptLines(lines)
            .Where(i => i < promotionStage)
            .Select(i => lines[i])
            .ToList();

        Assert.Contains(scripts, line => line.Contains("\"$env:TOOL_PATH\" plan", StringComparison.Ordinal));
        Assert.Contains(scripts, line => line.Contains("gather-drop", StringComparison.Ordinal));
        Assert.Contains(scripts, line => line.Contains("\"$env:TOOL_PATH\" stage", StringComparison.Ordinal));

        Assert.DoesNotContain(scripts, line => line.Contains("\"$env:TOOL_PATH\" filter", StringComparison.Ordinal));
        Assert.DoesNotContain(scripts, line => line.Contains("\"$env:TOOL_PATH\" verify", StringComparison.Ordinal));
        Assert.DoesNotContain(scripts, line => line.Contains("add-build-to-channel", StringComparison.Ordinal));
    }

    /// <summary>
    /// BAR channel promotion is a remote mutation and must be compile-time excluded unless
    /// both publishing and promotion were explicitly requested.
    /// </summary>
    [Fact]
    public void BAR_promotion_is_absent_from_a_dry_run()
    {
        var lines = File.ReadAllLines(PipelinePath);
        var promote = Array.FindIndex(
            lines,
            line => line.Contains("- stage: promote_workload_set", StringComparison.Ordinal));

        Assert.True(promote >= 0, "The workload promotion stage was not found.");

        var conditions = EnclosingConditions(lines, promote);
        Assert.Contains(
            conditions,
            condition =>
                condition.Contains("parameters.publishPackages, true", StringComparison.Ordinal) &&
                condition.Contains("parameters.promoteWorkloadSet, true", StringComparison.Ordinal));
    }

    /// <summary>
    /// gather-drop receives production credentials. A mutable "latest Darc" endpoint could
    /// change the executable after review, so every invocation must supply the checked-in
    /// version and that version must be tracked by Arcade dependency metadata.
    /// </summary>
    [Fact]
    public void Darc_is_pinned_to_a_tracked_dependency()
    {
        var pipeline = File.ReadAllLines(PipelinePath);
        var calls = pipeline
            .Where(line => line.Contains("Get-Darc", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(calls);
        Assert.All(
            calls,
            line => Assert.Contains("Get-Darc \"$(DarcVersion)\"", line, StringComparison.Ordinal));

        var versions = File.ReadAllText(Path.Combine(RepoRoot, "eng", "Versions.props"));
        var details = File.ReadAllText(Path.Combine(RepoRoot, "eng", "Version.Details.xml"));
        var configured = ((YamlSequenceNode)Pipeline["variables"])
            .Cast<YamlMappingNode>()
            .Single(variable =>
                variable.Children.ContainsKey("name") &&
                ((YamlScalarNode)variable["name"]).Value == "DarcVersion");

        Assert.Equal(
            "1.1.0-beta.26426.1",
            ((YamlScalarNode)configured["value"]).Value);

        Assert.Contains(
            "<MicrosoftDotNetDarcPackageVersion>1.1.0-beta.26426.1</MicrosoftDotNetDarcPackageVersion>",
            versions,
            StringComparison.Ordinal);
        Assert.Contains(
            "Name=\"Microsoft.DotNet.Darc\" Version=\"1.1.0-beta.26426.1\"",
            details,
            StringComparison.Ordinal);
        Assert.Contains(
            "<Sha>f271aca69409ef2985730f0de4983f5ba2423f28</Sha>",
            details,
            StringComparison.Ordinal);
    }
}
