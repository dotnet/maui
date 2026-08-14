#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.Browser.CustomTabs;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication
{
	partial class WebAuthenticatorImplementation : IWebAuthenticator, IPlatformWebAuthenticatorCallback
	{
		public bool OnResumeCallback(Intent intent) =>
			TryHandleBuiltInCallback(intent);

		internal static bool TryHandleBuiltInCallback(Intent intent)
		{
			var callback = intent?.Data?.ToString();
			if (string.IsNullOrEmpty(callback))
				return false;

			try
			{
				return WebAuthenticatorRequestManager.TryHandleCallback(new Uri(callback));
			}
			catch (Exception)
			{
				return false;
			}
		}

		public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions) =>
			AuthenticateAsync(webAuthenticatorOptions, CancellationToken.None);

		public async Task<WebAuthenticatorResult> AuthenticateAsync(
			WebAuthenticatorOptions webAuthenticatorOptions,
			CancellationToken cancellationToken)
		{
			if (webAuthenticatorOptions is null)
				throw new ArgumentNullException(nameof(webAuthenticatorOptions));

			var url = webAuthenticatorOptions.Url;
			var callbackUrl = webAuthenticatorOptions.CallbackUrl;
			var responseDecoder = webAuthenticatorOptions.ResponseDecoder;
			var prefersEphemeral = webAuthenticatorOptions.PrefersEphemeralWebBrowserSession;

			if (url is null)
				throw new ArgumentNullException(nameof(webAuthenticatorOptions.Url));
			if (callbackUrl is null)
				throw new ArgumentNullException(nameof(webAuthenticatorOptions.CallbackUrl));
			if (!url.IsAbsoluteUri)
				throw new ArgumentException("The authentication URL must be absolute.", nameof(webAuthenticatorOptions.Url));
			if (!callbackUrl.IsAbsoluteUri)
				throw new ArgumentException("The callback URL must be absolute.", nameof(webAuthenticatorOptions.CallbackUrl));
			var applicationContext = Application.Context;
			var callbackActivityAvailable = IsCallbackActivityAvailable(applicationContext, callbackUrl);
			var customTabsProvider = TryGetCustomTabsProvider(applicationContext);
			var useAuthTab = CanUseAuthTab(url, callbackUrl) &&
				IsAuthTabSupported(applicationContext, customTabsProvider);

			if (!useAuthTab && !callbackActivityAvailable)
			{
				throw new InvalidOperationException(
					$"You must subclass {nameof(WebAuthenticatorCallbackActivity)} and register an IntentFilter for the callback route.");
			}

			cancellationToken.ThrowIfCancellationRequested();

			var parentActivity = ActivityStateManager.Default.GetCurrentActivity(true) ??
				throw new InvalidOperationException("The current Android Activity is unavailable.");
			var request = WebAuthenticatorRequestManager.Begin(
				url,
				callbackUrl,
				responseDecoder,
				prefersEphemeral,
				cancellationToken);

			var registration = cancellationToken.Register(
				static state =>
				{
					var request = (WebAuthenticatorRequest)state!;
					WebAuthenticatorRequestManager.TryCancelFromCaller(request);
				},
				request);

			var usesIntermediateActivity = false;
			var callerCancellationWon = false;
			try
			{
				if (!request.Task.IsCompleted)
				{
					try
					{
						await MainThread.InvokeOnMainThreadAsync(() =>
						{
							if (request.Task.IsCompleted)
								return;

							if (useAuthTab)
							{
								WebAuthenticatorIntermediateActivity.StartAuthTab(
									parentActivity,
									request.Id,
									url,
									callbackUrl,
									customTabsProvider!,
									prefersEphemeral,
									callbackActivityAvailable);
								usesIntermediateActivity = true;
							}
							else if (!string.IsNullOrEmpty(customTabsProvider))
							{
								WebAuthenticatorIntermediateActivity.StartCustomTab(
									parentActivity,
									request.Id,
									url,
									customTabsProvider,
									prefersEphemeral);
								usesIntermediateActivity = true;
							}
							else
							{
								LaunchSystemBrowser(parentActivity, url);
							}
						});
					}
					catch (Exception)
					{
						WebAuthenticatorRequestManager.TryFail(
							request,
							new InvalidOperationException("Unable to open a browser for web authentication."));
					}
				}

				return await request.Task.ConfigureAwait(false);
			}
			catch (OperationCanceledException ex) when (
				ex.CancellationToken == cancellationToken && cancellationToken.IsCancellationRequested)
			{
				callerCancellationWon = true;
				throw;
			}
			finally
			{
				if (usesIntermediateActivity && callerCancellationWon)
				{
					try
					{
						await MainThread.InvokeOnMainThreadAsync(() =>
							WebAuthenticatorIntermediateActivity.RequestCleanup(applicationContext, request.Id));
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"Unable to close the Android WebAuthenticator activity ({ex}).");
					}
				}

				registration.Dispose();
				WebAuthenticatorRequestManager.End(request);
			}
		}

		internal static bool CanUseAuthTab(Uri url, Uri callbackUrl) =>
			url.Scheme is "http" or "https" &&
			callbackUrl.Scheme != "http" &&
			(callbackUrl.Scheme != "https" || callbackUrl.IsDefaultPort);

		internal static string? TryGetCustomTabsProvider(Context context)
		{
			try
			{
				return CustomTabsHelper.GetPackageNameToUse(context);
			}
			catch (Exception)
			{
				return null;
			}
		}

		internal static bool IsAuthTabSupported(Context context, string? provider)
		{
			if (string.IsNullOrEmpty(provider))
				return false;

			try
			{
				return CustomTabsClient.IsAuthTabSupported(context, provider);
			}
			catch (Exception)
			{
				return false;
			}
		}

		internal static bool IsEphemeralBrowsingSupported(Context context, string? provider)
		{
			if (string.IsNullOrEmpty(provider))
				return false;

			try
			{
				return CustomTabsClient.IsEphemeralBrowsingSupported(context, provider);
			}
			catch (Exception)
			{
				return false;
			}
		}

		static bool IsCallbackActivityAvailable(Context context, Uri callbackUrl)
		{
			var packageName = context.PackageName;
			if (string.IsNullOrEmpty(packageName))
				return false;

			using var intent = new Intent(Intent.ActionView);
			intent.AddCategory(Intent.CategoryBrowsable);
			intent.AddCategory(Intent.CategoryDefault);
			intent.SetPackage(packageName);
			intent.SetData(global::Android.Net.Uri.Parse(callbackUrl.OriginalString));

			return PlatformUtils.IsIntentSupported(intent, packageName);
		}

		internal static void LaunchSystemBrowser(Context context, Uri url)
		{
			using var browserIntent = new Intent(
				Intent.ActionView,
				global::Android.Net.Uri.Parse(url.OriginalString));
			if (context is not Activity)
				browserIntent.AddFlags(ActivityFlags.NewTask);

			context.StartActivity(browserIntent);
		}
	}
}
