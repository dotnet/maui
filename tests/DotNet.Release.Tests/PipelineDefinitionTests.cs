using System.Text.RegularExpressions;
using Xunit;
using YamlDotNet.RepresentationModel;

namespace DotNet.Release.Tests;

/// <summary>Verifies the shared pipeline's operator contract and safety barriers.</summary>
/// <remarks>
/// <c>eng/pipelines/ci-official-release.yml</c> is the Azure DevOps entry point. These tests
/// bind its repository choices to policy and verify the dry-run, approval, ordering, and
/// credential boundaries.
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

    private static string PipelinePath =>
        Path.Combine(RepoRoot, "eng", "pipelines", "ci-official-release.yml");

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

    [Fact]
    public void Workload_classification_comes_only_from_repository_policy()
    {
        var pipeline = File.ReadAllText(PipelinePath);
        var publish = File.ReadAllText(
            Path.Combine(RepoRoot, "eng", "pipelines", "stages", "publish-set.yml"));

        Assert.DoesNotContain("- name: isWorkload", pipeline, StringComparison.Ordinal);
        Assert.Equal(2, Regex.Matches(pipeline, @"expectedWorkload:\s*'true'").Count);
        Assert.Single(Regex.Matches(pipeline, @"expectedWorkload:\s*'false'").Cast<Match>());
        Assert.Contains("releasePlan.IsWorkload", publish, StringComparison.Ordinal);
        Assert.Contains("expectedWorkload", publish, StringComparison.Ordinal);
        Assert.Contains(
            "${{ parameters.artifactName }}/${{ parameters.setName }}/*.nupkg",
            publish,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Workload_stage_conditions_resolve_the_plan_output()
    {
        var root = File.ReadAllText(PipelinePath);
        var publish = File.ReadAllText(
            Path.Combine(RepoRoot, "eng", "pipelines", "stages", "publish-set.yml"));
        var references = Regex.Matches(
                root + "\n" + publish,
                @"dependencies\.(?<stage>\w+)\.outputs\['(?<job>\w+)\.(?<step>\w+)\.IsWorkload'\]")
            .Cast<Match>()
            .ToList();

        Assert.Collection(references, _ => { }, _ => { });

        foreach (var reference in references)
        {
            Assert.Contains($"- stage: {reference.Groups["stage"].Value}", root, StringComparison.Ordinal);
            Assert.Contains($"- job: {reference.Groups["job"].Value}", root, StringComparison.Ordinal);
            Assert.Contains($"name: {reference.Groups["step"].Value}", root, StringComparison.Ordinal);
        }

        var verbs = File.ReadAllText(
            Path.Combine(RepoRoot, "src", "DotNet.Release", "Commands", "PlanCommand.cs"));
        Assert.Contains("SetIsWorkload(resolved.Value.Workload)", verbs, StringComparison.Ordinal);
    }

    [Fact]
    public void Prepare_publishes_one_release_artifact_for_every_repository_type()
    {
        var pipeline = File.ReadAllText(PipelinePath);

        Assert.Contains("artifactName: Release", pipeline, StringComparison.Ordinal);
        Assert.DoesNotContain("artifactName: ReleasePacks", pipeline, StringComparison.Ordinal);
        Assert.DoesNotContain("artifactName: ReleaseManifests", pipeline, StringComparison.Ordinal);
        Assert.DoesNotContain("artifactName: ReleasePackages", pipeline, StringComparison.Ordinal);
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
    /// Repository and commit identify the release. Neither has a safe default.
    /// </summary>
    [Theory]
    [InlineData("ghRepo")]
    [InlineData("commitHash")]
    public void Release_identity_must_be_supplied_for_every_run(string parameter)
    {
        Assert.False(Parameter(parameter).Children.ContainsKey("default"));
    }

    [Theory]
    [InlineData("barBuildId")]
    [InlineData("includeFilters")]
    [InlineData("excludeFilters")]
    [InlineData("recoveryFilters")]
    public void Optional_string_parameters_default_to_empty(string parameter)
    {
        Assert.Equal(string.Empty, ((YamlScalarNode)Parameter(parameter)["default"]).Value);
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
    /// The publish stage reads integrity hashes across a stage boundary, which only resolve if
    /// the stage name, job name and step name in the expression all match the producer.
    /// A typo here yields an empty variable and an integrity check that compares against
    /// nothing.
    /// </summary>
    [Fact]
    public void The_publish_stage_reads_prepare_outputs_from_the_right_place()
    {
        var publish = File.ReadAllText(Path.Combine(RepoRoot, "eng", "pipelines", "stages", "publish-set.yml"));

        var references = Regex.Matches(
            publish,
            @"stageDependencies\.(?<stage>\w+)\.(?<job>\w+)\.outputs\['(?<step>\w+)\.(?<variable>\w+)'\]")
            .Cast<Match>()
            .ToList();

        Assert.Collection(references, _ => { }, _ => { }, _ => { }, _ => { });

        var pipeline = File.ReadAllText(PipelinePath);
        foreach (var reference in references)
        {
            Assert.Contains($"- stage: {reference.Groups["stage"].Value}", pipeline, StringComparison.Ordinal);
            Assert.Contains($"- job: {reference.Groups["job"].Value}", pipeline, StringComparison.Ordinal);
            Assert.Contains($"name: {reference.Groups["step"].Value}", pipeline, StringComparison.Ordinal);
            Assert.Contains($"variable={reference.Groups["variable"].Value}", pipeline, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// The tool is built before the Maestro-authenticated task and the same DLL is executed
    /// after approval from the immutable Release artifact.
    /// </summary>
    [Fact]
    public void The_tool_is_built_once_before_credentials_and_carried_in_the_artifact()
    {
        var root = File.ReadAllText(PipelinePath);
        var publish = File.ReadAllText(
            Path.Combine(RepoRoot, "eng", "pipelines", "stages", "publish-set.yml"));

        var build = root.IndexOf("dotnet build", StringComparison.Ordinal);
        var authenticatedTask = root.IndexOf("azureSubscription:", StringComparison.Ordinal);

        Assert.True(build >= 0);
        Assert.True(authenticatedTask > build);
        Assert.Contains("$(Build.ArtifactStagingDirectory)/_tool", root, StringComparison.Ordinal);
        Assert.Contains("buildTool.ToolHash", publish, StringComparison.Ordinal);
        Assert.Contains("_tool/release.dll", publish, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet run", publish, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet publish", root, StringComparison.Ordinal);
    }

    /// <summary>
    /// The publish job checks out the release-system metadata needed by UseDotNet and remains
    /// a production 1ES job.
    /// </summary>
    [Fact]
    public void The_publish_job_checks_out_source_and_runs_as_a_production_release_job()
    {
        var publish = File.ReadAllText(Path.Combine(RepoRoot, "eng", "pipelines", "stages", "publish-set.yml"));

        Assert.Contains("checkout: self", publish, StringComparison.Ordinal);
        Assert.Contains("persistCredentials: false", publish, StringComparison.Ordinal);
        Assert.Contains("UseDotNet@2", publish, StringComparison.Ordinal);
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
        Assert.Contains("dependsOn: preflight", publish, StringComparison.Ordinal);
        Assert.Contains("dependsOn: approval", publish, StringComparison.Ordinal);
    }

    [Fact]
    public void Preflight_publishes_the_exact_pruned_set_before_approval()
    {
        var path = Path.Combine(
            RepoRoot,
            "eng",
            "pipelines",
            "stages",
            "publish-set.yml");
        var lines = File.ReadAllLines(path);
        var text = string.Join("\n", lines);

        Assert.Contains("job: preflight", text, StringComparison.Ordinal);
        Assert.Contains(
            "artifactName: ${{ parameters.preparedArtifactName }}",
            text,
            StringComparison.Ordinal);
        Assert.Contains("dependsOn: preflight", text, StringComparison.Ordinal);

        var pruneSteps = Enumerable.Range(0, lines.Length)
            .Where(index => lines[index].Trim() == "prune-published `")
            .ToList();
        Assert.Collection(pruneSteps, _ => { }, _ => { });

        Assert.DoesNotContain(
            EnclosingConditions(lines, pruneSteps[0]),
            condition => condition.Contains("parameters.publishPackages, true", StringComparison.Ordinal));
        Assert.Contains(
            EnclosingConditions(lines, pruneSteps[1]),
            condition => condition.Contains("parameters.publishPackages, true", StringComparison.Ordinal));
    }

    /// <summary>
    /// Approval, package upload, and publication verification must be compile-time absent when
    /// publishPackages is false.
    /// </summary>
    /// <remarks>
    /// The preflight NuGet query is read-only and runs on dry runs. Operations that authorize,
    /// perform, or verify publication are descendants of <c>publishPackages=true</c>.
    /// </remarks>
    [Fact]
    public void Every_remote_publish_operation_is_nested_inside_the_publish_guard()
    {
        var path = Path.Combine(RepoRoot, "eng", "pipelines", "stages", "publish-set.yml");
        var lines = File.ReadAllLines(path);

        var operations = Enumerable.Range(0, lines.Length)
            .Where(i =>
                lines[i].Contains("ManualValidation@0", StringComparison.Ordinal) ||
                lines[i].Contains("1ES.PublishNuget@1", StringComparison.Ordinal) ||
                lines[i].Trim() == "verify `")
            .ToList();

        Assert.Collection(operations, _ => { }, _ => { }, _ => { });

        foreach (var operation in operations)
        {
            Assert.True(
                EnclosingConditions(lines, operation).Any(c =>
                    c.Contains("parameters.publishPackages, true", StringComparison.Ordinal)),
                $"publish-set.yml line {operation + 1} contains a remote release operation " +
                $"outside the publishPackages guard: {lines[operation].Trim()}");
        }
    }

    [Fact]
    public void Dry_run_executes_package_set_preflight_outside_the_publish_guard()
    {
        var publishPath = Path.Combine(
            RepoRoot,
            "eng",
            "pipelines",
            "stages",
            "publish-set.yml");
        var publishLines = File.ReadAllLines(publishPath);
        var preflight = Array.FindIndex(
            publishLines,
            line => line.Trim() == "prune-published `");

        Assert.True(preflight >= 0);
        Assert.DoesNotContain(
            EnclosingConditions(publishLines, preflight),
            condition => condition.Contains("parameters.publishPackages, true", StringComparison.Ordinal));

        var rootLines = File.ReadAllLines(PipelinePath);
        var includes = Enumerable.Range(0, rootLines.Length)
            .Where(i => rootLines[i].Contains("publish-set.yml", StringComparison.Ordinal))
            .ToList();
        Assert.Collection(includes, _ => { }, _ => { }, _ => { });
        Assert.All(
            includes,
            include => Assert.DoesNotContain(
                EnclosingConditions(rootLines, include),
                condition => condition.Contains("parameters.publishPackages, true", StringComparison.Ordinal)));
    }

    /// <summary>
    /// Walks outwards from a line, collecting every <c>${{ if }}</c> that encloses it.
    /// </summary>
    /// <remarks>
    /// Enclosure in YAML is indentation: each ancestor is the nearest preceding line at a
    /// strictly smaller indent.
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

        Assert.Contains("ManualValidation@0", promote, StringComparison.Ordinal);
        Assert.Contains("pool: server", promote, StringComparison.Ordinal);
        Assert.Contains("dependsOn: promote_approval", promote, StringComparison.Ordinal);
    }

    /// <summary>
    /// Run tags make release intent visible in Azure DevOps history.
    /// </summary>
    [Fact]
    public void Runs_are_tagged_with_their_intent()
    {
        var text = File.ReadAllText(PipelinePath);

        Assert.Contains("##vso[build.addbuildtag]PUBLISH", text, StringComparison.Ordinal);
        Assert.Contains("##vso[build.addbuildtag]DRY-RUN", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// A dry run may query NuGet.org but cannot obtain the publishing service connection or
    /// execute the upload task.
    /// </summary>
    [Fact]
    public void A_dry_run_contains_no_NuGet_org_mutation()
    {
        var root = string.Join(
            "\n",
            File.ReadAllLines(PipelinePath)
                .Where(line => !line.TrimStart().StartsWith('#')));
        var publish = File.ReadAllText(
            Path.Combine(RepoRoot, "eng", "pipelines", "stages", "publish-set.yml"));

        Assert.DoesNotContain("1ES.PublishNuget@1", root, StringComparison.Ordinal);
        Assert.DoesNotContain("nuget.org (dotnetframework)", root, StringComparison.Ordinal);

        Assert.Contains("prune-published", publish, StringComparison.Ordinal);
        Assert.Contains("1ES.PublishNuget@1", publish, StringComparison.Ordinal);
        Assert.Contains("nuget.org (dotnetframework)", publish, StringComparison.Ordinal);

        // The structural nesting test proves these operations are absent from a dry run.
        Every_remote_publish_operation_is_nested_inside_the_publish_guard();
    }

    /// <summary>
    /// The prepare stage builds the tool, reads BAR, gathers packages, stages them, and uploads
    /// the initial Release artifact. NuGet availability is queried by package-set preflight.
    /// </summary>
    [Fact]
    public void Prepare_stage_invokes_only_plan_gather_and_stage()
    {
        var lines = File.ReadAllLines(PipelinePath);
        var promotionStage = Array.FindIndex(
            lines,
            line => line.Contains("- stage: promote_workload_set", StringComparison.Ordinal));
        var scripts = ScriptLines(lines)
            .Where(i => i < promotionStage)
            .Select(i => lines[i])
            .ToList();

        Assert.Contains(scripts, line => line.Trim() == "plan `");
        Assert.Contains(scripts, line => line.Contains("gather-drop", StringComparison.Ordinal));
        Assert.Contains(scripts, line => line.Trim() == "stage `");

        Assert.DoesNotContain(scripts, line => line.Trim() == "prune-published `");
        Assert.DoesNotContain(scripts, line => line.Trim() == "verify `");
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

    /// <summary>The pipeline uses the Darc CLI installed on the agent image.</summary>
    [Fact]
    public void Darc_is_invoked_directly()
    {
        var pipeline = File.ReadAllText(PipelinePath);

        Assert.Contains("darc gather-drop", pipeline, StringComparison.Ordinal);
        Assert.Contains("darc add-build-to-channel", pipeline, StringComparison.Ordinal);
        Assert.DoesNotContain("Get-Darc", pipeline, StringComparison.Ordinal);
        Assert.DoesNotContain("DarcVersion", pipeline, StringComparison.Ordinal);
    }
}
