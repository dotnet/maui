using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui.Storage;
using AndroidUri = Android.Net.Uri;
using Uri = System.Uri;

namespace Microsoft.Maui.ApplicationModel
{
	partial class LauncherImplementation
	{
		Task<bool> PlatformCanOpenAsync(Uri uri)
		{
			var intent = new Intent(Intent.ActionView, AndroidUri.Parse(uri.OriginalString));

			if (Application.Context == null)
				return Task.FromResult(false);

			var manager = Application.Context.PackageManager;
			var supportedResolvedInfos = manager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
			return Task.FromResult(supportedResolvedInfos.Any());
		}

		Task<bool> PlatformOpenAsync(Uri uri)
		{
			var intent = new Intent(Intent.ActionView, AndroidUri.Parse(uri.OriginalString));
			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
#if __ANDROID_24__
			if (OperatingSystem.IsAndroidVersionAtLeast(24))
				flags |= ActivityFlags.LaunchAdjacent;
#endif
			intent.SetFlags(flags);

			Application.Context.StartActivity(intent);
			return Task.FromResult(true);
		}

		Task<bool> PlatformOpenAsync(OpenFileRequest request)
		{
			var contentUri = FileSystemUtils.GetShareableFileUri(request.File);

			var intent = new Intent(Intent.ActionView);
			intent.SetDataAndType(contentUri, request.File.ContentType);
			intent.SetFlags(ActivityFlags.GrantReadUriPermission);

			var chooserIntent = Intent.CreateChooser(intent, request.Title ?? string.Empty);
			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
#if __ANDROID_24__
			if (OperatingSystem.IsAndroidVersionAtLeast(24))
				flags |= ActivityFlags.LaunchAdjacent;
#endif
			chooserIntent.SetFlags(flags);

			Application.Context.StartActivity(chooserIntent);

			return Task.FromResult(true);
		}

		async Task<bool> PlatformTryOpenAsync(Uri uri)
		{
			var canOpen = await PlatformCanOpenAsync(uri);

			if (canOpen)
				await PlatformOpenAsync(uri);

			return canOpen;
		}
	}
}
