using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidEnvironment = Android.OS.Environment;
using AndroidUri = Android.Net.Uri;
#if __ANDROID_29__
using ContentFileProvider = AndroidX.Core.Content.FileProvider;
#else
using ContentFileProvider = Android.Support.V4.Content.FileProvider;
#endif

namespace Microsoft.Maui.Essentials
{
	[ContentProvider(
		new[] { "${applicationId}.fileProvider" },
		Name = "xamarin.essentials.fileProvider",
		Exported = false,
		GrantUriPermissions = true)]
	[MetaData(
		"android.support.FILE_PROVIDER_PATHS", // IMPORTANT: This string doesn't change with AndroidX
		Resource = "@xml/xamarin_essentials_fileprovider_file_paths")]
	public class FileProvider : ContentFileProvider
	{
		internal static bool AlwaysFailExternalMediaAccess { get; set; } = false;

		// This allows us to override the default temporary file location of Preferring external but falling back to internal
		// We can choose external only, or internal only as alternative options
		public static FileProviderLocation TemporaryLocation { get; set; } = FileProviderLocation.PreferExternal;

		internal static string Authority => Platform.AppContext.PackageName + ".fileProvider";

		internal static Java.IO.File GetTemporaryRootDirectory()
		{
			// If we specifically want the internal storage, no extra checks are needed, we have permission
			if (TemporaryLocation == FileProviderLocation.Internal)
				return Platform.AppContext.CacheDir;

			// If we explicitly want only external locations we need to do some permissions checking
			var externalOnly = TemporaryLocation == FileProviderLocation.External;

			// Check to see if we are >= API Level 19 (KitKat) since we don't need to declare the permission on these API levels to save to the external cache/storage
			// If we're not on 19 or higher we do need to check for permissions, but if we aren't limiting to external only, don't throw an exception if the
			// permission wasn't declared because we can always fall back to internal cache
			var hasPermission = Platform.HasApiLevel(BuildVersionCodes.Kitkat);

			if (!hasPermission)
			{
				hasPermission = Permissions.IsDeclaredInManifest(global::Android.Manifest.Permission.WriteExternalStorage);

				if (!hasPermission && externalOnly)
					throw new PermissionException("Cannot access external storage, the explicitly chosen FileProviderLocation.");
			}

			// make sure the external storage is available
			var hasExternalMedia = Platform.AppContext.ExternalCacheDir != null && IsMediaMounted(Platform.AppContext.ExternalCacheDir);

			// undo all the work if we have requested a fail (mainly for testing)
			if (AlwaysFailExternalMediaAccess)
				hasExternalMedia = false;

			// fail if we need the external storage, but there is none
			if (externalOnly && !hasExternalMedia)
				throw new InvalidOperationException("Unable to access the external storage, the media is not mounted.");

			// based on permssions, return the correct directory
			// if permission were required, then it would have already thrown
			return hasPermission && hasExternalMedia
				? Platform.AppContext.ExternalCacheDir
				: Platform.AppContext.CacheDir;
		}

		static bool IsMediaMounted(Java.IO.File location) =>
			Platform.HasApiLevel(BuildVersionCodes.Lollipop)
				? AndroidEnvironment.GetExternalStorageState(location) == AndroidEnvironment.MediaMounted
#pragma warning disable CS0618 // Type or member is obsolete
				: AndroidEnvironment.GetStorageState(location) == AndroidEnvironment.MediaMounted;
#pragma warning restore CS0618 // Type or member is obsolete

		internal static bool IsFileInPublicLocation(string filename)
		{
			// get the Android path, we use "CanonicalPath" instead of "AbsolutePath"
			// because we want to resolve any ".." and links/redirects
			var file = new Java.IO.File(filename);
			filename = file.CanonicalPath;

			// the shared paths from the "xamarin_essentials_fileprovider_file_paths.xml" resource
			var publicLocations = new List<string>
			{
#if __ANDROID_29__
                Platform.AppContext?.GetExternalFilesDir(null)?.CanonicalPath,
#else
#pragma warning disable CS0618 // Type or member is obsolete
                AndroidEnvironment.ExternalStorageDirectory?.CanonicalPath,
#pragma warning restore CS0618 // Type or member is obsolete
#endif
                Platform.AppContext?.ExternalCacheDir?.CanonicalPath
			};

			// the internal cache path is available only by file provider in N+
			if (Platform.HasApiLevelN)
				publicLocations.Add(Platform.AppContext?.CacheDir?.CanonicalPath);

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
			FileProvider.GetUriForFile(Platform.AppContext, Authority, file);
	}

	public enum FileProviderLocation
	{
		PreferExternal,
		Internal,
		External,
	}
}
