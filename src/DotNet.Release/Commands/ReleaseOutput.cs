namespace DotNet.Release;

internal static class ReleaseOutput
{
    public static void WriteResolvedRelease(TextWriter writer, ResolvedRelease release, string heading = "Resolved release", string indent = "")
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(release);

        writer.WriteLine($"{indent}{heading}:");
        writer.WriteLine($"{indent}  Schema version    : {release.SchemaVersion}");
        writer.WriteLine($"{indent}  Tool version      : {release.ToolVersion}");
        writer.WriteLine($"{indent}  Created UTC       : {release.CreatedUtc:O}");
        writer.WriteLine($"{indent}  Repository        : {release.Repository}");
        writer.WriteLine($"{indent}  Repository URL    : {release.RepositoryUrl}");
        writer.WriteLine($"{indent}  Commit            : {release.Commit}");
        writer.WriteLine($"{indent}  BAR build ID      : {release.BarBuildId}");
        writer.WriteLine($"{indent}  Repository origin : {release.RepositoryOrigin}");
        writer.WriteLine($"{indent}  Workload          : {release.Workload}");

        if (release.Channel is { } channel)
        {
            writer.WriteLine($"{indent}  Channel name      : {channel.Name}");
            writer.WriteLine($"{indent}  Channel ID        : {channel.Id}");
        }
        else
        {
            writer.WriteLine($"{indent}  Channel           : (none)");
        }
    }

    public static void WriteReleasePlan(TextWriter writer, ReleasePlan plan)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(plan);

        writer.WriteLine("Release plan:");
        writer.WriteLine($"  Schema version : {plan.SchemaVersion}");
        writer.WriteLine($"  Tool version   : {plan.ToolVersion}");
        writer.WriteLine($"  Created UTC    : {plan.CreatedUtc:O}");
        WriteResolvedRelease(writer, plan.Source, "Source", "  ");

        if (plan.WorkloadSet is { } target)
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
        foreach (var set in plan.Sets.OrderBy(set => set.Order))
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

    public static void WriteSelectedRelease(TextWriter writer, ReleasePlan plan, IReadOnlyList<ReleasePackageSet> sets, string expectedPlanHash)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(sets);

        WriteResolvedRelease(writer, plan.Source, "Selected release");
        writer.WriteLine($"Plan SHA-256 : {expectedPlanHash}");

        if (plan.WorkloadSet is { } target)
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
        writer.WriteLine($"  Schema version : {report.SchemaVersion}");
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
