using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class BrowserImplementation : IBrowser
	{
		public Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) =>
			global::Windows.System.Launcher.LaunchUriAsync(uri).AsTask();

		public Task OpenAsync(string uri)
		{
			return OpenAsync
						(
							new Uri(uri), 
							new BrowserLaunchOptions
							{
								Flags = BrowserLaunchFlags.None,
								LaunchMode = BrowserLaunchMode.SystemPreferred,
								TitleMode = BrowserTitleMode.Default
							}
						);
		}

		public Task OpenAsync(string uri, BrowserLaunchMode launchMode)
		{
			return OpenAsync
						(
							new Uri(uri), 
							new BrowserLaunchOptions
							{
								Flags = BrowserLaunchFlags.None,
								LaunchMode = launchMode,
								TitleMode = BrowserTitleMode.Default
							}
						);
		}
			
		public Task OpenAsync(string uri, BrowserLaunchOptions options)
		{
			return OpenAsync(new Uri(uri), options);
		}

		public Task OpenAsync(Uri uri)
		{
			return OpenAsync
						(
							uri,
							new BrowserLaunchOptions
							{
								Flags = BrowserLaunchFlags.None,
								LaunchMode = BrowserLaunchMode.SystemPreferred,
								TitleMode = BrowserTitleMode.Default
							}
						);
		}

		public Task OpenAsync(Uri uri, BrowserLaunchMode launchMode)
		{
			return OpenAsync
						(
							uri, 
							new BrowserLaunchOptions
							{
								Flags = BrowserLaunchFlags.None,
								LaunchMode = launchMode,
								TitleMode = BrowserTitleMode.Default
							}
						);
		}
	}
}
