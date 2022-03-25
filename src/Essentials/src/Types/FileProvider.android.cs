using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Maui.ApplicationModel;
using AndroidEnvironment = Android.OS.Environment;
using AndroidUri = Android.Net.Uri;
using ContentFileProvider = AndroidX.Core.Content.FileProvider;

namespace Microsoft.Maui.Storage
{
	[ContentProvider(
		new[] { "${applicationId}.fileProvider" },
		Name = "microsoft.maui.essentials.fileProvider",
		Exported = false,
		GrantUriPermissions = true)]
	[MetaData(
		"android.support.FILE_PROVIDER_PATHS", // IMPORTANT: This string doesn't change with AndroidX
		Resource = "@xml/microsoft_maui_essentials_fileprovider_file_paths")]
	public class FileProvider : ContentFileProvider
	{
		internal static bool AlwaysFailExternalMediaAccess { get; set; } = false;

		// This allows us to override the default temporary file location of Preferring external but falling back to internal
		// We can choose external only, or internal only as alternative options
		public static FileProviderLocation TemporaryLocation { get; set; } = FileProviderLocation.PreferExternal;

		internal static string Authority => Application.Context.PackageName + ".fileProvider";

		internal static Java.IO.File GetTemporaryRootDirectory()
		{
			// If we specifically want the internal storage, no extra checks are needed, we have permission
			if (TemporaryLocation == FileProviderLocation.Internal)
				return Application.Context.CacheDir;

			// If we explicitly want only external locations we need to do some permissions checking
			var externalOnly = TemporaryLocation == FileProviderLocation.External;

			// make sure the external storage is available
			var hasExternalMedia = Application.Context.ExternalCacheDir != null && IsMediaMounted(Application.Context.ExternalCacheDir);

			// undo all the work if we have requested a fail (mainly for testing)
			if (AlwaysFailExternalMediaAccess)
				hasExternalMedia = false;

			// fail if we need the external storage, but there is none
			if (externalOnly && !hasExternalMedia)
				throw new InvalidOperationException("Unable to access the external storage, the media is not mounted.");

			// based on permssions, return the correct directory
			// if permission were required, then it would have already thrown
			return hasExternalMedia
				? Application.Context.ExternalCacheDir
				: Application.Context.CacheDir;
		}

		static bool IsMediaMounted(Java.IO.File location) =>
			AndroidEnvironment.GetExternalStorageState(location) == AndroidEnvironment.MediaMounted;

		internal static bool IsFileInPublicLocation(string filename)
		{
			// get the Android path, we use "CanonicalPath" instead of "AbsolutePath"
			// because we want to resolve any ".." and links/redirects
			var file = new Java.IO.File(filename);
			filename = file.CanonicalPath;

			// the shared paths from the "microsoft_maui_essentials_fileprovider_file_paths.xml" resource
			var publicLocations = new List<string>
			{
#if __ANDROID_29__
				Application.Context?.GetExternalFilesDir(null)?.CanonicalPath,
#else
#pragma warning disable CS0618 // Type or member is obsolete
				AndroidEnvironment.ExternalStorageDirectory?.CanonicalPath,
#pragma warning restore CS0618 // Type or member is obsolete
#endif
				Application.Context?.ExternalCacheDir?.CanonicalPath
			};

			// the internal cache path is available only by file provider in N+
			if (OperatingSystem.IsAndroidVersionAtLeast(24))
				publicLocations.Add(Application.Context?.CacheDir?.CanonicalPath);

			foreach (var location in publicLocations)
			{
				if (string.IsNullOrWhiteSpace(location))
					continue;

				// make sure we have a trailing slash
				var suffixedPath = filename.EndsWith(Java.IO.File.Separator)
					? filename
					: filename + Java.IO.File.Separator;

				// check if the requested file is in a folder
				if (suffixedPath.StartsWith(location, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		internal static AndroidUri GetUriForFile(Java.IO.File file) =>
			FileProvider.GetUriForFile(Application.Context, Authority, file);
	}

	public enum FileProviderLocation
	{
		PreferExternal,
		Internal,
		External,
	}
}
