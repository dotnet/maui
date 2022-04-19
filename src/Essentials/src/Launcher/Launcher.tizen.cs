using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Tizen.Applications;

namespace Microsoft.Maui.ApplicationModel
{
	partial class LauncherImplementation
	{
		Task<bool> PlatformCanOpenAsync(Uri uri)
			=> Task.FromResult(uri.IsWellFormedOriginalString());

		Task<bool> PlatformOpenAsync(Uri uri)
		{
			Permissions.EnsureDeclared<Permissions.LaunchApp>();

			var appControl = new AppControl
			{
				Operation = AppControlOperations.ShareText,
				Uri = uri.AbsoluteUri
			};

			if (uri.AbsoluteUri.StartsWith("geo:"))
				appControl.Operation = AppControlOperations.Pick;
			else if (uri.AbsoluteUri.StartsWith("http"))
				appControl.Operation = AppControlOperations.View;
			else if (uri.AbsoluteUri.StartsWith("mailto:"))
				appControl.Operation = AppControlOperations.Compose;
			else if (uri.AbsoluteUri.StartsWith("sms:"))
				appControl.Operation = AppControlOperations.Compose;
			else if (uri.AbsoluteUri.StartsWith("tel:"))
				appControl.Operation = AppControlOperations.Dial;

			AppControl.SendLaunchRequest(appControl);

			return Task.FromResult(true);
		}

		Task<bool> PlatformOpenAsync(OpenFileRequest request)
		{
			if (string.IsNullOrEmpty(request.File.FullPath))
				throw new ArgumentNullException(nameof(request.File.FullPath));

			Permissions.EnsureDeclared<Permissions.LaunchApp>();

			var appControl = new AppControl
			{
				Operation = AppControlOperations.View,
				Mime = FileMimeTypes.All,
				Uri = "file://" + request.File.FullPath,
			};

			AppControl.SendLaunchRequest(appControl);

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
