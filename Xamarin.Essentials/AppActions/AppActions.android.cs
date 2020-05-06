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
        internal static bool PlatformIsSupported
            => Platform.HasApiLevelNMr1;

        static IEnumerable<AppAction> PlatformGetActions()
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

#if __ANDROID_25__
            return Platform.ShortcutManager.DynamicShortcuts.Select(s => s.ToAppAction());
#else
            return null;
#endif
        }

        static void PlatformSetActions(IEnumerable<AppAction> actions)
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

#if __ANDROID_25__
            Platform.ShortcutManager.SetDynamicShortcuts(actions.Select(a => a.ToShortcutInfo()).ToList());
#endif
        }

        static AppAction ToAppAction(this ShortcutInfo shortcutInfo) =>
            new AppAction(shortcutInfo.Id, shortcutInfo.ShortLabel, shortcutInfo.LongLabel);

        static ShortcutInfo ToShortcutInfo(this AppAction action)
        {
            var shortcut = new ShortcutInfo.Builder(Platform.AppContext, action.ActionType)
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

            if (action.Uri != null)
            {
                shortcut.SetIntent(new Intent(Intent.ActionView, AndroidUri.Parse(action.Uri.ToString())));
            }
            else
            {
                shortcut.SetIntent(new Intent(action.ActionType));
            }

            return shortcut.Build();
        }
    }
}
