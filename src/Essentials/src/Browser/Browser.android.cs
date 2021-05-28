using System;
using System.Threading.Tasks;
using Android.Content;
using AndroidX.Browser.CustomTabs;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.Essentials
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
					if (options.PreferredToolbarColor != null)
#pragma warning disable CS0618 // Type or member is obsolete
						tabsBuilder.SetToolbarColor(options.PreferredToolbarColor.ToInt());
#pragma warning restore CS0618 // Type or member is obsolete
					if (options.TitleMode != BrowserTitleMode.Default)
						tabsBuilder.SetShowTitle(options.TitleMode == BrowserTitleMode.Show);

					var tabsIntent = tabsBuilder.Build();
					ActivityFlags? tabsFlags = null;

					Context context = Platform.GetCurrentActivity(false);

					if (context == null)
					{
						context = Platform.AppContext;

						// If using ApplicationContext we need to set ClearTop/NewTask (See #225)
						tabsFlags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
					}

#if __ANDROID_24__
					if (Platform.HasApiLevelN && options.HasFlag(BrowserLaunchFlags.LaunchAdjacent))
					{
						if (tabsFlags.HasValue)
							tabsFlags |= ActivityFlags.LaunchAdjacent | ActivityFlags.NewTask;
						else
							tabsFlags = ActivityFlags.LaunchAdjacent | ActivityFlags.NewTask;
					}
#endif

					// Check if there's flags specified to use
					if (tabsFlags.HasValue)
						tabsIntent.Intent.SetFlags(tabsFlags.Value);

					tabsIntent.LaunchUrl(context, nativeUri);

					break;
				case BrowserLaunchMode.External:
					var intent = new Intent(Intent.ActionView, nativeUri);
					var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
#if __ANDROID_24__
					if (Platform.HasApiLevelN && options.HasFlag(BrowserLaunchFlags.LaunchAdjacent))
						flags |= ActivityFlags.LaunchAdjacent;
#endif
					intent.SetFlags(flags);

					if (!Platform.IsIntentSupported(intent))
						throw new FeatureNotSupportedException();

					Platform.AppContext.StartActivity(intent);
					break;
			}

			return Task.FromResult(true);
		}
	}
}
