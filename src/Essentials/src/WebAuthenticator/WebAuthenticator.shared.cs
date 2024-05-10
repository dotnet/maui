#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication
{
	/// <summary>
	/// A web navigation API intended to be used for authentication with external web services such as OAuth.
	/// </summary>
	public interface IWebAuthenticator
	{
		/// <summary>
		/// Begin an authentication flow by navigating to the specified URL and waiting for a callback/redirect to the callback URL scheme.
		/// </summary>
		/// <param name="webAuthenticatorOptions">A <see cref="WebAuthenticatorOptions"/> instance containing additional configuration for this authentication call.</param>
		/// <returns>A <see cref="WebAuthenticatorResult"/> object with the results of this operation.</returns>
		/// <exception cref="TaskCanceledException">Thrown when the user canceled the authentication flow.</exception>
		/// <exception cref="HttpRequestException">Windows: Thrown when a HTTP Request error occured.</exception>
		/// <exception cref="Exception">Windows: Thrown when a unexpected HTTP response was received.</exception>
		/// <exception cref="FeatureNotSupportedException">iOS/macOS: Thrown when iOS version is less than 13 is used or macOS less than 13.1 is used.</exception>
		/// <exception cref="InvalidOperationException">
		/// <para>Windows: Thrown when the callback custom URL scheme is not registered in the AppxManifest.xml file.</para>
		/// <para>Android: Thrown when the no IntentFilter has been created for the callback URL.</para>
		/// </exception>
		Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions);
	}

	/// <summary>
	/// Provides abstractions for the platform web authenticator callbacks triggered when using <see cref="WebAuthenticator"/>.
	/// </summary>
	public interface IPlatformWebAuthenticatorCallback
	{
#if IOS || MACCATALYST || MACOS
		/// <summary>
		/// Opens the specified URI to start the authentication flow.
		/// </summary>
		/// <param name="uri">The URI to open that will start the authentication flow.</param>
		/// <returns><see langword="true"/> when the URI has been opened, otherwise <see langword="false"/>.</returns>
		bool OpenUrlCallback(Uri uri);
#elif ANDROID
		/// <summary>
		/// The event that is triggered when an authentication flow calls back into the Android application.
		/// </summary>
		/// <param name="intent">An <see cref="Android.Content.Intent"/> object containing additional data about this resume operation.</param>
		/// <returns><see langword="true"/> when the callback can be processed, otherwise <see langword="false"/>.</returns>
		bool OnResumeCallback(Android.Content.Intent intent);
#endif
	}

	/// <summary>
	/// Provides abstractions used for decoding a URI returned from a authentication request, for use with <see cref="IWebAuthenticator"/>.
	/// </summary>
	public interface IWebAuthenticatorResponseDecoder
	{
		/// <summary>
		/// Decodes the given URIs query string into a dictionary.
		/// </summary>
		/// <param name="uri">The <see cref="Uri"/> object to decode the query parameters from.</param>
		/// <returns>A <see cref="IDictionary{TKey, TValue}"/> object where each of the query parameters values of <paramref name="uri"/> are accessible through their respective keys.</returns>
		IDictionary<string, string>? DecodeResponse(Uri uri);
	}

	/// <summary>
	/// A web navigation API intended to be used for Authentication with external web services such as OAuth.
	/// </summary>
	/// <remarks>
	/// This API helps with navigating to a start URL and waiting for a callback URL to the app.  Your app must 
	/// be registered to handle the callback scheme you provide in the call to authenticate.
	/// </remarks>
	public static class WebAuthenticator
	{
		/// <summary>Begin an authentication flow by navigating to the specified url and waiting for a callback/redirect to the callbackUrl scheme.</summary>
		/// <param name="url"> Url to navigate to, beginning the authentication flow.</param>
		/// <param name="callbackUrl"> Expected callback url that the navigation flow will eventually redirect to.</param>
		/// <returns>Returns a result parsed out from the callback url.</returns>		
		public static Task<WebAuthenticatorResult> AuthenticateAsync(Uri url, Uri callbackUrl)
			=> Current.AuthenticateAsync(url, callbackUrl);

		/// <summary>Begin an authentication flow by navigating to the specified url and waiting for a callback/redirect to the callbackUrl scheme.The start url and callbackUrl are specified in the webAuthenticatorOptions.</summary>
		/// <param name="webAuthenticatorOptions">Options to configure the authentication request.</param>
		/// <returns>Returns a result parsed out from the callback url.</returns>
		public static Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
			=> Current.AuthenticateAsync(webAuthenticatorOptions);

		static IWebAuthenticator Current => Authentication.WebAuthenticator.Default;

		static IWebAuthenticator? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IWebAuthenticator Default =>
			defaultImplementation ??= new WebAuthenticatorImplementation();

		internal static void SetDefault(IWebAuthenticator? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// This class contains static extension methods for use with <see cref="WebAuthenticator"/>.
	/// </summary>
	public static class WebAuthenticatorExtensions
	{
		static IPlatformWebAuthenticatorCallback AsPlatformCallback(this IWebAuthenticator webAuthenticator)
		{
			if (webAuthenticator is not IPlatformWebAuthenticatorCallback platform)
				throw new PlatformNotSupportedException("This implementation of IWebAuthenticator does not implement IPlatformWebAuthenticatorCallback.");
			return platform;
		}

#if ANDROID
		internal static bool IsAuthenticatingWithCustomTabs(this IWebAuthenticator webAuthenticator)
			=> (webAuthenticator as WebAuthenticatorImplementation)?.AuthenticatingWithCustomTabs ?? false;
#endif

		/// <summary>
		/// Begin an authentication flow by navigating to the specified url and waiting for a callback/redirect to the callbackUrl scheme.
		/// </summary>
		/// <param name="webAuthenticator">The <see cref="IWebAuthenticator"/> to use for the authentication flow.</param>
		/// <param name="url"> Url to navigate to, beginning the authentication flow.</param>
		/// <param name="callbackUrl"> Expected callback url that the navigation flow will eventually redirect to.</param>
		/// <returns>Returns a result parsed out from the callback url.</returns>
		public static Task<WebAuthenticatorResult> AuthenticateAsync(this IWebAuthenticator webAuthenticator, Uri url, Uri callbackUrl) =>
			webAuthenticator.AuthenticateAsync(new WebAuthenticatorOptions { Url = url, CallbackUrl = callbackUrl });

#if IOS || MACCATALYST || MACOS
		/// <inheritdoc cref="IPlatformWebAuthenticatorCallback.OpenUrlCallback(Uri)"/>
		public static bool OpenUrl(this IWebAuthenticator webAuthenticator, Uri uri) =>
			webAuthenticator.AsPlatformCallback().OpenUrlCallback(uri);

		/// <inheritdoc cref="ApplicationModel.Platform.OpenUrl(UIKit.UIApplication, Foundation.NSUrl, Foundation.NSDictionary)"/>
		public static bool OpenUrl(this IWebAuthenticator webAuthenticator, UIKit.UIApplication app, Foundation.NSUrl url, Foundation.NSDictionary options) 
		{
			if(url?.AbsoluteString != null)
			{
				return webAuthenticator.OpenUrl(new Uri(url.AbsoluteString));
			}
			return false;
		}

		/// <inheritdoc cref="ApplicationModel.Platform.ContinueUserActivity(UIKit.UIApplication, Foundation.NSUserActivity, UIKit.UIApplicationRestorationHandler)"/>
		public static bool ContinueUserActivity(this IWebAuthenticator webAuthenticator, UIKit.UIApplication application, Foundation.NSUserActivity userActivity, UIKit.UIApplicationRestorationHandler completionHandler)
		{
			var uri = userActivity?.WebPageUrl?.AbsoluteString;
			if (string.IsNullOrEmpty(uri))
				return false;

			return webAuthenticator.OpenUrl(new Uri(uri));
		}

#elif ANDROID
		/// <inheritdoc cref="IPlatformWebAuthenticatorCallback.OnResumeCallback(Android.Content.Intent)"/>
		public static bool OnResume(this IWebAuthenticator webAuthenticator, Android.Content.Intent intent) =>
			webAuthenticator.AsPlatformCallback().OnResumeCallback(intent);
#endif
	}

	/// <summary>
	/// Represents additional options for <see cref="WebAuthenticator"/>.
	/// </summary>
	public class WebAuthenticatorOptions
	{
		/// <summary>
		/// Gets or sets the URL that will start the authentication flow.
		/// </summary>
		public Uri? Url { get; set; }

		/// <summary>
		/// Gets or sets the callback URL that should be called when authentication completes.
		/// </summary>
		public Uri? CallbackUrl { get; set; }

		/// <summary>
		/// Gets or sets whether the browser used for the authentication flow is short-lived.
		/// This means it will not share session nor cookies with the regular browser on this device if set the <see langword="true"/>. 
		/// </summary>
		/// <remarks>This setting only has effect on iOS.</remarks>
		public bool PrefersEphemeralWebBrowserSession { get; set; }

		/// <summary>
		/// Gets or sets the decoder implementation used to decode the incoming authentication result URI. 
		/// </summary>
		public IWebAuthenticatorResponseDecoder? ResponseDecoder { get; set; }
	}
}
