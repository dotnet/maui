#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	public interface IBrowser
	{
		Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options);
	}

	public static partial class Browser
	{
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
