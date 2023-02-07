#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Authentication
{
	public interface IWebAuthenticator
	{
		Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions);
	}

	public interface IPlatformWebAuthenticatorCallback
	{
#if IOS || MACCATALYST || MACOS
		bool OpenUrlCallback(Uri uri);
#elif ANDROID
		bool OnResumeCallback(Android.Content.Intent intent);
#endif
	}

	public interface IWebAuthenticatorResponseDecoder
	{
		IDictionary<string, string>? DecodeResponse(Uri uri);
	}

	/// <summary>A web navigation API intended to be used for Authentication with external web services such as OAuth.</summary>
	/// <remarks>
	/// This API helps with navigating to a start URL and waiting for a callback URL to the app.  Your app must 
	/// be registered to handle the callback scheme you provide in the call to authenticate.
	/// </remarks>
	public static class WebAuthenticator
	{
		/// <summary>Begin an authentication flow by navigating to the specified url and waiting for a callback/redirect to the callbackUrl scheme.</summary>
		/// <param name="url" > Url to navigate to, beginning the authentication flow.</param>
		/// <param name="callbackUrl" > Expected callback url that the navigation flow will eventually redirect to.</param>
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

		public static IWebAuthenticator Default =>
			defaultImplementation ??= new WebAuthenticatorImplementation();

		internal static void SetDefault(IWebAuthenticator? implementation) =>
			defaultImplementation = implementation;
	}

	public static class WebAuthenticatorExtensions
	{
		static IPlatformWebAuthenticatorCallback AsPlatformCallback(this IWebAuthenticator webAuthenticator)
		{
			if (webAuthenticator is not IPlatformWebAuthenticatorCallback platform)
				throw new PlatformNotSupportedException("This implementation of IWebAuthenticator does not implement IPlatformWebAuthenticatorCallback.");
			return platform;
		}

		/// <summary>Begin an authentication flow by navigating to the specified url and waiting for a callback/redirect to the callbackUrl scheme.</summary>
		/// <param name="webAuthenticator">The <see cref="IWebAuthenticator"/> to use for the authentication flow.</param>
		/// <param name="url"> Url to navigate to, beginning the authentication flow.</param>
		/// <param name="callbackUrl"> Expected callback url that the navigation flow will eventually redirect to.</param>
		/// <returns>Returns a result parsed out from the callback url.</returns>
		public static Task<WebAuthenticatorResult> AuthenticateAsync(this IWebAuthenticator webAuthenticator, Uri url, Uri callbackUrl) =>
			webAuthenticator.AuthenticateAsync(new WebAuthenticatorOptions { Url = url, CallbackUrl = callbackUrl });

#if IOS || MACCATALYST || MACOS

		public static bool OpenUrl(this IWebAuthenticator webAuthenticator, Uri uri) =>
			webAuthenticator.AsPlatformCallback().OpenUrlCallback(uri);

		public static bool OpenUrl(this IWebAuthenticator webAuthenticator, UIKit.UIApplication app, Foundation.NSUrl url, Foundation.NSDictionary options) 
		{
			if(url?.AbsoluteString != null)
			{
				return webAuthenticator.OpenUrl(new Uri(url.AbsoluteString));
			}
			return false;
		}

		public static bool ContinueUserActivity(this IWebAuthenticator webAuthenticator, UIKit.UIApplication application, Foundation.NSUserActivity userActivity, UIKit.UIApplicationRestorationHandler completionHandler)
		{
			var uri = userActivity?.WebPageUrl?.AbsoluteString;
			if (string.IsNullOrEmpty(uri))
				return false;

			return webAuthenticator.OpenUrl(new Uri(uri));
		}

#elif ANDROID

		public static bool OnResume(this IWebAuthenticator webAuthenticator, Android.Content.Intent intent) =>
			webAuthenticator.AsPlatformCallback().OnResumeCallback(intent);

#endif
	}

	public class WebAuthenticatorOptions
	{
		public Uri? Url { get; set; }

		public Uri? CallbackUrl { get; set; }

		public bool PrefersEphemeralWebBrowserSession { get; set; }

		public IWebAuthenticatorResponseDecoder? ResponseDecoder { get; set; }
	}
}
