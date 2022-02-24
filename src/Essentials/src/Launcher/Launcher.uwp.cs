using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using WinLauncher = Windows.System.Launcher;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class LauncherImplementation : ILauncher
	{
		public async Task<bool> CanOpenAsync(string uri)
		{
			return await CanOpenAsync(new Uri(uri));
		}

		public async Task<bool> CanOpenAsync(Uri uri)
		{
			var supported = await WinLauncher.QueryUriSupportAsync(uri, LaunchQuerySupportType.Uri);
			return supported == LaunchQuerySupportStatus.Available;
		}

		public async Task OpenAsync(string uri)
		{
			return await OpenAsync(new Uri(uri));
		}

		public Task OpenAsync(Uri uri) =>
			WinLauncher.LaunchUriAsync(uri).AsTask();

		public async Task OpenAsync(OpenFileRequest request)
		{
			var storageFile = request.File.File ?? await StorageFile.GetFileFromPathAsync(request.File.FullPath);

			await WinLauncher.LaunchFileAsync(storageFile).AsTask();
		}

		public async Task<bool> TryOpenAsync(string uri)
		{
			return await TryOpenAsync(new Uri(uri));
		}

		public async Task<bool> TryOpenAsync(Uri uri)
		{
			var canOpen = await CanOpenAsync(uri);

			if (canOpen)
				return await WinLauncher.LaunchUriAsync(uri).AsTask();

			return canOpen;
		}
	}
}
