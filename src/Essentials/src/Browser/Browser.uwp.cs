#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	partial class BrowserImplementation : IBrowser
	{
		public Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) =>
			global::Windows.System.Launcher.LaunchUriAsync(uri).AsTask();
	}
}
