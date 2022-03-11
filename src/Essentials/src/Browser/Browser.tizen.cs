using System;
using System.Linq;
using System.Threading.Tasks;
using Tizen.Applications;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class BrowserImplementation : IBrowser
	{
		public Task<bool> PlatformOpenAsync(Uri uri, BrowserLaunchOptions launchMode)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			Permissions.EnsureDeclared<Permissions.LaunchApp>();

			var appControl = new AppControl
			{
				Operation = AppControlOperations.View,
				Uri = uri.AbsoluteUri
			};

			var hasMatches = AppControl.GetMatchedApplicationIds(appControl).Any();

			if (hasMatches)
				AppControl.SendLaunchRequest(appControl);

			return Task.FromResult(hasMatches);
		}

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
