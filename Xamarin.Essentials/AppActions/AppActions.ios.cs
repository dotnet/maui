using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class AppActions
    {
        internal static bool PlatformIsSupported
            => Platform.HasOSVersion(9, 0);

        static IEnumerable<AppAction> PlatformGetActions()
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

            return UIApplication.SharedApplication.ShortcutItems.Select(s => s.ToAppAction());
        }

        static void PlatformSetActions(IEnumerable<AppAction> actions)
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

            UIApplication.SharedApplication.ShortcutItems = actions.Select(a => a.ToShortcutItem()).ToArray();
        }

        static AppAction ToAppAction(this UIApplicationShortcutItem shortcutItem) =>
            new AppAction(shortcutItem.Type, shortcutItem.LocalizedTitle, shortcutItem.LocalizedSubtitle);

        static UIApplicationShortcutItem ToShortcutItem(this AppAction action) =>
            new UIApplicationShortcutItem(
                action.ActionType,
                action.Title,
                action.Subtitle,
                action.Icon != null ? UIApplicationShortcutIcon.FromTemplateImageName(action.Icon) : null,
                new NSDictionary<NSString, NSObject>((NSString)"uri", (NSString)action.Uri.ToString()));
    }
}
