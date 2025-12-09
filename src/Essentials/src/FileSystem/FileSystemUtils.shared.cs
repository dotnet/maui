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
		/// Combines a base path with a relative path and ensures the result doesn't escape the base directory.
		/// This method prevents path traversal attacks by validating that the resulting path stays within the base directory.
		/// </summary>
		/// <param name="basePath">The base directory path that the result must stay within.</param>
		/// <param name="relativePath">The relative path to append to the base path.</param>
		/// <returns>The combined, normalized full path.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="basePath"/> or <paramref name="relativePath"/> is null.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown when the resulting path would escape the base directory (path traversal detected).</exception>
		public static string GetSecurePath(string basePath, string relativePath)
		{
			if (basePath is null)
				throw new ArgumentNullException(nameof(basePath));
			if (relativePath is null)
				throw new ArgumentNullException(nameof(relativePath));

			// Normalize the relative path first to handle mixed slashes
			relativePath = NormalizePath(relativePath);

			// Combine and get the full absolute path
			string fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));

			// Get the normalized base path
			string normalizedBasePath = Path.GetFullPath(basePath);

			// Check if the full path equals the base path exactly (e.g., when relativePath is ".")
			if (fullPath.Equals(normalizedBasePath, StringComparison.OrdinalIgnoreCase))
			{
				return fullPath;
			}

			// Ensure the base path ends with a directory separator for proper comparison
			// This prevents false positives where basePath="/app/data" would match "/app/data_other"
			if (!normalizedBasePath.EndsWith(Path.DirectorySeparatorChar))
				normalizedBasePath += Path.DirectorySeparatorChar;

			// Validate that the resolved path is within the base directory
			if (!fullPath.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase))
			{
				throw new UnauthorizedAccessException($"Access to path '{relativePath}' is not allowed. Path traversal detected.");
			}

			return fullPath;
		}
	}
}