using System;
using System.Threading.Tasks;
using Android.Content;
#if __ANDROID_29__
using AndroidX.Browser.CustomTabs;
#else
using Android.Support.CustomTabs;
#endif

namespace Microsoft.Maui.Essentials
{
	public partial class WebAuthenticator
	{
		static TaskCompletionSource<WebAuthenticatorResult> tcsResponse = null;
		static Uri currentRedirectUri = null;

		internal static bool OnResume(Intent intent)
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

				tcsResponse?.TrySetResult(new WebAuthenticatorResult(intentUri));
				return true;
			}
			catch (Exception ex)
			{
				tcsResponse.TrySetException(ex);
				return false;
			}
		}

		static async Task<WebAuthenticatorResult> PlatformAuthenticateAsync(Uri url, Uri callbackUrl)
		{
			var packageName = Platform.AppContext.PackageName;

			// Create an intent to see if the app developer wired up the callback activity correctly
			var intent = new Intent(Intent.ActionView);
			intent.AddCategory(Intent.CategoryBrowsable);
			intent.AddCategory(Intent.CategoryDefault);
			intent.SetPackage(packageName);
			intent.SetData(global::Android.Net.Uri.Parse(callbackUrl.OriginalString));

			// Try to find the activity for the callback intent
			if (!Platform.IsIntentSupported(intent, packageName))
				throw new InvalidOperationException($"You must subclass the `{nameof(WebAuthenticatorCallbackActivity)}` and create an IntentFilter for it which matches your `{nameof(callbackUrl)}`.");

			// Cancel any previous task that's still pending
			if (tcsResponse?.Task != null && !tcsResponse.Task.IsCompleted)
				tcsResponse.TrySetCanceled();

			tcsResponse = new TaskCompletionSource<WebAuthenticatorResult>();
			currentRedirectUri = callbackUrl;

			var parentActivity = Platform.GetCurrentActivity(true);

			var customTabsActivityManager = CustomTabsActivityManager.From(parentActivity);
			try
			{
				if (await BindServiceAsync(customTabsActivityManager))
				{
					var customTabsIntent = new CustomTabsIntent.Builder(customTabsActivityManager.Session)
						.SetShowTitle(true)
						.Build();

					customTabsIntent.Intent.SetData(global::Android.Net.Uri.Parse(url.OriginalString));

					WebAuthenticatorIntermediateActivity.StartActivity(parentActivity, customTabsIntent.Intent);
				}
				else
				{
					// Fall back to opening the system browser if necessary
					var browserIntent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(url.OriginalString));
					Platform.CurrentActivity.StartActivity(browserIntent);
				}

				return await tcsResponse.Task;
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
