namespace DotNet.Release;

/// <summary>Writes the human-readable BAR, manifest, and pruning audit shown in pipeline logs.</summary>
internal static class ReleaseOutput
{
    public static void WriteResolvedBuild(TextWriter writer, ResolvedBuild build)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(build);

        writer.WriteLine("Resolved build:");
        writer.WriteLine($"  Repository        : {build.Repository}");
        writer.WriteLine($"  Repository URL    : {build.RepositoryUrl}");
        writer.WriteLine($"  Commit            : {build.Commit}");
        writer.WriteLine($"  BAR build ID      : {build.BarBuildId}");
        writer.WriteLine($"  Repository origin : {build.RepositoryOrigin}");
        writer.WriteLine($"  Workload          : {build.Workload}");

        WriteChannel(writer, build.Channel, "  ");
    }

    private static void WriteReleaseSource(TextWriter writer, ReleaseSource source, string heading, string indent)
    {
        writer.WriteLine($"{indent}{heading}:");
        writer.WriteLine($"{indent}  Repository        : {source.Repository}");
        writer.WriteLine($"{indent}  Repository URL    : {source.RepositoryUrl}");
        writer.WriteLine($"{indent}  Commit            : {source.Commit}");
        writer.WriteLine($"{indent}  BAR build ID      : {source.BarBuildId}");
        writer.WriteLine($"{indent}  Workload          : {source.Workload}");
        WriteChannel(writer, source.Channel, $"{indent}  ");
    }

    private static void WriteChannel(TextWriter writer, ChannelReference? channel, string indent)
    {
        if (channel is not null)
        {
            writer.WriteLine($"{indent}Channel name      : {channel.Name}");
            writer.WriteLine($"{indent}Channel ID        : {channel.Id}");
        }
        else
        {
            writer.WriteLine($"{indent}Channel           : (none)");
        }
    }

    public static void WriteReleaseManifest(TextWriter writer, ReleaseManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(manifest);

        writer.WriteLine("Release manifest:");
        writer.WriteLine($"  Schema version : {manifest.SchemaVersion}");
        writer.WriteLine($"  Tool version   : {manifest.ToolVersion}");
        writer.WriteLine($"  Created UTC    : {manifest.CreatedUtc:O}");
        WriteReleaseSource(writer, manifest.Source, "Source", "  ");

        if (manifest.WorkloadSet is { } target)
        {
            writer.WriteLine("  Workload target:");
            writer.WriteLine($"    Band    : {target.Band}");
            writer.WriteLine($"    Channel : {target.Channel}");
            writer.WriteLine($"    Feed    : {target.Feed}");
        }
        else
        {
            writer.WriteLine("  Workload target : (none)");
        }

        writer.WriteLine("  Package sets:");
        foreach (var set in manifest.Sets.OrderBy(set => set.Order))
        {
            writer.WriteLine("    Set:");
            writer.WriteLine($"      Name          : {set.Name}");
            writer.WriteLine($"      Order         : {set.Order}");
            writer.WriteLine($"      Artifact name : {set.ArtifactName}");
            writer.WriteLine("      Packages:");

            foreach (var package in set.Packages)
            {
                writer.WriteLine("        Package:");
                writer.WriteLine($"          ID                 : {package.Id}");
                writer.WriteLine($"          Raw version        : {package.Version}");
                writer.WriteLine($"          Normalized version : {package.NormalizedVersion}");
                writer.WriteLine($"          File name          : {package.FileName}");
                writer.WriteLine($"          SHA-256            : {package.Sha256}");
            }
        }
    }

    public static void WriteSelectedRelease(
        TextWriter writer,
        ReleaseManifest manifest,
        IReadOnlyList<ReleasePackageSet> sets,
        string expectedManifestHash)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(sets);

        WriteReleaseSource(writer, manifest.Source, "Selected release", "");
        writer.WriteLine($"Manifest SHA-256 : {expectedManifestHash}");

        if (manifest.WorkloadSet is { } target)
        {
            writer.WriteLine($"Workload target: .NET {target.Band}, channel '{target.Channel}', feed '{target.Feed}'");
        }
        else
        {
            writer.WriteLine("Workload target: (none)");
        }

        writer.WriteLine(
            $"Selected package set(s): {string.Join(", ", sets.Select(set => set.ArtifactName))}");
    }

    public static void WritePruneReport(TextWriter writer, ReleasePackageSet set, PruneReport report)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(report);

        writer.WriteLine("Prune report:");
        writer.WriteLine($"  Set name       : {report.SetName}");
        writer.WriteLine($"  Set order      : {set.Order}");
        writer.WriteLine($"  Artifact name  : {set.ArtifactName}");
        writer.WriteLine("  Decisions:");
        var packages = set.Packages.ToDictionary(
            package => package.FileName, StringComparer.OrdinalIgnoreCase);

        foreach (var decision in report.Decisions)
        {
            var package = packages[decision.FileName];
            writer.WriteLine("    Decision:");
            writer.WriteLine($"      ID                 : {decision.Id}");
            writer.WriteLine($"      Raw version        : {package.Version}");
            writer.WriteLine($"      Normalized version : {decision.NormalizedVersion}");
            writer.WriteLine($"      File name          : {decision.FileName}");
            writer.WriteLine($"      SHA-256            : {package.Sha256}");
            writer.WriteLine($"      Disposition        : {decision.Disposition}");
        }

        writer.WriteLine($"  Pending count : {report.PendingCount}");
    }
}
