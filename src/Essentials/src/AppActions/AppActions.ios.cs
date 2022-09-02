using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Microsoft.Maui.ApplicationModel
{
	partial class AppActionsImplementation : IAppActions, IPlatformAppActions
	{
		public const string ShortcutType = "XE_APP_ACTION_TYPE";

		public bool IsSupported => true;

		public Task<IEnumerable<AppAction>> GetAsync()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			return Task.FromResult(UIApplication.SharedApplication.ShortcutItems.Select(s => s.ToAppAction()));
		}

		public Task SetAsync(IEnumerable<AppAction> actions)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			UIApplication.SharedApplication.ShortcutItems = actions.Select(a => a.ToShortcutItem()).ToArray();

			return Task.CompletedTask;
		}

		public event EventHandler<AppActionEventArgs> AppActionActivated;

		public void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
		{
			if (shortcutItem.Type == ShortcutType)
			{
				var appAction = shortcutItem.ToAppAction();

				AppActionActivated?.Invoke(null, new AppActionEventArgs(appAction));
			}
		}
	}

	static partial class AppActionsExtensions
	{
		internal static AppAction ToAppAction(this UIApplicationShortcutItem shortcutItem)
		{
			string id = null;
			if (shortcutItem.UserInfo.TryGetValue((NSString)"id", out var idObj))
				id = idObj?.ToString();

			string icon = null;
			if (shortcutItem.UserInfo.TryGetValue((NSString)"icon", out var iconObj))
				icon = iconObj?.ToString();

			return new AppAction(id, shortcutItem.LocalizedTitle, shortcutItem.LocalizedSubtitle, icon);
		}

		internal static UIApplicationShortcutItem ToShortcutItem(this AppAction action)
		{
			var keys = new List<NSString>();
			var values = new List<NSObject>();

			// id
			keys.Add((NSString)"id");
			values.Add((NSString)action.Id);

			// icon
			if (!string.IsNullOrEmpty(action.Icon))
			{
				keys.Add((NSString)"icon");
				values.Add((NSString)action.Icon);
			}

			return new UIApplicationShortcutItem(
				AppActionsImplementation.ShortcutType,
				action.Title,
				action.Subtitle,
				action.Icon != null ? UIApplicationShortcutIcon.FromTemplateImageName(action.Icon) : null,
				new NSDictionary<NSString, NSObject>(keys.ToArray(), values.ToArray()));
		}
	}
}
