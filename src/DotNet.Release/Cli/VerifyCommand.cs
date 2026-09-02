using System.CommandLine;

namespace DotNet.Release;

/// <summary>
/// Implements <c>release verify</c>, which waits for every package in a manifest set to
/// become available on the target feed.
/// </summary>
internal static class VerifyCommand
{
    public static Command Build(TextWriter outputWriter)
    {
        var manifest = new Option<FileInfo>("--manifest")
        {
            Description = "release-manifest.json.",
            Required = true,
        };
        var maxDuration = new Option<int>("--max-duration-minutes")
        {
            Description = "Verification deadline.",
            DefaultValueFactory = _ => 30,
        };
        var interval = new Option<int>("--poll-seconds")
        {
            Description = "Delay between polls.",
            DefaultValueFactory = _ => 20,
        };
        var feed = new Option<string?>("--feed")
        {
            Description = "Feed index to query. Defaults to NuGet.org.",
        };
        var set = new Option<string>("--set")
        {
            Description = "Package-set directory name.",
            Required = true,
        };
        var expectedHash = new Option<string>("--expected-manifest-hash")
        {
            Description = "The SHA-256 emitted by the prepare stage.",
            Required = true,
        };
        var command = new Command("verify", "Poll until every package in the set is indexed on NuGet.org.")
        {
            manifest, maxDuration, interval, feed, set, expectedHash,
        };

        command.SetAction((parse, cancellationToken) =>
        {
            var manifestFile = parse.GetValue(manifest)!;
            using var lookup = new NuGetClient(parse.GetValue(feed));

            return ExecuteAsync(outputWriter, lookup, File.ReadAllText(manifestFile.FullName), TimeSpan.FromMinutes(parse.GetValue(maxDuration)),
                TimeSpan.FromSeconds(parse.GetValue(interval)),
                () => DateTimeOffset.UtcNow, Task.Delay, parse.GetValue(set), parse.GetValue(expectedHash)!, cancellationToken);
        });

        return command;
    }

    public static async Task ExecuteAsync(TextWriter outputWriter, INuGetClient lookup, string manifestJson, TimeSpan maxDuration,
        TimeSpan pollInterval, Func<DateTimeOffset> clock, Func<TimeSpan, CancellationToken, Task> delay, string? setName,
        string expectedManifestHash,
        CancellationToken cancellationToken)
    {
        var manifest = ReleaseManifestSerializer.VerifyAndDeserialize(manifestJson, expectedManifestHash);
        var sets = ReleaseArtifact.SelectSets(manifest, setName);
        var packages = sets.SelectMany(set => set.Packages).ToList();
        var deadline = clock() + maxDuration;
        IReadOnlyList<PlannedPackage> missing = packages;

        while (true)
        {
            try
            {
                var availability = await lookup.GetAvailabilityAsync(packages, cancellationToken).ConfigureAwait(false);
                missing = GetMissing(packages, availability);

                if (missing.Count == 0)
                {
                    outputWriter.WriteLine($"Verified all {packages.Count} packages on NuGet.org.");
                    return;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                outputWriter.WriteLine($"warning: NuGet.org query failed, will retry: {ex.Message}");
            }

            if (clock() + pollInterval >= deadline)
            {
                throw new DotNetReleaseException(DescribeMissing(missing));
            }

            outputWriter.WriteLine($"Waiting for {missing.Count} package(s) to become available on NuGet.org.");
            await delay(pollInterval, cancellationToken).ConfigureAwait(false);
        }
    }

    internal static IReadOnlyList<PlannedPackage> GetMissing(IEnumerable<PlannedPackage> packages, IReadOnlyDictionary<string, bool> availability)
    {
        ArgumentNullException.ThrowIfNull(packages);
        ArgumentNullException.ThrowIfNull(availability);

        return
        [
            .. packages.Where(package => !availability.TryGetValue(package.IdentityKey, out var isPublished) || !isPublished)
        ];
    }

    internal static string DescribeMissing(IReadOnlyList<PlannedPackage> missing) => "The following packages are not available from NuGet.org: " + string.Join(
            ", ", missing
                .Select(package => $"{package.Id} {package.Version}").Order(StringComparer.Ordinal));
}
