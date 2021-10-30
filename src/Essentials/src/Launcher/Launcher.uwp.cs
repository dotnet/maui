using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using WinLauncher = Windows.System.Launcher;

namespace Microsoft.Maui.Essentials
{
	public static partial class Launcher
	{
		static async Task<bool> PlatformCanOpenAsync(Uri uri)
		{
			var supported = await WinLauncher.QueryUriSupportAsync(uri, LaunchQuerySupportType.Uri);
			return supported == LaunchQuerySupportStatus.Available;
		}

		static Task PlatformOpenAsync(Uri uri) =>
			WinLauncher.LaunchUriAsync(uri).AsTask();

		static async Task PlatformOpenAsync(OpenFileRequest request)
		{
			var storageFile = request.File.File ?? await StorageFile.GetFileFromPathAsync(request.File.FullPath);

			await WinLauncher.LaunchFileAsync(storageFile).AsTask();
		}

		static async Task<bool> PlatformTryOpenAsync(Uri uri)
		{
			var canOpen = await PlatformCanOpenAsync(uri);

			if (canOpen)
				return await WinLauncher.LaunchUriAsync(uri).AsTask();

			return canOpen;
		}
	}
}
