#nullable enable
using System;
using System.IO;

namespace Microsoft.Maui.Storage
{
	static partial class FileSystemUtils
	{
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

			if (Path.IsPathRooted(relativePath))
				return false;

			if (relativePath.Contains("..", StringComparison.Ordinal))
				return false;

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
			var fullPath = Path.GetFullPath(combined);

			var normalizedRoot = Path.GetFullPath(rootDirectory);
			if (!normalizedRoot.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
				normalizedRoot += Path.DirectorySeparatorChar;

			if (!fullPath.StartsWith(normalizedRoot, StringComparison.Ordinal))
				return null;

			return fullPath;
		}
	}
}