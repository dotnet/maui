using System;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;


namespace Microsoft.Maui.Essentials
{
	public interface IBrowser
	{
		Task OpenAsync(string uri);

		Task OpenAsync(string uri, BrowserLaunchMode launchMode);
			
		Task OpenAsync(string uri, BrowserLaunchOptions options);

		Task OpenAsync(Uri uri);

		Task OpenAsync(Uri uri, BrowserLaunchMode launchMode);

		Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Browser']/Docs" />
	public static partial class Browser
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][1]/Docs" />
		public static Task OpenAsync(string uri) =>
			Current.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][3]/Docs" />
		public static Task OpenAsync(string uri, BrowserLaunchMode launchMode) =>
			Current.OpenAsync(uri, new BrowserLaunchOptions()
			{
				LaunchMode = launchMode
			});

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][4]/Docs" />
		public static Task OpenAsync(string uri, BrowserLaunchOptions options)
		{
			if (string.IsNullOrWhiteSpace(uri))
			{
				throw new ArgumentNullException(nameof(uri), $"Uri cannot be empty.");
			}

			return Current.OpenAsync(new Uri(uri), options);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][2]/Docs" />
		public static Task OpenAsync(Uri uri) =>
			Current.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][5]/Docs" />
		public static Task OpenAsync(Uri uri, BrowserLaunchMode launchMode) =>
			Current.OpenAsync(uri, new BrowserLaunchOptions()
			{
				LaunchMode = launchMode
			});

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][6]/Docs" />
		public static Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) =>
			Current.OpenAsync(EscapeUri(uri), options);

		internal static Uri EscapeUri(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			var idn = new global::System.Globalization.IdnMapping();
			return new Uri(uri.Scheme + "://" + idn.GetAscii(uri.Authority) + uri.PathAndQuery + uri.Fragment);
		}

#nullable enable
		static IBrowser? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IBrowser Current =>
			currentImplementation ??= new BrowserImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IBrowser? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}
}
