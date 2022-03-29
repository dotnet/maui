using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Runtime;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class AppActionsImplementation : IAppActions
	{
		public string Type => "XE_APP_ACTION_TYPE";
		[SupportedOSPlatformGuard("android26.0")]
		public bool IsSupported
			=> Platform.HasApiLevelNMr1;

		[SupportedOSPlatform("android26.0")]
		public Task<IEnumerable<AppAction>> GetAsync()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

#if __ANDROID_25__
			return Task.FromResult(Platform.ShortcutManager.DynamicShortcuts.Select(s => s.ToAppAction()));
#else
			return Task.FromResult < IEnumerable < AppAction >> (null);
#endif
		}

		[SupportedOSPlatform("android26.0")]
		public Task SetAsync(IEnumerable<AppAction> actions)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

#if __ANDROID_25__
			using var list = new JavaList<ShortcutInfo>(actions.Select(a => a.ToShortcutInfo()));
			Platform.ShortcutManager.SetDynamicShortcuts(list);
#endif
			return Task.CompletedTask;
		}

		public Task SetAsync(params AppAction[] actions)
		{	
			return SetAsync(actions);
		}
	}

	internal static partial class AppActionsExtensions
	{
		[SupportedOSPlatform("android25.0")]
		internal static AppAction ToAppAction(this ShortcutInfo shortcutInfo) =>
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

		[SupportedOSPlatform("android25.0")]
		internal static ShortcutInfo ToShortcutInfo(this AppAction action)
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
