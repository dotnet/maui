using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Xunit;

namespace DotNet.Release.Cli.Tests;

/// <summary>
/// Enforces the tool's two headline guarantees across <b>every shipping assembly</b>, not
/// just the one that could never have broken them.
/// </summary>
/// <remarks>
/// <para>
/// The original architecture tests scanned only <c>DotNet.Release.Core</c>. That assembly is
/// I/O-free by construction — it has no dependency capable of pushing and no process API in
/// reach — so scanning it proved almost nothing. The assembly that <i>can</i> publish is
/// <c>DotNet.Release.NuGet</c>, which references <c>NuGet.Protocol</c> and therefore has
/// <c>PackageUpdateResource.Push</c> one call away. The only thing preventing its use was a
/// comment in a csproj.
/// </para>
/// <para>
/// These tests close that gap. A "convenience" push added to any shipping assembly, or a
/// shell-out to darc, now fails a test instead of passing one.
/// </para>
/// </remarks>
public class WholeToolArchitectureTests
{
    /// <summary>
    /// Every assembly that ships as part of the tool, by output file name.
    /// </summary>
    /// <remarks>
    /// The CLI's assembly name is <c>release</c>, not <c>DotNet.Release.Cli</c>, because that
    /// is the command operators type.
    /// </remarks>
    public static TheoryData<string> ShippingAssemblies =>
    [
        "DotNet.Release.Core",
        "DotNet.Release.Maestro",
        "DotNet.Release.NuGet",
        "release",
    ];

    /// <summary>
    /// Types no part of the tool may reference.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>PackageUpdateResource</c> is NuGet's push API. Its absence is what makes
    /// "the tool cannot publish" structural rather than a promise — 1ES.PublishNuget@1 owns
    /// every upload.</item>
    /// <item><c>Process</c> / <c>ProcessStartInfo</c>: the design states there is no
    /// process-execution abstraction anywhere. darc is invoked from YAML, never from here.
    /// </item>
    /// </list>
    /// </remarks>
    private static readonly (string Namespace, string Name)[] ForbiddenEverywhere =
    [
        ("NuGet.Protocol.Core.Types", "PackageUpdateResource"),
        ("NuGet.Protocol.Core.Types", "PackageUpdateResourceV3Provider"),
        ("NuGet.Protocol", "PackageUpdateResource"),
        ("System.Diagnostics", "Process"),
        ("System.Diagnostics", "ProcessStartInfo"),
    ];

    private static string LocateAssembly(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, $"{name}.dll");
        Assert.True(File.Exists(path), $"Could not find {name}.dll next to the test assembly.");
        return path;
    }

    private static IReadOnlyList<string> ReferencedTypes(string assemblyPath)
    {
        using var stream = File.OpenRead(assemblyPath);
        using var peReader = new PEReader(stream);
        var metadata = peReader.GetMetadataReader();

        return
        [
            .. metadata.TypeReferences
                .Select(metadata.GetTypeReference)
                .Select(t => $"{metadata.GetString(t.Namespace)}.{metadata.GetString(t.Name)}")
        ];
    }

    [Theory]
    [MemberData(nameof(ShippingAssemblies))]
    public void No_shipping_assembly_can_push_a_package_or_start_a_process(string assemblyName)
    {
        var referenced = ReferencedTypes(LocateAssembly(assemblyName)).ToHashSet(StringComparer.Ordinal);

        var offenders = ForbiddenEverywhere
            .Select(t => $"{t.Namespace}.{t.Name}")
            .Where(referenced.Contains)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            $"{assemblyName} must never push a package or start a process, but references: " +
            $"{string.Join(", ", offenders)}");
    }

    /// <summary>
    /// The push API must be genuinely reachable in the dependency graph, otherwise the test
    /// above passes for the wrong reason and would keep passing if NuGet.Protocol were
    /// swapped for something that does expose a push.
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

    [Theory]
    [MemberData(nameof(ShippingAssemblies))]
    public void No_shipping_assembly_exposes_a_publishing_member(string assemblyName)
    {
        var assembly = Assembly.LoadFrom(LocateAssembly(assemblyName));

        // Includes non-public types: an internal push would be just as real as a public one.
        var offenders = assembly.GetTypes()
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
            $"{assemblyName} must expose no publishing capability; 1ES.PublishNuget@1 owns " +
            $"every upload. Found: {string.Join(", ", offenders)}");
    }
}
