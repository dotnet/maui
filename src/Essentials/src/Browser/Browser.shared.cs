using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Browser']/Docs" />
	public static partial class Browser
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][0]/Docs" />
		public static Task OpenAsync(string uri) =>
			OpenAsync(uri, BrowserLaunchMode.SystemPreferred);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][2]/Docs" />
		public static Task OpenAsync(string uri, BrowserLaunchMode launchMode) =>
			OpenAsync(uri, new BrowserLaunchOptions()
			{
				LaunchMode = launchMode
			});

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][3]/Docs" />
		public static Task OpenAsync(string uri, BrowserLaunchOptions options)
		{
			if (string.IsNullOrWhiteSpace(uri))
			{
				throw new ArgumentNullException(nameof(uri), $"Uri cannot be empty.");
			}

			return OpenAsync(new Uri(uri), options);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][1]/Docs" />
		public static Task OpenAsync(Uri uri) =>
			OpenAsync(uri, BrowserLaunchMode.SystemPreferred);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][4]/Docs" />
		public static Task OpenAsync(Uri uri, BrowserLaunchMode launchMode) =>
			OpenAsync(uri, new BrowserLaunchOptions()
			{
				LaunchMode = launchMode
			});

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][5]/Docs" />
		public static Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) =>
			PlatformOpenAsync(EscapeUri(uri), options);

		internal static Uri EscapeUri(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			var idn = new global::System.Globalization.IdnMapping();
			return new Uri(uri.Scheme + "://" + idn.GetAscii(uri.Authority) + uri.PathAndQuery + uri.Fragment);
		}
	}
}
