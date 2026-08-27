using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Xunit;

namespace DotNet.Release.Core.Tests;

/// <summary>
/// Enforces the structural guarantees the design depends on.
/// </summary>
/// <remarks>
/// These are the difference between "the tool cannot publish" as an architectural claim and
/// as a comment somebody will delete in eighteen months. Requirement 4 asks for a dry run
/// that is structurally incapable of publishing; that is only true if the capability is
/// absent from the binary, and only provable if something checks.
/// </remarks>
public class ArchitectureTests
{
    private static Assembly Core => typeof(ReleasePlan).Assembly;

    /// <summary>
    /// Assemblies Core must never reference.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>NuGet.Protocol / NuGet.Packaging — feed and file access belong in adapters.</item>
    /// <item>Microsoft.DotNet.* — the BAR client belongs in DotNet.Release.Maestro.</item>
    /// <item>System.Net.Http — Core makes no network calls.</item>
    /// <item>System.Diagnostics.Process — there are no subprocesses anywhere (design P2).</item>
    /// </list>
    /// </remarks>
    private static readonly string[] ForbiddenAssemblyPrefixes =
    [
        "NuGet.Protocol",
        "NuGet.Packaging",
        "NuGet.Commands",
        "Microsoft.DotNet.",
        "Azure.",
        "System.Net.Http",
        "System.Diagnostics.Process",
    ];

    /// <summary>
    /// Types Core must never reference, checked against the assembly's TypeRef table.
    /// </summary>
    /// <remarks>
    /// Checking types as well as assemblies matters because most of these live in
    /// <c>System.Runtime</c> once the reference assemblies are collapsed, so an
    /// assembly-name check alone would not catch them.
    /// </remarks>
    private static readonly (string Namespace, string Name)[] ForbiddenTypes =
    [
        ("System.Diagnostics", "Process"),
        ("System.Diagnostics", "ProcessStartInfo"),
        ("System.Net.Http", "HttpClient"),
        ("System.Net", "WebClient"),
        ("System.IO", "File"),
        ("System.IO", "Directory"),
        ("System.IO", "FileStream"),
        ("System.IO", "DirectoryInfo"),
        ("System.Net.Sockets", "Socket"),
    ];

    [Fact]
    public void Core_references_no_io_capable_assemblies()
    {
        var offenders = Core.GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .Where(name => ForbiddenAssemblyPrefixes.Any(p => name.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.True(
            offenders.Count == 0,
            $"DotNet.Release.Core must perform no I/O, but references: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Core_references_no_io_capable_types()
    {
        var offenders = new List<string>();

        using var stream = File.OpenRead(Core.Location);
        using var peReader = new PEReader(stream);
        var metadata = peReader.GetMetadataReader();

        foreach (var handle in metadata.TypeReferences)
        {
            var typeRef = metadata.GetTypeReference(handle);
            var ns = metadata.GetString(typeRef.Namespace);
            var name = metadata.GetString(typeRef.Name);

            if (ForbiddenTypes.Any(t => t.Namespace == ns && t.Name == name))
            {
                offenders.Add($"{ns}.{name}");
            }
        }

        Assert.True(
            offenders.Count == 0,
            $"DotNet.Release.Core must perform no I/O and must never start a process, but " +
            $"references: {string.Join(", ", offenders.Distinct().Order(StringComparer.Ordinal))}");
    }

    /// <summary>
    /// Version normalization is policy, not I/O, so the dependency that provides it must
    /// actually be present — otherwise the substring hack could quietly come back.
    /// </summary>
    [Fact]
    public void Core_references_NuGet_Versioning_because_normalization_is_policy()
    {
        Assert.Contains(
            Core.GetReferencedAssemblies(),
            a => string.Equals(a.Name, "NuGet.Versioning", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 1ES.PublishNuget@1 is a compliance requirement and performs every push, so the tool
    /// must expose no publishing surface at all — not a flag, not a code path.
    /// </summary>
    [Fact]
    public void Core_exposes_no_publishing_capability()
    {
        var suspicious = Core.GetExportedTypes()
            .SelectMany(t => t.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(m => $"{t.Name}.{m.Name}"))
            .Where(name =>
                name.Contains("Push", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Publish", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Upload", StringComparison.OrdinalIgnoreCase))
            // "IsPublished" / "AlreadyPublished" / "PackagesToPublish" describe *observed*
            // state and the pipeline variable read by the publish task's condition. None of
            // them can cause a push.
            .Where(name => !name.Contains("IsPublished", StringComparison.Ordinal))
            .Where(name => !name.Contains("AlreadyPublished", StringComparison.Ordinal))
            .Where(name => !name.Contains("PackagesToPublish", StringComparison.Ordinal))
            .Where(name => !name.Contains("HasPackagesToPublish", StringComparison.Ordinal))
            .ToList();

        Assert.True(
            suspicious.Count == 0,
            $"The tool must never push packages; 1ES.PublishNuget@1 owns that. Found: " +
            $"{string.Join(", ", suspicious)}");
    }

    /// <summary>
    /// Adapters are named in Core as interfaces only, so Core can be tested with fakes and
    /// the real clients stay out of the pure layer.
    /// </summary>
    [Fact]
    public void Core_declares_its_adapters_as_interfaces_only()
    {
        foreach (var name in new[] { "IBuildRegistry", "IPackageIdentityReader", "IPackageAvailabilityProbe" })
        {
            var type = Core.GetExportedTypes().SingleOrDefault(t => t.Name == name);

            Assert.NotNull(type);
            Assert.True(type.IsInterface, $"{name} must stay an interface in Core.");
        }
    }

    /// <summary>
    /// The availability probe is read-only by construction: it answers a question and
    /// returns nothing that could be used to publish.
    /// </summary>
    [Fact]
    public void The_feed_abstraction_is_read_only()
    {
        var methods = typeof(IPackageAvailabilityProbe).GetMethods();

        Assert.Equal("GetAvailabilityAsync", Assert.Single(methods).Name);
    }
}
