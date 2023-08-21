// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using WinLauncher = Windows.System.Launcher;

namespace Microsoft.Maui.ApplicationModel
{
	partial class LauncherImplementation
	{
		async Task<bool> PlatformCanOpenAsync(Uri uri)
		{
			var supported = await WinLauncher.QueryUriSupportAsync(uri, LaunchQuerySupportType.Uri);
			return supported == LaunchQuerySupportStatus.Available;
		}

		Task<bool> PlatformOpenAsync(Uri uri) =>
			WinLauncher.LaunchUriAsync(uri).AsTask();

		async Task<bool> PlatformOpenAsync(OpenFileRequest request)
		{
			var storageFile = request.File.File ?? await StorageFile.GetFileFromPathAsync(request.File.FullPath);

			return await WinLauncher.LaunchFileAsync(storageFile).AsTask();
		}

		async Task<bool> PlatformTryOpenAsync(Uri uri)
		{
			var canOpen = await PlatformCanOpenAsync(uri);

			if (canOpen)
				return await WinLauncher.LaunchUriAsync(uri).AsTask();

			return canOpen;
		}
	}
}
