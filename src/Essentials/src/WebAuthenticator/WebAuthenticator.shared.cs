using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IWebAuthenticator
	{
		Task<WebAuthenticatorResult> AuthenticateAsync(Uri url, Uri callbackUrl);

		Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions);

#if IOS || MACCATALYST
		bool OpenUrlCallback(Uri uri);
#elif MACOS
		bool OpenUrlCallback(global::Foundation.NSUrl uri);
#elif ANDROID
		bool OnResumeCallback(global::Android.Content.Intent intent);
#endif
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.WebAuthenticator']/Docs" />
	public static partial class WebAuthenticator
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticator.xml" path="//Member[@MemberName='AuthenticateAsync']/Docs" />
		public static Task<WebAuthenticatorResult> AuthenticateAsync(Uri url, Uri callbackUrl)
			=> Current.AuthenticateAsync(new WebAuthenticatorOptions { Url = url, CallbackUrl = callbackUrl });

		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticator.xml" path="//Member[@MemberName='AuthenticateAsync']/Docs" />
		public static Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
			=> Current.AuthenticateAsync(webAuthenticatorOptions);

#if IOS || MACCATALYST
		internal static bool OpenUrl(Uri uri)
			=> Current.OpenUrlCallback(uri);
#elif MACOS
		internal static bool OpenUrl(global::Foundation.NSUrl uri)
			=> Current.OpenUrlCallback(uri);
#elif ANDROID
		internal static bool OnResume(global::Android.Content.Intent intent)
			=> Current.OnResumeCallback(intent);
#endif

#nullable enable
		static IWebAuthenticator? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IWebAuthenticator Current =>
			currentImplementation ??= new WebAuthenticatorImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IWebAuthenticator? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}

	public class WebAuthenticatorOptions
	{
		public Uri Url { get; set; }

		public Uri CallbackUrl { get; set; }

		public bool PrefersEphemeralWebBrowserSession { get; set; }
	}
}
