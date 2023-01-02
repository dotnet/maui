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
			return Task.FromResult(PlatformUtils.IsIntentSupported(intent));
		}

		Task<bool> PlatformOpenAsync(Uri uri)
		{
			var intent = new Intent(Intent.ActionView, AndroidUri.Parse(uri.OriginalString));
			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
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
