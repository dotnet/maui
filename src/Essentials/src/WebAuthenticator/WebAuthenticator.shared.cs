#nullable enable
using System;
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

	/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.WebAuthenticator']/Docs/*" />
	public static class WebAuthenticator
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticator.xml" path="//Member[@MemberName='AuthenticateAsync'][1]/Docs/*" />
		public static Task<WebAuthenticatorResult> AuthenticateAsync(Uri url, Uri callbackUrl)
			=> Current.AuthenticateAsync(url, callbackUrl);

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
	}
}
