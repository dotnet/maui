#nullable enable
using System.IO;

namespace Microsoft.Maui.Storage
{
	static partial class FileSystemUtils
	{
		public static string NormalizePath(string filename) =>
			filename
				.Replace('\\', Path.DirectorySeparatorChar)
				.Replace('/', Path.DirectorySeparatorChar);
	}
}