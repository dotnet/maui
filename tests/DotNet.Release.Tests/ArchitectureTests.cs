using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Xunit;

namespace DotNet.Release.Tests;

/// <summary>
/// Enforces the guarantees the release system's safety case rests on.
/// </summary>
/// <remarks>
/// The tool ships as one assembly, so the scan covers every command, policy function, and
/// adapter that can execute in the release process.
/// </remarks>
public class ArchitectureTests
{
    private static Assembly Tool => typeof(ReleasePlan).Assembly;

    /// <summary>
    /// Types the tool must never reference.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>PackageUpdateResource</c> is NuGet's push API. <c>NuGet.Protocol</c> is
    /// referenced for read-only availability queries and ships this type alongside them, so
    /// asserting its absence is what makes "the tool cannot publish" structural rather than a
    /// promise. <c>1ES.PublishNuget@1</c> owns every upload — a compliance requirement.</item>
    /// <item><c>Process</c> / <c>ProcessStartInfo</c>: darc is invoked from pipeline YAML,
    /// where Azure DevOps owns the exit code and the log. There is no process-execution
    /// abstraction anywhere in this codebase, and this is what keeps that true.</item>
    /// </list>
    /// </remarks>
    private static readonly (string Namespace, string Name)[] Forbidden =
    [
        ("NuGet.Protocol.Core.Types", "PackageUpdateResource"),
        ("NuGet.Protocol.Core.Types", "PackageUpdateResourceV3Provider"),
        ("NuGet.Protocol", "PackageUpdateResource"),
        ("System.Diagnostics", "Process"),
        ("System.Diagnostics", "ProcessStartInfo"),
    ];

    private static IReadOnlyList<string> ReferencedTypes()
    {
        using var stream = File.OpenRead(Tool.Location);
        using var peReader = new PEReader(stream);
        var metadata = peReader.GetMetadataReader();

        return
        [
            .. metadata.TypeReferences
                .Select(metadata.GetTypeReference)
                .Select(t => $"{metadata.GetString(t.Namespace)}.{metadata.GetString(t.Name)}")
        ];
    }

    [Fact]
    public void Tool_source_references_no_push_or_process_execution_APIs()
    {
        var referenced = ReferencedTypes().ToHashSet(StringComparer.Ordinal);

        var offenders = Forbidden
            .Select(t => $"{t.Namespace}.{t.Name}")
            .Where(referenced.Contains)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "The tool source must not reference package-push or process-execution APIs: " +
            string.Join(", ", offenders));
    }

    /// <summary>
    /// The push API must be genuinely reachable in the dependency graph, otherwise the test
    /// above passes for the wrong reason and would keep passing if the dependency changed.
    /// </summary>
    [Fact]
    public void The_push_api_is_present_in_the_dependency_graph_so_its_absence_is_meaningful()
    {
        var protocol = Directory.GetFiles(AppContext.BaseDirectory, "NuGet.Protocol.dll").SingleOrDefault();

        Assert.NotNull(protocol);
        Assert.Contains(
            Assembly.LoadFrom(protocol).GetExportedTypes(),
            t => t.Name == "PackageUpdateResource");
    }

    /// <summary>
    /// No publishing surface at all — including internal members, which would be just as real
    /// as public ones.
    /// </summary>
    [Fact]
    public void The_tool_exposes_no_publishing_capability()
    {
        var offenders = Tool.GetTypes()
            .SelectMany(t => t
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(m => $"{t.Name}.{m.Name}"))
            .Where(name =>
                name.Contains("Push", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Upload", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            offenders.Count == 0,
            $"The tool must expose no publishing capability; 1ES.PublishNuget@1 owns every " +
            $"upload. Found: {string.Join(", ", offenders)}");
    }

    /// <summary>
    /// Version normalization is policy, so the NuGet parser that defines the canonical form
    /// must remain a direct dependency.
    /// </summary>
    [Fact]
    public void NuGet_Versioning_is_referenced_because_normalization_is_policy()
    {
        Assert.Contains(
            Tool.GetReferencedAssemblies(),
            a => string.Equals(a.Name, "NuGet.Versioning", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Remote and package reads stay behind interfaces so tests run without a network, BAR
    /// account, or real feed.
    /// </summary>
    [Theory]
    [InlineData("IBuildRegistry")]
    [InlineData("IPackageIdentityReader")]
    [InlineData("IPackageAvailabilityProbe")]
    public void Adapters_are_reachable_only_through_interfaces(string name)
    {
        var type = Tool.GetExportedTypes().SingleOrDefault(t => t.Name == name);

        Assert.NotNull(type);
        Assert.True(type.IsInterface, $"{name} must stay an interface.");
    }

    /// <summary>
    /// The feed abstraction is read-only by construction: it answers a question and returns
    /// nothing that could be used to publish.
    /// </summary>
    [Fact]
    public void The_feed_abstraction_is_read_only()
    {
        var methods = typeof(IPackageAvailabilityProbe).GetMethods();

        Assert.Equal("GetAvailabilityAsync", Assert.Single(methods).Name);
    }

    /// <summary>
    /// The layering survives as folders now that the assembly boundary is gone. Policy types
    /// take and return plain data, so they stay unit-testable without a filesystem or a feed.
    /// </summary>
    [Theory]
    [InlineData("ReleasePolicy")]
    [InlineData("BuildResolver")]
    [InlineData("StagePlanner")]
    [InlineData("FilterPlanner")]
    [InlineData("StagedSetIntegrity")]
    [InlineData("VerificationEvaluator")]
    [InlineData("PackageGlob")]
    [InlineData("PackageVersions")]
    public void Policy_types_expose_no_file_or_network_types_in_their_signatures(string typeName)
    {
        var type = Tool.GetExportedTypes().SingleOrDefault(t => t.Name == typeName);
        Assert.NotNull(type);

        var forbidden = new[] { "FileInfo", "DirectoryInfo", "FileStream", "HttpClient", "Stream", "Process" };

        var offenders = type
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(m => m.GetParameters().Select(p => p.ParameterType).Append(m.ReturnType))
            .Select(t => t.Name)
            .Where(n => forbidden.Contains(n, StringComparer.Ordinal))
            .Distinct()
            .ToList();

        Assert.True(
            offenders.Count == 0,
            $"{typeName} is policy and must operate on plain data, but its signatures mention: " +
            string.Join(", ", offenders));
    }
}
