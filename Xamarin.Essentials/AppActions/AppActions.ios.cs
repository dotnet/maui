using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class AppActions
    {
        static IEnumerable<AppAction> PlatformGetActions() =>
            UIApplication.SharedApplication.ShortcutItems.Select(s => s.ToAppAction());

        static void PlatformSetActions(IEnumerable<AppAction> actions) =>
            UIApplication.SharedApplication.ShortcutItems = actions.Select(a => a.ToShortcutItem()).ToArray();

        static AppAction ToAppAction(this UIApplicationShortcutItem shortcutItem) =>
            new AppAction
            {
                ActionType = shortcutItem.Type,
                LocalizedTitle = shortcutItem.LocalizedTitle,
                LocalizedSubtitle = shortcutItem.LocalizedSubtitle,
                Icon = null
            };

        static UIApplicationShortcutItem ToShortcutItem(this AppAction action) =>
            new UIApplicationShortcutItem(
                action.ActionType,
                action.LocalizedTitle,
                action.LocalizedSubtitle,
                action.Icon != null ? UIApplicationShortcutIcon.FromTemplateImageName(action.Icon) : null,
                new NSDictionary<NSString, NSObject>((NSString)"uri", (NSString)action.Uri.ToString()));
    }
}
