using System;
using System.Threading.Tasks;

using AndroidUri = Android.Net.Uri;

namespace Microsoft.Caboodle
{
    public static partial class Browser
    {
        public static Task OpenAsync(Uri uri, BrowserLaunchType launchType)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri), "Uri cannot be null.");
            }

            var nativeUri = AndroidUri.Parse(uri.OriginalString);

            switch (launchType)
            {
                case BrowserLaunchType.SystemPreferred:
                    var tabsBuilder = new Android.Support.CustomTabs.CustomTabsIntent.Builder();
                    var tabsIntent = tabsBuilder.Build();
                    tabsBuilder.SetShowTitle(true);
                    tabsIntent.LaunchUrl(Platform.CurrentContext, nativeUri);
                    break;
                case BrowserLaunchType.External:
                    var intent = new Android.Content.Intent(Android.Content.Intent.ActionView, nativeUri);
                    intent.SetFlags(Android.Content.ActivityFlags.ClearTop);
                    intent.SetFlags(Android.Content.ActivityFlags.NewTask);

                    if (!Platform.IsIntentSupported(intent))
                        throw new FeatureNotSupportedException();

                    Platform.CurrentContext.StartActivity(intent);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
