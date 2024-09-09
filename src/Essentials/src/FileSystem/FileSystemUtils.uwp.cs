#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Storage
{
	static partial class FileSystemUtils
	{
		public static bool AppPackageFileExists(string filename)
		{
			var file = PlatformGetFullAppPackageFilePath(filename);
			return File.Exists(file);
		}

		public static string PlatformGetFullAppPackageFilePath(string filename)
		{
			if (filename is null)
				throw new ArgumentNullException(nameof(filename));

			filename = NormalizePath(filename);
			return Path.Combine(AppInfoUtils.PlatformGetFullAppPackageFilePath, filename);
		}

		public static bool TryGetAppPackageFileUri(string filename, [NotNullWhen(true)] out string? uri)
		{
			var path = PlatformGetFullAppPackageFilePath(filename);

			if (File.Exists(path))
			{
				if (AppInfoUtils.IsPackagedApp)
					uri = $"ms-appx:///{filename.Replace('\\', '/')}";
				else
					uri = $"file:///{path.Replace('\\', '/')}";

				return true;
			}

			uri = null;
			return false;
		}
	}
}
