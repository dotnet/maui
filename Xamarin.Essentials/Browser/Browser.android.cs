using System;
using System.Threading.Tasks;
using Android.Content;
#if __ANDROID_29__
using AndroidX.Browser.CustomTabs;
#else
using Android.Support.CustomTabs;
#endif
using AndroidUri = Android.Net.Uri;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        static Task<bool> PlatformOpenAsync(Uri uri, BrowserLaunchOptions options)
        {
            var nativeUri = AndroidUri.Parse(uri.AbsoluteUri);

            switch (options.LaunchMode)
            {
                case BrowserLaunchMode.SystemPreferred:
                    var tabsBuilder = new CustomTabsIntent.Builder();
                    tabsBuilder.SetShowTitle(true);
                    if (options.PreferredToolbarColor.HasValue)
                        tabsBuilder.SetToolbarColor(options.PreferredToolbarColor.Value.ToInt());
                    if (options.TitleMode != BrowserTitleMode.Default)
                        tabsBuilder.SetShowTitle(options.TitleMode == BrowserTitleMode.Show);

                    var tabsIntent = tabsBuilder.Build();
                    tabsIntent.Intent.SetFlags(ActivityFlags.ClearTop);
                    tabsIntent.Intent.SetFlags(ActivityFlags.NewTask);
#if __ANDROID_25__
                    tabsIntent.LaunchUrl(Platform.AppContext, nativeUri);
#else
                    tabsIntent.LaunchUrl(Platform.GetCurrentActivity(true), nativeUri);
#endif
                    break;
                case BrowserLaunchMode.External:
                    var intent = new Intent(Intent.ActionView, nativeUri);
                    intent.SetFlags(ActivityFlags.ClearTop);
                    intent.SetFlags(ActivityFlags.NewTask);

                    if (!Platform.IsIntentSupported(intent))
                        throw new FeatureNotSupportedException();

                    Platform.AppContext.StartActivity(intent);
                    break;
            }

            return Task.FromResult(true);
        }
    }
}
