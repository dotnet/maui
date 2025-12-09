#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Storage
{
	static partial class FileSystemUtils
	{
		// Use case-insensitive comparison on Windows, case-sensitive on other platforms
		static StringComparison PathComparison => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
			? StringComparison.OrdinalIgnoreCase
			: StringComparison.Ordinal;

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
		/// <exception cref="ArgumentException">Thrown when <paramref name="relativePath"/> is empty, contains only whitespace, is an absolute path, or is a UNC path (Windows).</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown when the resulting path would escape the base directory (path traversal detected).</exception>
		/// <remarks>
		/// <para>
		/// <strong>Security Considerations:</strong>
		/// </para>
		/// <list type="bullet">
		/// <item><description>This method rejects absolute paths (paths starting with / or drive letters like C:\) to prevent complete bypass of the base directory.</description></item>
		/// <item><description>On Windows, UNC paths (\\server\share) are rejected to prevent network path attacks.</description></item>
		/// <item><description>Empty strings, whitespace, tabs, and newlines are rejected.</description></item>
		/// <item><description>Path comparison uses case-insensitive matching on Windows and case-sensitive matching on Unix-based platforms (iOS, Android, macOS).</description></item>
		/// </list>
		/// <para>
		/// <strong>Limitations:</strong>
		/// </para>
		/// <list type="bullet">
		/// <item><description>This method is subject to TOCTOU (time-of-check-time-of-use) race conditions. The path is validated at call time but the filesystem could change before the file is actually accessed.</description></item>
		/// <item><description>Symbolic links are resolved by <see cref="Path.GetFullPath(string)"/>. If symlinks point outside the base directory, this will be detected. However, symlinks created after validation could potentially bypass protection.</description></item>
		/// </list>
		/// </remarks>
		public static string GetSecurePath(string basePath, string relativePath)
		{
			if (basePath is null)
				throw new ArgumentNullException(nameof(basePath));
			if (relativePath is null)
				throw new ArgumentNullException(nameof(relativePath));
			if (string.IsNullOrWhiteSpace(relativePath))
				throw new ArgumentException("Path cannot be empty or whitespace.", nameof(relativePath));

			// Check for absolute paths - these could bypass the base directory entirely
			// Path.Combine("/app/data", "/etc/passwd") would return "/etc/passwd"
			if (Path.IsPathRooted(relativePath))
				throw new ArgumentException("Absolute paths are not allowed.", nameof(relativePath));

			// Check for UNC paths on Windows (e.g., \\server\share\file.txt)
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
			    relativePath.Length >= 2 &&
			    (relativePath.StartsWith(@"\\", StringComparison.Ordinal) ||
			     relativePath.StartsWith("//", StringComparison.Ordinal)))
			{
				throw new ArgumentException("UNC paths are not allowed.", nameof(relativePath));
			}

			// Normalize the relative path first to handle mixed slashes
			relativePath = NormalizePath(relativePath);

			// Combine and get the full absolute path
			string fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));

			// Get the normalized base path
			string normalizedBasePath = Path.GetFullPath(basePath);

			// Check if the full path equals the base path exactly (e.g., when relativePath is ".")
			if (fullPath.Equals(normalizedBasePath, PathComparison))
			{
				return fullPath;
			}

			// Ensure the base path ends with a directory separator for proper comparison
			// This prevents false positives where basePath="/app/data" would match "/app/data_other"
			// Check for both primary and alternate directory separators
			if (!normalizedBasePath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) &&
			    !normalizedBasePath.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
			{
				normalizedBasePath += Path.DirectorySeparatorChar;
			}

			// Validate that the resolved path is within the base directory
			// Use platform-appropriate case sensitivity
			if (!fullPath.StartsWith(normalizedBasePath, PathComparison))
			{
				throw new UnauthorizedAccessException($"Access to path '{relativePath}' is not allowed. Path traversal detected.");
			}

			return fullPath;
		}
	}
}