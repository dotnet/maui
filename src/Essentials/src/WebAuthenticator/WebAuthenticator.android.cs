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
		TaskCompletionSource<WebAuthenticatorResult> tcsResponse = null;
		Uri currentRedirectUri = null;
		WebAuthenticatorOptions currentOptions = null;

		internal bool AuthenticatingWithCustomTabs { get; private set; } = false;

		public bool OnResumeCallback(Intent intent)
		{
			// If we aren't waiting on a task, don't handle the url
			if (tcsResponse?.Task?.IsCompleted ?? true)
				return false;

			if (intent?.Data == null)
			{
				tcsResponse.TrySetCanceled();
				return false;
			}

			try
			{
				var intentUri = new Uri(intent.Data.ToString());

				// Only handle schemes we expect
				if (!WebUtils.CanHandleCallback(currentRedirectUri, intentUri))
				{
					tcsResponse.TrySetException(new InvalidOperationException($"Invalid Redirect URI, detected `{intentUri}` but expected a URI in the format of `{currentRedirectUri}`"));
					return false;
				}
				tcsResponse?.TrySetResult(new WebAuthenticatorResult(intentUri, currentOptions?.ResponseDecoder));
				return true;
			}
			catch (Exception ex)
			{
				tcsResponse.TrySetException(ex);
				return false;
			}
		}

		public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
			=> await AuthenticateAsync(webAuthenticatorOptions, CancellationToken.None);

		public async Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions, CancellationToken cancellationToken)
		{
			currentOptions = webAuthenticatorOptions;
			var url = webAuthenticatorOptions?.Url;
			var callbackUrl = webAuthenticatorOptions?.CallbackUrl;
			var packageName = Application.Context.PackageName;

			// Create an intent to see if the app developer wired up the callback activity correctly
			var intent = new Intent(Intent.ActionView);
			intent.AddCategory(Intent.CategoryBrowsable);
			intent.AddCategory(Intent.CategoryDefault);
			intent.SetPackage(packageName);
			intent.SetData(global::Android.Net.Uri.Parse(callbackUrl.OriginalString));

			// Try to find the activity for the callback intent
			if (!PlatformUtils.IsIntentSupported(intent, packageName))
				throw new InvalidOperationException($"You must subclass the `{nameof(WebAuthenticatorCallbackActivity)}` and create an IntentFilter for it which matches your `{nameof(callbackUrl)}`.");

			// Cancel any previous task that's still pending
			if (tcsResponse?.Task != null && !tcsResponse.Task.IsCompleted)
				tcsResponse.TrySetCanceled();

			tcsResponse = new TaskCompletionSource<WebAuthenticatorResult>();
			currentRedirectUri = callbackUrl;

			// Use the CancellationToken to cancel the authentication if requested
			using (cancellationToken.Register(() => tcsResponse.TrySetCanceled()))
			{
				// Try to start with custom tabs if the system supports it and we resolve it
				AuthenticatingWithCustomTabs = await StartCustomTabsActivity(url);

				// Fall back to using the system browser if necessary
				if (!AuthenticatingWithCustomTabs)
				{
					// Fall back to opening the system-registered browser if necessary
					var urlOriginalString = url.OriginalString;
					var browserIntent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(urlOriginalString));
					Platform.CurrentActivity.StartActivity(browserIntent);
				}

				return await tcsResponse.Task;
			}
		}


		static async Task<bool> StartCustomTabsActivity(Uri url)
		{
			// Is only set to true if BindServiceAsync succeeds and no exceptions are thrown
			var success = false;
			var parentActivity = ActivityStateManager.Default.GetCurrentActivity(true);

			var customTabsActivityManager = new CustomTabsActivityManager(parentActivity);
			try
			{
				if (await BindServiceAsync(customTabsActivityManager))
				{
					var customTabsIntent = new CustomTabsIntent.Builder(customTabsActivityManager.Session)
						.SetShowTitle(true)
						.Build();

					customTabsIntent.Intent.SetData(global::Android.Net.Uri.Parse(url.OriginalString));

					if (customTabsIntent.Intent.ResolveActivity(parentActivity.PackageManager) != null)
					{
						WebAuthenticatorIntermediateActivity.StartActivity(parentActivity, customTabsIntent.Intent);
						success = true;
					}
				}
			}
			finally
			{
				try
				{
					customTabsActivityManager.Client?.Dispose();
				}
				finally
				{
				}
			}

			return success;
		}

		static Task<bool> BindServiceAsync(CustomTabsActivityManager manager)
		{
			var tcs = new TaskCompletionSource<bool>();

			manager.CustomTabsServiceConnected += OnCustomTabsServiceConnected;

			if (!manager.BindService())
			{
				manager.CustomTabsServiceConnected -= OnCustomTabsServiceConnected;
				tcs.TrySetResult(false);
			}

			return tcs.Task;

			void OnCustomTabsServiceConnected(ComponentName name, CustomTabsClient client)
			{
				manager.CustomTabsServiceConnected -= OnCustomTabsServiceConnected;
				tcs.TrySetResult(true);
			}
		}
	}
}
