using System;
using System.Collections.Generic;

namespace Xamarin.Essentials
{
    public static partial class AppActions
    {
        internal static bool IsSupported
            => PlatformIsSupported;

        public static IEnumerable<AppAction> Actions
        {
            get => PlatformGetActions();
            set => PlatformSetActions(value);
        }
    }

    public class AppAction
    {
        public AppAction(string actionType, string title, string subtitle = null, Uri uri = null, string icon = null)
        {
            ActionType = actionType;
            Title = title;
            Subtitle = subtitle;
            Icon = icon;
            Uri = uri;
        }

        public string ActionType { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public Uri Uri { get; set; }

        internal string Icon { get; set; }
    }
}
