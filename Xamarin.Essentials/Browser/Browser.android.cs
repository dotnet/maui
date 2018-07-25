using System;
using System.Threading.Tasks;

using Android.Content;
using Android.Support.CustomTabs;

using AndroidUri = Android.Net.Uri;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        static Task PlatformOpenAsync(Uri uri, BrowserLaunchType launchType)
        {
            var nativeUri = AndroidUri.Parse(uri.AbsoluteUri);

            switch (launchType)
            {
                case BrowserLaunchType.SystemPreferred:
                    var tabsBuilder = new CustomTabsIntent.Builder();
                    tabsBuilder.SetShowTitle(true);
                    var tabsIntent = tabsBuilder.Build();
                    tabsIntent.Intent.SetFlags(ActivityFlags.ClearTop);
                    tabsIntent.Intent.SetFlags(ActivityFlags.NewTask);
                    tabsIntent.LaunchUrl(Platform.AppContext, nativeUri);
                    break;
                case BrowserLaunchType.External:
                    var intent = new Intent(Intent.ActionView, nativeUri);
                    intent.SetFlags(ActivityFlags.ClearTop);
                    intent.SetFlags(ActivityFlags.NewTask);

                    if (!Platform.IsIntentSupported(intent))
                        throw new FeatureNotSupportedException();

                    Platform.AppContext.StartActivity(intent);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
