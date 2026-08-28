namespace DotNet.Release;

/// <summary>
/// Every filesystem effect the tool can have.
/// </summary>
/// <remarks>
/// <para>
/// This exists to turn the design's organising constraint — <i>the tool mutates nothing
/// outside its own output directory</i> — from a property of the code that happened to be
/// written into an invariant something enforces.
/// </para>
/// <para>
/// Reads are deliberately unrooted: the tool legitimately reads the policy file, the gathered
/// drop and the published tool binary, all outside its output directory. <b>Writes and
/// deletes are rooted</b>, which is where the risk is: <c>filter</c> deletes files whose paths
/// come from a plan file, and a plan is data.
/// </para>
/// <para>
/// It is also the seam that makes "this run mutated nothing" testable. A recording
/// implementation can assert that <c>plan</c> wrote exactly one file, or that <c>filter</c>
/// deleted exactly the non-pending packages and nothing else — assertions that cannot be
/// written against direct <c>File.*</c> calls, because you cannot prove a negative about code
/// that reaches the filesystem directly.
/// </para>
/// <para>
/// Note the boundary this does <i>not</i> cross. There is no push capability here and there
/// never will be: <c>1ES.PublishNuget@1</c> owns every upload, and the tool holds no NuGet.org
/// credential. That guarantee comes from the absence of the capability and from credential
/// scoping, not from an interface (docs/design.md section 3).
/// </para>
/// </remarks>
public interface IReleaseFileSystem
{
    // ---- reads: unrooted, because the inputs legitimately live elsewhere ----

    bool FileExists(string path);

    bool DirectoryExists(string path);

    IReadOnlyList<string> EnumerateFiles(string directory, string pattern, bool recursive);

    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken);

    Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken);

    // ---- effects: rooted ----

    void CreateDirectory(string path);

    void CopyFile(string sourcePath, string destinationPath);

    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken);

    void DeleteFile(string path);
}

/// <summary>
/// The real filesystem, with every write and delete confined to one directory tree.
/// </summary>
/// <remarks>
/// <para>
/// The containment check is the point. <c>filter</c> resolves deletion targets from
/// <c>release-plan.json</c>, and although <c>stage</c> validates that package file names carry
/// no directory component, <c>filter</c> re-derives nothing and a plan supplied by another
/// route is just data. Rooting means a crafted plan cannot reach outside the staging tree
/// even if every other check were bypassed — the containment does not depend on the hash
/// chain, on validation order, or on the caller.
/// </para>
/// <para>
/// Paths are compared after <see cref="Path.GetFullPath(string)"/>, which resolves
/// <c>..</c> segments, so a traversal is rejected rather than normalised away silently.
/// </para>
/// </remarks>
public sealed class PhysicalReleaseFileSystem : IReleaseFileSystem
{
    private readonly string? _writeRoot;

    /// <param name="writeRoot">
    /// Directory that every write and delete must stay within. When null, effects are
    /// unrestricted — intended only for callers that perform none.
    /// </param>
    public PhysicalReleaseFileSystem(string? writeRoot = null) =>
        _writeRoot = writeRoot is null ? null : Path.GetFullPath(writeRoot);

    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public IReadOnlyList<string> EnumerateFiles(string directory, string pattern, bool recursive) =>
        Directory.Exists(directory)
            ? [.. Directory.EnumerateFiles(
                directory,
                pattern,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)]
            : [];

    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken) =>
        File.ReadAllTextAsync(path, cancellationToken);

    public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken) =>
        File.ReadAllBytesAsync(path, cancellationToken);

    public void CreateDirectory(string path) => Directory.CreateDirectory(Guard(path));

    public void CopyFile(string sourcePath, string destinationPath) =>
        File.Copy(sourcePath, Guard(destinationPath), overwrite: true);

    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken) =>
        File.WriteAllTextAsync(Guard(path), content, cancellationToken);

    public void DeleteFile(string path) => File.Delete(Guard(path));

    /// <summary>Rejects any effect whose resolved path escapes the write root.</summary>
    private string Guard(string path)
    {
        if (_writeRoot is null)
        {
            return path;
        }

        var resolved = Path.GetFullPath(path);

        var contained = resolved.Equals(_writeRoot, PathComparison) ||
            resolved.StartsWith(_writeRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, PathComparison);

        if (!contained)
        {
            throw new UnauthorizedAccessException(
                $"The release tool may only write below '{_writeRoot}', but something " +
                $"attempted to modify '{resolved}'.");
        }

        return path;
    }

    /// <summary>Windows and macOS paths are case-insensitive; Linux is not.</summary>
    private static StringComparison PathComparison =>
        OperatingSystem.IsLinux() ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
}
