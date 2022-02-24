using System;
using System.Threading.Tasks;
using Tizen.Applications;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class LauncherImplementation : ILauncher
	{
		public Task<bool> CanOpenAsync(string uri)
		{
			return CanOpenAsync(new Uri(uri));
		}

		public Task<bool> CanOpenAsync(Uri uri)
			=> Task.FromResult(uri.IsWellFormedOriginalString());

		public Task OpenAsync(string uri)
		{
			return OpenAsync(new Uri(uri));
		}

		public Task OpenAsync(Uri uri)
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

			return Task.CompletedTask;
		}

		public Task OpenAsync(OpenFileRequest request)
		{
			if (string.IsNullOrEmpty(request.File.FullPath))
				throw new ArgumentNullException(nameof(request.File.FullPath));

			Permissions.EnsureDeclared<Permissions.LaunchApp>();

			var appControl = new AppControl
			{
				Operation = AppControlOperations.View,
				Mime = FileSystem.MimeTypes.All,
				Uri = "file://" + request.File.FullPath,
			};

			AppControl.SendLaunchRequest(appControl);

			return Task.CompletedTask;
		}

		public Task<bool> TryOpenAsync(string uri)
		{
			return TryOpenAsync(new Uri(uri));
		}

		public async Task<bool> TryOpenAsync(Uri uri)
		{
			var canOpen = await CanOpenAsync(uri);

			if (canOpen)
				await OpenAsync(uri);

			return canOpen;
		}
	}
}
