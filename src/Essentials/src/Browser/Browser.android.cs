#nullable enable
using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.Browser.CustomTabs;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.ApplicationModel
{
	partial class BrowserImplementation : IBrowser
	{
		public Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options)
		{
			var nativeUri = AndroidUri.Parse(uri.AbsoluteUri);

			switch (options.LaunchMode)
			{
				case BrowserLaunchMode.SystemPreferred:
					LaunchChromeTabs(options, nativeUri);
					break;
				case BrowserLaunchMode.External:
					LaunchExternalBrowser(options, nativeUri);
					break;
			}

			return Task.FromResult(true);
		}

		static void LaunchChromeTabs(BrowserLaunchOptions options, AndroidUri? nativeUri)
		{
			var tabsBuilder = new CustomTabsIntent.Builder();
			tabsBuilder.SetShowTitle(true);
#pragma warning disable CS0618 // Type or member is obsolete
			if (options.PreferredToolbarColor != null)
				tabsBuilder.SetToolbarColor(options.PreferredToolbarColor.ToInt());
#pragma warning restore CS0618 // Type or member is obsolete
			if (options.TitleMode != BrowserTitleMode.Default)
				tabsBuilder.SetShowTitle(options.TitleMode == BrowserTitleMode.Show);

			var tabsIntent = tabsBuilder.Build();
			ActivityFlags? tabsFlags = null;

			Context? context = ActivityStateManager.Default.GetCurrentActivity(false);

			if (context == null)
			{
				context = Application.Context;

				// If using ApplicationContext we need to set ClearTop/NewTask (See #225)
				tabsFlags = ActivityFlags.ClearTop | ActivityFlags.NewTask;
			}

#if __ANDROID_24__
			if (OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				if (options.HasFlag(BrowserLaunchFlags.LaunchAdjacent))
				{
					if (tabsFlags.HasValue)
						tabsFlags |= ActivityFlags.LaunchAdjacent | ActivityFlags.NewTask;
					else
						tabsFlags = ActivityFlags.LaunchAdjacent | ActivityFlags.NewTask;
				}
			}
#endif

			// Check if there's flags specified to use
			if (tabsFlags.HasValue && tabsIntent?.Intent != null)
				tabsIntent.Intent.SetFlags(tabsFlags.Value);

			if (nativeUri != null && context != null && tabsIntent != null)
				tabsIntent.LaunchUrl(context, nativeUri);
		}

		static void LaunchExternalBrowser(BrowserLaunchOptions options, AndroidUri? nativeUri)
		{
			var intent = new Intent(Intent.ActionView, nativeUri);
			var flags = ActivityFlags.ClearTop | ActivityFlags.NewTask;

#if __ANDROID_24__
			if (OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				if (options.HasFlag(BrowserLaunchFlags.LaunchAdjacent))
					flags |= ActivityFlags.LaunchAdjacent;
			}
#endif
			intent.SetFlags(flags);

			if (!PlatformUtils.IsIntentSupported(intent))
				throw new FeatureNotSupportedException();

			Application.Context.StartActivity(intent);
		}
	}
}
