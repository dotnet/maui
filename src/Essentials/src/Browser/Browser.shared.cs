#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// Provides a way to display a web page inside an app.
	/// </summary>
	public interface IBrowser
	{
		/// <summary>
		/// Open the browser to specified URI.
		/// </summary>
		/// <param name="uri">URI to open.</param>
		/// <param name="options">Launch options for the browser.</param>
		/// <returns>Completed task when browser is launched, but not necessarily closed. Result indicates if launching was successful or not.</returns>
		Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options);
	}

	/// <summary>
	/// Provides a way to display a web page inside an app.
	/// </summary>
	public static class Browser
	{
		/// <summary>
		/// Open the browser to specified URI.
		/// </summary>
		/// <param name="uri">URI to open.</param>
		/// <returns>Completed task when browser is launched, but not necessarily closed. Result indicates if launching was successful or not.</returns>
		public static Task<bool> OpenAsync(string uri) => Default.OpenAsync(uri);

		/// <summary>
		/// Open the browser to specified URI.
		/// </summary>
		/// <param name="uri">URI to open.</param>
		/// <param name="launchMode">How to launch the browser.</param>
		/// <returns>Completed task when browser is launched, but not necessarily closed. Result indicates if launching was successful or not.</returns>
		public static Task<bool> OpenAsync(string uri, BrowserLaunchMode launchMode) => Default.OpenAsync(uri, launchMode);

		/// <summary>
		/// Open the browser to specified URI.
		/// </summary>
		/// <param name="uri">URI to open.</param>
		/// <param name="options">Launch options for the browser.</param>
		/// <returns>Completed task when browser is launched, but not necessarily closed. Result indicates if launching was successful or not.</returns>
		public static Task<bool> OpenAsync(string uri, BrowserLaunchOptions options) => Default.OpenAsync(uri, options);

		/// <summary>
		/// Open the browser to specified URI.
		/// </summary>
		/// <param name="uri">URI to open.</param>
		/// <returns>Completed task when browser is launched, but not necessarily closed. Result indicates if launching was successful or not.</returns>
		public static Task<bool> OpenAsync(Uri uri) => Default.OpenAsync(uri);

		/// <summary>
		/// Open the browser to specified URI.
		/// </summary>
		/// <param name="uri">URI to open.</param>
		/// <param name="launchMode">How to launch the browser.</param>
		/// <returns>Completed task when browser is launched, but not necessarily closed. Result indicates if launching was successful or not.</returns>
		public static Task<bool> OpenAsync(Uri uri, BrowserLaunchMode launchMode) => Default.OpenAsync(uri, launchMode);

		/// <summary>
		/// Open the browser to specified URI.
		/// </summary>
		/// <param name="uri">URI to open.</param>
		/// <param name="options">Launch options for the browser.</param>
		/// <returns>Completed task when browser is launched, but not necessarily closed.  Result indicates if launching was successful or not.</returns>
		public static Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) => Default.OpenAsync(uri, options);

		static IBrowser? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IBrowser Default =>
			defaultImplementation ??= new BrowserImplementation();

		internal static void SetDefault(IBrowser? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// This class contains static extension methods for use with <see cref="IBrowser"/>.
	/// </summary>
	public static class BrowserExtensions
	{
		/// <summary>
		/// Open the browser to specified URI.
		/// </summary>
		/// <param name="browser">The <see cref="IBrowser"/> instance to invoke this method on.</param>
		/// <param name="uri">URI to open.</param>
		/// <returns>Completed task when browser is launched, but not necessarily closed. Result indicates if launching was successful or not.</returns>
		public static Task<bool> OpenAsync(this IBrowser browser, string uri) =>
			browser.OpenAsync(new Uri(uri), new BrowserLaunchOptions());

		/// <summary>
		/// Open the browser to specified URI.
		/// </summary>
		/// <param name="browser">The <see cref="IBrowser"/> instance to invoke this method on.</param>
		/// <param name="uri">URI to open.</param>
		/// <param name="launchMode">How to launch the browser.</param>
		/// <returns>Completed task when browser is launched, but not necessarily closed. Result indicates if launching was successful or not.</returns>
		public static Task<bool> OpenAsync(this IBrowser browser, string uri, BrowserLaunchMode launchMode) =>
			browser.OpenAsync(new Uri(uri), new BrowserLaunchOptions { LaunchMode = launchMode });

		/// <summary>
		/// Open the browser to specified URI.
		/// </summary>
		/// <param name="browser">The <see cref="IBrowser"/> instance to invoke this method on.</param>
		/// <param name="uri">URI to open.</param>
		/// <param name="options">Launch options for the browser.</param>
		/// <returns>Completed task when browser is launched, but not necessarily closed.  Result indicates if launching was successful or not.</returns>
		public static Task<bool> OpenAsync(this IBrowser browser, string uri, BrowserLaunchOptions options) =>
			browser.OpenAsync(new Uri(uri), options);

		/// <summary>
		/// Open the browser to specified URI.
		/// </summary>
		/// <param name="browser">The <see cref="IBrowser"/> instance to invoke this method on.</param>
		/// <param name="uri">URI to open.</param>
		/// <returns>Completed task when browser is launched, but not necessarily closed. Result indicates if launching was successful or not.</returns>
		public static Task<bool> OpenAsync(this IBrowser browser, Uri uri) =>
			browser.OpenAsync(uri, new BrowserLaunchOptions());

		/// <summary>
		/// Open the browser to specified URI.
		/// </summary>
		/// <param name="browser">The <see cref="IBrowser"/> instance to invoke this method on.</param>
		/// <param name="uri">URI to open.</param>
		/// <param name="launchMode">How to launch the browser.</param>
		/// <returns>Completed task when browser is launched, but not necessarily closed. Result indicates if launching was successful or not.</returns>
		public static Task<bool> OpenAsync(this IBrowser browser, Uri uri, BrowserLaunchMode launchMode) =>
			browser.OpenAsync(uri, new BrowserLaunchOptions { LaunchMode = launchMode });
	}
}
