using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class Browser
	{
		public static Task OpenAsync(string uri) =>
			OpenAsync(uri, BrowserLaunchMode.SystemPreferred);

		public static Task OpenAsync(string uri, BrowserLaunchMode launchMode) =>
			OpenAsync(uri, new BrowserLaunchOptions()
			{
				LaunchMode = launchMode
			});

		public static Task OpenAsync(string uri, BrowserLaunchOptions options)
		{
			if (string.IsNullOrWhiteSpace(uri))
			{
				throw new ArgumentNullException(nameof(uri), $"Uri cannot be empty.");
			}

			return OpenAsync(new Uri(uri), options);
		}

		public static Task OpenAsync(Uri uri) =>
			OpenAsync(uri, BrowserLaunchMode.SystemPreferred);

		public static Task OpenAsync(Uri uri, BrowserLaunchMode launchMode) =>
			OpenAsync(uri, new BrowserLaunchOptions()
			{
				LaunchMode = launchMode
			});

		public static Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) =>
			PlatformOpenAsync(EscapeUri(uri), options);

		internal static Uri EscapeUri(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

#if NETSTANDARD1_0
			return uri;
#else
            var idn = new System.Globalization.IdnMapping();
            return new Uri(uri.Scheme + "://" + idn.GetAscii(uri.Authority) + uri.PathAndQuery + uri.Fragment);
#endif
		}
	}
}
