using PcsModels = Microsoft.DotNet.ProductConstructionService.Client.Models;

namespace DotNet.Release;

/// <summary>
/// Maps the BAR client's <c>Build</c> model onto <see cref="BarBuild"/>.
/// </summary>
/// <remarks>
/// <para>
/// Kept as a pure function, separate from <see cref="MaestroBuildRegistry"/>, because this
/// is where the risk lives and it can then be tested without any Azure.Core paging plumbing.
/// </para>
/// <para>
/// <b>The client assembly is not nullable-annotated.</b> Verified by reflection against
/// <c>Microsoft.DotNet.ProductConstructionService.Client 1.1.0-beta.26426.2</c>:
/// <c>Build.GitHubRepository</c>, <c>Build.AzureDevOpsRepository</c>, <c>Build.Commit</c>
/// and <c>Channel.Name</c> are all declared <c>string</c> with nullability state
/// <c>Unknown</c>. The compiler therefore gives no warning when they are null, and
/// <c>GitHubRepository</c> <i>is</i> null in production — that is the case behind SkiaSharp
/// BAR build 328857. Every string crossing this boundary is normalized explicitly rather
/// than trusted.
/// </para>
/// </remarks>
public static class BarBuildMapper
{
    public static BarBuild Map(PcsModels.Build build)
    {
        ArgumentNullException.ThrowIfNull(build);

        return new BarBuild(
            build.Id,
            NullIfBlank(build.Commit) ?? string.Empty,
            NullIfBlank(build.GitHubRepository),
            NullIfBlank(build.AzureDevOpsRepository),
            MapChannels(build.Channels));
    }

    private static IReadOnlyList<ChannelReference> MapChannels(IEnumerable<PcsModels.Channel>? channels)
    {
        if (channels is null)
        {
            return [];
        }

        // A channel with no usable name cannot satisfy a required-channel check, and mapping
        // it to "" would make it look like a real channel that merely failed to match.
        // Dropping it keeps the resulting failure message honest about what BAR returned.
        return [.. channels
            .Where(c => c is not null && !string.IsNullOrWhiteSpace(c.Name))
            .Select(c => new ChannelReference(c.Name.Trim(), c.Id))];
    }

    /// <summary>
    /// Collapses null and whitespace to null, so downstream code has one empty case to
    /// handle rather than three.
    /// </summary>
    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
