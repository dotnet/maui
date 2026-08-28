using System.Security.Cryptography;
using NuGet.Packaging;

namespace DotNet.Release;

/// <summary>
/// Reads package identity and content hash from a <c>.nupkg</c> on local disk.
/// </summary>
/// <remarks>
/// <c>PackageArchiveReader</c> locates and parses the nuspec, and <c>NuGetVersion</c>
/// supplies the normalized version used for availability queries.
/// </remarks>
public sealed class NupkgIdentityReader : IPackageIdentityReader
{
    /// <inheritdoc />
    public async Task<Result<DropPackage>> ReadAsync(string packageFilePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageFilePath);

        var fileName = Path.GetFileName(packageFilePath);

        if (!File.Exists(packageFilePath))
        {
            return Result<DropPackage>.Failure(
                ErrorCodes.PackageFileMissing,
                $"Package file '{packageFilePath}' was not found.");
        }

        string id;
        string version;
        try
        {
            using var stream = File.OpenRead(packageFilePath);
            using var reader = new PackageArchiveReader(stream);
            var identity = reader.NuspecReader.GetIdentity();

            if (identity is null || string.IsNullOrWhiteSpace(identity.Id) || identity.Version is null)
            {
                return Result<DropPackage>.Failure(
                    ErrorCodes.PackageMalformed,
                    $"Package '{fileName}' has no ID or version.");
            }

            id = identity.Id.Trim();

            // The nuspec's literal version is kept for display and for the audit trail; the
            // normalized form is what NuGet.org indexes under and what queries are built from.
            version = identity.Version.ToFullString();
        }
        catch (Exception ex) when (ex is InvalidDataException
            or global::NuGet.Packaging.Core.PackagingException
            or System.Xml.XmlException)
        {
            return Result<DropPackage>.Failure(
                ErrorCodes.PackageMalformed,
                $"Package '{fileName}' could not be read: {ex.Message}");
        }

        var normalized = PackageVersions.Normalize(version);
        if (normalized.IsFailure)
        {
            return normalized.ToFailure<DropPackage>();
        }

        var sha256 = await ComputeSha256Async(packageFilePath, cancellationToken).ConfigureAwait(false);

        return Result<DropPackage>.Success(new DropPackage(fileName, id, version, normalized.Value, sha256));
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexStringLower(hash);
    }
}
