using Azure;
using Azure.Core;
using Microsoft.DotNet.ProductConstructionService.Client;
using PcsModels = Microsoft.DotNet.ProductConstructionService.Client.Models;

namespace DotNet.Release.Tests;

/// <summary>
/// A fake <see cref="IBuilds"/>.
/// </summary>
/// <remarks>
/// Every mutating member throws. That is not defensive padding: <see cref="IBuilds"/>
/// exposes <c>CreateAsync</c> and <c>UpdateAsync</c>, which write to BAR, and the design
/// requires that the tool mutate nothing. If the adapter ever reaches for one, the test
/// fails loudly instead of the guarantee quietly becoming untrue.
/// </remarks>
internal sealed class FakeBuilds : IBuilds
{
    private readonly Func<int, PcsModels.Build?>? _getById;
    private readonly Exception? _throwOnGet;

    public FakeBuilds(Func<int, PcsModels.Build?>? getById = null, Exception? throwOnGet = null)
    {
        _getById = getById;
        _throwOnGet = throwOnGet;
    }

    public bool? LastIncludeAssetLocation { get; private set; }

    public Task<PcsModels.Build> GetBuildAsync(int id, bool? includeAssetLocation = null, CancellationToken cancellationToken = default)
    {
        LastIncludeAssetLocation = includeAssetLocation;

        if (_throwOnGet is not null)
        {
            throw _throwOnGet;
        }

        return Task.FromResult(_getById?.Invoke(id)!);
    }

    public AsyncPageable<PcsModels.Build> ListBuildsAsync(string? azdoAccount = null, int? azdoBuildId = null, string? azdoProject = null,
        string? buildNumber = null, string? commit = null, int? channelId = null, bool? loadCollections = null, DateTimeOffset? notAfter = null,
        DateTimeOffset? notBefore = null, string? repository = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(nameof(ListBuildsAsync));

    // ---- mutating members: never valid for this tool ----

    public Task<PcsModels.Build> CreateAsync(PcsModels.BuildData body, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("The release tool must never create a BAR build.");

    public Task<PcsModels.Build> UpdateAsync(PcsModels.BuildUpdate body, int buildId, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("The release tool must never update a BAR build.");

    // ---- unused read members ----

    public Task<PcsModels.BuildGraph> GetBuildGraphAsync(int id, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(nameof(GetBuildGraphAsync));

    public Task<Page<PcsModels.Build>> ListBuildsPageAsync(
        string? azdoAccount = null, int? azdoBuildId = null, string? azdoProject = null, string? buildNumber = null,
        string? commit = null, int? channelId = null, bool? loadCollections = null, DateTimeOffset? notAfter = null,
        DateTimeOffset? notBefore = null, int? page = null, int? perPage = null, string? repository = null,
        CancellationToken cancellationToken = default) => throw new NotSupportedException(nameof(ListBuildsPageAsync));

    public Task<PcsModels.Build> GetLatestAsync(string? buildNumber = null, string? commit = null, int? channelId = null, bool? loadCollections = null,
        DateTimeOffset? notAfter = null, DateTimeOffset? notBefore = null, string? repository = null,
        CancellationToken cancellationToken = default) => throw new NotSupportedException(nameof(GetLatestAsync));

    public Task<PcsModels.Commit> GetCommitAsync(int buildId, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(nameof(GetCommitAsync));

    public Task<List<PcsModels.SourceManifestEntry>> GetSourceManifestAsync(int buildId, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(nameof(GetSourceManifestAsync));
}

/// <summary>Minimal <see cref="Response"/> so a <see cref="RestApiException"/> can be built.</summary>
internal sealed class FakeResponse(int status) : Response
{
    public override int Status { get; } = status;

    public override string ReasonPhrase => $"HTTP {Status}";

    public override Stream? ContentStream { get; set; }

    public override string ClientRequestId { get; set; } = "fake";

    public override void Dispose() { }

    protected override bool ContainsHeader(string name) => false;

    protected override IEnumerable<HttpHeader> EnumerateHeaders() => [];

    protected override bool TryGetHeader(string name, out string value)
    {
        value = null!;
        return false;
    }

    protected override bool TryGetHeaderValues(string name, out IEnumerable<string> values)
    {
        values = null!;
        return false;
    }
}

/// <summary>Minimal <see cref="Request"/> so a <see cref="RestApiException"/> can be built.</summary>
internal sealed class FakeRequest : Request
{
    public FakeRequest()
    {
        Method = RequestMethod.Get;
        Uri = new RequestUriBuilder { Scheme = "https", Host = "maestro.dot.net", Path = "/api/builds" };
    }

    public override string ClientRequestId { get; set; } = "fake";

    protected override void AddHeader(string name, string value) { }

    protected override bool ContainsHeader(string name) => false;

    protected override IEnumerable<HttpHeader> EnumerateHeaders() => [];

    protected override bool RemoveHeader(string name) => false;

    protected override bool TryGetHeader(string name, out string value)
    {
        value = null!;
        return false;
    }

    protected override bool TryGetHeaderValues(string name, out IEnumerable<string> values)
    {
        values = null!;
        return false;
    }

    public override void Dispose() { }
}

internal static class BuildFactory
{
    public const string Commit = "f14581760a1c9e4b0f1e2d3c4b5a6978899aabbc";

    /// <summary>
    /// Builds a PCS <c>Build</c>. Note that <c>gitHubRepository</c> and
    /// <c>azureDevOpsRepository</c> are settable <c>string</c> properties with no nullable
    /// annotation, which is exactly why the mapper normalizes them.
    /// </summary>
    public static PcsModels.Build Create(int id = 4242, string? commit = Commit,
        string? gitHubRepository = "https://github.com/dotnet/skiasharp",
        string? azureDevOpsRepository = null, params (int Id, string? Name)[] channels)
    {
        var build = new PcsModels.Build(id, DateTimeOffset.UnixEpoch, staleness: 0, released: false, stable: true, commit!,
            [.. channels.Select(c => new PcsModels.Channel(c.Id, c.Name!, "product"))], assets: [], dependencies: [], incoherencies: [])
        {
            GitHubRepository = gitHubRepository!,
            AzureDevOpsRepository = azureDevOpsRepository!,
        };

        return build;
    }

    public static RestApiException NotFound() => new(new FakeRequest(), new FakeResponse(404), "{}");

    public static RestApiException ServerError() => new(new FakeRequest(), new FakeResponse(500), "{}");
}
