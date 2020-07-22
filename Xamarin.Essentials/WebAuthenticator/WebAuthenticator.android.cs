using System;
using System.Threading.Tasks;
using Android.Content;
#if __ANDROID_29__
using AndroidX.Browser.CustomTabs;
#else
using Android.Support.CustomTabs;
#endif

namespace Xamarin.Essentials
{
    public partial class WebAuthenticator
    {
        static TaskCompletionSource<WebAuthenticatorResult> tcsResponse = null;

        static Uri uri = null;

        static CustomTabsActivityManager CustomTabsActivityManager { get; set; }

        static Uri RedirectUri { get; set; }

        internal static Task<WebAuthenticatorResult> ResponseTask
            => tcsResponse?.Task;

        internal static bool OnResume(Intent intent)
        {
            // If we aren't waiting on a task, don't handle the url
            if (tcsResponse?.Task?.IsCompleted ?? true)
                return false;

            if (intent == null)
            {
                tcsResponse.TrySetCanceled();
                return false;
            }

            try
            {
                var intentUri = new Uri(intent.Data.ToString());

                // Only handle schemes we expect
                if (!WebUtils.CanHandleCallback(RedirectUri, intentUri))
                {
                    tcsResponse.TrySetException(new InvalidOperationException($"Invalid Redirect URI, detected `{intentUri}` but expected a URI in the format of `{RedirectUri}`"));
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

        static Task<WebAuthenticatorResult> PlatformAuthenticateAsync(Uri url, Uri callbackUrl)
        {
            var packageName = Platform.AppContext.PackageName;

            // Create an intent to see if the app developer wired up the callback activity correctly
            var intent = new Intent(Intent.ActionView);
            intent.AddCategory(Intent.CategoryBrowsable);
            intent.AddCategory(Intent.CategoryDefault);
            intent.SetPackage(packageName);
            intent.SetData(global::Android.Net.Uri.Parse(callbackUrl.OriginalString));

            // Try to find the activity for the callback intent
            var c = intent.ResolveActivity(Platform.AppContext.PackageManager);

            if (c == null || c.PackageName != packageName)
                throw new InvalidOperationException($"You must subclass the `{nameof(WebAuthenticatorCallbackActivity)}` and create an IntentFilter for it which matches your `{nameof(callbackUrl)}`.");

            // Cancel any previous task that's still pending
            if (tcsResponse?.Task != null && !tcsResponse.Task.IsCompleted)
                tcsResponse.TrySetCanceled();

            tcsResponse = new TaskCompletionSource<WebAuthenticatorResult>();
            tcsResponse.Task.ContinueWith(t =>
            {
                // Cleanup when done
                if (CustomTabsActivityManager != null)
                {
                    CustomTabsActivityManager.NavigationEvent -= CustomTabsActivityManager_NavigationEvent;
                    CustomTabsActivityManager.CustomTabsServiceConnected -= CustomTabsActivityManager_CustomTabsServiceConnected;

                    try
                    {
                        CustomTabsActivityManager?.Client?.Dispose();
                    }
                    finally
                    {
                        CustomTabsActivityManager = null;
                    }
                }
            });

            uri = url;
            RedirectUri = callbackUrl;

            CustomTabsActivityManager = CustomTabsActivityManager.From(Platform.GetCurrentActivity(true));
            CustomTabsActivityManager.NavigationEvent += CustomTabsActivityManager_NavigationEvent;
            CustomTabsActivityManager.CustomTabsServiceConnected += CustomTabsActivityManager_CustomTabsServiceConnected;

            if (!CustomTabsActivityManager.BindService())
            {
                // Fall back to opening the system browser if necessary
                var browserIntent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(url.OriginalString));
                Platform.CurrentActivity.StartActivity(browserIntent);
            }

            return WebAuthenticator.ResponseTask;
        }

        static void CustomTabsActivityManager_CustomTabsServiceConnected(ComponentName name, CustomTabsClient client)
        {
            var builder = new CustomTabsIntent.Builder(CustomTabsActivityManager.Session)
                                                  .SetShowTitle(true);

            var customTabsIntent = builder.Build();
            customTabsIntent.Intent.AddFlags(ActivityFlags.SingleTop | ActivityFlags.NoHistory | ActivityFlags.NewTask);

            var ctx = Platform.CurrentActivity;

            CustomTabsHelper.AddKeepAliveExtra(ctx, customTabsIntent.Intent);

            customTabsIntent.LaunchUrl(ctx, global::Android.Net.Uri.Parse(uri.OriginalString));
        }

        static void CustomTabsActivityManager_NavigationEvent(int navigationEvent, global::Android.OS.Bundle extras) =>
            System.Diagnostics.Debug.WriteLine($"CustomTabs.NavigationEvent: {navigationEvent}");
    }
}
