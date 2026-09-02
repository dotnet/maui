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
    private static Assembly Tool => typeof(ReleaseManifest).Assembly;

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
        ("System.Net.Http", "HttpClient"),
        ("System.Net.Http", "HttpClientHandler"),
        ("System.Net", "WebRequest"),
        ("System.Net", "HttpWebRequest"),
    ];

    private static IReadOnlyList<string> ReferencedTypes()
    {
        using var stream = File.OpenRead(Tool.Location);
        using var peReader = new PEReader(stream);
        var metadata = peReader.GetMetadataReader();

        return
        [.. metadata.TypeReferences.Select(metadata.GetTypeReference)
                .Select(t => $"{metadata.GetString(t.Namespace)}.{metadata.GetString(t.Name)}")
        ];
    }

    [Fact]
    public void Tool_source_references_no_push_process_or_hand_rolled_HTTP_APIs()
    {
        var referenced = ReferencedTypes().ToHashSet(StringComparer.Ordinal);

        var offenders = Forbidden
            .Select(t => $"{t.Namespace}.{t.Name}").Where(referenced.Contains).ToList();

        Assert.True(offenders.Count == 0, "The tool source must not reference package-push, process-execution, or " + "hand-rolled HTTP APIs: " +
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
        Assert.Contains(Assembly.LoadFrom(protocol).GetExportedTypes(),
            t => t.Name == "PackageUpdateResource");
    }

}
