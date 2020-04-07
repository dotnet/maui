using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics.Drawables;
using AndroidUri = Android.Net.Uri;

namespace Xamarin.Essentials
{
    public static partial class AppActions
    {
        static IEnumerable<AppAction> PlatformGetActions() =>
            Platform.ShortcutManager.DynamicShortcuts.Select(s => s.ToAppAction());

        static void PlatformSetActions(IEnumerable<AppAction> actions) =>
            Platform.ShortcutManager.SetDynamicShortcuts(actions.Select(a => a.ToShortcutInfo()).ToList());

        static AppAction ToAppAction(this ShortcutInfo shortcutInfo) =>
            new AppAction
            {
                ActionType = shortcutInfo.Id,
                Title = shortcutInfo.ShortLabel,
                Subtitle = shortcutInfo.LongLabel,
                Icon = null
            };

        static ShortcutInfo ToShortcutInfo(this AppAction action)
        {
            var shortcut = new ShortcutInfo.Builder(Platform.AppContext, action.ActionType)
                .SetIntent(new Intent(Intent.ActionView, AndroidUri.Parse(action.Uri.ToString())))
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

            return shortcut.Build();
        }
    }
}
