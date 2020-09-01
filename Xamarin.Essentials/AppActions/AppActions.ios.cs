using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class AppActions
    {
        internal const string Type = "XE_APP_ACTION_TYPE";

        internal static bool PlatformIsSupported
            => Platform.HasOSVersion(9, 0);

        static Task<IEnumerable<AppAction>> PlatformGetAsync()
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

            return Task.FromResult(UIApplication.SharedApplication.ShortcutItems.Select(s => s.ToAppAction()));
        }

        static Task PlatformSetAsync(IEnumerable<AppAction> actions)
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

            UIApplication.SharedApplication.ShortcutItems = actions.Select(a => a.ToShortcutItem()).ToArray();

            return Task.CompletedTask;
        }

        internal static AppAction ToAppAction(this UIApplicationShortcutItem shortcutItem)
        {
            string id = null;
            if (shortcutItem.UserInfo.TryGetValue((NSString)"id", out var idObj))
                id = idObj?.ToString();

            return new AppAction(shortcutItem.Type, id, shortcutItem.LocalizedTitle, shortcutItem.LocalizedSubtitle);
        }

        static UIApplicationShortcutItem ToShortcutItem(this AppAction action) =>
            new UIApplicationShortcutItem(
                AppActions.Type,
                action.Title,
                action.Subtitle,
                action.Icon != null ? UIApplicationShortcutIcon.FromTemplateImageName(action.Icon) : null,
                new NSDictionary<NSString, NSObject>((NSString)"id", (NSString)action.Id.ToString()));
    }
}
