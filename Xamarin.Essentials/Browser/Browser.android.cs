using System;
using System.Threading.Tasks;

using Android.Content;
using Android.Support.CustomTabs;

using AndroidUri = Android.Net.Uri;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        public static Task OpenAsync(Uri uri, BrowserLaunchType launchType)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var nativeUri = AndroidUri.Parse(uri.OriginalString);

            switch (launchType)
            {
                case BrowserLaunchType.SystemPreferred:
                    var tabsBuilder = new CustomTabsIntent.Builder();
                    var tabsIntent = tabsBuilder.Build();
                    tabsBuilder.SetShowTitle(true);
                    tabsIntent.LaunchUrl(Platform.CurrentContext, nativeUri);
                    break;
                case BrowserLaunchType.External:
                    var intent = new Intent(Intent.ActionView, nativeUri);
                    intent.SetFlags(ActivityFlags.ClearTop);
                    intent.SetFlags(ActivityFlags.NewTask);

                    if (!Platform.IsIntentSupported(intent))
                        throw new FeatureNotSupportedException();

                    Platform.CurrentContext.StartActivity(intent);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
