namespace DotNet.Release;

/// <summary>
/// A GitHub repository identity, normalized to lower case.
/// </summary>
/// <remarks>
/// GitHub owner and repository names are case-insensitive, and BAR records them with
/// inconsistent casing, so every comparison in a release must be made on a normalized
/// form. Normalizing once here removes that decision from every call site.
/// </remarks>
internal readonly record struct RepositoryId
{
    private RepositoryId(string owner, string name)
    {
        Owner = owner;
        Name = name;
    }

    public string Owner { get; }

    public string Name { get; }

    public string FullName => $"{Owner}/{Name}";

    public string GitHubUrl => $"https://github.com/{Owner}/{Name}";

    /// <summary>Parses the <c>owner/name</c> form used by the policy file and the CLI.</summary>
    public static Result<RepositoryId> Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<RepositoryId>.Failure(
                ErrorCodes.RepositoryUnparseable,
                "A repository must be supplied as 'owner/name'.");
        }

        var parts = value.Trim().Split('/');
        if (parts.Length != 2 || parts.Any(string.IsNullOrWhiteSpace))
        {
            return Result<RepositoryId>.Failure(
                ErrorCodes.RepositoryUnparseable,
                $"Repository '{value}' is not in 'owner/name' form.");
        }

        return Result<RepositoryId>.Success(
            new RepositoryId(Normalize(parts[0]), Normalize(parts[1])));
    }

    /// <summary>
    /// Parses a BAR <c>gitHubRepository</c> URL, tolerating the variations BAR actually
    /// stores: a trailing slash, a <c>.git</c> suffix, a <c>www.</c> host, and query or
    /// fragment noise.
    /// </summary>
    public static Result<RepositoryId> FromGitHubUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
        {
            return Result<RepositoryId>.Failure(
                ErrorCodes.RepositoryUnparseable,
                $"'{url}' is not an absolute repository URL.");
        }

        var host = uri.Host.ToLowerInvariant();
        if (host is not ("github.com" or "www.github.com"))
        {
            return Result<RepositoryId>.Failure(
                ErrorCodes.RepositoryUnparseable,
                $"Repository URL '{url}' is not hosted on github.com.");
        }

        var path = uri.GetLeftPart(UriPartial.Path)[uri.GetLeftPart(UriPartial.Authority).Length..]
            .Trim('/');

        if (path.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            path = path[..^4];
        }

        return Parse(path);
    }

    /// <summary>
    /// Reproduces Arcade's Azure DevOps mirror naming convention: lower-case the mirror
    /// repository name and treat its first <c>-</c> as the owner/name separator.
    /// </summary>
    /// <remarks>
    /// This exists because BAR stores a null <c>gitHubRepository</c> whenever Arcade could
    /// not verify the AzDO-to-GitHub mapping at publish time (it silently nulls the field on
    /// a GitHub API 404). Such a build is reachable only by BAR ID, and its identity must
    /// still be established from the mirror identity.
    /// </remarks>
    public static Result<RepositoryId> FromAzureDevOpsMirror(string? azureDevOpsRepository)
    {
        if (string.IsNullOrWhiteSpace(azureDevOpsRepository))
        {
            return Result<RepositoryId>.Failure(
                ErrorCodes.BarMirrorNameInvalid,
                "The build has neither a GitHub repository nor an Azure DevOps repository.");
        }

        var mirrorName = azureDevOpsRepository.Trim().TrimEnd('/').Split('/')[^1].ToLowerInvariant();
        var separator = mirrorName.IndexOf('-', StringComparison.Ordinal);
        if (separator <= 0 || separator == mirrorName.Length - 1)
        {
            return Result<RepositoryId>.Failure(
                ErrorCodes.BarMirrorNameInvalid,
                $"The build has no GitHub repository and mirror name '{mirrorName}' does not " +
                "follow the '<owner>-<name>' convention, so its identity cannot be established.");
        }

        return Result<RepositoryId>.Success(
            new RepositoryId(mirrorName[..separator], mirrorName[(separator + 1)..]));
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    public override string ToString() => FullName;
}

/// <summary>How a build's repository identity was established.</summary>
internal enum RepositoryOrigin
{
    /// <summary>BAR recorded a GitHub URL for the build.</summary>
    GitHubRepository,

    /// <summary>Derived from the Azure DevOps mirror name; see <see cref="RepositoryId.FromAzureDevOpsMirror"/>.</summary>
    AzureDevOpsMirrorConvention,
}
