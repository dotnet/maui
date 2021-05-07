using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;

namespace Microsoft.Maui.Essentials
{
	public static partial class AppActions
	{
		internal static bool PlatformIsSupported
			=> Platform.HasApiLevelNMr1;

		static Task<IEnumerable<AppAction>> PlatformGetAsync()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

#if __ANDROID_25__
			return Task.FromResult(Platform.ShortcutManager.DynamicShortcuts.Select(s => s.ToAppAction()));
#else
			return Task.FromResult < IEnumerable < AppAction >>> (null);
#endif
		}

		static Task PlatformSetAsync(IEnumerable<AppAction> actions)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

#if __ANDROID_25__
			Platform.ShortcutManager.SetDynamicShortcuts(actions.Select(a => a.ToShortcutInfo()).ToList());
#endif
			return Task.CompletedTask;
		}

		static AppAction ToAppAction(this ShortcutInfo shortcutInfo) =>
			new AppAction(shortcutInfo.Id, shortcutInfo.ShortLabel, shortcutInfo.LongLabel);

		const string extraAppActionId = "EXTRA_XE_APP_ACTION_ID";
		const string extraAppActionTitle = "EXTRA_XE_APP_ACTION_TITLE";
		const string extraAppActionSubtitle = "EXTRA_XE_APP_ACTION_SUBTITLE";
		const string extraAppActionIcon = "EXTRA_XE_APP_ACTION_ICON";

		internal static AppAction ToAppAction(this Intent intent)
			=> new AppAction(
				intent.GetStringExtra(extraAppActionId),
				intent.GetStringExtra(extraAppActionTitle),
				intent.GetStringExtra(extraAppActionSubtitle),
				intent.GetStringExtra(extraAppActionIcon));

		static ShortcutInfo ToShortcutInfo(this AppAction action)
		{
			var shortcut = new ShortcutInfo.Builder(Platform.AppContext, action.Id)
				.SetShortLabel(action.Title);

			if (!string.IsNullOrWhiteSpace(action.Subtitle))
			{
				shortcut.SetLongLabel(action.Subtitle);
			}

			if (!string.IsNullOrWhiteSpace(action.Icon))
			{
				var iconResId = Platform.AppContext.Resources.GetIdentifier(action.Icon, "drawable", Platform.AppContext.PackageName);

				shortcut.SetIcon(Icon.CreateWithResource(Platform.AppContext, iconResId));
			}

			var intent = new Intent(Platform.Intent.ActionAppAction);
			intent.SetPackage(Platform.AppContext.PackageName);
			intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
			intent.PutExtra(extraAppActionId, action.Id);
			intent.PutExtra(extraAppActionTitle, action.Title);
			intent.PutExtra(extraAppActionSubtitle, action.Subtitle);
			intent.PutExtra(extraAppActionIcon, action.Icon);

			shortcut.SetIntent(intent);

			return shortcut.Build();
		}
	}
}
