#nullable enable
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
	}
}