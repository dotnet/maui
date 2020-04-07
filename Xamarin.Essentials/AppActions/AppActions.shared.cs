using System;
using System.Collections.Generic;

namespace Xamarin.Essentials
{
    public static partial class AppActions
    {
        public static IEnumerable<AppAction> Actions
        {
            get => PlatformGetActions();
            set => PlatformSetActions(value);
        }
    }

    public class AppAction
    {
        public string ActionType { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Icon { get; set; }

        public Uri Uri { get; set; }
    }
}
