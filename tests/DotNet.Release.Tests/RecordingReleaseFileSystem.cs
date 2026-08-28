namespace DotNet.Release.Tests;

/// <summary>
/// Wraps a real filesystem and records every effect, so a test can assert what a verb did —
/// and, more usefully, what it did <b>not</b> do.
/// </summary>
/// <remarks>
/// This is the assertion that could not be written while the verbs called <c>File.*</c>
/// directly: you cannot prove a negative about code that reaches the filesystem itself. With
/// the seam in place, "this run deleted nothing" and "this run wrote only under --out" become
/// ordinary unit tests.
/// </remarks>
internal sealed class RecordingReleaseFileSystem(IReleaseFileSystem inner) : IReleaseFileSystem
{
    public List<string> Created { get; } = [];

    public List<string> Copied { get; } = [];

    public List<string> Written { get; } = [];

    public List<string> Deleted { get; } = [];

    /// <summary>Every path this run modified, in effect order.</summary>
    public IReadOnlyList<string> Mutations => [.. Created, .. Copied, .. Written, .. Deleted];

    public bool FileExists(string path) => inner.FileExists(path);

    public bool DirectoryExists(string path) => inner.DirectoryExists(path);

    public IReadOnlyList<string> EnumerateFiles(string directory, string pattern, bool recursive) =>
        inner.EnumerateFiles(directory, pattern, recursive);

    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken) =>
        inner.ReadAllTextAsync(path, cancellationToken);

    public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken) =>
        inner.ReadAllBytesAsync(path, cancellationToken);

    public void CreateDirectory(string path)
    {
        Created.Add(path);
        inner.CreateDirectory(path);
    }

    public void CopyFile(string sourcePath, string destinationPath)
    {
        Copied.Add(destinationPath);
        inner.CopyFile(sourcePath, destinationPath);
    }

    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken)
    {
        Written.Add(path);
        return inner.WriteAllTextAsync(path, content, cancellationToken);
    }

    public void DeleteFile(string path)
    {
        Deleted.Add(path);
        inner.DeleteFile(path);
    }
}
