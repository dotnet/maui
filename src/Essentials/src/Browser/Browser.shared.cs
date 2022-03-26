#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	public interface IBrowser
	{
		Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Browser']/Docs" />
	public static class Browser
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][1]/Docs" />
		public static Task<bool> OpenAsync(string uri) => Default.OpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][3]/Docs" />
		public static Task<bool> OpenAsync(string uri, BrowserLaunchMode launchMode) => Default.OpenAsync(uri, launchMode);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][4]/Docs" />
		public static Task<bool> OpenAsync(string uri, BrowserLaunchOptions options) => Default.OpenAsync(uri, options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][2]/Docs" />
		public static Task<bool> OpenAsync(Uri uri) => Default.OpenAsync(uri);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][5]/Docs" />
		public static Task<bool> OpenAsync(Uri uri, BrowserLaunchMode launchMode) => Default.OpenAsync(uri, launchMode);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Browser.xml" path="//Member[@MemberName='OpenAsync'][6]/Docs" />
		public static Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) => Default.OpenAsync(uri, options);

		static IBrowser? defaultImplementation;

		public static IBrowser Default =>
			defaultImplementation ??= new BrowserImplementation();

		internal static void SetDefault(IBrowser? implementation) =>
			defaultImplementation = implementation;
	}

	public static class BrowserExtensions
	{
		public static Task<bool> OpenAsync(this IBrowser browser, string uri) =>
			browser.OpenAsync(new Uri(uri), new BrowserLaunchOptions());

		public static Task<bool> OpenAsync(this IBrowser browser, string uri, BrowserLaunchMode launchMode) =>
			browser.OpenAsync(new Uri(uri), new BrowserLaunchOptions { LaunchMode = launchMode });

		public static Task<bool> OpenAsync(this IBrowser browser, string uri, BrowserLaunchOptions options) =>
			browser.OpenAsync(new Uri(uri), options);

		public static Task<bool> OpenAsync(this IBrowser browser, Uri uri) =>
			browser.OpenAsync(uri, new BrowserLaunchOptions());

		public static Task<bool> OpenAsync(this IBrowser browser, Uri uri, BrowserLaunchMode launchMode) =>
			browser.OpenAsync(uri, new BrowserLaunchOptions { LaunchMode = launchMode });
	}
}
