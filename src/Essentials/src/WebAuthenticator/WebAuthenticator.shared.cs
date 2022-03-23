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

	public static class WebAuthenticator
	{
		static IWebAuthenticator? defaultImplementation;

		public static IWebAuthenticator Default =>
			defaultImplementation ??= new WebAuthenticatorImplementation();

		internal static void SetDefault(IWebAuthenticator? implementation) =>
			defaultImplementation = implementation;
	}

	public static class WebAuthenticatorExtensions
	{
		public static Task<WebAuthenticatorResult> AuthenticateAsync(this IWebAuthenticator webAuthenticator, Uri url, Uri callbackUrl) =>
			webAuthenticator.AuthenticateAsync(new WebAuthenticatorOptions { Url = url, CallbackUrl = callbackUrl });

#if IOS || MACCATALYST || MACOS
		public static bool OpenUrl(this IWebAuthenticator webAuthenticator, Uri uri)
		{
			if (webAuthenticator is not IPlatformWebAuthenticatorCallback platform)
				throw new PlatformNotSupportedException("This implementation of IWebAuthenticator does not implement IPlatformWebAuthenticatorCallback.");

			return platform.OpenUrlCallback(uri);
		}
#elif ANDROID
		public static bool OnResume(this IWebAuthenticator webAuthenticator, Android.Content.Intent intent)
		{
			if (webAuthenticator is not IPlatformWebAuthenticatorCallback platform)
				throw new PlatformNotSupportedException("This implementation of IWebAuthenticator does not implement IPlatformWebAuthenticatorCallback.");

			return platform.OnResumeCallback(intent);
		}
#endif
	}

	public class WebAuthenticatorOptions
	{
		public Uri? Url { get; set; }

		public Uri? CallbackUrl { get; set; }

		public bool PrefersEphemeralWebBrowserSession { get; set; }
	}
}
