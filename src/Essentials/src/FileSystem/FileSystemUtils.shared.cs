#nullable enable
using System;
using System.IO;

namespace Microsoft.Maui.Storage
{
	static partial class FileSystemUtils
	{
		/// <summary>
		/// Normalizes a display name from a content provider by stripping directory components
		/// and appending the given extension when the name has none.
		/// </summary>
		internal static string EnsureFileName(string? displayName, string? extension)
		{
			var filename = displayName ?? Guid.NewGuid().ToString("N");

			// strip any directory components — some providers return full paths
			filename = Path.GetFileName(filename);

			// if we ended up with nothing useful, or with a reserved directory name,
			// fall back to a generated name
			if (string.IsNullOrWhiteSpace(filename) || filename == "." || filename == "..")
				filename = Guid.NewGuid().ToString("N");

			// add the best/known extension
			if (!Path.HasExtension(filename) && !string.IsNullOrEmpty(extension))
				filename = Path.ChangeExtension(filename, extension);

			return filename;
		}

		/// <summary>
		/// Normalizes the given file path for the current platform.
		/// </summary>
		/// <param name="filename">The file path to normalize.</param>
		/// <returns>
		/// The normalized version of the file path provided in <paramref name="filename"/>.
		/// Forward and backward slashes will be replaced by <see cref="Path.DirectorySeparatorChar"/>
		/// so that it is correct for the current platform.
		/// </returns>
		public static string NormalizePath(string filename) =>
			filename
				.Replace('\\', Path.DirectorySeparatorChar)
				.Replace('/', Path.DirectorySeparatorChar);

		/// <summary>
		/// Validates that a relative path does not resolve to an absolute or rooted
		/// location and does not contain relative parent (..) segments.
		/// </summary>
		internal static bool IsValidRelativePath(string? relativePath)
		{
			if (string.IsNullOrEmpty(relativePath))
				return true;

			if (Path.IsPathRooted(relativePath!))
				return false;

			// Check for ".." as a path segment, not as a substring,
			// so that filenames like "foo..bar.js" are not rejected.
			var segments = relativePath!.Split(new[] { '\\', '/' }, StringSplitOptions.None);
			foreach (var segment in segments)
			{
				if (string.Equals(segment, "..", StringComparison.Ordinal))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Combines a root directory with a relative path, validates the relative path,
		/// and verifies the resolved full path is still within the root directory.
		/// Returns <c>null</c> if the relative path is invalid or the combined path
		/// falls outside the root.
		/// </summary>
		internal static string? Combine(string rootDirectory, string relativePath)
		{
			if (!IsValidRelativePath(relativePath))
				return null;

			var combined = Path.Combine(rootDirectory, relativePath);

			// When the root directory is absolute, resolve and verify the full path
			// stays within the root using the file system's canonical paths.
			if (Path.IsPathRooted(rootDirectory))
			{
				var fullPath = Path.GetFullPath(combined);

				var normalizedRoot = Path.GetFullPath(rootDirectory);
				if (!normalizedRoot.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
					normalizedRoot += Path.DirectorySeparatorChar;

				// Use case-insensitive comparison to handle platforms with case-insensitive
				// file systems (e.g., Windows, macOS) without requiring platform detection.
				if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(fullPath + Path.DirectorySeparatorChar, normalizedRoot, StringComparison.OrdinalIgnoreCase))
					return null;

				return fullPath;
			}

			// For relative roots (e.g., app-package or asset-relative paths like
			// Android's "HybridTestRoot" or "wwwroot"), preserve the relative semantics.
			// IsValidRelativePath has already ensured the relativePath is not rooted
			// and does not contain "..", so the combined path stays within the root.
			return NormalizePath(combined);
		}
	}
}