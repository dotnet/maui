using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Xunit;

namespace DotNet.Release.Tests;

/// <summary>
/// Enforces the guarantees the release system's safety case rests on.
/// </summary>
/// <remarks>
/// <para>
/// The tool is one assembly, so these scan the whole thing — there is nowhere for a
/// violation to hide. That is a strict improvement on the previous arrangement, where the
/// scan covered only the pure policy project: the assembly that could actually have pushed a
/// package was the one nothing looked at.
/// </para>
/// <para>
/// <b>What was traded away by merging the projects:</b> "the policy layer performs no I/O"
/// used to be enforced by an assembly boundary and is now a convention expressed by the
/// <c>Policy/</c> and <c>Adapters/</c> folders. That was an internal design discipline. The
/// two guarantees that are externally load-bearing — the tool cannot push, and the tool
/// cannot start a process — are unaffected and are asserted below.
/// </para>
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
    public void The_tool_cannot_push_a_package_or_start_a_process()
    {
        var referenced = ReferencedTypes().ToHashSet(StringComparer.Ordinal);

        var offenders = Forbidden
            .Select(t => $"{t.Namespace}.{t.Name}")
            .Where(referenced.Contains)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "The tool must never push a package or start a process, but references: " +
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
    /// Version normalization is policy, so the pure parser that provides it must be present —
    /// otherwise the substring hack it replaced could quietly return.
    /// </summary>
    [Fact]
    public void NuGet_Versioning_is_referenced_because_normalization_is_policy()
    {
        Assert.Contains(
            Tool.GetReferencedAssemblies(),
            a => string.Equals(a.Name, "NuGet.Versioning", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// The adapters stay behind interfaces. This is the abstraction worth keeping after the
    /// projects were merged: it is what lets every test run with no network, no BAR account
    /// and no real feed.
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
